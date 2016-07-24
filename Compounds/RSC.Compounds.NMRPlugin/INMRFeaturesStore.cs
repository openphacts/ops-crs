using RSC.Compounds.NMRFeatures.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.NMRFeatures
{
	public interface INMRFeaturesStore
	{
		IEnumerable<NMRFeature> GetAllFeatures();
		IEnumerable<CompoundNMRFeature> GetCompoundFeatures(int compoundId);
		void AddCompoundFeature(int compoundId, string name, int count);
		void AddCompoundFeatures(int compoundId, IEnumerable<Tuple<string, int>> features);
		void AddCompoundsFeatures(IEnumerable<Tuple<int, string, int>> compoundFeatures);
		IEnumerable<int> SearchFeatures(IDictionary<string, string> query);
	}
}
