using ChemSpider.Search;
using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RSC.Compounds.Search.Old
{
	public class CSCAdvancedSearchOptions : AdvancedSearchOptions
	{
		[DataMember]
		public NMRFeaturesSearchOptions NMRFeaturesSearchOptions { get; set; }

		public override bool IsEmpty()
		{
			return SearchOptions.IsNullOrEmpty(NMRFeaturesSearchOptions) && base.IsEmpty();
		}
	}

	public class CSCAdvancedSearch : CSAdvancedSearch
	{
		public CSCNMRFeaturesSearch NMRFeaturesSearch { get; set; }

		public CSCAdvancedSearch(ISqlSearchProvider sqlProvider)
		{
			if (sqlProvider == null)
			{
				throw new ArgumentNullException("sqlProvider");
			}

			m_sqlProvider = sqlProvider;

			IntrinsicPropertiesSearch = new CSCIntrinsicPropertiesSearch();
			StructureSearch = new CSCExactStructureSearch();
			CmpIdListSearch = new CSCCmpIdListSearch();
			NMRFeaturesSearch = new CSCNMRFeaturesSearch(sqlProvider);
		}

		public new CSCAdvancedSearchOptions Options
		{
			get { return base.Options as CSCAdvancedSearchOptions; }
		}

		public override void SetOptions(SearchOptions options, CommonSearchOptions common, SearchScopeOptions scopeOptions, SearchResultOptions resultOptions)
		{
			if (!SearchOptions.IsNullOrEmpty((options as CSCAdvancedSearchOptions).NMRFeaturesSearchOptions))
				NMRFeaturesSearch.SetOptions((options as CSCAdvancedSearchOptions).NMRFeaturesSearchOptions, common, scopeOptions, resultOptions);

			base.SetOptions(options, common, scopeOptions, resultOptions);
		}

		protected override bool GetSqlParts(List<string> predicates, List<string> tables, List<string> orderby, List<string> visual, List<string> columns)
		{
			bool bAdded = base.GetSqlParts(predicates, tables, orderby, visual, columns);

			// Properties
			if (!SearchOptions.IsNullOrEmpty(Options.NMRFeaturesSearchOptions))
				bAdded = NMRFeaturesSearch.GetSqlAndSubstitute(predicates, tables, orderby, visual, columns) || bAdded;

			return bAdded;
		}

		protected override CSSubstructureSearch SubstructureSearchInstance()
		{
			return new CSCSubstructureSearch();
		}

		protected override CSSimilarityStructureSearch SimilarityStructureSearchInstance()
		{
			return new CSCSimilarityStructureSearch();
		}
	}
}
