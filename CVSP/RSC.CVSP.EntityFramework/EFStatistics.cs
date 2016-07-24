using RSC.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.EntityFramework
{
	public class EFStatistics : IStatistics
	{
		private readonly string connectionString;
		private readonly int timeout;

		public EFStatistics(string connectionString, int timeout)
		{
			this.connectionString = connectionString;
			this.timeout = timeout;
		}

		/// <summary>
		/// takes deposition guid and returns a statistic information about deposition
		/// </summary>
		/// <param name="depositionId">deposition guid</param>
		/// <returns>Deposition's statistic object</returns>
		public Statistic GetDepositionStats(Guid depositionId)
		{
			var logEntries = LogManager.Logger.EntryTypes.ToList();

			using (var db = new CVSPContext(this.connectionString, this.timeout))
			{
				var stats = new Statistic();

				stats.RecordsNumber = (from r in db.Records
									   where r.Deposition.Guid == depositionId
									   select r.ExternalId.ObjectId).Count();

				var errorCodes = logEntries.Where(le => le.Severity == Severity.Error).Select(le => le.Code).ToList();
				stats.ErrorsNumber = (from i in db.Issues
									  where i.Record.Deposition.Guid == depositionId && errorCodes.Contains(i.Code)
									  select i.RecordId).Distinct().Count();

				var warningCodes = logEntries.Where(le => le.Severity == Severity.Warning).Select(le => le.Code).ToList();
				stats.WarningsNumber = (from i in db.Issues
										where i.Record.Deposition.Guid == depositionId && warningCodes.Contains(i.Code)
										select i.RecordId).Distinct().Count();

				var infoCodes = logEntries.Where(le => le.Severity == Severity.Information).Select(le => le.Code).ToList();
				stats.InfosNumber = (from i in db.Issues
									 where i.Record.Deposition.Guid == depositionId && infoCodes.Contains(i.Code)
									 select i.RecordId).Distinct().Count();

				stats.Issues = (from r in db.Records
								join i in db.Issues on r.Id equals i.Record.Id
								where r.Deposition.Guid == depositionId
								group i.Record.Id by new { i.Code } into g
								select new IssueStat()
								{
									Code = g.Key.Code,
									Count = g.Distinct().Count()
								}).ToList();

				return stats;
			}
		}
	}
}
