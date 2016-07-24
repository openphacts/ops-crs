namespace RSC.CVSP.EntityFramework
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
	using RSC.CVSP.Search;

	public class EFCVSPSearch : CVSPSearch
	{
		private readonly CVSPContext db;

		public EFCVSPSearch(string connectionString, int timeout)
		{
			this.db = new CVSPContext(connectionString, timeout);
		}

		/// <summary>Run depositions search</summary>
		/// <param name="options">General CVSP options for search</param>
		/// <param name="commonOptions">Common CVSP options</param>
		/// <param name="scopeOptions">Scope CVSP options</param>
		/// <param name="resultsOptions">Results CVSP options</param>
		/// <returns>List of depositions' GUIDs that match the search request</returns>
		public override IEnumerable<Guid> DepositionsSearch(CVSPDepositionsSearchOptions options, CVSPCommonSearchOptions commonOptions, CVSPSearchScopeOptions scopeOptions, CVSPSearchResultOptions resultsOptions)
		{
			var query = from d in this.db.Depositions select d;

			if (options.User != Guid.Empty)
			{
				query = from d in query where d.UserProfile.Guid == options.User select d;
			}

			if (options.Status.Length > 0)
			{
				query = from d in query join s in options.Status on d.Status equals (int)s select d;
			}

			return (from r in query select r.Guid).ToList().GroupBy(g => g).Select(g => g.First());
		}

		/// <summary>Run records search</summary>
		/// <param name="options">General CVSP options for search</param>
		/// <param name="commonOptions">Common CVSP options</param>
		/// <param name="scopeOptions">Scope CVSP options</param>
		/// <param name="resultsOptions">Results CVSP options</param>
		/// <returns>List of records' GUIDs that match the search request</returns>
		public override IEnumerable<Guid> RecordsSearch(CVSPRecordsSearchOptions options, CVSPCommonSearchOptions commonOptions, CVSPSearchScopeOptions scopeOptions, CVSPSearchResultOptions resultsOptions)
		{
			var query = from r in this.db.Records select r;

			if (options.Deposition != Guid.Empty)
			{
				query = from r in query join d in this.db.Depositions on r.Deposition.Id equals d.Id where d.Guid == options.Deposition select r;
			}

			if (options.Codes != null && options.Codes.Length > 0)
			{
				query = from r in query join i in this.db.Issues on r.Id equals i.Record.Id join c in options.Codes on i.Code equals c select r;
			}

			if (options.Ordinals != null && options.Ordinals.Length > 0)
			{
				query = from r in query join o in options.Ordinals on r.Ordinal + 1 equals o select r;
			}

			//query.AsQueryable().OrderBy("Ordinal ASC");
			//query.OrderByDescending(r => r.Ordinal);

			return (from r in query orderby r.Ordinal select (Guid)r.ExternalId.ObjectId).ToList().GroupBy(g => g).Select(g => g.First());
		}
	}
}
