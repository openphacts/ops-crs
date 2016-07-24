using System;
using System.Collections;
using System.Collections.Generic;
using System.Configuration;
using System.Data.SqlClient;
using System.Data.SqlTypes;
using System.Data;
using System.Text;
using ChemSpider.Utilities;
using ChemSpider.Database;

namespace ChemSpider.Data.Database
{
	public class ChemSpiderBlobsDB : ChemSpiderBaseDB
	{
		public static string ConnectionString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["ChemSpiderBlobsConnectionString"] == null ?
					null :
					ConfigurationManager.ConnectionStrings["ChemSpiderBlobsConnectionString"].ConnectionString;
			}
		}

		public static string RO_ConnectionString
		{
			get
			{
				return ConfigurationManager.ConnectionStrings["ChemSpiderBlobsROConnectionString"] == null ?
					ConnectionString :
					ConfigurationManager.ConnectionStrings["ChemSpiderBlobsROConnectionString"].ConnectionString;
			}
		}

		protected override string ConnString
		{
			get
			{
				return ConnectionString;
			}
		}

		protected override string RO_ConnString
		{
			get
			{
				return RO_ConnectionString;
			}
		}

        protected override string DatabaseName
        {
            get { return "ChemSpiderBlobs"; }
        }

		public ChemSpiderBlobsDB()
		{
			
		}

		public void cmp_set_image(int cmp_id, byte[] preview)
		{
			Hashtable args = new Hashtable();
			args["@cmp_id"] = cmp_id;
			args["@image"] = preview;
			SqlCommand cmd = DBU.m_createCommand("SetRecordThumbnail", args, CommandType.StoredProcedure);
			cmd.ExecuteNonQuery();
		}

		public void cmp_set_sdf(int cmp_id, string sdf, bool create_only)
		{
			byte[] mol = ZipUtils.zip_sdf(sdf);

			Hashtable args = new Hashtable();
			args["@cmp_id"] = cmp_id;
			args["@sdf"] = mol;
			args["@createonly"] = create_only;
			SqlCommand cmd = DBU.m_createCommand("SetRecordSdf", args, CommandType.StoredProcedure);
			cmd.ExecuteNonQuery();
		}

		public void cmp_set_sdf_3d(int cmp_id, string sdf, bool create_only)
		{
			object o = DBU.m_querySingleValue(string.Format("SELECT 1 FROM sdfs_3d WHERE cmp_id = {0}", cmp_id));
			if ( o != null && create_only )
				return;
			byte[] mol = ZipUtils.zip_sdf(sdf);

			Hashtable args = new Hashtable();
			args["@cmp_id"] = cmp_id;
			args["@sdf"] = mol;
			SqlCommand cmd = DBU.m_createCommand("SetRecordSdf3D", args, CommandType.StoredProcedure);
			cmd.ExecuteNonQuery();
		}

		public void GetMeSHRecordDescriptor(string unique_id, out string xml_descriptor, out string descriptor_name)
		{
			using ( SqlCommand cmd = DBU.m_createCommand("GetMeSHRecordDescriptor") ) {
				cmd.Parameters.Add("@unique_id", SqlDbType.VarChar).Value = unique_id;
				cmd.Parameters.Add("@xml_descriptor", SqlDbType.Xml, int.MaxValue).Direction = ParameterDirection.Output;
				cmd.Parameters.Add("@descriptor_name", SqlDbType.VarChar, 300).Direction = ParameterDirection.Output;

				int res = cmd.ExecuteNonQuery();

				xml_descriptor = cmd.Parameters["@xml_descriptor"].Value.ToString();
				descriptor_name = cmd.Parameters["@descriptor_name"].Value.ToString();
			}
		}

        /// <summary>
        /// Total hack to get around problems caused by refactoring.
        /// This is the original version of this function - a new version was required 
        /// that didn't default to 2d if 3d was not present.
        /// </summary>
        public string getSdf(int cmp_id, bool b3DSDF, bool bCalculate)
        {
            return getSdf(cmp_id, b3DSDF, bCalculate, true);
        }

        /// <summary>
        /// Code to strip hydrogens has been moved upstream to ChemSpider.Formats
        /// </summary>
        public string getSdf(int cmp_id, bool b3DSDF, bool bCalculate, bool bDefaultTo2D)
		{
			byte[] buffer = null;

			try {
				using ( SqlConnection conn = new SqlConnection(RO_ConnectionString) ) {
					conn.Open();
					SqlCommand cmd = new SqlCommand(String.Format("select sdf from {0} where cmp_id = {1}", b3DSDF ? "sdfs_3d" : "sdfs", cmp_id), conn);
					buffer = cmd.ExecuteScalar() as byte[];

					// We don't have 3D stored, take 2D, but only flag is set.
                    if (buffer == null && b3DSDF && bDefaultTo2D) {
						cmd.CommandText = String.Format("select sdf from sdfs where cmp_id = {0}", cmp_id);
						buffer = cmd.ExecuteScalar() as byte[];
					}
				}
			}
			catch { }

            return ( buffer == null ) ? null : ZipUtils.ungzip(buffer, Encoding.UTF8);
		}




        public string getSdf(int cmp_id)
        {
            return getSdf(cmp_id, false, false);
        }

		public string getSdf(int cmp_id, SqlConnection conn, SqlTransaction tran)
		{
			SqlCommand cmd = new SqlCommand("select sdf from sdfs where cmp_id = " + cmp_id.ToString(), conn);
			cmd.Transaction = tran;
			byte[] buffer = cmd.ExecuteScalar() as byte[];
            return buffer != null ? ZipUtils.ungzip(buffer, Encoding.UTF8) : null;
		}

        public Dictionary<int, string> getMultipleSdfDictionary(string sqlcriteria)
        {
            string thismol;
            int thiscsid;
            Dictionary<int, string> molsandcsids = new Dictionary<int, string>();

            string sqlcommandtext = "select sdf, cmp_id from sdfs" + sqlcriteria +";";
            SqlDataReader rdr = null;
            byte[] newbuffer = new byte[1024];
            byte[] oldbuffer = null;
            long blobSize;
            long currPos;
            int buffersize;
            try {
                using (SqlConnection conn = new SqlConnection(RO_ConnectionString))
                {
                    conn.Open();
                    SqlCommand cmd = new SqlCommand(sqlcommandtext, conn);
                    cmd.CommandText = sqlcommandtext;
                    rdr = cmd.ExecuteReader(CommandBehavior.SequentialAccess);
                    if (rdr == null)
                    {
                        return null;
                    }
                    while (rdr.Read())
                    {
                        blobSize = rdr.GetBytes(0, 0, null, 0, 0);
                        currPos = 0;
                        oldbuffer = null;
                        while (currPos < blobSize)
                        {
                            currPos += rdr.GetBytes(0, currPos, newbuffer, 0, 1024);
                            if (oldbuffer == null)
                            {
                                Array.Resize(ref oldbuffer, newbuffer.Length);
                                System.Buffer.BlockCopy(newbuffer, 0, oldbuffer, 0, newbuffer.Length);
                            }
                            else
                            {
                                buffersize = oldbuffer.Length;
                                Array.Resize(ref oldbuffer, oldbuffer.Length + newbuffer.Length);
                                System.Buffer.BlockCopy(newbuffer, 0, oldbuffer, buffersize, newbuffer.Length);
                            }
                        }
                        thiscsid = rdr.GetInt32(1);
                        if (oldbuffer != null)
                        {
                            thismol = ZipUtils.ungzip(oldbuffer, Encoding.UTF8);
                            molsandcsids.Add(thiscsid, thismol);
                        }
                    }
                    rdr.Close();
                    conn.Close();
                    return molsandcsids;
                }
            }
            catch 
            {
                return null;
            }
        }

		public string getCml(int cmp_id)
		{
			byte[] buffer = null;
			using ( SqlConnection conn = new SqlConnection(RO_ConnectionString) ) {
				conn.Open();
				SqlCommand cmd = new SqlCommand(String.Format("select cml from cmls where cmp_id = {0}", cmp_id), conn);
				buffer = cmd.ExecuteScalar() as byte[];
			}

            return buffer != null ? ZipUtils.ungzip(buffer, Encoding.UTF8) : null;
		}

		public string getEpiText(int cmp_id)
		{
			byte[] buffer = null;
			using ( SqlConnection conn = new SqlConnection(RO_ConnectionString) ) {
				conn.Open();
				SqlCommand cmd = new SqlCommand(String.Format("select epi_text from epi where cmp_id = {0}", cmp_id), conn);
				buffer = cmd.ExecuteScalar() as byte[];
			}

            return buffer != null ? ZipUtils.ungzip(buffer, Encoding.UTF8) : null;
		}
	}
}
