using System;
using System.Threading;
using ChemSpider.Search;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace RSC.Compounds.Tests
{
	[TestClass]
	public class CRSSearches
	{
		[TestMethod]
		public void ExactSearch_ExactMatch()
		{
			ExactStructureSearchOptions searchOptions = new ExactStructureSearchOptions
			{
				MatchType = ExactStructureSearchOptions.EMatchType.ExactMatch,
				Molecule = @"CC1NC=NC=1CSCC/N=C(/NC#N)\NC"
			};
			CommonSearchOptions commonOptions = new CommonSearchOptions { };
			SearchScopeOptions scopeOptions = new SearchScopeOptions { };
			SearchResultOptions resultOptions = new SearchResultOptions { };

			CSSearchResult result = runExactSearch(searchOptions, commonOptions, scopeOptions, resultOptions);
			Assert.IsTrue(result.FoundCount == 1);
		}

		[TestMethod]
		public void ExactSearch_AllTautomers()
		{
			ExactStructureSearchOptions searchOptions = new ExactStructureSearchOptions
			{
				MatchType = ExactStructureSearchOptions.EMatchType.AllTautomers,
				Molecule = @"CC1NC=NC=1CSCC/N=C(/NC#N)\NC"
			};
			CommonSearchOptions commonOptions = new CommonSearchOptions { };
			SearchScopeOptions scopeOptions = new SearchScopeOptions { };
			SearchResultOptions resultOptions = new SearchResultOptions { };

			CSSearchResult result = runExactSearch(searchOptions, commonOptions, scopeOptions, resultOptions);
			Assert.IsTrue(result.FoundCount > 1);
		}

		[TestMethod]
		public void ExactSearch_SameSkeletonIncludingH()
		{
			ExactStructureSearchOptions searchOptions = new ExactStructureSearchOptions
			{
				MatchType = ExactStructureSearchOptions.EMatchType.SameSkeletonIncludingH,
				Molecule = @"CC1NC=NC=1CSCC/N=C(/NC#N)\NC"
			};
			CommonSearchOptions commonOptions = new CommonSearchOptions { };
			SearchScopeOptions scopeOptions = new SearchScopeOptions { };
			SearchResultOptions resultOptions = new SearchResultOptions { };

			CSSearchResult result = runExactSearch(searchOptions, commonOptions, scopeOptions, resultOptions);
			Assert.IsTrue(result.FoundCount > 1);
		}

		[TestMethod]
		public void ExactSearch_Sildenafil_ExactMatch()
		{
			ExactStructureSearchOptions searchOptions = new ExactStructureSearchOptions
			{
				MatchType = ExactStructureSearchOptions.EMatchType.ExactMatch,
				Molecule = @"CCCC1=NN(C2=C1NC(=NC2=O)C3=C(C=CC(=C3)S(=O)(=O)N4CCN(CC4)C)OCC)C"
			};
			CommonSearchOptions commonOptions = new CommonSearchOptions { };
			SearchScopeOptions scopeOptions = new SearchScopeOptions { };
			SearchResultOptions resultOptions = new SearchResultOptions { };

			CSSearchResult result = runExactSearch(searchOptions, commonOptions, scopeOptions, resultOptions);
			Assert.IsTrue(result.FoundCount == 1);
		}

		[TestMethod]
		public void ExactSearch_Sildenafil_AllTautomers()
		{
			ExactStructureSearchOptions searchOptions = new ExactStructureSearchOptions
			{
				MatchType = ExactStructureSearchOptions.EMatchType.AllTautomers,
				Molecule = @"CCCC1=NN(C2=C1NC(=NC2=O)C3=C(C=CC(=C3)S(=O)(=O)N4CCN(CC4)C)OCC)C"
			};
			CommonSearchOptions commonOptions = new CommonSearchOptions { };
			SearchScopeOptions scopeOptions = new SearchScopeOptions { };
			SearchResultOptions resultOptions = new SearchResultOptions { };

			CSSearchResult result = runExactSearch(searchOptions, commonOptions, scopeOptions, resultOptions);
			Assert.IsTrue(result.FoundCount > 1);
		}

		private static CSSearchResult runExactSearch(ExactStructureSearchOptions searchOptions, CommonSearchOptions commonOptions, SearchScopeOptions scopeOptions, SearchResultOptions resultOptions)
		{
			CSSearchResult result = SearchUtility.RunSearch(null /* CSCSearchFactory.GetExactStructureSearch() */, searchOptions, commonOptions, scopeOptions, resultOptions);
			Assert.IsTrue(result != null && !String.IsNullOrEmpty(result.Rid));

			DateTime start = DateTime.Now;
			do { Thread.Sleep(100); }
			while (result.Status != ERequestStatus.ResultReady && (DateTime.Now - start).TotalSeconds < 10);
			return result;
		}
	}
}
