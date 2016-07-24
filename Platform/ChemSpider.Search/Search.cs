using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.Serialization;
using ChemSpider.Security;

namespace ChemSpider.Search
{
    [DataContract]
    public class ResultRecord
    {
        [DataMember(EmitDefaultValue = false)]
        [Description("Record ID")]
        public int Id { get; set; }

        [DataMember(EmitDefaultValue = false)]
        [Description("Relevance that specify record's accuracy")]
        public double Relevance { get; set; }
    }

    public enum ESearchMode
    {
        eSearchMode_Simple,
        eSearchMode_Structure,
        eSearchMode_LASSOSimple,
        eSearchMode_LASSOAdvanced,
        eSearchMode_Advanced,
        eSearchMode_Elements,
        eSearchMode_Properties,
        eSearchMode_CalculatedProperties,
        eSearchMode_DataSource,
        eSearchMode_ChemRefer,
        eSearchMode_Entrez,
        eSearchMode_PubChem,
        eSearchMode_Identifier,
        eSearchMode_Name,
        eSearchMode_Flexible,
        eSearchMode_SuppInfo
    }

    public enum EResultObjectType
    {
        Compound,
        Synonym,
        SynonymCompound
    }

    [Serializable]
    public class ResultList
    {
        EResultObjectType m_Type;
        List<int>[] m_Keys;
        string[] m_Columns;

        public EResultObjectType ObjectType { get { return m_Type; } }
        public int Count { get { return m_Keys == null ? 0 : m_Keys[0].Count; } }

        public ResultList()
            : this(EResultObjectType.Compound, null)
        {

        }

        public ResultList(EResultObjectType t)
            : this(t, null)
        {
        }

        public ResultList(EResultObjectType t, List<int> list)
        {
            m_Type = t;
            switch (t)
            {
                case EResultObjectType.Synonym:
                    m_Keys = new List<int>[1];
                    m_Columns = new string[] { "syn_id" };
                    break;
                case EResultObjectType.Compound:
                    m_Keys = new List<int>[1];
                    m_Columns = new string[] { "cmp_id" };
                    break;
                case EResultObjectType.SynonymCompound:
                    m_Keys = new List<int>[2];
                    m_Columns = new string[] { "cmp_id", "syn_id" };
                    break;
            }

            for (int i = 0; i < m_Keys.Length; ++i)
                m_Keys[i] = new List<int>();

            if (list != null)
                m_Keys[0] = list;
        }

        public void SetResultRecords(List<ResultRecord> list)
        {
            m_Keys[0] = (from r in list select r.Id).ToList();
            Relevances = (from r in list select r.Relevance).ToList();
        }

        public void Get(int index, int[] element)
        {
            for (int i = 0; i < m_Keys.Length; ++i)
                element[i] = m_Keys[i][index];
        }

        public void Add(int[] keys)
        {
            for (int i = 0; i < m_Keys.Length; ++i)
                m_Keys[i].Add(keys[i]);
        }

        public void Add(int i1)
        {
            m_Keys[0].Add(i1);
        }

        public void Add(int i1, int i2)
        {
            m_Keys[0].Add(i1);
            m_Keys[1].Add(i2);
        }

        public int[] CreateKeysArray()
        {
            return m_Keys == null ? null : new int[m_Keys.Length];
        }

        public List<int> ToList()
        {
            return m_Keys == null ? null : m_Keys[0];
        }

        public List<double> Relevances
        {
            get;
            private set;
        }

        public List<ResultRecord> GetResultRecords()
        {
            List<int> found = this.ToList();

            List<ResultRecord> res = new List<ResultRecord>();
            for (int i = 0; i < found.Count; i++)
                res.Add(new ResultRecord { Id = found[i], Relevance = Relevances != null ? Relevances[i] : 0 });

            return res;
        }
    }

    /// <summary>
    /// Base class for all searches
    /// </summary>
    public abstract class CSSearch
    {
        private SearchOptions m_Options;
        public SearchOptions Options
        {
            get { return m_Options; }
        }

        private CommonSearchOptions m_CommonOptions;
        public CommonSearchOptions CommonOptions
        {
            get { return m_CommonOptions; }
        }

        private SearchScopeOptions m_ScopeOptions;
        public SearchScopeOptions ScopeOptions
        {
            get { return m_ScopeOptions; }
        }

        private SearchResultOptions m_ResultOptions;
        public SearchResultOptions ResultOptions
        {
            get { return m_ResultOptions; }
        }

        private string m_Description;
        public virtual string Description
        {
            get { return m_Description; }
            protected set { m_Description = value; }
        }

        public virtual void SetOptions(SearchOptions options, CommonSearchOptions common, SearchScopeOptions scopeOptions, SearchResultOptions resultOptions)
        {
            m_Options = options;
            m_CommonOptions = common;
            m_ScopeOptions = scopeOptions;
            m_ResultOptions = resultOptions;
        }

        public abstract void Run(Sandbox sandbox, CSSearchResult result);
    }

    public abstract class CSSearchStatus
    {
        public abstract ERequestStatus Status { get; set; }
        public abstract int Count { get; set; }
        public abstract string Message { get; set; }
        public abstract float Progress { get; set; }
        public abstract TimeSpan Elapsed { get; }
    }

    public abstract class CSSearchResult : CSSearchStatus
    {
        public abstract object SearchFormState { get; set; }
        // public abstract int Limit { get; set; }
        public abstract string Description { get; set; }
        public abstract ResultList Found { get; set; }
        public abstract int FoundCount { get; }
        public abstract string Rid { get; set; }
        public abstract TimeSpan Timeout { get; set; }
        public abstract ESimpleSearchMatchType ResultMatchType { get; set; }
        public abstract void Update();
    }

    public class CSRequestSearchResult : CSSearchResult
    {
        protected Request request;

        public CSRequestSearchResult()
        {
            request = new Request();
            request.registerTransaction(null, Environment.MachineName, null, "A");
        }

        public CSRequestSearchResult(string rid)
        {
            request = Request.loadFromTransaction(rid);
        }

        public CSRequestSearchResult(Request r)
        {
            request = r;
        }

        public Request Request
        {
            get { return request; }
        }
        public override ERequestStatus Status
        {
            get { return request.Status; }
            set { request.Status = value; }
        }
        public override int Count
        {
            get { return request.Count; }
            set { request.Count = value; }
        }
        /*public override int Limit
        { 
            get { return request.Limit; }
            set { request.Limit = value; }
        }*/
        public override string Description
        {
            get { return request.Description; }
            set { request.Description = value; }
        }
        public override ResultList Found
        {
            get { return (ResultList)request.Found; }
            set { request.Found = value; }
        }
        public override object SearchFormState
        {
            get { return request.SearchFormState; }
            set { request.SearchFormState = value; }
        }
        public override int FoundCount
        {
            get { return Found == null ? 0 : Found.Count; }
        }
        public override string Message
        {
            get { return request.Message; }
            set { request.Message = value; }
        }
        public override ESimpleSearchMatchType ResultMatchType
        {
            get { return request.ResultMatchType; }
            set { request.ResultMatchType = value; }
        }
        public override float Progress
        {
            get { return request.Progress; }
            set { request.Progress = value; }
        }
        public override string Rid
        {
            get { return request.Rid; }
            set { request.Rid = value; }
        }
        public override TimeSpan Timeout
        {
            get { return request.Timeout; }
            set { request.Timeout = value; }
        }
        public override TimeSpan Elapsed
        {
            get { return request.Elapsed; }
        }

        public override void Update()
        {
            request.saveToTransaction();
        }
    }
}
