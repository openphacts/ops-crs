using RSC.Compounds.NMRFeatures.Models;
//using ChemSpider.Compounds.NMRFeatures.Search;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.NMRFeatures
{
	public class NMRFeaturesService : INMRFeaturesService
	{
		private readonly INMRFeaturesStore store;

		public NMRFeaturesService(INMRFeaturesStore store)
		{
			if (store == null)
			{
				throw new ArgumentNullException("store");
			}

			this.store = store;
		}

		public IEnumerable<NMRFeature> GetAllFeatures()
		{
			return store.GetAllFeatures();
		}
		public IEnumerable<CompoundNMRFeature> GetCompoundFeatures(int compoundId)
		{
			return store.GetCompoundFeatures(compoundId);
		}
		public void AddCompoundFeature(int compoundId, string name, int count)
		{
			store.AddCompoundFeature(compoundId, name, count);
		}
		public void AddCompoundFeatures(int compoundId, IEnumerable<Tuple<string, int>> features)
		{
			store.AddCompoundFeatures(compoundId, features);
		}
		public void AddCompoundsFeatures(IEnumerable<Tuple<int, string, int>> compoundFeatures)
		{
			store.AddCompoundsFeatures(compoundFeatures);
		}
	}
}
