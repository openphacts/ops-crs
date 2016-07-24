using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.Serialization;
using System.ComponentModel;

namespace ChemSpider.Search
{
    [Serializable]
    public abstract class SearchOptions
    {
        public abstract bool IsEmpty();
        public static bool IsNullOrEmpty(SearchOptions o)
        {
            return o == null || o.IsEmpty();
        }
    }

    [Serializable]
    [DataContract]
    public class CommonSearchOptions : SearchOptions
    {
        public enum EComplexity
        {
            Any,
            Single,
            Multi
        }

        public enum EIsotopic
        {
            Any,
            Labeled,
            NotLabeled
        }

        [DataMember]
        public EComplexity Complexity { get; set; }
        [DataMember]
        public EIsotopic Isotopic { get; set; }
        [DataMember]
        public bool HasSpectra { get; set; }
        [DataMember]
        public bool HasPatents { get; set; }

        public override bool IsEmpty() { return false; }
    }

    [Serializable]
    [DataContract]
    [Description("Simple search options")]
    public class SimpleSearchOptions : SearchOptions
    {
        public SimpleSearchOptions()
        {
            AllowVagueSearch = true;
        }

        [DataMember]
        public string QueryText { get; set; }
        [DataMember]
        public bool AllowVagueSearch { get; set; }

        public override bool IsEmpty() { return string.IsNullOrEmpty(QueryText); }
    }

    [Serializable]
    [DataContract]
    public class StructureSearchOptions : SearchOptions
    {
        [DataMember]
        [Description("Specifying molecule in SMILES or MOL format")]
        public string Molecule
        {
            get;
            set;
        }

        public enum ESearchType
        {
            Exact,
            Substructure,
            Similarity
        }

        public override bool IsEmpty() { return string.IsNullOrEmpty(Molecule); }
    }

    [Serializable]
    [DataContract]
    public class ExactStructureSearchOptions : StructureSearchOptions
    {
        public ExactStructureSearchOptions()
        {
        }

        public enum EMatchType
        {
            ExactMatch,
            AllTautomers,
            SameSkeletonIncludingH,
            SameSkeletonExcludingH,
            AllIsomers
        }

        [DataMember]
        public EMatchType MatchType { get; set; }
    }

    [Serializable]
    [DataContract]
    public class SubstructureSearchOptions : StructureSearchOptions
    {
        public enum EMolType
        {
            SMILES,
            SMARTS
        }

        public SubstructureSearchOptions()
        {
            MolType = EMolType.SMILES;
        }

        [DataMember]
        public bool MatchTautomers { get; set; }

        [DataMember]
        public EMolType MolType { get; set; }
    }

    [Serializable]
    [DataContract]
    public class SimilaritySearchOptions : StructureSearchOptions
    {
        public SimilaritySearchOptions()
        {
        }

        public enum ESimilarityType
        {
            Tanimoto,
            Tversky,
            Euclidian
        }

        [DataMember]
        [Description("Specifying the metric to use: Tanimoto, Tversky or Euclidian")]
        public ESimilarityType SimilarityType { get; set; }

        [DataMember]
        [Description("The lower limit of the desired similarity")]
        public float Threshold { get; set; }

        [DataMember]
        [Description("In case of Tversky metric, there are optional 'alpha' and 'beta' parameters: Tversky 0.9 0.1 denotes alpha = 0.9, beta = 0.1. The default is alpha = beta = 0.5")]
        public float Alpha { get; set; }

        [DataMember]
        [Description("In case of Tversky metric, there are optional 'alpha' and 'beta' parameters: Tversky 0.9 0.1 denotes alpha = 0.9, beta = 0.1. The default is alpha = beta = 0.5")]
        public float Beta { get; set; }
    }

    [Serializable]
    [DataContract]
    public class IntrinsicPropertiesSearchOptions : SearchOptions
    {
        [DataMember]
        public string EmpiricalFormula { get; set; }

