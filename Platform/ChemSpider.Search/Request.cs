using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.IO.Compression;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Web.UI;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using ChemSpider.Utilities;
using System.Web;
using System.Data.SqlTypes;
using System.Runtime.Serialization;

namespace ChemSpider.Search
{
    [Serializable]
    public enum ERequestStatus
    {
        Unknown,
        Created,
        Scheduled,
        Processing,
        Suspended,
        PartialResultReady,
        ResultReady,
        Failed,
        TooManyRecords
    };

    /// <summary>
    /// An enum used for storing the type of match.
    /// </summary>
    [Serializable]
    public enum ESimpleSearchMatchType
    {
        None = 0,
        CSID,
        Synonym,
        NonApprovedSynonym,
        InChIKey,
        InChIKeySkeleton,
        ConvertedToInChI,
        ConvertedToInChITautomerStereoMismatch,
        ConvertedToInChIConnectivityMatch,
        ConvertedToInChISkeletonMatch,
        MolecularFormula,
        FullTextSynonym,
        TokenizedSynonym,
        TokenizedFullTextNearSynonym
    }

    [DataContract]
    public class RequestStatus
    {
        [DataMember]
        public ERequestStatus Status { get; internal set; }
        [DataMember]
        public int Count { get; internal set; }
        [DataMember]
        public string Message { get; internal set; }
        [DataMember]
        public float Progress { get; internal set; }
        [DataMember]
        public TimeSpan Elapsed { get; internal set; }
    }

    [Serializable]
    public class Request
    {
        private Hashtable pars = new Hashtable();

        public Request()
        {
            DateStarted = DateTime.Now;
        }

        private static string AsString(object value)
        {
            return value == null ? null : value.ToString();
        }

        private static float AsFloat(object value)
        {
            return value == null ? 0.0f : (float)value;
        }

        public Hashtable Parameters
        {
            get { return pars; }
        }

        public string Service
        {
            get { return AsString(Parameters["service"]); }
            set { Parameters["service"] = value; }
        }

        public object SearchFormState
        {
            get { return Parameters["SearchFormState"]; }
            set { Parameters["SearchFormState"] = value; }
        }

        public object Found
        {
            get { return Parameters["Result.records_found"]; }
            set { Parameters["Result.records_found"] = value; }
        }

        public string Rid
        {
            get { return AsString(Parameters["rid"]); }
            set { Parameters["rid"] = value; }
        }

        public string Mol
        {
            get { return AsString(Parameters["query"]); }
            set { Parameters["query"] = value; }
        }

        public string Sql
        {
            get { return AsString(Parameters["sql_query"]); }
            set { Parameters["sql_query"] = value; }
        }

        public string CountSql
        {
            get { return AsString(Parameters["count_sql_query"]); }
            set { Parameters["count_sql_query"] = value; }
        }

        public int Count
        {
            get { return Convert.ToInt32(Parameters["count"]); }
            set { Parameters["count"] = value; }
        }

        /*public int Limit
        {
            get { return Convert.ToInt32(Parameters["Options.return_limit"]); }
            set { Parameters["Options.return_limit"] = value; }
        }*/

        public int PageSize
        {
            get { return Convert.ToInt32(Parameters["Options.page_size"]); }
            set { Parameters["Options.page_size"] = value; }
        }

        public int RecordsFiltered
        {
            get { return Convert.ToInt32(Parameters["Result.records_filtered"]); }
        }

        /*public string OrderByColumn
        {
            get { return Convert.ToString(Parameters["Options.order_by"]); }
            set { Parameters["Options.order_by"] = value; }
        }*/

        public string SSType
        {
            get { return AsString(Parameters["Options.eSearchType"]); }
            set { Parameters["Options.eSearchType"] = value; }
        }

        public float Progress
        {
            get { return AsFloat(Parameters["progress"]); }
            set { Parameters["progress"] = value; }
        }

        public string Description
        {
            get { return AsString(Parameters["description"]); }
            set { Parameters["description"] = value; }
        }

        public string Message
        {
            get { return AsString(Parameters["message"]); }
            set { Parameters["message"] = value; }
        }

        public ESimpleSearchMatchType ResultMatchType
        {
            get { return (ESimpleSearchMatchType)(Parameters["match_type"] ?? 0); }
            set { Parameters["match_type"] = value; }
        }

