using RSC.Compounds.NMRFeatures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Transactions;
using EntityFramework.BulkInsert.Extensions;
using System.Text.RegularExpressions;

namespace RSC.Compounds.NMRFeatures.EntityFramework
{
	public class NMRFeaturesStore : INMRFeaturesStore
	{
		private NMRFeaturesDbContext db = null;

		public NMRFeaturesStore()
		{
			db = new NMRFeaturesDbContext();
		}

		public IEnumerable<NMRFeature> GetAllFeatures()
		{
			return from f in db.NMRFeatures select f;
		}
		public IEnumerable<CompoundNMRFeature> GetCompoundFeatures(int compoundId)
		{
			var query = from f in db.CompoundNMRFeatures where f.CompoundId == compoundId select f;

			return query.ToList();
		}

		public void AddCompoundFeature(int compoundId, string name, int count)
		{
			var query = from f in db.CompoundNMRFeatures where f.CompoundId == compoundId && f.NMRFeature.Name.Equals(name, StringComparison.OrdinalIgnoreCase) select f;

			if (query.Any())
			{
				//	feature already registered for this compound... 
				var feature = query.First();
				feature.Count = count;
				db.SaveChanges();
			}
			else
			{
				//	register new feature for this compound...
				var featureQuery = from f in db.NMRFeatures where f.Name.Equals(name, StringComparison.OrdinalIgnoreCase) select f;
				if (featureQuery.Any())
				{
					//	feature with the specified name registered in the system...

					NMRFeature feature = featureQuery.First();

					db.CompoundNMRFeatures.Add(
						new CompoundNMRFeature()
						{
							CompoundId = compoundId,
							NMRFeatureId = feature.Id,
							Count = count
						});

					db.SaveChanges();
				}
			}
		}

		/// <summary>
		/// Add new features to compound by tuple (FeatureId, Count)
		/// </summary>
		/// <param name="compoundId">Compound ID</param>
		/// <param name="features">List of tuples (FeatureId, Count)</param>
		private void _addCompoundFeatures(int compoundId, IEnumerable<Tuple<int, int>> features)
		{
			var featuresId = from f in features select f.Item1;

			using (var ctx = new NMRFeaturesDbContext())
			{
				ctx.Configuration.AutoDetectChangesEnabled = false;
				ctx.Configuration.ValidateOnSaveEnabled = false;

				using (var transactionScope = new TransactionScope())
				{
					var alreadyExist = (from f in ctx.CompoundNMRFeatures where f.CompoundId == compoundId && featuresId.Any(id => f.NMRFeatureId == id) select f.NMRFeatureId).ToList();

					if (alreadyExist.Any())
					{
						var updateFeatures = (from f in features where alreadyExist.Any(id => id == f.Item1) select f).ToList();
						foreach (var feature in updateFeatures)
						{
							var dbFeature = ctx.CompoundNMRFeatures.Where(f => f.CompoundId == compoundId && f.NMRFeatureId == feature.Item1).First();
							dbFeature.Count = feature.Item2;
						}
					}

					var insertFeatures = (from f in features
										  where !alreadyExist.Any(id => id == f.Item1)
										  select new CompoundNMRFeature()
										  {
											  CompoundId = compoundId,
											  NMRFeatureId = f.Item1,
											  Count = f.Item2
										  }).ToList();

					if (insertFeatures.Any())
					{
						//	add new features...
						ctx.BulkInsert(insertFeatures);
					}

					ctx.SaveChanges();
					transactionScope.Complete();
				}
			}
		}

		public void AddCompoundFeatures(int compoundId, IEnumerable<Tuple<string, int>> features)
		{
			var registeredFeatures = GetAllFeatures().ToDictionary(f => f.Name, f => f.Id);

			_addCompoundFeatures(compoundId, from f in features select new Tuple<int, int>(registeredFeatures[f.Item1], f.Item2));
		}

		public void AddCompoundsFeatures(IEnumerable<Tuple<int, string, int>> compoundFeatures)
		{
			throw new NotImplementedException("AddCompoundsFeatures");
		}

		public IEnumerable<int> SearchFeatures(IDictionary<string, string> query)
		{
			IEnumerable<int> ids = null;

			Regex range = new Regex("(?<from>[0-9]+)-(?<to>[0-9]+)");

			foreach (var feature in query)
			{
				IEnumerable<int> res = null;

				Match match = range.Match(feature.Value);

				if (match.Success)
				{
					var from = Convert.ToInt32(match.Groups["from"].Value);
					var to = Convert.ToInt32(match.Groups["to"].Value);

					res = from r in db.CompoundNMRFeatures
						  join f in db.NMRFeatures on r.NMRFeatureId equals f.Id
						  where f.Name == feature.Key && r.Count >= @from && r.Count <= @to
						  select r.CompoundId;
				}
				else
				{
					var count = Convert.ToInt32(feature.Value);

					res = from r in db.CompoundNMRFeatures
						  join f in db.NMRFeatures on r.NMRFeatureId equals f.Id
						  where f.Name == feature.Key && r.Count == count
						  select r.CompoundId;
				}

				if (ids == null)
					ids = res;
				else
					ids = Enumerable.Intersect(ids, res);

				if (ids.Count() == 0)
					break;
			}

			return ids;
		}
	}
}