        [DataMember]
        public double? MolWeightMin { get; set; }
        [DataMember]
        public double? MolWeightMax { get; set; }

        [DataMember]
        public double? NominalMassMin { get; set; }
        [DataMember]
        public double? NominalMassMax { get; set; }

        [DataMember]
        public double? AverageMassMin { get; set; }
        [DataMember]
        public double? AverageMassMax { get; set; }

        [DataMember]
        public double? MonoisotopicMassMin { get; set; }
        [DataMember]
        public double? MonoisotopicMassMax { get; set; }

        public bool IsMWSearch { get { return MolWeightMin != null && MolWeightMax != null; } }
        public bool IsNMSearch { get { return NominalMassMin != null && NominalMassMax != null; } }
        public bool IsAMSearch { get { return AverageMassMin != null && AverageMassMax != null; } }
        public bool IsMMSearch { get { return MonoisotopicMassMin != null && MonoisotopicMassMax != null; } }

        public override bool IsEmpty()
        {
            return string.IsNullOrEmpty(EmpiricalFormula) && MolWeightMin == null && MolWeightMax == null &&
                   NominalMassMax == null && NominalMassMin == null && AverageMassMax == null && AverageMassMin == null &&
                   MonoisotopicMassMax == null && MonoisotopicMassMin == null;
        }
    }

    [Serializable]
    [DataContract]
    public class DataSourceSearchOptions : SearchOptions
    {
        [DataMember]
        public List<string> DataSources { get; set; }
        [DataMember]
        public List<string> DataSourceTypes { get; set; }
        [DataMember]
        public List<string> FocusedLibraries { get; set; }
        [DataMember]
        public List<string> XSections { get; set; }

        public DataSourceSearchOptions()
        {
            DataSources = new List<string>();
            DataSourceTypes = new List<string>();
            FocusedLibraries = new List<string>();
            XSections = new List<string>();
        }

        public override bool IsEmpty()
        {
            return DataSources.Count == 0 && DataSourceTypes.Count == 0 && FocusedLibraries.Count == 0 && XSections.Count == 0;
        }
    }

    [Serializable]
    [DataContract]
    public class SearchScopeOptions : SearchOptions
    {
        [DataMember]
        public List<string> DataSources { get; set; }
        [DataMember]
        public List<string> DataSourceTypes { get; set; }
        [DataMember]
        public List<string> XSections { get; set; }

        public SearchScopeOptions()
        {
            DataSources = new List<string>();
            DataSourceTypes = new List<string>();
            XSections = new List<string>();
        }

        public override bool IsEmpty()
        {
            return DataSources.Count == 0 && DataSourceTypes.Count == 0 && XSections.Count == 0;
        }
    }

    [Serializable]
    [DataContract]
    public class SearchResultOptions : SearchOptions
    {
        [DataMember]
        public int Limit { get; set; }
        [DataMember]
        public int Start { get; set; }
        [DataMember]
        public int Length { get; set; }
        [DataMember]
        public List<string> SortOrder { get; set; }

        public SearchResultOptions()
        {
            Limit = -1;
            Start = 0;
            Length = -1;
            SortOrder = new List<string>();
        }

        public override bool IsEmpty()
        {
            return false;
        }
    }

    [Serializable]
    [DataContract]
    public class ElementsSearchOptions : SearchOptions
    {
        // true = include all
        // false = include any
        [DataMember]
        public bool IncludeAll { get; set; }

        [DataMember]
        public List<string> IncludeElements { get; set; }
        [DataMember]
        public List<string> ExcludeElements { get; set; }

        public ElementsSearchOptions()
        {
            IncludeElements = new List<string>();
            ExcludeElements = new List<string>();
        }

        public override bool IsEmpty()
        {
            return IncludeElements.Count == 0 && ExcludeElements.Count == 0;
        }
    }