        public DateTime DateStarted
        {
            get { return (DateTime)Parameters["date_started"]; }
            set { Parameters["date_started"] = value; }
        }

        public DateTime DateUpdated
        {
            get { return Parameters["date_updated"] == null ? DateStarted : (DateTime)Parameters["date_updated"]; }
            set { Parameters["date_updated"] = value; }
        }

        public TimeSpan Elapsed
        {
            get { return DateUpdated - DateStarted; }
        }

        public TimeSpan Timeout
        {
            get { return Parameters["timeout"] == null ? TimeSpan.Zero : (TimeSpan)Parameters["timeout"]; }
            set { Parameters["timeout"] = value; }
        }

        public ERequestStatus Status
        {
            get { return string2status(Parameters["status"] as string); }
            set { Parameters["status"] = status2string(value); }
        }

        public object this[string key]
        {
            get { return Parameters[key]; }
            set { Parameters[key] = value; }
        }

        private static ERequestStatus string2status(string status)
        {
            switch (status)
            {
                case "C":
                    return ERequestStatus.Created;
                case "S":
                    return ERequestStatus.Scheduled;
                case "P":
                    return ERequestStatus.Processing;
                case "U":
                    return ERequestStatus.Suspended;
                case "A":
                    return ERequestStatus.PartialResultReady;
                case "R":
                    return ERequestStatus.ResultReady;
                case "F":
                    return ERequestStatus.Failed;
                case "T":
                    return ERequestStatus.TooManyRecords;
                default:
                    return ERequestStatus.Unknown;
            }
        }

        private static string status2string(ERequestStatus status)
        {
            switch (status)
            {
                case ERequestStatus.Created:
                    return "C";
                case ERequestStatus.Scheduled:
                    return "S";
                case ERequestStatus.Processing:
                    return "P";
                case ERequestStatus.Suspended:
                    return "S";
                case ERequestStatus.PartialResultReady:
                    return "A";
                case ERequestStatus.ResultReady:
                    return "R";
                case ERequestStatus.Failed:
                    return "F";
                case ERequestStatus.TooManyRecords:
                    return "T";
                default:
                    return "U";
            }
        }

        public void registerTransaction(string type)
        {
            string sessionId = null;
            string hostName = Environment.MachineName;
            string referrer = null;

            if (HttpContext.Current != null)
            {
                HttpRequest httpReq = HttpContext.Current.Request;
                if (httpReq != null)
                {
                    sessionId = httpReq.Cookies["CSSESSID"] != null ? httpReq.Cookies["CSSESSID"].Value : null;

                    //Jon Steele - 29-Nov-2011 - Added "HTTP_X_FORWARDED_FOR" as the address.
                    hostName = httpReq.ServerVariables["HTTP_X_FORWARDED_FOR"] != null ? httpReq.ServerVariables["HTTP_X_FORWARDED_FOR"] : httpReq.UserHostName;
                    //hostName = httpReq.UserHostName;
                    referrer = httpReq.UrlReferrer != null ? httpReq.UrlReferrer.AbsoluteUri : null;
                }
            }

            registerTransaction(sessionId, hostName, referrer, type);
        }

        public void registerTransaction(string ses_id, string address, string referrer, string type)
        {
            if (ses_id == String.Empty)
                ses_id = null;

            using (SqlConnection conn = new SqlConnection(ChemTranDB.ConnectionString))
            {
                Status = ERequestStatus.Processing;
                DateStarted = DateTime.Now;
                if (string.IsNullOrEmpty(Rid))
                    Rid = Guid.NewGuid().ToString();
                conn.ExecuteCommand(@"exec RegisterTransaction @trn_id, @ses_id, @address, @referrer, @type, @predicate, @status, @result_data",
                    new
                    {
                        trn_id = Rid,
                        ses_id = ses_id,
                        address = address,
                        referrer = referrer.First(200),
                        type = type,
                        predicate = Description,
                        status = status2string(ERequestStatus.Processing),
                        result_data = SerializerHelpers.SerializeCompressed(this)
                    }
                ).ToString();
            }
        }