    [Serializable]
    [DataContract]
    public class PredictedPropertiesSearchOptions : SearchOptions
    {
        [DataMember]
        public double? LogPMax { get; set; }
        [DataMember]
        public double? LogPMin { get; set; }
        [DataMember]
        public double? LogD55Max { get; set; }
        [DataMember]
        public double? LogD55Min { get; set; }
        [DataMember]
        public double? LogD74Max { get; set; }
        [DataMember]
        public double? LogD74Min { get; set; }
        [DataMember]
        public int? RuleOf5Max { get; set; }
        [DataMember]
        public int? RuleOf5Min { get; set; }
        [DataMember]
        public int? HAcceptorsMax { get; set; }
        [DataMember]
        public int? HAcceptorsMin { get; set; }
        [DataMember]
        public int? HDonorsMax { get; set; }
        [DataMember]
        public int? HDonorsMin { get; set; }
        [DataMember]
        public int? FreelyRotatableBondsMax { get; set; }
        [DataMember]
        public int? FreelyRotatableBondsMin { get; set; }
        [DataMember]
        public double? PolarSurfaceAreaMax { get; set; }
        [DataMember]
        public double? PolarSurfaceAreaMin { get; set; }
        [DataMember]
        public double? MolarVolumeMax { get; set; }
        [DataMember]
        public double? MolarVolumeMin { get; set; }
        [DataMember]
        public double? RefractiveIndexMax { get; set; }
        [DataMember]
        public double? RefractiveIndexMin { get; set; }
        [DataMember]
        public double? BoilingPointMax { get; set; }
        [DataMember]
        public double? BoilingPointMin { get; set; }
        [DataMember]
        public double? FlashPointMax { get; set; }
        [DataMember]
        public double? FlashPointMin { get; set; }
        [DataMember]
        public double? DensityMax { get; set; }
        [DataMember]
        public double? DensityMin { get; set; }
        [DataMember]
        public double? SurfaceTensionMax { get; set; }
        [DataMember]
        public double? SurfaceTensionMin { get; set; }

        public override bool IsEmpty()
        {
            return LogPMax == null && LogPMin == null && LogD55Max == null && LogD55Min == null &&
                    LogD74Max == null && LogD74Min == null && RuleOf5Max == null && RuleOf5Min == null &&
                    HAcceptorsMax == null && HAcceptorsMin == null && HDonorsMax == null && HDonorsMin == null &&
                    FreelyRotatableBondsMax == null && FreelyRotatableBondsMin == null &&
                    PolarSurfaceAreaMax == null && PolarSurfaceAreaMin == null &&
                    MolarVolumeMax == null && MolarVolumeMin == null &&
                    RefractiveIndexMax == null && RefractiveIndexMin == null &&
                    BoilingPointMax == null && BoilingPointMin == null &&
                    FlashPointMax == null && FlashPointMin == null &&
                    DensityMax == null && DensityMin == null &&
                    SurfaceTensionMax == null && SurfaceTensionMin == null;
        }
    }

    [Serializable]
    [DataContract]
    public class LassoSearchOptions : SearchOptions
    {
        [DataMember]
        public double ThresholdMin { get; set; }
        [DataMember]
        public string FamilyMin { get; set; }

        [DataMember]
        public double ThresholdMax { get; set; }
        [DataMember]
        public List<string> FamilyMax { get; set; }

        public LassoSearchOptions()
        {
            FamilyMax = new List<string>();
        }

        public override bool IsEmpty()
        {
            return FamilyMax.Count == 0;
        }
    }

    [Serializable]
    [DataContract]
    public class KeywordSearchOptions : SimpleSearchOptions
    {
        public enum EQueryType  // TODO: Extend with something really useful - this enum is from old days
        {
            Synonym,
            InChI,
            InChIKey,
            Substring,
            Approximate
        }
        [DataMember]
        public EQueryType QueryType { get; set; }
    }

    [Serializable]
    [DataContract]
    public class BaseNameSearchOptions : SimpleSearchOptions
    {
        public enum EMatchType
        {
            Exact,
            Substring,
            Regex,
            Approximate
        }

        [DataMember]
        public EMatchType MatchType { get; set; }
        [DataMember]
        public string Language { get; set; }
        [DataMember]
        public List<string> SetFlags { get; set; }
        [DataMember]
        public List<string> UnsetFlags { get; set; }

        public enum EApprovedAssociationsClause
        {
            Include,
            Exclude,
            Only
        }

        [DataMember]
        public EApprovedAssociationsClause ApprovedAssociationsClause { get; set; }

        public BaseNameSearchOptions()
        {
            SetFlags = new List<string>();
            UnsetFlags = new List<string>();
        }
    }

    [Serializable]
    [DataContract]
    public class NameSearchOptions : BaseNameSearchOptions
    {
    }

    [Serializable]
    [DataContract]
    public class IdentifierSearchOptions : BaseNameSearchOptions
    {
        public enum EIdentifierType
        {
            SMILES,
            Name,
            InChI
        }

        [DataMember]
        public EIdentifierType IdentifierType { get; set; }
    }

    [Serializable]
    [DataContract]
    public class AdvancedIdentifierSearchOptions : SearchOptions
    {
        [DataMember]
        public IdentifierSearchOptions Options1 { get; set; }
        [DataMember]
        public bool Options2Not { get; set; }
        [DataMember]
        public IdentifierSearchOptions Options2 { get; set; }

        public override bool IsEmpty()
        {
            return SearchOptions.IsNullOrEmpty(Options1);
        }
    }

    [Serializable]
    [DataContract]
    public class AdvancedSearchOptions : SearchOptions
    {
        [DataMember]
        public SubstructureSearchOptions SubstructureSearchOptions { get; set; }
        [DataMember]
        public SimilaritySearchOptions SimilaritySearchOptions { get; set; }
        [DataMember]
        public ExactStructureSearchOptions ExactStructureSearchOptions { get; set; }

        [DataMember]
        public StructureSearchOptions StructureSearchOptions { get; set; }
        [DataMember]
        public KeywordSearchOptions KeywordSearchOptions { get; set; }
        [DataMember]
        public ElementsSearchOptions ElementsSearchOptions { get; set; }
        [DataMember]
        public IntrinsicPropertiesSearchOptions IntrinsicPropertiesSearchOptions { get; set; }
        [DataMember]
        public PredictedPropertiesSearchOptions PredictedPropertiesSearchOptions { get; set; }
        [DataMember]
        public DataSourceSearchOptions DataSourceSearchOptions { get; set; }
        [DataMember]
        public LassoSearchOptions LassoSearchOptions { get; set; }
        [DataMember]
        public SuppInfoSearchOptions SuppInfoSearchOptions { get; set; }
        [DataMember]
        public CmpIdListSearchOptions CmpIdListSearchOptions { get; set; }

        public override bool IsEmpty()
        {
            return SearchOptions.IsNullOrEmpty(StructureSearchOptions) && SearchOptions.IsNullOrEmpty(KeywordSearchOptions) && SearchOptions.IsNullOrEmpty(ElementsSearchOptions) &&
                    SearchOptions.IsNullOrEmpty(IntrinsicPropertiesSearchOptions) && SearchOptions.IsNullOrEmpty(PredictedPropertiesSearchOptions) &&
                    SearchOptions.IsNullOrEmpty(DataSourceSearchOptions) && SearchOptions.IsNullOrEmpty(LassoSearchOptions) && SearchOptions.IsNullOrEmpty(SuppInfoSearchOptions);
        }
    }

    [Serializable]
    [DataContract]
    public class SearchOptionsPair
    {
        [DataMember]
        public CSSearch Search { get; set; }
        [DataMember]
        public SearchOptions Options { get; set; }

        public SearchOptionsPair()
        {
        }

        public SearchOptionsPair(CSSearch search, SearchOptions options)
        {
            Search = search;
            Options = options;
        }
    }