        public void saveToTransaction()
        {
            using (SqlConnection conn = new SqlConnection(ChemTranDB.ConnectionString))
            {
                conn.Open();

                SqlCommand cmd = new SqlCommand("exec UpdateTransaction @trn_id = @trn_id, @result_data = @result_data", conn);
                cmd.Parameters.Add(new SqlParameter("@result_data", SerializerHelpers.SerializeCompressed(this)));

                if (Parameters.Contains("count"))
                {
                    cmd.CommandText += ", @count = @count";
                    cmd.Parameters.Add(new SqlParameter("@count", Count));
                }
                if (Parameters.Contains("progress"))
                {
                    cmd.CommandText += ", @progress = @progress";
                    cmd.Parameters.Add(new SqlParameter("@progress", Progress));
                }
                if (Parameters.Contains("status"))
                {
                    cmd.CommandText += ", @status = @status";
                    cmd.Parameters.Add(new SqlParameter("@status", status2string(Status)));
                }
                if (Parameters.Contains("message"))
                {
                    cmd.CommandText += ", @message = @message";
                    cmd.Parameters.Add(new SqlParameter("@message", Message.Length < 1000 ? Message : Message.Substring(0, 1000)));
                }
                cmd.Parameters.Add(new SqlParameter("@trn_id", Rid));
                cmd.ExecuteNonQuery();
            }
        }

        public static Request loadFromTransaction(string rid)
        {
            using (SqlConnection conn = new SqlConnection(ChemTranDB.ConnectionString))
            {
                Request res = null;
                conn.ExecuteReader("select date_started, date_updated, progress, status, message, [count], result_data from transactions where trn_id = @trn_id",
                    r =>
                    {
                        if (r["result_data"] != DBNull.Value)
                            res = (Request)SerializerHelpers.DeserializeCompressed((byte[])r["result_data"]);
                        else
                        {
                            res = new Request();
                            res.Rid = rid;
                        }

                        if (r["date_started"] != DBNull.Value)
                            res.DateStarted = (DateTime)r["date_started"];
                        if (r["date_updated"] != DBNull.Value)
                            res.DateUpdated = (DateTime)r["date_updated"];
                        if (r["progress"] != DBNull.Value)
                            res.Progress = (float)(double)r["progress"];
                        if (r["status"] != DBNull.Value)
                            res.Status = Request.string2status(r["status"] as string);
                        if (r["message"] != DBNull.Value)
                            res.Message = r["message"] as string;
                        if (r["count"] != DBNull.Value)
                            res.Count = (int)r["count"];
                    },
                    new { trn_id = rid });

                return res;
            }
        }

        public static ERequestStatus getStatus(string rid)
        {
            return getRequestStatus(rid).Status;
        }

        public static RequestStatus getRequestStatus(string rid)
        {
            using (SqlConnection conn = new SqlConnection(ChemTranDB.ConnectionString))
            {
                RequestStatus rstat = null;
                conn.ExecuteReader("select status, [count], message, progress, cast(getdate() - date_started as time) as elapsed from transactions where trn_id = @trn_id",
                    r =>
                    {
                        rstat = new RequestStatus
                        {
                            Status = Request.string2status(r["status"] as string),
                            Count = r["count"] is DBNull ? 0 : (int)r["count"],
                            Message = r["message"] as string,
                            Progress = r["progress"] is DBNull ? 0f : (float)(double)r["progress"],
                            Elapsed = (TimeSpan)r["elapsed"]
                        };
                    },
                    new { trn_id = rid });

                return rstat;
            }
        }

        public static bool isResultReady(ERequestStatus status)
        {
            return status == ERequestStatus.Failed ||
                   status == ERequestStatus.ResultReady ||
                   status == ERequestStatus.PartialResultReady ||
                   status == ERequestStatus.TooManyRecords;
        }

        public bool isStop()
        {
            using (SqlConnection conn = new SqlConnection(ChemTranDB.ConnectionString))
                return conn.ExecuteScalar<bool?>("select stop from transactions where trn_id = @trn_id", new { trn_id = Rid }) ?? false;
        }

        public void updateProgress(float progress)
        {
            using (SqlConnection conn = new SqlConnection(ChemTranDB.ConnectionString))
                conn.ExecuteCommand("exec UpdateTransaction @progress = @progress, trn_id = @trn_id", new { progress = progress, trn_id = Rid });
        }
    };
}