    [Serializable]
    [DataContract]
    public class FlexibleSearchOptions : SearchOptions
    {
        [DataMember]
        public List<SearchOptionsPair> InnerSearches { get; set; }

        public FlexibleSearchOptions()
        {
            if (InnerSearches == null)
                InnerSearches = new List<SearchOptionsPair>();
        }

        public override bool IsEmpty()
        {
            return InnerSearches.All(p => p.Options.IsEmpty());
        }
    }

    [Serializable]
    [DataContract]
    public class CmpIdListSearchOptions : SearchOptions
    {
        [DataMember]
        public List<int> CmpIdList { get; set; }

        public CmpIdListSearchOptions()
        {
            CmpIdList = new List<int>();
        }

        public CmpIdListSearchOptions(List<int> opts)
        {
            CmpIdList = opts;
        }

        public override bool IsEmpty()
        {
            return CmpIdList.Count == 0;
        }
    }

    [Serializable]
    [DataContract]
    public class SuppInfoSearchOptions : SearchOptions
    {
        [DataMember]
        public List<TextPropertySearchOptions> TextSearchOptions { get; set; }

        [DataMember]
        public List<NumericPropertySearchOptions> NumericSearchOptions { get; set; }

        [DataMember]
        public List<AnnotationSearchOptions> AnnotationSearchOptions { get; set; }

        [DataMember]
        public bool AnnotationSearchOr { get; set; }

        public override bool IsEmpty()
        {
            return TextSearchOptions.Count == 0 && NumericSearchOptions.Count == 0 && AnnotationSearchOptions.Count == 0;
        }
    }

    [Serializable]
    [DataContract]
    public class TextPropertySearchOptions : SearchOptions
    {
        [DataMember]
        public int PropertyId { get; set; }

        [DataMember]
        public string PropertyValue { get; set; }

        [DataMember]
        public string PropertyLabel { get; set; }

        public override bool IsEmpty()
        {
            return string.IsNullOrEmpty(PropertyValue);
        }
    }

    [Serializable]
    [DataContract]
    public class NumericPropertySearchOptions : SearchOptions
    {
        [DataMember]
        public int PropertyId { get; set; }

        [DataMember]
        public int PropertyTypeId { get; set; }

        [DataMember]
        public int PropertyDefaultPrimaryUnitId { get; set; }

        [DataMember]
        public int PropertyPrimaryUnitId { get; set; }

        [DataMember]
        public string PropertyPrimaryUnit { get; set; }

        [DataMember]
        public int PropertyDefaultSecondaryUnitId { get; set; }

        [DataMember]
        public string PropertyDefaultSecondaryValue { get; set; }

        [DataMember]
        public int PropertySecondaryUnitId { get; set; }

        [DataMember]
        public string PropertySecondaryUnit { get; set; }

        [DataMember]
        public string PropertySecondaryValue { get; set; }

        [DataMember]
        public string PropertyLabel { get; set; }

        [DataMember]
        public string PropertyValue { get; set; }

        [DataMember]
        public string PropertyValueDelta { get; set; }

        [DataMember]
        public string PropertyValueMin { get; set; }

        [DataMember]
        public string PropertyValueMax { get; set; }

        [DataMember]
        public bool PropertyMMSelected { get; set; }

        //TODO: Add primary units, secondary units and secondary value.
        public override bool IsEmpty()
        {
            return string.IsNullOrEmpty(PropertyValue)
                    && string.IsNullOrEmpty(PropertyValueMin)
                    && string.IsNullOrEmpty(PropertyValueMax);
        }
    }

    [Serializable]
    [DataContract]
    public class AnnotationSearchOptions : SearchOptions
    {
        [DataMember]
        public int AnnotationId { get; set; }

        [DataMember]
        public string AnnotationValue { get; set; }

        public override bool IsEmpty()
        {
            return string.IsNullOrEmpty(AnnotationValue);
        }
    }
}
