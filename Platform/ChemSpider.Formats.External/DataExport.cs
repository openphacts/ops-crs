using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Data.SqlClient;
using System.Threading;
using System.Xml;
using System.Xml.Linq;
using System.Data;
using System.Configuration;
using System.Diagnostics;
using ChemSpider.Database;
using ChemSpider.Data.Database;
using ChemSpider.Utilities;
using ChemSpider.Molecules;
using OpenEyeNet;

//ChemSpider Web Services.
using ChemSpider.Formats.External.CSWebService_Synonyms;
using ChemSpider.Formats.External.CSWebService_PropertiesImport;

//dotNetRDF
using VDS.RDF;
using VDS.RDF.Writing;

namespace ChemSpider.Formats
{
	#region DataExport

	public interface IDataExport : IDisposable
	{
		/// <summary>
		/// List containing the DataSnaphots for the Export.
		/// </summary>
		List<IDataSnapshot> ExportSnapshots();

		/// <summary>
		/// List containing the DataExportFiles for the Export.
		/// </summary>
		List<IDataExportFile> ExportFiles();

		/// <summary>
		/// The encoding of the exported file.
		/// </summary>
		Encoding Encoding { get; set; }

		/// <summary>
		/// Should the exported files be compressed.
		/// </summary>
		bool CompressFiles { get; set; }

		/// <summary>
		/// Should the exported files be ftp's to another location.
		/// </summary>
		bool FtpFiles { get; set; }

		/// <summary>
		/// The base ftp url
		/// </summary>
		string FtpUrl { get; set; }

		/// <summary>
		/// The Ftp username.
		/// </summary>
		string FtpUsername { get; set; }

		/// <summary>
		/// The Ftp password.
		/// </summary>
		string FtpPassword { get; set; }

		/// <summary>
		/// Is this a differential export, rather than a full export.
		/// </summary>
		bool Differential { get; set; }

		/// <summary>
		/// The Linked server we must run the export against.
		/// </summary>
		string LinkedServer { get; set; }

		/// <summary>
		/// The database connection string.
		/// </summary>
		string DBConnection { get; set; }

		/// <summary>
		/// The users database connection string.
		/// </summary>
		string UsersDBConnection { get; set; }

		/// <summary>
		/// The reason why the Export failed.
		/// </summary>
		string ErrorMessage { get; set; }

		/// <summary>
		/// Whether the Export failed or not.
		/// </summary>
		bool ExportFailed { get; set; }

		/// <summary>
		/// The Export Id for this export.
		/// </summary>
		int ExportId { get; set; }

		/// <summary>
		/// The DateTime of the Export.
		/// </summary>
		DateTime DateTimeExported { get; set; }

		/// <summary>
		/// Adds a file to the Data Export.
		/// </summary>
		/// <param name="fileToAdd">The file to be added.</param>
		void AddFile(IDataExportFile fileToAdd);

		/// <summary>
		/// Adds a data snapshot to the Data Export.
		/// </summary>
		/// <param name="fileToAdd">The snapshot to be added.</param>
		void AddSnapshot(IDataSnapshot snapshotToAdd);

		/// <summary>
		/// Returns the ExportId after the Export has been created in the database.
		/// </summary>
		/// <returns>ExportID as integer</returns>
		int GetExportId();

		/// <summary>
		/// Updates the current status and error message.
		/// </summary>
		void UpdateExport();

		/// <summary>
		/// Performs the export and snapshots.
		/// </summary>
		void Export();
	}

	/// <summary>
	///  The Data Export base class - this should be reused for different types of Data Exports.
	/// </summary>
	public class DataExport : IDataExport
	{
		/// <summary>
		/// List containing the DataSnaphots for the Export.
		/// </summary>
		private List<IDataExportFile> _export_files;

		/// <summary>
		/// List containing the DataExportFiles for the Export.
		/// </summary>
		private List<IDataSnapshot> _export_snapshots;

		/// <summary>
		/// Constructor which is passed the Snapshots and Export Files.
		/// </summary>
		public DataExport()
		{
			_export_files = new List<IDataExportFile>();
			_export_snapshots = new List<IDataSnapshot>();
		}

		/// <summary>
		/// A list of IDataExportFile items.
		/// </summary>
		public List<IDataExportFile> ExportFiles()
		{
			return _export_files;
		}

		/// <summary>
		/// A list of IDataSnapshot items.
		/// </summary>
		public List<IDataSnapshot> ExportSnapshots()
		{
			return _export_snapshots;
		}

		/// <summary>
		/// The encoding of the exported files.
		/// </summary>
		public Encoding Encoding { get; set; }

		/// <summary>
		/// Should the exported files be compressed.
		/// </summary>
		public bool CompressFiles { get; set; }

		/// <summary>
		/// Should the exported files be ftp's to another location.
		/// </summary>
		public bool FtpFiles { get; set; }

		/// <summary>
		/// The base ftp url
		/// </summary>
		public string FtpUrl { get; set; }

		/// <summary>
		/// The Ftp username.
		/// </summary>
		public string FtpUsername { get; set; }

		/// <summary>
		/// The Ftp password.
		/// </summary>
		public string FtpPassword { get; set; }

		/// <summary>
		/// Is this a differential export, rather than a full export.
		/// </summary>
		public bool Differential { get; set; }

		/// <summary>
		/// The Linked server we must run the export against.
		/// </summary>
		public string LinkedServer { get; set; }

		/// <summary>
		/// The database connection string.
		/// </summary>
		public string DBConnection { get; set; }

		/// <summary>
		/// The users database connection string.
		/// </summary>
		public string UsersDBConnection { get; set; }

		/// <summary>
		/// The reason why the Export failed.
		/// </summary>
		public string ErrorMessage { get; set; }

		/// <summary>
		/// Whether the Export failed or not.
		/// </summary>
		public bool ExportFailed { get; set; }

		/// <summary>
		/// The Export Id for this export.
		/// </summary>
		public int ExportId { get; set; }

		/// <summary>
		/// The DateTime of the Export.
		/// </summary>
		public DateTime DateTimeExported { get; set; }

		/// <summary>
		/// Adds a file to the Data Export.
		/// </summary>
		/// <param name="fileToAdd">The file to be added.</param>
		public void AddFile(IDataExportFile fileToAdd)
		{
			_export_files.Add(fileToAdd);
		}

		/// <summary>
		/// Adds a data snapshot to the Data Export.
		/// </summary>
		/// <param name="fileToAdd">The snapshot to be added.</param>
		public void AddSnapshot(IDataSnapshot snapshotToAdd)
		{
			_export_snapshots.Add(snapshotToAdd);
		}

		/// <summary>
		/// Returns the ExportId after the Export has been created in the database.
		/// </summary>
		/// <returns>ExportID as integer</returns>
		public virtual int GetExportId()
		{
			//This must be implemented by the inherited class and the inherited class must insert it into the database.
			return 0;
		}

		/// <summary>
		/// Updates the status and error message of the export - must be implemented by the inherited class.
		/// </summary>
		public virtual void UpdateExport()
		{
			//Override.
		}

		/// <summary>
		/// Executes the Data Export.
		/// </summary>
		public virtual void Export()
		{
			try
			{
				bool snapshots_written = true;

				//We need an ExportId from the inherited class.
				ExportId = GetExportId();

				//Create the FtpFileUploader object.
				Ftp.FtpFileUploader uploader = null;
				if (FtpFiles)
					uploader = new Ftp.FtpFileUploader(FtpUrl, FtpUsername, FtpPassword);

				//First create the database snapshots before exporting the snapshot to file.
				foreach (IDataSnapshot snapshot in _export_snapshots)
				{
					//Perform any pre-processing required prior to taking the snaphots.
					if (!snapshot.PreProcessing(this, LinkedServer))
					{
						snapshots_written = false;
						break;
					}
					else
					{
						//Write the snapshot for this data into the database.
						if (!snapshot.WriteSnapshotToDB(this, LinkedServer))
						{
							snapshots_written = false;
							break;
						}
					}
				}

				//Only write to file if the snapshot was ok.
				if (snapshots_written)
				{
					//Export all the files.
					foreach (IDataExportFile file in _export_files)
					{
						//Export the file to disk using the snapshot identified using the Export object and the specified Encoding.
						file.Export(this, Encoding);

						//Compress the file if required.
						if (CompressFiles)
							file.Compress();

						//Ftp the file if required.
						if (FtpFiles)
							file.Ftp(uploader, FtpUrl);
					}
				}

				//Wait until all the ftp jobs have completed.
				if (FtpFiles)
				{
					while (uploader.JobsRemaining > 0)
						Thread.Sleep(1000);
				}
			}
			catch (Exception ex)
			{
				//Set the export as failed and the error message.
				ExportFailed = true;
				ErrorMessage = ex.Message;
			}
			finally
			{
				//Trace listener messages.
				if (ExportFailed)
					Trace.TraceError(String.Format("Export Failed: Export Id:{0}, Message:{1}", ExportId, ErrorMessage));
				else
					Trace.TraceInformation(String.Format("Export Succeeded: Export Id:{0}", ExportId));

				//Finally update the export status success / failure, finished, error message etc (if implemented).
				UpdateExport();
			}
		}

		/// <summary>
		/// Close and Dispose if required.
		/// </summary>
		public void Dispose()
		{
			//TODO: clean up.
		}
	}

	#endregion

	#region DataExportFile

	public interface IDataExportFile
	{
		/// <summary>
		/// List containing the Child files.
		/// </summary>
		List<FileInfo> ChildFiles();

		/// <summary>
		/// The encoding of the exported file.
		/// </summary>
		Encoding Encoding { get; set; }

		/// <summary>
		/// The filename of the exported file.
		/// </summary>
		string FileName { get; set; }

		/// <summary>
		/// The full file path of the exported file.
		/// </summary>
		string FileFullPath { get; set; }

		/// <summary>
		/// The compressed filename of the exported file.
		/// </summary>
		string CompressedFileName { get; set; }

		/// <summary>
		/// The compressed full file path of the exported file.
		/// </summary>
		string CompressedFileFullPath { get; set; }

		/// <summary>
		/// The target FTP directory.
		/// </summary>
		string FtpTargetDirectory { get; set; }

		/// <summary>
		/// Adds a child file to the Data Export File.
		/// </summary>
		/// <param name="fileToAdd">The file to be added.</param>
		void AddChildFile(FileInfo fileToAdd);

		/// <summary>
		/// Performs the export.
		/// </summary>
		void Export(IDataExport Exp, Encoding Encoding);

		/// <summary>
		/// Compresses the file.
		/// </summary>
		void Compress();

		/// <summary>
		/// Uploads the file to the FTP site.
		/// </summary>
		/// <param name="uploader">A reference to the FtpFileUploader object.</param>
		/// <param name="baseFtpUrl">The base ftp url.</param>
		void Ftp(Ftp.FtpFileUploader uploader, string baseFtpUrl);
	}

	/// <summary>
	/// Base class used for Data Export Files - this class should be reused for all types of Data Export Files.
	/// </summary>
	public class DataExportFile : IDataExportFile
	{
		/// <summary>
		/// List containing the DataSnaphots for the Export.
		/// </summary>
		private List<FileInfo> _child_files;

		/// <summary>
		/// The encoding of the exported file.
		/// </summary>
		public Encoding Encoding { get; set; }

		/// <summary>
		/// The filename of the exported file.
		/// </summary>
		public string FileName { get; set; }

		/// <summary>
		/// The full file path of the exported file.
		/// </summary>
		public string FileFullPath { get; set; }

		/// <summary>
		/// The compressed filename of the exported file.
		/// </summary>
		public string CompressedFileName { get; set; }

		/// <summary>
		/// The compressed full file path of the exported file.
		/// </summary>
		public string CompressedFileFullPath { get; set; }

		/// <summary>
		/// The target FTP directory.
		/// </summary>
		public string FtpTargetDirectory { get; set; }

		/// <summary>
		/// List of files when an export file is split into chunks.
		/// </summary>
		public List<FileInfo> ChildFiles()
		{
			return _child_files;
		}

		/// <summary>
		/// Initialise lists in constructor.
		/// </summary>
		public DataExportFile()
		{
			_child_files = new List<FileInfo>();
		}

		/// <summary>
		/// Adds a child file to the Data Export File.
		/// </summary>
		/// <param name="fileToAdd">The file to be added.</param>
		public void AddChildFile(FileInfo fileToAdd)
		{
			_child_files.Add(fileToAdd);
		}

		/// <summary>
		/// Performs the Export for this file and is passed flag to say whether it is a differential.
		/// </summary>
		public virtual void Export(IDataExport Exp, Encoding Encoding)
		{
			//Set the FileFullPath
			if (FileName != string.Empty)
			{
				FileInfo info = new FileInfo(FileName);
				FileFullPath = info.FullName;

				//If the file already exists then delete it.
				if (File.Exists(FileFullPath))
					File.Delete(FileFullPath);
			}
		}

		/// <summary>
		/// Compresses the file[s].
		/// </summary>
		public void Compress()
		{
			//Zip the child files or the single file and set the compressed filename and path.
			FileInfo zipped_file;
			if (ChildFiles().Count > 0)
			{
				for (int i = 0; i < _child_files.Count(); i++)
				{
					zipped_file = ZipUtils.Compress(_child_files[i]);
					//Update the child file reference to the zipped file.
					_child_files[i] = zipped_file;
				}
			}
			else
			{
				zipped_file = ZipUtils.Compress(new FileInfo(FileName));
				CompressedFileName = zipped_file.Name;
				CompressedFileFullPath = zipped_file.FullName;
			}
		}

		/// <summary>
		/// Ftp's the file asynchronously.
		/// </summary>
		/// <param name="uploader">Passed a reference to the FtpUploader.</param>
		/// <param name="baseFtpUrl">Passed the base ftp url.</param>
		public void Ftp(Ftp.FtpFileUploader uploader, string baseFtpUrl)
		{
			//Create the target directory.
			uploader.FtpCreateDirectoryIfNotExists(String.Format("{0}/{1}", baseFtpUrl, FtpTargetDirectory));

			if (ChildFiles().Count > 0)
			{
				foreach (FileInfo file in ChildFiles())
				{
					uploader.UploadAsync(file.FullName, String.Format("{0}/{1}/{2}", baseFtpUrl, FtpTargetDirectory, file.Name));
				}
			}
			else
			{
				//Upload the file, either the compressed or normal file.
				uploader.UploadAsync(CompressedFileFullPath ?? FileFullPath, String.Format("{0}/{1}/{2}", baseFtpUrl, FtpTargetDirectory, CompressedFileName ?? FileName));
			}
		}
	}

	#endregion

	#region DataSnapshot

	public interface IDataSnapshot : IDisposable
	{
		/// <summary>
		/// The SQL required to write the data snapshot.
		/// </summary>
		string GetSQL(int ExportId, string LinkedServer);

		/// <summary>
		/// Writes the snapshot to the database.
		/// </summary>
		/// <param name="Exp">The DataExport object.</param>
		/// <param name="LinkedServer">Optionally a Linked server we will use.</param>
		/// <returns>True if it succeeded, false if an error occurred.</returns>
		bool WriteSnapshotToDB(IDataExport Exp, string LinkedServer);

		/// <summary>
		/// Performs any pre-processing required prior to writing the snapshot.
		/// </summary>
		/// <param name="Exp">The DataExport object.</param>
		/// <param name="LinkedServer">Optionally a Linked server we will use.</param>
		/// <returns>True if it succeeded, false if an error occurred.</returns>
		bool PreProcessing(IDataExport Exp, string LinkedServer);
	}

	/// <summary>
	/// Base class for Data Snapshots - this class should be reused for all types of Data Snapshots.
	/// </summary>
	public class DataSnapshot : IDataSnapshot
	{
		/// <summary>
		/// The filename of the exported file.
		/// </summary>
		public virtual string GetSQL(int ExportId, string LinkedServer)
		{
			//Each snapshot type must have its own implementation.
			return string.Empty;
		}

		/// <summary>
		/// Exports the data to be exported to the db snapshot table.
		/// </summary>
		/// <param name="Exp">The Data Export</param>
		/// <param name="LinkedServer">If a linked server is used for selecting the data it should be supplied.</param>
		/// <returns>Boolean indicating success</returns>
		public virtual bool WriteSnapshotToDB(IDataExport Exp, string LinkedServer)
		{
			try
			{
				//Executes the SQL to write the snapshot.
				using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
				{
					conn.ExecuteCommand(GetSQL(Exp.ExportId, LinkedServer));
				}

				//No error so return true.
				return true;
			}
			catch (Exception ex)
			{
				//Error so return false and set the error message.
				Exp.ExportFailed = true;
				Exp.ErrorMessage = ex.Message;
				return false;
			}
		}

		/// <summary>
		/// Performs any pre processing required prior to writing the snapshot - must be overridden if required.
		/// </summary>
		/// <param name="Exp">The DataExport object.</param>
		/// <param name="LinkedServer">Optionally a Linked server we will use.</param>
		/// <returns>True if it succeeded, false if an error occurred.</returns>
		public virtual bool PreProcessing(IDataExport Exp, string LinkedServer)
		{
			return true;
		}

		public void Dispose()
		{
			//TODO: clean up.
		}
	}

	/// <summary>
	/// The Data Source Snapshot class is used for creating Data Snapshots relating to a particular Data Source.
	/// </summary>
	public class DataSourceSnapshot : DataSnapshot
	{
		//The export is specific to a particular data source.
		protected string _dsn;
		protected int _dsn_id;

		public DataSourceSnapshot(IDataExport exp, string dsn)
		{
			_dsn = dsn;

			//Map the dsn to the dsn_id.
			using (SqlConnection conn = new SqlConnection(exp.DBConnection))
			{
				int dsn_id = conn.ExecuteScalar<int>("SELECT dsn_id FROM ChemUsers..data_sources WHERE name = @name", new { name = _dsn });
				_dsn_id = dsn_id;
			}
		}
	}

	/// <summary>
	/// The Data Source Snapshot class is used for creating Data Snapshots relating to a particular Data Source.
	/// </summary>
	public class CRSDataSourceSnapshot : DataSnapshot
	{
		//The export is specific to a particular data source.
		protected string _dsn;
		protected int _dsn_id;

		public CRSDataSourceSnapshot(IDataExport exp, string dsn)
		{
			_dsn = dsn;

			//Map the dsn to the dsn_id.
			using (SqlConnection conn = new SqlConnection(exp.DBConnection))
			{
				//Get the dsn_id from the CRS database.
				int dsn_id = conn.ExecuteScalar<int>("SELECT dsn_id FROM dbo.datasources WHERE dsn_name = @name", new { name = _dsn });
				_dsn_id = dsn_id;
			}
		}
	}

	/// <summary>
	/// Used for creating Data Snapshots of ChemSpider Identifiers.
	/// </summary>
	public class IdentifiersDataSourceSnapshot : DataSourceSnapshot
	{
		public IdentifiersDataSourceSnapshot(IDataExport exp, string dsn) : base(exp, dsn) { }

		public override string GetSQL(int ExportId, string LinkedServer)
		{
			return String.Format(@"INSERT INTO rdf_exported_identifiers_snapshot(exp_id, cmp_id, inchi_key, inchi, SMILES, name, molecular_formula)
																SELECT {0}, i.cmp_id, i.inchi_key, i.inchi, c.SMILES, isnull(dbo.fGetCompoundTitle(c.cmp_id), dbo.fGetSysName(c.cmp_id)) as name, c.PUBCHEM_OPENEYE_MF as molecular_formula
																	FROM (select distinct s.cmp_id
																		FROM {2}ChemSpider.dbo.substances s 
																			WHERE s.deleted_yn = 0 AND s.dsn_id = {1}) a 
																	JOIN {2}ChemSpider.dbo.inchis_std i ON a.cmp_id = i.cmp_id 
																	JOIN {2}ChemSpider.dbo.compounds c ON c.cmp_id = a.cmp_id
																		WHERE c.deleted_yn = 0", ExportId, _dsn_id, LinkedServer);
		}
	}

	/// <summary>
	/// Used for creating Data Snapshots of ChemSpider Identifiers for the CRS.
	/// </summary>
	public class IdentifiersCRSDataSourceSnapshot : CRSDataSourceSnapshot
	{
		public IdentifiersCRSDataSourceSnapshot(IDataExport exp, string dsn) : base(exp, dsn) { }

		public override string GetSQL(int ExportId, string LinkedServer)
		{
			//Create the identifiers snapshot SQL.
			return String.Format(@"INSERT INTO rdf_exported_identifiers_snapshot(exp_id, csid, cmp_id, inchi_key, inchi, SMILES, name, molecular_formula)
									SELECT {0}, c.csid, c.chemspider_csid, i.inchi_key, i.inchi, ISNULL(sm.oe_abs_smiles, ''), cs_title.synonym as name, comp_p.mf_formatted as molecular_formula
										FROM (SELECT DISTINCT sc.csid
												FROM substances s
												JOIN sid_csid sc ON sc.sid = s.sid
													WHERE s.dsn_id = {1} AND isRevoked = 0) a
											JOIN compounds c ON c.csid = a.csid
												LEFT JOIN chemspider_properties p ON p.chemspider_csid = c.chemspider_csid
												LEFT JOIN inchis i ON i.inc_id = c.inc_id
												LEFT JOIN compounds_smiles sm ON sm.csid = a.csid
												LEFT JOIN chemspider_synonyms cs_title ON cs_title.chemspider_csid = c.chemspider_csid AND cs_title.compound_title_yn = 1
												LEFT JOIN compounds_properties comp_p ON comp_p.csid = c.csid", ExportId, _dsn_id);
		}
	}

	/// <summary>
	/// Used for creating Data Snapshots of ChemSpider External References.
	/// </summary>
	public class ReferencesDataSourceSnapshot : DataSourceSnapshot
	{
		public ReferencesDataSourceSnapshot(IDataExport exp, string dsn) : base(exp, dsn) { }

		public override string GetSQL(int ExportId, string LinkedServer)
		{
			return String.Format(@"INSERT INTO rdf_exported_ext_refs_snapshot (exp_id, cmp_id, ext_id, ext_url)
									SELECT DISTINCT {0}, s.cmp_id, s.ext_id, s.ext_url
										FROM {2}ChemSpider.dbo.substances s 
										JOIN {2}ChemSpider.dbo.compounds c ON c.cmp_id = s.cmp_id
											WHERE s.deleted_yn = 0 AND c.deleted_yn = 0 AND (s.ext_id IS NOT NULL OR s.ext_url IS NOT NULL) 
												AND s.dsn_id = {1}", ExportId, _dsn_id, LinkedServer);
		}
	}

	/// <summary>
	/// Used for creating Data Snapshots of ChemSpider External References for the CRS.
	/// </summary>
	public class ReferencesCRSDataSourceSnapshot : CRSDataSourceSnapshot
	{
		public ReferencesCRSDataSourceSnapshot(IDataExport exp, string dsn) : base(exp, dsn) { }

		public override string GetSQL(int ExportId, string LinkedServer)
		{
			//Create the references snapshot SQL.
			return String.Format(@"INSERT INTO rdf_exported_ext_refs_snapshot (exp_id, cmp_id, ext_id, ext_url)
									SELECT DISTINCT {0}, sc.csid, s.ext_regid, s.ext_url
										FROM substances s
											JOIN sid_csid sc ON sc.sid = s.sid
												WHERE s.dsn_id = {1} AND isRevoked = 0 
													AND (s.ext_regid IS NOT NULL OR s.ext_url IS NOT NULL)", ExportId, _dsn_id);
		}
	}

	/// <summary>
	/// Used for creating Data Snapshots of ChemSpider Synonyms.
	/// </summary>
	public class SynonymsDataSourceSnapshot : DataSourceSnapshot
	{
		public SynonymsDataSourceSnapshot(IDataExport exp, string dsn) : base(exp, dsn) { }

		public override string GetSQL(int ExportId, string LinkedServer)
		{
			return String.Format(@"INSERT INTO rdf_exported_synonyms_snapshot (exp_id, cmp_id, synonym, lang_id, validated, is_dbid)
									SELECT distinct {0}, c.cmp_id, y.synonym, y.lang_id1, validated = CASE cs.opinion WHEN 'Y' THEN CASE cs.approved_yn WHEN 1 THEN 1 ELSE 0 END ELSE 0 END, CASE WHEN ISNULL(ssf.flag_id, 0) > 0 THEN 1 ELSE 0 END
										FROM {2}ChemSpider.dbo.substances s 
											JOIN {2}ChemSpider.dbo.compounds c ON s.cmp_id = c.cmp_id
											JOIN {2}ChemSpider.dbo.compounds_synonyms cs ON c.cmp_id = cs.cmp_id
											JOIN {2}ChemSpider.dbo.synonyms y ON cs.syn_id = y.syn_id
											LEFT JOIN {2}ChemSpider.dbo.synonyms_synonyms_flags ssf ON ssf.syn_id = y.syn_id AND ssf.flag_id = 1
												WHERE s.deleted_yn = 0 AND c.deleted_yn = 0 AND cs.deleted_yn = 0 AND y.deleted_yn = 0
													AND s.dsn_id = {1}", ExportId, _dsn_id, LinkedServer);
		}
	}

	/// <summary>
	/// Used for creating Data Snapshots of ChemSpider Synonyms for the CRS.
	/// </summary>
	public class SynonymsCRSDataSourceSnapshot : CRSDataSourceSnapshot
	{
		public SynonymsCRSDataSourceSnapshot(IDataExport exp, string dsn) : base(exp, dsn) { }

		public override string GetSQL(int ExportId, string LinkedServer)
		{
			//Create the Synonyms snapshot SQL. Includes Chemspider Synonyms and also those deposited into the CRS.
			return String.Format(@"INSERT INTO rdf_exported_synonyms_snapshot (exp_id, cmp_id, synonym, lang_id, validated, is_dbid)
									SELECT DISTINCT {0}, sc.csid, syn.synonym, syn.lang_id1, syn.validated_yn, syn.dbid_yn
										FROM compounds c
										JOIN sid_csid sc ON sc.csid = c.csid
										JOIN substances s ON s.sid = sc.sid
										JOIN chemspider_synonyms syn ON syn.chemspider_csid = c.chemspider_csid
										WHERE s.dsn_id = {1} AND isRevoked = 0", ExportId, _dsn_id);

			//REMOVED THIS PART AS WE DON'T NEED CRS DEPOSITED SYNONYMS ANYMORE
			//            return String.Format(@"INSERT INTO rdf_exported_synonyms_snapshot (exp_id, cmp_id, synonym, lang_id, validated, is_dbid)
			//                                    SELECT DISTINCT {0}, sc.csid, syn.synonym, syn.lang_id1, syn.validated_yn, syn.dbid_yn
			//		                                FROM compounds c
			//		                                JOIN sid_csid sc ON sc.csid = c.csid
			//		                                JOIN substances s ON s.sid = sc.sid
			//										JOIN chemspider_synonyms syn ON syn.chemspider_csid = c.chemspider_csid
			//			                            WHERE s.dsn_id = {1} AND isRevoked = 0
			//			                        UNION
			//                                    SELECT DISTINCT {0}, sc.csid, syn.synonym, 'en', syn.validatedBy, 0
			//		                                FROM compounds c
			//		                                JOIN sid_csid sc ON sc.csid = c.csid
			//		                                JOIN substances s ON s.sid = sc.sid
			//		                                JOIN curated_synonym_csid csc ON csc.csid = c.csid
			//		                                JOIN curated_synonyms syn ON syn.curated_synonym_id = csc.curated_synonym_id
			//			                            WHERE s.dsn_id = {1} AND isRevoked = 0", ExportId, _dsn_id);
		}

		/// <summary>
		/// Populates the chemspider_synonyms table using the ChemSpider Web Service.
		/// </summary>
		/// <param name="Exp">The Export object.</param>
		/// <param name="LinkedServer">Optional Linked server.</param>
		/// <returns></returns>
		public override bool PreProcessing(IDataExport Exp, string LinkedServer)
		{
			//Call the synonyms.asmx web service in blocks of 1000 InChIKeys and populate the chemspider_synonyms table.
			try
			{
				bool bFinished = false;
				int lastCSID = 0;

				//Get the number of days between synonym downloads.
				int synonymsNoOfDays = 7; //Default to a week
				if (ConfigurationManager.AppSettings["synonyms_no_of_days"] != null)
					synonymsNoOfDays = Convert.ToInt32(ConfigurationManager.AppSettings["synonyms_no_of_days"]);
				DateTime lastRetrievedCutoff = DateTime.Now.AddDays(0 - synonymsNoOfDays); //Set the cut off date to 1 week in the past.

				List<int> csids = new List<int>();

				//Delete all the previous synonyms for this data source as they may have changed since the last export - so we need to get them every export.
				using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
				{
					conn.ExecuteCommand(@"DELETE FROM chemspider_synonyms WHERE chemspider_csid IN (SELECT DISTINCT c.chemspider_csid
												FROM compounds c
													JOIN sid_csid sc ON sc.csid = c.csid
													JOIN substances s ON s.sid = sc.sid
													JOIN chemspider_properties p ON p.chemspider_csid = c.chemspider_csid
												WHERE s.dsn_id = @dsn_id 
														AND isRevoked = 0 
														AND c.chemspider_csid IS NOT NULL
														AND p.last_retrieved < @last_retrieved_cutoff)"
										, new
										{
											dsn_id = _dsn_id
										,
											last_retrieved_cutoff = lastRetrievedCutoff
										});
				}

				while (!bFinished)
				{
					using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
					{
						//Retrieve the next block of InChIKeys - we will only retrieve those with chemspider_csid populated.
						DataTable result = conn.FillDataTable(String.Format(@"SELECT DISTINCT TOP {0} sc.csid csid, c.chemspider_csid
																				FROM substances s
																					JOIN sid_csid sc ON sc.sid = s.sid
																					JOIN compounds c ON c.csid = sc.csid
																					JOIN inchis i ON i.inc_id = c.inc_id
																					JOIN chemspider_properties p ON p.chemspider_csid = c.chemspider_csid
																				WHERE s.dsn_id = @dsn_id AND isRevoked = 0 AND c.chemspider_csid IS NOT NULL
																					  AND c.csid > @last_cmp_id 
																					  AND (p.last_retrieved < @last_retrieved_cutoff OR p.last_retrieved IS NULL)
																				ORDER BY csid ASC"
																				, ConfigurationManager.AppSettings["max_inchi_keys"])
																				, new
																				{
																					dsn_id = _dsn_id,
																					last_cmp_id = lastCSID,
																					last_retrieved_cutoff = lastRetrievedCutoff
																				});

						//If we have nothing left to process then finish.
						if (result.Rows.Count == 0)
							bFinished = true;
						else
						{
							//Clear the previous list of csids.
							csids.Clear();

							foreach (DataRow row in (result as DataTable).Select())
							{
								csids.Add(Convert.ToInt32(row["chemspider_csid"]));

								//Store the last CSID.
								lastCSID = Convert.ToInt32(row["csid"]);
							}

							//Call the Synonyms.asmx ChemSpider Web Service.
							Synonyms ws_synonyms = new Synonyms();
							ws_synonyms.Timeout = 300000; //Set timeout to 5 minutes.

							//Get the number of retries from the config file.
							int ws_retries = Convert.ToInt32(ConfigurationManager.AppSettings["ws_retries"]);
							bool ws_success = false;
							SynonymsInfo[] results = null;

							//Keep retrying the web service until we have a success or number of retries exceeded.
							while (!ws_success)
							{
								try
								{
									results = ws_synonyms.RetrieveByCSIDList(csids.ToArray(), ConfigurationManager.AppSettings["security_token"]);
									ws_success = true;
								}
								catch (Exception ex)
								{
									ws_retries--;
									if (ws_retries == 0)
										throw ex; //We have exceeded the retries count to throw this error.
								}
							}

							//Store the results in the chemspider_synonyms table.
							int count = 0;
							bool bLoaded = false;

							//SQL to insert the synonyms.
							string sql_insert = "INSERT INTO chemspider_synonyms (chemspider_csid, synonym, lang_id1, validated_yn, dbid_yn, compound_title_yn) VALUES ";
							string sql = sql_insert;

							//SQL to update the last_retrieved date on chemspider_properties.
							string csid_sql = string.Empty;
							foreach (int csid in csids)
							{
								csid_sql += string.Format("'{0}',", csid);
							}
							csid_sql = csid_sql.Substring(0, csid_sql.Length - 1);
							string sql2 = String.Format("UPDATE chemspider_properties SET last_retrieved = GETDATE() WHERE chemspider_csid IN ({0})", csid_sql);

							while (!bLoaded)
							{
								SynonymsInfo info = results[count];
								count++;

								//Build the Sql Statement.
								sql += String.Format("({0},'{1}','{2}',{3},{4},{5}),"
															, info.CSID
															, info.Synonym.Replace("'", "''") //Double up the quotes - i know not ideal - but required for building multiple insert statement.
															, info.LangId
															, info.IsValidated ? 1 : 0
															, info.IsDbId ? 1 : 0
															, info.IsCompoundTitle ? 1 : 0);

								if (count == results.Length || count % 1000 == 0) //We cannot insert more than 1000 rows at a time.
								{
									//Trim off the final comma.
									sql = sql.Substring(0, sql.Length - 1);

									//Execute the insert on another connection.
									using (SqlConnection conn1 = new SqlConnection(Exp.DBConnection))
									{
										//Insert multiple rows in a single statement - if results were returned.
										if (results.Length > 0)
											conn1.ExecuteCommand(sql);

										//Set the last_retrieved date for compounds we have just pulled the synonyms for.
										conn1.ExecuteCommand(sql2);
									}

									//Reset the SQL statement.
									sql = sql_insert;

									//Drop out if we have finished.
									if (count == results.Length)
										bLoaded = true;
								}
							}
						}
					}
				}

				//No error so return true.
				return true;
			}
			catch (Exception ex)
			{
				//Error so return false and set the error message.
				Exp.ExportFailed = true;
				Exp.ErrorMessage = "Failed to retrieve Synonyms from ChemSpider. Details:" + ex.Message;
				return false;
			}
		}
	}

	/// <summary>
	/// Used for creating Data Snapshots of ChemSpider Properties.
	/// </summary>
	public class PropertiesDataSourceSnapshot : DataSourceSnapshot
	{
		public PropertiesDataSourceSnapshot(IDataExport exp, string dsn) : base(exp, dsn) { }

		public override string GetSQL(int ExportId, string LinkedServer)
		{
			return String.Format(@"INSERT INTO rdf_exported_props_snapshot (exp_id, cmp_id, ALogP, XLogP, Average_Mass, Density, Density_Error, Index_Of_Refraction, Index_Of_Refraction_Error, Molar_Refractivity, Molar_Refractivity_Error, Molar_Volume, Molar_Volume_Error, Monoisotopic_Mass, Nominal_Mass, Parachor, Parachor_Error, Polarizability, Polarizability_Error, Surface_Tension, Surface_Tension_Error, FP, FP_Error, Enthalpy, Enthalpy_Error, BP, BP_Error, VP, VP_Error, LogP, LogP_Error, RuleOf5, RuleOf5_HDonors, RuleOf5_HAcceptors, RuleOf5_FRB, RuleOf5_MW, RuleOf5_PSA, LogD_1, LogD_2, BCF_1, BCF_2, KOC_1, KOC_2)
															SELECT {0}, c.cmp_id, p.ALogP, p.XLogP, p.Average_Mass, p.Density, p.Density_Error, p.Index_Of_Refraction, p.Index_Of_Refraction_Error, p.Molar_Refractivity, p.Molar_Refractivity_Error, p.Molar_Volume, p.Molar_Volume_Error, p.Monoisotopic_Mass, p.Nominal_Mass, p.Parachor, p.Parachor_Error, p.Polarizability, p.Polarizability_Error, p.Surface_Tension, p.Surface_Tension_Error, p.FP, p.FP_Error, p.Enthalpy, p.Enthalpy_Error, p.BP, p.BP_Error, p.VP, p.VP_Error, p.LogP, p.LogP_Error, p.RuleOf5, p.RuleOf5_HDonors, p.RuleOf5_HAcceptors, p.RuleOf5_FRB, p.RuleOf5_MW, p.RuleOf5_PSA, p.LogD_1, p.LogD_2, p.BCF_1, p.BCF_2, p.KOC_1, p.KOC_2
																FROM (select distinct s.cmp_id
																		FROM {2}ChemSpider.dbo.substances s 
																			WHERE s.deleted_yn = 0 AND s.dsn_id = {1}) a 
																JOIN {2}ChemSpider.dbo.acdlabs_props_v12 p ON a.cmp_id = p.cmp_id
																JOIN {2}ChemSpider.dbo.compounds c ON c.cmp_id = a.cmp_id
																	WHERE c.deleted_yn = 0", ExportId, _dsn_id, LinkedServer);
		}
	}

	/// <summary>
	/// Used for creating Data Snapshots of ChemSpider Properties for the CRS.
	/// </summary>
	public class PropertiesCRSDataSourceSnapshot : CRSDataSourceSnapshot
	{
		public PropertiesCRSDataSourceSnapshot(IDataExport exp, string dsn) : base(exp, dsn) { }

		public override string GetSQL(int ExportId, string LinkedServer)
		{
			//Create the Properties snapshot SQL.
			return String.Format(@"INSERT INTO rdf_exported_props_snapshot (exp_id, cmp_id, ALogP, XLogP, Average_Mass, Density, Density_Error, Index_Of_Refraction, Index_Of_Refraction_Error, Molar_Refractivity, Molar_Refractivity_Error, Molar_Volume, Molar_Volume_Error, Monoisotopic_Mass, Nominal_Mass, Parachor, Parachor_Error, Polarizability, Polarizability_Error, Surface_Tension, Surface_Tension_Error, FP, FP_Error, Enthalpy, Enthalpy_Error, BP, BP_Error, VP, VP_Error, LogP, LogP_Error, RuleOf5, RuleOf5_HDonors, RuleOf5_HAcceptors, RuleOf5_FRB, RuleOf5_MW, RuleOf5_PSA, LogD_1, LogD_2, BCF_1, BCF_2, KOC_1, KOC_2)
									SELECT {0}, c.csid, p.ALogP, p.XLogP, p.Average_Mass, p.Density, p.Density_Error, p.Index_Of_Refraction, p.Index_Of_Refraction_Error, p.Molar_Refractivity, p.Molar_Refractivity_Error, p.Molar_Volume, p.Molar_Volume_Error, p.Monoisotopic_Mass, p.Nominal_Mass, p.Parachor, p.Parachor_Error, p.Polarizability, p.Polarizability_Error, p.Surface_Tension, p.Surface_Tension_Error, p.FP, p.FP_Error, p.Enthalpy, p.Enthalpy_Error, p.BP, p.BP_Error, p.VP, p.VP_Error, p.LogP, p.LogP_Error, p.RuleOf5, p.RuleOf5_HDonors, p.RuleOf5_HAcceptors, p.RuleOf5_FRB, p.RuleOf5_MW, p.RuleOf5_PSA, p.LogD_1, p.LogD_2, p.BCF_1, p.BCF_2, p.KOC_1, p.KOC_2
										FROM (SELECT DISTINCT sc.csid
												FROM substances s
													JOIN sid_csid sc ON sc.sid = s.sid
												WHERE s.dsn_id = {1} AND isRevoked = 0) a
										JOIN compounds c ON c.csid = a.csid
										LEFT JOIN chemspider_properties p ON p.chemspider_csid = c.chemspider_csid", ExportId, _dsn_id);
		}

		private struct InChIKeyInfo
		{
			public int csid;
			public int inc_id;
			public string InChIKey;
		}

		/// <summary>
		/// Converts the value to a string for building dynamic SQL statement - we need NULL to be returned as a string.
		/// </summary>
		/// <param name="value">The property value.</param>
		/// <returns>A string representing the value.</returns>
		private string propertyValue(object value)
		{
			if (value == null)
				return "NULL";
			else
				return value.ToString();
		}

		/// <summary>
		/// Populates the chemspider_properties table using the ChemSpider Web Service.
		/// </summary>
		/// <param name="Exp">The Export object.</param>
		/// <param name="LinkedServer">Optional Linked server.</param>
		/// <returns></returns>
		public override bool PreProcessing(IDataExport Exp, string LinkedServer)
		{
			//Call the PropertiesImport.asmx web service in blocks of InChIKeys and populate the chemspider_properties table.
			try
			{
				bool bFinished = false;
				int block_end = 0;
				int block_begin = 0;
				List<InChIKeyInfo> inchi_key_info = new List<InChIKeyInfo>();

				using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
				{
					//First of all we must update the chemspider_ids of records that have the same inchi_key as records which already have chemspider_ids.
					conn.ExecuteCommand(@"UPDATE c1
												SET c1.chemspider_csid = c2.chemspider_csid
													FROM compounds c1
														JOIN compounds c2 ON c2.inc_id = c1.inc_id AND c2.chemspider_csid IS NOT NULL
															WHERE c1.chemspider_csid IS NULL
																AND c1.csid IN (SELECT DISTINCT sc.csid csid
																					FROM substances s
																						JOIN sid_csid sc ON sc.sid = s.sid
																						JOIN compounds c ON c.csid = sc.csid
																						LEFT JOIN inchis i ON i.inc_id = c.inc_id
																					WHERE s.dsn_id = @dsn_id  AND isRevoked = 0 AND c.chemspider_csid IS NULL
																						AND EXISTS (SELECT 1 FROM compounds c2 
																										JOIN chemspider_properties cp 
																											ON cp.chemspider_csid = c2.chemspider_csid
																												WHERE c2.inc_id = c.inc_id))", new { dsn_id = _dsn_id });
					//Delete from chemspider_synonyms where the record is not used on any compounds.
					conn.ExecuteCommand(@"DELETE cs
											FROM chemspider_synonyms cs
												LEFT JOIN compounds c ON c.chemspider_csid = cs.chemspider_csid
											WHERE c.csid IS NULL");

					//Delete from chemspider_properties where the record is not used on any compounds.
					conn.ExecuteCommand(@"DELETE cp
											FROM chemspider_properties cp
												LEFT JOIN compounds c ON c.chemspider_csid = cp.chemspider_csid
											WHERE c.csid IS NULL");
				}

				while (!bFinished)
				{
					//Begin at the end of the last block.
					block_begin = block_end;

					using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
					{
						//Retrieve the next block of compounds we need to retrieve from ChemSpider.
						DataTable result = conn.FillDataTable(String.Format(@"SELECT DISTINCT TOP {0} sc.csid csid, i.inchi_key, c.inc_id
																				FROM substances s
																					JOIN sid_csid sc ON sc.sid = s.sid
																					JOIN compounds c ON c.csid = sc.csid
																					LEFT JOIN inchis i ON i.inc_id = c.inc_id
																				WHERE s.dsn_id = @dsn_id AND isRevoked = 0 AND c.chemspider_csid IS NULL
																						AND c.csid > @last_cmp_id 
																						AND NOT EXISTS (SELECT 1 FROM compounds c2 
																											JOIN chemspider_properties cp ON cp.chemspider_csid = c2.chemspider_csid
																												WHERE c2.inc_id = c.inc_id) ORDER BY csid ASC"
																				, ConfigurationManager.AppSettings["max_inchi_keys"])
																				, new
																				{
																					dsn_id = _dsn_id,
																					last_cmp_id = block_end
																				});
						if (result.Rows.Count == 0)
						{
							bFinished = true;
						}
						else
						{
							inchi_key_info.Clear();
							foreach (DataRow row in result.Select())
							{
								//Store the List of InChIKeys, csid and inc_id.
								InChIKeyInfo info = new InChIKeyInfo();
								info.csid = Convert.ToInt32(row["csid"]);
								info.inc_id = Convert.ToInt32(row["inc_id"]);
								info.InChIKey = row["inchi_key"] as string;
								inchi_key_info.Add(info);

								//Store the last CSID.
								block_end = info.csid;
							}

							//Get the InChIKeys from the List.
							List<string> inchi_keys = new List<string>();
							inchi_key_info.ForEach(i => inchi_keys.Add(i.InChIKey));
							List<string> processed_inchikeys = new List<string>();

							//Call the Synonyms.asmx ChemSpider Web Service.
							Properties ws_properties = new Properties();
							ws_properties.Timeout = 300000; //Set timeout to 5 minutes.

							PropertiesInfo[] results = null;

							//Get the number of retries from the config file.
							int ws_retries = Convert.ToInt32(ConfigurationManager.AppSettings["ws_retries"]);
							bool ws_success = false;

							//Keep retrying the web service until we have a success or number of retries exceeded.
							while (!ws_success)
							{
								try
								{
									results = ws_properties.RetrieveByInChIKeyList(inchi_keys.ToArray(), ConfigurationManager.AppSettings["security_token"]);
									ws_success = true;
								}
								catch (Exception ex)
								{
									ws_retries--;
									if (ws_retries == 0)
										throw ex; //We have exceeded the retries count to throw this error.
								}
							}

							//If we got some results back then process them.
							if (results.Length > 0)
							{
								//Store the results in the chemspider_properties table.
								int count = 0;
								bool bLoaded = false;

								string sql_insert = @"INSERT INTO chemspider_properties (chemspider_csid, ALogP, XLogP, Average_Mass, Density, Density_Error, Index_Of_Refraction, Index_Of_Refraction_Error, Molar_Refractivity
																							, Molar_Refractivity_Error, Molar_Volume, Molar_Volume_Error, Monoisotopic_Mass, Nominal_Mass, Parachor, Parachor_Error
																							, Polarizability, Polarizability_Error, Surface_Tension, Surface_Tension_Error, FP, FP_Error, Enthalpy, Enthalpy_Error
																							, BP, BP_Error, VP, VP_Error, LogP, LogP_Error, RuleOf5, RuleOf5_HDonors, RuleOf5_HAcceptors, RuleOf5_FRB, RuleOf5_MW
																							, RuleOf5_PSA, LogD_1, LogD_2, BCF_1, BCF_2, KOC_1, KOC_2) VALUES ";

								string sql_update = @"UPDATE compounds SET chemspider_csid = {0} WHERE inc_id = {1};";
								string sql1 = string.Empty;
								string sql2 = string.Empty;

								while (!bLoaded)
								{
									InChIKeyInfo item = inchi_key_info[count];
									count++;

									//Find the associated item in the results.
									PropertiesInfo info = results.Where(n => n.InChIKey == item.InChIKey).FirstOrDefault();

									//If this InCHIKey is in Chemspider then update the SQL.
									if (info != null && !processed_inchikeys.Contains(item.InChIKey))
									{
										//Add to the list of process InChIKeys.
										processed_inchikeys.Add(item.InChIKey);

										//Build the insert statement.
										sql1 += String.Format("{42} ({0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10},{11},{12},{13},{14},{15},{16},{17},{18},{19},{20},{21},{22},{23},{24},{25},{26},{27},{28},{29},{30},{31},{32},{33},{34},{35},{36},{37},{38},{39},{40},{41})"
																	, info.CSID
																	, propertyValue(info.ALogP)
																	, propertyValue(info.XLogP)
																	, propertyValue(info.AverageMass)
																	, propertyValue(info.Density)
																	, propertyValue(info.DensityError)
																	, propertyValue(info.IndexOfRefraction)
																	, propertyValue(info.IndexOfRefractionError)
																	, propertyValue(info.MolarRefactivity)
																	, propertyValue(info.MolarRefactivityError)
																	, propertyValue(info.MolarVolume)
																	, propertyValue(info.MolarVolumeError)
																	, propertyValue(info.MonoisotopicMass)
																	, propertyValue(info.NominalMass)
																	, propertyValue(info.Parachor)
																	, propertyValue(info.ParachorError)
																	, propertyValue(info.Polarizability)
																	, propertyValue(info.PolarizabilityError)
																	, propertyValue(info.SurfaceTension)
																	, propertyValue(info.SurfaceTensionError)
																	, propertyValue(info.FP)
																	, propertyValue(info.FPError)
																	, propertyValue(info.Enthalpy)
																	, propertyValue(info.EnthalpyError)
																	, propertyValue(info.BP)
																	, propertyValue(info.BPError)
																	, propertyValue(info.VP)
																	, propertyValue(info.VPError)
																	, propertyValue(info.LogP)
																	, propertyValue(info.LogPError)
																	, propertyValue(info.RuleOf5)
																	, propertyValue(info.RuleOf5HDonors)
																	, propertyValue(info.RuleOf5HAcceptors)
																	, propertyValue(info.RuleOf5FRB)
																	, propertyValue(info.RuleOf5MW)
																	, propertyValue(info.RuleOf5PSA)
																	, propertyValue(info.LogD1)
																	, propertyValue(info.LogD2)
																	, propertyValue(info.BCF1)
																	, propertyValue(info.BCF2)
																	, propertyValue(info.KOC1)
																	, propertyValue(info.KOC2)
																	, sql_insert
																	);

										//Build the sql statement to map csid to all records in compounds with the same inc_id.
										sql2 += String.Format(sql_update, info.CSID, item.inc_id);
									}

									if (count == inchi_key_info.Count || count % 1000 == 0) //We cannot insert more than 1000 rows at a time.
									{
										//Execute the insert on another connection.
										using (SqlConnection conn1 = new SqlConnection(Exp.DBConnection))
										{
											//Insert multiple rows in a single statement.
											conn1.ExecuteCommand(sql1);
											//Run multiple updates in a single statement.
											conn1.ExecuteCommand(sql2);
										}

										//Reset the insert SQL statement.
										sql1 = string.Empty;

										//Drop out if we have finished.
										if (count == inchi_key_info.Count)
											bLoaded = true;
									}
								}
							}
						}
					}
				}
				//No error so return true.
				return true;
			}
			catch (Exception ex)
			{
				//Error so return false and set the error message.
				Exp.ExportFailed = true;
				Exp.ErrorMessage = "Failed to retrieve Properties from ChemSpider. Details:" + ex.Message;
				return false;
			}
		}
	}

	/// <summary>
	/// Used for creating Data Snapshots of ChemSpider Identifiers for the CRS.
	/// </summary>
	public class ParentChildCRSDataSourceSnapshot : CRSDataSourceSnapshot
	{
		public ParentChildCRSDataSourceSnapshot(IDataExport exp, string dsn) : base(exp, dsn) { }

		public override string GetSQL(int ExportId, string LinkedServer)
		{
			//Create the identifiers snapshot SQL.
			return String.Format(@"INSERT INTO rdf_exported_parent_child_snapshot(exp_id, csid_daughter, csid_parent, parent_id)
									SELECT DISTINCT {0}, pd.csid_daughter, pd.csid_parent, pt.parent_id
										FROM substances s
											JOIN sid_csid sc ON sc.sid = s.sid
											JOIN parent_daughters pd ON pd.csid_daughter = sc.csid
											JOIN parent_types pt ON pt.parent_id = pd.parent_id
												WHERE s.dsn_id = {1} AND isRevoked = 0 
													AND (s.ext_regid IS NOT NULL OR s.ext_url IS NOT NULL)
									UNION
									SELECT DISTINCT {0}, pd.csid_daughter, pd.csid_parent, pt.parent_id
										FROM substances s
											JOIN sid_csid sc ON sc.sid = s.sid
											JOIN parent_daughters pd ON pd.csid_parent = sc.csid
											JOIN parent_types pt ON pt.parent_id = pd.parent_id
												WHERE s.dsn_id = {1} AND isRevoked = 0 
													AND (s.ext_regid IS NOT NULL OR s.ext_url IS NOT NULL)", ExportId, _dsn_id);
		}
	}

	/// <summary>
	/// Used for creating Data Snapshots of Issues for the CRS.
	/// </summary>
	public class IssuesCRSDataSourceSnapshot : CRSDataSourceSnapshot
	{
		public IssuesCRSDataSourceSnapshot(IDataExport exp, string dsn) : base(exp, dsn) { }

		public override string GetSQL(int ExportId, string LinkedServer)
		{
			//Create the issues snapshot SQL include all the issues recorded as well as those records which are not present in the sid_csid table.
			return String.Format(@"INSERT INTO rdf_exported_issues_snapshot(exp_id, issue_severity, issue_type, issue_description, issue_additional_description, ext_regid)
									SELECT DISTINCT {0}, si.issue_severity, si.issue_type, si.issue_description, si.issue_additional_description, s.ext_regid FROM substance_issues si
												JOIN substances s ON s.sid = si.sid
												WHERE s.dsn_id = {1}
									UNION ALL
									SELECT DISTINCT {0}, 2, 1, 'Fatal error - failed to process the sdf.', '', s.ext_regid FROM substances s
												LEFT JOIN sid_csid sc ON sc.sid = s.sid
												WHERE s.dsn_id = {1} 
													AND sc.sid IS NULL", ExportId, _dsn_id);
		}
	}

	#endregion

	#region RdfVoidDataSourcesExport

	/// <summary>
	/// Class for generating the Void.ttl metadata information for a list of data sources, for the OPS project.
	/// </summary>
	public class RdfVoidDataSourceExport : DataExport
	{
		//The export is specific to a particular list of data sources.
		private Dictionary<string, int> _dsn_exp_ids;

		/// <summary>
		/// Constructor is passed the dsns to generate the export for.
		/// </summary>
		/// <param name="dsn">The dsn to generate the export for.</param>
		public RdfVoidDataSourceExport(Dictionary<string, int> dsn_exp_ids)
		{
			_dsn_exp_ids = dsn_exp_ids;
		}

		/// <summary>
		/// Executes the Data Export.
		/// </summary>
		public override void Export()
		{
			//Create the DataExportFiles for the export.
			AddFile(new VoidDataExportFile(_dsn_exp_ids));

			//Export the data.
			base.Export();
		}
	}
	#endregion

	#region SdfSupplementaryInfoDataExport

	/// <summary>
	/// Class used for generating an SDF file containing Supplementary Information.
	/// Tony wanted this to show to JC Bradley on 2-Nov-2012.
	/// </summary>
	public class SdfSupplementaryInfoDataExport : DataExport
	{
		/// <summary>
		/// Executes the Data Export.
		/// </summary>
		public override void Export()
		{
			//Create the DataExportFiles for the export.
			AddFile(new SupplementaryInfoDataExportFile());
			base.Export();
		}
	}

	/// <summary>
	/// Exports an SDF file containing compounds that have supplementary information.
	/// 
	/// It includes:
	///     Structure, CSID, SMILES, InChIString, InChIKey, LogP, BP (boiling point), FP (Flash Point)
	///     
	/// For all records containing experimental values of:
	///     LogP, BP (boiling point), FP (Flash Point)
	/// </summary>
	public class SupplementaryInfoDataExportFile : DataExportFile
	{
		private const string ATTR_UNITS_ID = "units_Id";
		private const string ATTR_VALUE = "value";

		/// <summary>
		/// Maps an Xml property element to its units.
		/// </summary>
		/// <param name="units">Xml document containing the units for the supplementary info.</param>
		/// <param name="property">Xml node containing the property we want to map the units for.</param>
		/// <returns>String containing the mapped units.</returns>
		private string mapPropertyToUnits(XDocument units, XElement property)
		{
			//Iterate the units.
			foreach (XElement unit in units.Root.Elements())
			{
				if (unit.Attribute(XName.Get(ATTR_UNITS_ID)) != null && property.Attribute(XName.Get(ATTR_UNITS_ID)) != null)
				{
					if (property.Attribute(XName.Get(ATTR_UNITS_ID)).Value == unit.Attribute(XName.Get(ATTR_UNITS_ID)).Value)
					{
						return unit.Attribute(XName.Get(ATTR_VALUE)).Value;
					}
				}
			}
			return string.Empty;
		}

		/// <summary>
		/// Adds the properties attributes to the SDF file.
		/// </summary>
		/// <param name="attributeName">The name of the attribute to be added.</param>
		/// <param name="properties">The list of properties we will add.</param>
		/// <param name="units">The list of units to look up the unit_id in.</param>
		/// <param name="record">The SDF Record to add the properties to.</param>
		private void addProperties(string attributeName, XDocument properties, XDocument units, SdfRecord record)
		{
			//Iterate the properties and get the units.
			foreach (XElement prop in properties.Root.Elements())
			{
				//We must map each property to the units section.
				string p_units = mapPropertyToUnits(units, prop);
				string p = prop.Attribute(XName.Get("value")).Value;

				if (p_units != String.Empty)
					p = String.Format("{0} {1}", p, p_units); //Add a leading space in front of the units.

				//Output the property to the SDF file.
				record.AddField(attributeName, p);
			}
		}

		/// <summary>
		/// Exports an SDF file containing Supplementary information.
		/// </summary>
		public override void Export(IDataExport Exp, Encoding Encoding)
		{
			//Generate the file name.
			FileName = "SUPPLEMENTARY_INFO_" + Exp.DateTimeExported.ToString("yyyy-MM-dd") + ".sdf";

			//Create the list of SDF records.
			List<SdfRecord> records = new List<SdfRecord>();

			using (SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString))
			{
				//Write the SDF file containing supplementary info.
				using (TextWriter sdf = new StreamWriter(FileName, false, Encoding))
				{
					conn.ExecuteReader(String.Format(@"WITH XMLNAMESPACES ('chemspider:xmlns:user-data' AS userdata)
														SELECT [supp_info].query('/cs-record/userdata:user-data-tree/userdata:categories/userdata:experimental-physchem-properties/userdata:experimental-boiling-point') AS bp,
															   [supp_info].query('/cs-record/userdata:user-data-tree/userdata:categories/userdata:experimental-physchem-properties/userdata:experimental-flash-point') AS fp,
															   [supp_info].query('/cs-record/userdata:user-data-tree/userdata:categories/userdata:experimental-physchem-properties/userdata:experimental-logp') AS logp,
															   [supp_info].query('/cs-record/userdata:user-data-tree/userdata:common-meta/userdata:units') AS units
															   , c.cmp_id
															   , c.SMILES
															   , i.inchi
															   , i.inchi_key
															   , s.sdf
														FROM compounds_supp_info csi
															JOIN compounds c 
																ON csi.cmp_id = c.cmp_id
															JOIN inchis_std i
																ON i.cmp_id = c.cmp_id
															JOIN [ChemSpiderBlobs]..sdfs s
																ON s.cmp_id = c.cmp_id
														WHERE
															csi.supp_info.exist('/cs-record/userdata:user-data-tree/userdata:categories/userdata:experimental-physchem-properties/userdata:experimental-boiling-point') = 1		
															OR csi.supp_info.exist('/cs-record/userdata:user-data-tree/userdata:categories/userdata:experimental-physchem-properties/userdata:experimental-flash-point') = 1
															OR csi.supp_info.exist('/cs-record/userdata:user-data-tree/userdata:categories/userdata:experimental-physchem-properties/userdata:experimental-logp') = 1
														ORDER BY c.cmp_id ASC"),
					r =>
					{
						//Create a new SDF record.
						SdfRecord rec = new SdfRecord();

						string mol = ZipUtils.ungzip(r["sdf"] as byte[], Encoding.UTF8);
						if (!string.IsNullOrEmpty(mol))
							mol = OpenEyeUtility.GetInstance().StripHydrogens(mol);

						//First output the standard properties to the SDF.
						rec.Mol = mol;
						rec.AddField("CSID", r["cmp_id"].ToString());
						rec.AddField("SMILES", r["SMILES"].ToString());
						rec.AddField("InChI", r["inchi"].ToString());
						rec.AddField("InChIKey", r["inchi_key"].ToString());

						//Next output the experimental properties to the SDF.

						//Get the units.
						XDocument units = XDocument.Parse(String.Format("<units>{0}</units>", r["units"].ToString()));

						//Get the properties.
						XDocument bp_props = XDocument.Parse(String.Format("<bps>{0}</bps>", r["bp"].ToString()));
						XDocument fp_props = XDocument.Parse(String.Format("<fps>{0}</fps>", r["fp"].ToString()));
						XDocument logp_props = XDocument.Parse(String.Format("<logps>{0}</logps>", r["logp"].ToString()));

						//Add the properties to the SDF Record.
						addProperties("BP", bp_props, units, rec);
						addProperties("FP", fp_props, units, rec);
						addProperties("LOGP", logp_props, units, rec);

						//Write the sdf record to the output.
						sdf.WriteLine(rec.ToString());
					});
				}
			}
		}
	}
	#endregion

	#region SdfDataExport

	/// <summary>
	/// Class used for generating an SDF file containing compounds from a list of data sources.
	/// </summary>
	public class SdfDataSourceExport : DataExport
	{
		//The export is specific to a particular list of data sources.
		protected List<int> _dsn_ids;

		/// <summary>
		/// Constructor is passed the dsns to generate the export for.
		/// </summary>
		/// <param name="dsn">The dsns to generate the export for.</param>
		public SdfDataSourceExport(List<int> dsn_ids)
		{
			_dsn_ids = dsn_ids;
		}

		/// <summary>
		/// Executes the Data Export.
		/// </summary>
		public override void Export()
		{
			//Create the SdfDataExportFile for the export (if we havent already specified them in a derived class).
			if (ExportFiles().Count == 0)
				AddFile(new SdfDataExportFile(_dsn_ids));

			//Export the data.
			base.Export();
		}
	}

	/// <summary>
	/// Class for generating the Void.ttl metadata information for a list of data sources, for the OPS project.
	/// </summary>
	public class CRSIssuesSDFDataSourceExport : DataExport
	{
		//The export is specific to a particular list of data sources.
		private Dictionary<string, int> _dsn_exp_ids;

		/// <summary>
		/// Constructor is passed the dsns to generate the export for.
		/// </summary>
		/// <param name="dsn">The dsn to generate the export for.</param>
		public CRSIssuesSDFDataSourceExport(Dictionary<string, int> dsn_exp_ids)
		{
			_dsn_exp_ids = dsn_exp_ids;
		}

		/// <summary>
		/// Executes the Data Export.
		/// </summary>
		public override void Export()
		{
			//Create the DataExportFiles for the export.
			AddFile(new CRSIssuesSDFDataSourceExportFile(_dsn_exp_ids));

			//Export the data.
			base.Export();
		}
	}


	/// <summary>
	/// Exports an SDF file containing compounds.
	/// </summary>
	public class CRSIssuesSDFDataSourceExportFile : DataSourceListDataExportFile
	{
		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public CRSIssuesSDFDataSourceExportFile(Dictionary<string, int> dsn_exp_ids) : base(dsn_exp_ids) { }


		private int getDsnIdFromDsn(string DBConnection, string dsn)
		{
			int dsn_id;
			using (SqlConnection conn = new SqlConnection(DBConnection))
			{
				dsn_id = conn.ExecuteScalar<int>("SELECT dsn_id FROM dbo.datasources WHERE dsn_name = @name", new { name = dsn });
			}
			return dsn_id;
		}

		/// <summary>
		/// Exports an SDF file containing Issues.
		/// </summary>
		public override void Export(IDataExport Exp, Encoding Encoding)
		{
			string formatted_dsn_ids = string.Empty;
			string formatted_exp_ids = string.Empty;
			string ext_regid = string.Empty;

			//Generate the file name.
			if (_dsn_exp_ids.Count > 1)
			{
				//We have a list of DSNs in the SDF.
				FileName = "ISSUES_" + Exp.DateTimeExported.ToString("yyyy-MM-dd") + ".sdf";

				//Create the list of dsn_ids.
				foreach (KeyValuePair<string, int> dsn_exp_id in _dsn_exp_ids)
				{
					formatted_dsn_ids += getDsnIdFromDsn(Exp.DBConnection, dsn_exp_id.Key.ToString()).ToString() + ",";
					formatted_exp_ids += dsn_exp_id.Value.ToString() + ",";
				}
				formatted_dsn_ids = formatted_dsn_ids.Substring(0, formatted_dsn_ids.Length - 1);
				formatted_exp_ids = formatted_exp_ids.Substring(0, formatted_exp_ids.Length - 1);
			}
			else
			{
				//We only have a single DSN so name the file using that DSN.
				FileName = _dsn_exp_ids.FirstOrDefault().Key.Replace(' ','_') + "_ISSUES_" + Exp.DateTimeExported.ToString("yyyy-MM-dd") + ".sdf";
				formatted_dsn_ids = getDsnIdFromDsn(Exp.DBConnection, _dsn_exp_ids.FirstOrDefault().Key).ToString();
				formatted_exp_ids = _dsn_exp_ids.FirstOrDefault().Value.ToString();
			}

			//Create the list of SDF records.
			List<SdfRecord> records = new List<SdfRecord>();
			SdfRecord rec = null;

			using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
			{
				//Write the SDF file containing supplementary info.
				using (TextWriter sdf = new StreamWriter(FileName, false, Encoding))
				{
					conn.ExecuteReader(String.Format(@"SELECT i.*, sdf.sdf FROM rdf_exported_issues_snapshot i
														JOIN substances s ON s.ext_regid = i.ext_regid AND s.dsn_id IN ({0})
														JOIN substances_sdf sdf ON sdf.sid = s.sid
														WHERE exp_id IN ({1}) AND i.ext_regid IN (SELECT ext_regid FROM rdf_exported_issues_snapshot i2 
																									WHERE i2.exp_id IN ({1}) 
																									AND i2.issue_type = 1 AND i2.issue_severity = 2)
														ORDER BY i.ext_regid ASC, i.issue_type ASC, i.issue_severity ASC", formatted_dsn_ids, formatted_exp_ids),
					r =>
					{
						//This is the next SDF Record as the ext_regid has changed.
						if (ext_regid != r["ext_regid"].ToString())
						{
							//Write the previous sdf record to the output.
							if(rec != null)
								sdf.Write(rec.ToString());

							//Assign ext_regid.
							ext_regid = r["ext_regid"].ToString();

							//Create a new SDF record.
							rec = new SdfRecord();

							//string mol = ZipUtils.ungzip(r["sdf"] as byte[], Encoding.UTF8);
							string mol = Encoding.UTF8.GetString(r["sdf"] as byte[]);

							//Output the mol to the SDF.
							rec.Mol = mol;

							rec.AddField("EXT_REGID", r["ext_regid"].ToString());
						}

						//Output CHEMSPIDER_MESSAGE.
						rec.AddField("CHEMSPIDER_MESSAGE"
										, String.Format("{0}:{1} -> {2}{3}"
										, Turtle.getIssueSeverityDescription(Convert.ToInt32(r["issue_severity"]))
										, Turtle.getIssueTypeDescription(Convert.ToInt32(r["issue_type"]))
										, r["issue_description"].ToString()
										, r["issue_additional_description"].ToString() != string.Empty 
											? String.Format(" - {0}", r["issue_additional_description"].ToString()) 
											: string.Empty));
					});

					//Write the final sdf record to the output.
					if (rec != null)
						sdf.Write(rec.ToString());
				}
			}
		}
	}


	public class ColinsSynonymsSdfChunkedDataSourceExport : SdfChunkedDataSourceExport
	{
		public ColinsSynonymsSdfChunkedDataSourceExport(List<int> dsn_ids
													, int? chunk_size
													, string file_label
													, bool include_substance_links
													, bool include_csid
													, bool include_inchis
													, bool include_deprecated_flag
													, bool include_datasource_count
													, bool include_non_validated_synonyms
													, bool include_database_ids)
			: base(dsn_ids
					, chunk_size
					, file_label
					, include_substance_links
					, include_csid
					, include_inchis
					, include_deprecated_flag
					, include_datasource_count
					, include_non_validated_synonyms
					, include_database_ids) { }

		/// <summary>
		/// Executes the Data Export.
		/// </summary>
		public override void Export()
		{
			//Create the DataExportFiles for the export.
			if (ExportFiles().Count == 0)
				AddFile(new ColinsSynonymsChunkedSDFExportFile(_dsn_ids, _chunk_size, _file_label, _include_substance_links, _include_csid, _include_inchis, _include_deprecated_flag, _include_datasource_count, _include_non_validated_synonyms, _include_database_ids));

			//Export the data.
			base.Export();
		}
	}

	public class SynonymsSdfChunkedDataSourceExport : SdfChunkedDataSourceExport
	{
		public SynonymsSdfChunkedDataSourceExport(List<int> dsn_ids
													, int? chunk_size
													, string file_label
													, bool include_substance_links
													, bool include_csid
													, bool include_inchis
													, bool include_deprecated_flag
													, bool include_datasource_count
													, bool include_non_validated_synonyms
													, bool include_database_ids)
			: base(dsn_ids
					, chunk_size
					, file_label
					, include_substance_links
					, include_csid
					, include_inchis
					, include_deprecated_flag
					, include_datasource_count
					, include_non_validated_synonyms
					, include_database_ids) { }

		/// <summary>
		/// Executes the Data Export.
		/// </summary>
		public override void Export()
		{
			//Create the DataExportFiles for the export.
			if (ExportFiles().Count == 0)
				AddFile(new SynonymsChunkedSDFExportFile(_dsn_ids, _chunk_size, _file_label, _include_substance_links, _include_csid, _include_inchis, _include_deprecated_flag, _include_datasource_count, _include_non_validated_synonyms, _include_database_ids));

			//Export the data.
			base.Export();
		}
	}

	public class MinMaxCSIDSdfChunkedDataSourceExport : SdfChunkedDataSourceExport
	{
		protected int _csid_min;
		protected int _csid_max;

		public MinMaxCSIDSdfChunkedDataSourceExport(List<int> dsn_ids, int? chunk_size, string file_label, bool include_substance_links, bool include_csid, bool include_inchis, bool include_deprecated_flag, bool include_datasource_count, int csid_min, int csid_max)
			: base(dsn_ids, chunk_size, file_label, include_substance_links, include_csid, include_inchis, include_deprecated_flag, include_datasource_count, false, false)
		{
			_csid_min = csid_min;
			_csid_max = csid_max;
		}

		/// <summary>
		/// Executes the Data Export.
		/// </summary>
		public override void Export()
		{
			//Create the DataExportFiles for the export.
			if (ExportFiles().Count == 0)
				AddFile(new MinMaxCSIDChunkedSDFExportFile(_dsn_ids, _chunk_size, _file_label, _include_substance_links, _include_csid, _include_inchis, _include_deprecated_flag, _include_datasource_count, _csid_min, _csid_max));

			//Export the data.
			base.Export();
		}
	}

	/// <summary>
	/// Class used for generating an SDF file containing compounds from a list of data sources.
	/// </summary>
	public class SdfChunkedDataSourceExport : SdfDataSourceExport
	{
		protected int? _chunk_size;
		protected string _file_label;
		protected bool _include_substance_links;
		protected bool _include_csid;
		protected bool _include_inchis;
		protected bool _include_deprecated_flag;
		protected bool _include_datasource_count;
		protected bool _include_non_validated_synonyms;
		protected bool _include_database_ids;

		/// <summary>
		/// Constructor is passed the dsns to generate the export for.
		/// </summary>
		/// <param name="dsn">The dsns to generate the export for.</param>
		public SdfChunkedDataSourceExport(List<int> dsn_ids
											, int? chunk_size
											, string file_label
											, bool include_substance_links
											, bool include_csid
											, bool include_inchis
											, bool include_deprecated_flag
											, bool include_datasource_count
											, bool include_non_validated_synonyms
											, bool include_database_ids)
			: base(dsn_ids)
		{
			_chunk_size = chunk_size;
			_file_label = file_label;
			_include_substance_links = include_substance_links;
			_include_csid = include_csid;
			_include_inchis = include_inchis;
			_include_deprecated_flag = include_deprecated_flag;
			_include_datasource_count = include_datasource_count;
			_include_non_validated_synonyms = include_non_validated_synonyms;
			_include_database_ids = include_database_ids;
		}

		/// <summary>
		/// Executes the Data Export.
		/// </summary>
		public override void Export()
		{
			//Create the DataExportFiles for the export.
			if (ExportFiles().Count == 0)
				AddFile(new SdfChunkedDataExportFile(_dsn_ids
														, _chunk_size
														, _file_label
														, _include_substance_links
														, _include_csid
														, _include_inchis
														, _include_deprecated_flag
														, _include_datasource_count
														, _include_non_validated_synonyms
														, _include_database_ids));

			//Export the data.
			base.Export();
		}
	}

	/// <summary>
	/// Exports an SDF file containing compounds.
	/// </summary>
	public class SdfDataExportFile : DataSourceListDataExportFile
	{
		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public SdfDataExportFile(List<int> dsn_ids) : base(dsn_ids) { }

		/// <summary>
		/// Exports an SDF file containing Supplementary information.
		/// </summary>
		public override void Export(IDataExport Exp, Encoding Encoding)
		{
			//Generate the file name.
			FileName = "SDF_EXPORT_" + Exp.DateTimeExported.ToString("yyyy-MM-dd") + ".sdf";

			//Create the list of dsn_ids.
			string formatted_dsn_ids = string.Empty;
			foreach (int dsn in _dsn_ids)
			{
				formatted_dsn_ids += dsn.ToString() + ",";
			}
			formatted_dsn_ids = formatted_dsn_ids.Substring(0, formatted_dsn_ids.Length - 1);

			//Create the list of SDF records.
			List<SdfRecord> records = new List<SdfRecord>();

			using (SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString))
			{
				//Write the SDF file containing supplementary info.
				using (TextWriter sdf = new StreamWriter(FileName, false, Encoding))
				{
					conn.ExecuteReader(String.Format(@"SELECT c.cmp_id, s.sdf, sub.ext_id, sub.ext_url FROM compounds c
														JOIN substances sub ON sub.cmp_id = c.cmp_id AND sub.dsn_id IN ({0})
														JOIN compounds_datasources cds ON cds.cmp_id = c.cmp_id
														JOIN [ChemSpiderBlobs]..sdfs s ON s.cmp_id = c.cmp_id
															WHERE cds.dsn_id IN ({0})
																AND c.deleted_yn = 0
																AND sub.deleted_yn = 0
																ORDER BY c.cmp_id ASC", formatted_dsn_ids),
					r =>
					{
						//Create a new SDF record.
						SdfRecord rec = new SdfRecord();

						string mol = ZipUtils.ungzip(r["sdf"] as byte[], Encoding.UTF8);

						//Output the mol to the SDF.
						rec.Mol = mol;

						//Output ext_id and ext_url.
						rec.AddField("ext_id", r["ext_id"].ToString());
						rec.AddField("ext_url", r["ext_url"].ToString());

						//Write the sdf record to the output.
						sdf.Write(rec.ToString());
					});
				}
			}
		}
	}

	/// <summary>
	/// Exports an SDF file containing compounds and CSID
	/// </summary>
	public class SdfChunkedDataExportFile : DataSourceListDataExportFile
	{
		protected int? _chunk_size;
		protected string _file_label;
		protected bool _include_substance_links;
		protected bool _include_csid;
		protected bool _include_inchis;
		protected bool _include_deprecated_flag;
		protected bool _include_datasource_count;
		protected bool _include_non_validated_synonyms;
		protected bool _include_database_ids;

		/// <summary>
		/// Override to change the default export when no Data sources are sepcified.
		/// </summary>
		/// <returns>Default full export SQL - no inchis, substance links or synonyms.</returns>
		protected virtual string GetSQL()
		{
			return @"SELECT TOP 10 c.cmp_id, s.sdf FROM compounds c WITH (NOLOCK)
						JOIN [ChemSpiderBlobs]..sdfs s WITH (NOLOCK) ON s.cmp_id = c.cmp_id
							WHERE c.deleted_yn = 0
								ORDER BY c.cmp_id ASC";
		}

		/// <summary>
		/// Call the base constructor and set the chunk_size.
		/// </summary>
		public SdfChunkedDataExportFile(List<int> dsn_ids
										, int? chunk_size
										, string file_label
										, bool include_substance_links
										, bool include_csid
										, bool include_inchis
										, bool include_deprecated_flag
										, bool include_datasource_count
										, bool include_non_validated_synonyms
										, bool include_database_ids)
			: base(dsn_ids)
		{
			_chunk_size = chunk_size;
			_file_label = file_label;
			_include_substance_links = include_substance_links;
			_include_csid = include_csid;
			_include_inchis = include_inchis;
			_include_deprecated_flag = include_deprecated_flag;
			_include_datasource_count = include_datasource_count;
			_include_non_validated_synonyms = include_non_validated_synonyms;
			_include_database_ids = include_database_ids;
		}

		/// <summary>
		/// Exports an SDF file containing Supplementary information.
		/// </summary>
		public override void Export(IDataExport Exp, Encoding Encoding)
		{
			//Create the list of dsn_ids.
			string formatted_dsn_ids = string.Empty;
			if (_dsn_ids != null)
			{
				foreach (int dsn in _dsn_ids)
				{
					formatted_dsn_ids += dsn.ToString() + ",";
				}
				formatted_dsn_ids = formatted_dsn_ids.Substring(0, formatted_dsn_ids.Length - 1);
			}

			//Create the list of SDF records.
			List<SdfRecord> records = new List<SdfRecord>();
			List<string> validated_synonyms = new List<string>();
			List<string> non_validated_synonyms = new List<string>();
			List<string> database_ids = new List<string>();
			Object[] previous_row = null;
			Object[] sdf_row = null;
			Dictionary<string, int> column_mappings = new Dictionary<string, int>();

			//The format string for each filename.
			string FormatChildFileName = "SDF_EXPORT_{0}" + Exp.DateTimeExported.ToString("yyyy-MM-dd") + "_{1}.sdf";
			FileName = String.Format("{0}_" + Exp.DateTimeExported.ToString("yyyy-MM-dd"), _file_label);

			//Set the filename if this is a single file output.
			if ((_chunk_size ?? 0) == 0)
				FileName = String.Format("{0}_" + Exp.DateTimeExported.ToString("yyyy-MM-dd") + ".sdf", _file_label);

			//Reset counts.
			int count = 0;
			int chunk_count = 1;

			using (SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString))
			{
				//Create the sdf output steam.
				bool writerOpen = false;
				TextWriter sdf = null;

				string sql = string.Empty;
				if (!string.IsNullOrEmpty(formatted_dsn_ids))
				{
					sql = String.Format(@"SELECT c.cmp_id, s.sdf, sub.ext_id, sub.ext_url FROM compounds c
													JOIN substances sub ON sub.cmp_id = c.cmp_id AND sub.dsn_id IN ({0})
													JOIN compounds_datasources cds ON cds.cmp_id = c.cmp_id
													JOIN [ChemSpiderBlobs]..sdfs s ON s.cmp_id = c.cmp_id
														WHERE cds.dsn_id IN ({0})
															AND c.deleted_yn = 0
															AND sub.deleted_yn = 0
															ORDER BY c.cmp_id ASC", formatted_dsn_ids);
				}
				else
				{
					sql = GetSQL();

					//Note: This SQL statement can be changed to do ad-hoc exports based on specific requirements.
					//Eg. the following gets those compounds which have no entry in the acd properties table or have a null average_mass.
					/*
					sql = @"SELECT c.cmp_id, s.sdf FROM compounds c WITH (NOLOCK)
								JOIN [ChemSpiderBlobs]..sdfs s WITH (NOLOCK) ON s.cmp_id = c.cmp_id
									WHERE NOT EXISTS (SELECT 1 FROM acdlabs_props_v12 a WITH (NOLOCK)
														WHERE a.cmp_id = c.cmp_id)
										AND c.deleted_yn = 0
							UNION
							SELECT c.cmp_id, s.sdf FROM compounds c WITH (NOLOCK)
								JOIN [ChemSpiderBlobs]..sdfs s WITH (NOLOCK) ON s.cmp_id = c.cmp_id
								JOIN acdlabs_props_v12 a WITH (NOLOCK) ON a.cmp_id = c.cmp_id
									WHERE a.Average_Mass IS NULL AND
										c.deleted_yn = 0
							ORDER BY c.cmp_id ASC";
					*/
					/*
					//Eg. the following exports those ChemSpider records with no titles set.
					sql = @"SELECT c.cmp_id, s.sdf
								FROM compounds c WITH (NOLOCK)
									JOIN [ChemSpiderBlobs]..sdfs s WITH (NOLOCK) ON s.cmp_id = c.cmp_id
								WHERE c.deleted_yn = 0
									AND ISNULL(dbo.fGetCompoundTitle(c.cmp_id), ISNULL(dbo.fGetSysName(c.cmp_id), '')) = ''
							ORDER BY c.cmp_id ASC";
					*/
					/*
					sql = @"SELECT TOP 10 c.cmp_id, s.sdf FROM compounds c WITH (NOLOCK)
													JOIN [ChemSpiderBlobs]..sdfs s WITH (NOLOCK) ON s.cmp_id = c.cmp_id
														WHERE c.deleted_yn = 0
															ORDER BY c.cmp_id ASC";
					*/
					/*
					sql = @"SELECT TOP 10  s.syn_id, 
									 s.synonym, 
									 cs.cmp_id, 
									 CONVERT(nvarchar,c.SMILES) as SMILES,
									 i.inchi,
									 i2.inchi as std_inchi,
									 sdf.sdf
							 FROM compounds_synonyms cs WITH (NOLOCK)
							 JOIN synonyms s WITH (NOLOCK) ON cs.syn_id = s.syn_id 
							 JOIN compounds c WITH (NOLOCK) ON c.cmp_id = cs.cmp_id
							 JOIN inchis i WITH (NOLOCK) ON i.cmp_id = cs.cmp_id
							 JOIN inchis_std i2 WITH (NOLOCK) ON i2.cmp_id = cs.cmp_id
							 JOIN [ChemSpiderBlobs]..sdfs sdf WITH (NOLOCK) ON sdf.cmp_id = c.cmp_id
							 WHERE approved_yn = 1
							 AND ISNULL(opinion, '') != 'N'
							 AND lang_id1 = 'en'
							 AND cs.deleted_yn = 0
							 AND s.deleted_yn = 0
							 AND c.deleted_yn = 0
							 AND NOT EXISTS (SELECT *
												FROM v_synonyms_synonyms_flags x
													WHERE x.syn_id = s.syn_id
														AND x.name IN ('EINECS', 'RN', 'DBID', 'WLN', 'Beilstein', 'Formula'))
							ORDER BY c.cmp_id ASC";
					 */
				}

				conn.ExecuteReader(sql, r =>
				{
					if (count == 0 && !writerOpen)
					{
						if ((_chunk_size ?? 0) > 0)
						{
							//Add a new child file if we are at the beginning of a new chunk.
							string child_filename = String.Format(FormatChildFileName, _file_label, chunk_count);
							AddChildFile(new FileInfo(child_filename));
							sdf = new StreamWriter(child_filename, false, Encoding);
							writerOpen = true;
						}
						else
						{
							//Single file export so create the stream writer.
							sdf = new StreamWriter(FileName, false, Encoding);
							writerOpen = true;
						}
					}

					//Are we on the next compound?
					if (previous_row != null)
					{
						//Add the synonyms from the previous row.
						int type = 1;  //If no type is specified assume this is a validated synonym.
						string synonym = string.Empty;

						if (column_mappings.ContainsKey("type"))
							type = (int)previous_row[column_mappings["type"]];
						else
							sdf_row = previous_row; //Store the sdf row each time as each row contains the sdf.

						//If we are not doing anything with synonyms set the type to 0 - it's a 1:1 mapping between rows and records in SDF.
						if (column_mappings.ContainsKey("synonym"))
							synonym = previous_row[column_mappings["synonym"]].ToString();
						else
							type = 0;

						switch (type)
						{
							//SDF Row.
							case 0:
								sdf_row = previous_row;
								break;
							//Validated synonym.
							case 1:
								validated_synonyms.Add(synonym);
								break;
							//Non-validated synonym.
							case 2:
								if (_include_non_validated_synonyms)
									non_validated_synonyms.Add(synonym);
								break;
							//Database identifier.
							case 3:
								if (_include_database_ids)
									database_ids.Add(synonym);
								break;
						}   

						//Are we still on the same compound?
						if (previous_row[r.GetOrdinal("cmp_id")].ToString() != r["cmp_id"].ToString())
						{
							count++;

							//Export the record to the Sdf as we are on the next compound.
							ExportSDFRecord(column_mappings, sdf, validated_synonyms, non_validated_synonyms, database_ids, sdf_row);

							//Reset the chunk if we are at the end.
							if ((_chunk_size ?? 0) > 0)
								if (count == _chunk_size)
								{
									count = 0;
									chunk_count++;

									//Close and dispose.
									sdf.Close();
									sdf.Dispose();
									writerOpen = false;
								}
						}
					}
					else
					{
						//Create the column names mapping.
						for (int i = 0; i < r.FieldCount; i++)
							column_mappings.Add(r.GetName(i), i);
					}

					//Store the previous row.
					previous_row = new Object[r.FieldCount];
					r.GetValues(previous_row);
				});

				//Export the final record to the Sdf as we are on the last compound.
				ExportSDFRecord(column_mappings, sdf, validated_synonyms, non_validated_synonyms, database_ids, sdf_row);

				//Close the writer if it's still open.
				if (writerOpen)
				{
					sdf.Close();
					sdf.Dispose();
				}
			}
		}

		private void ExportSDFRecord(Dictionary<string, int> column_mappings
									, TextWriter sdf
									, List<string> validated_synonyms
									, List<string> non_validated_synonyms
									, List<string> database_ids
									, Object[] row)
		{
			//Create a new SDF record.
			SdfRecord rec = new SdfRecord();

			string mol = ZipUtils.ungzip(row[column_mappings["sdf"]] as byte[], Encoding.UTF8);

			//Output the mol to the SDF.
			rec.Mol = mol;

			//Output ext_id and ext_url.
			if (_include_substance_links)
			{
				rec.AddField("ext_id", row[column_mappings["ext_id"]].ToString());
				rec.AddField("ext_url", row[column_mappings["ext_url"]].ToString());
			}

			//Output cmp_id.
			if (_include_csid)
			{
				rec.AddField("csid", row[column_mappings["cmp_id"]].ToString());
			}

			//Output the inchis.
			if (_include_inchis)
			{
				rec.AddField("inchi", row[column_mappings["inchi"]].ToString());
				rec.AddField("std_inchi", row[column_mappings["std_inchi"]].ToString());
			}

			//Output the deprecated flag.
			if (_include_deprecated_flag)
			{
				rec.AddField("deprecated", row[column_mappings["deprecated"]].ToString());
			}

			//Output the data source count.
			if (_include_datasource_count)
			{
				rec.AddField("datasource_count", row[column_mappings["datasource_count"]].ToString());
			}

			//Output the validated synonyms.
			if (validated_synonyms.Count > 0)
			{
				foreach (string validated_synonym in validated_synonyms)
					rec.AddField("validated_synonyms", validated_synonym);
			}

			//Output the non-validated synonyms.
			if (non_validated_synonyms.Count > 0)
			{
				foreach (string non_validated_synonym in non_validated_synonyms)
					rec.AddField("non_validated_synonyms", non_validated_synonym);
			}

			//Output the database ids.
			if (database_ids.Count > 0)
			{
				foreach (string database_id in database_ids)
					rec.AddField("database_ids", database_id);
			}

			//Write the sdf record to the output.
			sdf.Write(rec.ToString());

			//Clear the lists.
			validated_synonyms.Clear();
			non_validated_synonyms.Clear();
			database_ids.Clear();
		}
	}

	/// <summary>
	/// Class for performing chunked SDF synonyms exports.
	/// </summary>
	public class MinMaxCSIDChunkedSDFExportFile : SdfChunkedDataExportFile
	{
		protected int _min_csid;
		protected int _max_csid;

		public MinMaxCSIDChunkedSDFExportFile(List<int> dsn_ids, int? chunk_size, string file_label, bool include_substance_links, bool include_csid, bool include_inchis, bool include_deprecated_flag, bool include_datasource_count, int min_csid, int max_csid)
			: base(dsn_ids, chunk_size, file_label, include_substance_links, include_csid, include_inchis, include_deprecated_flag, include_datasource_count, false, false)
		{
			_min_csid = min_csid;
			_max_csid = max_csid;
		}

		/// <summary>
		/// Overridden to change the export to include required information.
		/// </summary>
		protected override string GetSQL()
		{
			return String.Format(@"SELECT s.syn_id, 
							 s.synonym,
							 c.cmp_id, 
							 c.deleted_yn deprecated,
							 ds.ds_count datasource_count,
							 CONVERT(nvarchar,c.SMILES) as SMILES,
							 i.inchi,
							 i2.inchi as std_inchi,
							 sdf.sdf
						FROM compounds c WITH (NOLOCK)
							LEFT JOIN compounds_synonyms cs WITH (NOLOCK) ON cs.cmp_id = c.cmp_id 
							LEFT JOIN synonyms s WITH (NOLOCK) ON s.syn_id = cs.syn_id
							JOIN inchis i WITH (NOLOCK) ON i.cmp_id = c.cmp_id
							JOIN inchis_std i2 WITH (NOLOCK) ON i2.cmp_id = c.cmp_id
							JOIN [ChemSpiderBlobs]..sdfs sdf WITH (NOLOCK) ON sdf.cmp_id = c.cmp_id
							JOIN v_compound_ds_count ds ON ds.cmp_id = c.cmp_id
							WHERE ((cs.approved_yn = 1
								AND ISNULL(cs.opinion, '') != 'N'
								AND cs.deleted_yn = 0
								AND s.lang_id1 = 'en'
								AND s.deleted_yn = 0
								AND NOT EXISTS (SELECT * FROM v_synonyms_synonyms_flags x
													WHERE x.syn_id = s.syn_id
														AND x.name IN ('EINECS', 'RN', 'DBID', 'WLN', 'Beilstein', 'Formula'))))
								AND c.cmp_id >= {0} AND c.cmp_id <= {1}
						UNION
						SELECT NULL syn_id, 
							 NULL synonym,
							 c.cmp_id, 
							 c.deleted_yn deprecated,
							 ds.ds_count datasource_count,
							 CONVERT(nvarchar,c.SMILES) as SMILES,
							 i.inchi,
							 i2.inchi as std_inchi,
							 sdf.sdf
						FROM compounds c WITH (NOLOCK)
							JOIN inchis i WITH (NOLOCK) ON i.cmp_id = c.cmp_id
							JOIN inchis_std i2 WITH (NOLOCK) ON i2.cmp_id = c.cmp_id
							JOIN [ChemSpiderBlobs]..sdfs sdf WITH (NOLOCK) ON sdf.cmp_id = c.cmp_id
							JOIN v_compound_ds_count ds ON ds.cmp_id = c.cmp_id
							WHERE c.cmp_id >= {0} AND c.cmp_id <= {1}
						ORDER BY c.cmp_id ASC", _min_csid, _max_csid);
		}
	}

	/// <summary>
	/// Class for performing chunked SDF synonyms exports.
	/// </summary>
	public class SynonymsChunkedSDFExportFile : SdfChunkedDataExportFile
	{
		public SynonymsChunkedSDFExportFile(List<int> dsn_ids, int? chunk_size, string file_label, bool include_substance_links, bool include_csid, bool include_inchis, bool include_deprecated_flag, bool include_datasource_count, bool include_non_validated_synonyms, bool include_database_ids)
			: base(dsn_ids, chunk_size, file_label, include_substance_links, include_csid, include_inchis, include_deprecated_flag, include_datasource_count, include_non_validated_synonyms, include_database_ids)
		{ }

		/// <summary>
		/// Overridden to change the export to include synonyms info.
		/// </summary>
		protected override string GetSQL()
		{
			//Always include smiles, inchis, sdf and validated synonyms.
			string sql = @"SELECT 0 as type,
									NULL as syn_id, 
									NULL as synonym, 
									c.cmp_id, 
									CONVERT(nvarchar, c.SMILES) as SMILES,
									i.inchi,
									i2.inchi as std_inchi,
									sdf.sdf
							FROM compounds c WITH (NOLOCK)
								JOIN inchis i WITH (NOLOCK) ON i.cmp_id = c.cmp_id
								JOIN inchis_std i2 WITH (NOLOCK) ON i2.cmp_id = c.cmp_id
								JOIN [ChemSpiderBlobs]..sdfs sdf WITH (NOLOCK) ON sdf.cmp_id = c.cmp_id
							WHERE c.deleted_yn = 0
						UNION ALL
							SELECT 1 as type,
									validated_syns.syn_id, 
									validated_syns.synonym, 
									cs.cmp_id, 
									NULL as SMILES,
									NULL as inchi,
									NULL as std_inchi,
									NULL as sdf
							FROM compounds_synonyms cs WITH (NOLOCK)
								JOIN synonyms validated_syns WITH (NOLOCK) ON cs.syn_id = validated_syns.syn_id 
								JOIN compounds c WITH (NOLOCK) ON c.cmp_id = cs.cmp_id
							WHERE approved_yn = 1
								AND ISNULL(opinion, '') != 'N'
								AND cs.deleted_yn = 0
								AND validated_syns.deleted_yn = 0
								AND c.deleted_yn = 0
								AND NOT EXISTS (SELECT 1 FROM v_synonyms_synonyms_flags x
													WHERE x.syn_id = validated_syns.syn_id
														AND x.name IN ('DBID'))";

			//Non-validated synonyms.
			if (_include_non_validated_synonyms)
			{
				sql += @" UNION ALL
							SELECT 2 as type,
									s.syn_id, 
									s.synonym,
									cs.cmp_id, 
									NULL as SMILES,
									NULL as inchi,
									NULL as std_inchi,
									NULL as sdf
							FROM compounds_synonyms cs WITH (NOLOCK)
								JOIN synonyms s WITH (NOLOCK) ON cs.syn_id = s.syn_id 
								JOIN compounds c WITH (NOLOCK) ON c.cmp_id = cs.cmp_id
							WHERE cs.approved_yn = 0
								AND cs.opinion IS NULL
								AND cs.deleted_yn = 0
								AND s.deleted_yn = 0
								AND c.deleted_yn = 0
								AND NOT EXISTS (SELECT 1 FROM v_synonyms_synonyms_flags x
													WHERE x.syn_id = s.syn_id
														AND x.name IN ('DBID'))";
			}

			//Database identifiers.
			if (_include_database_ids)
			{
				sql += @" UNION ALL
							SELECT 3 as type,
									s.syn_id, 
									s.synonym,
									cs.cmp_id, 
									NULL as SMILES,
									NULL as inchi,
									NULL as std_inchi,
									NULL as sdf
							FROM compounds_synonyms cs WITH (NOLOCK)
								JOIN synonyms s WITH (NOLOCK) ON cs.syn_id = s.syn_id 
								JOIN compounds c WITH (NOLOCK) ON c.cmp_id = cs.cmp_id
								JOIN v_synonyms_synonyms_flags f WITH (NOLOCK) ON f.syn_id = s.syn_id AND f.name = 'DBID'
							WHERE cs.deleted_yn = 0
								AND s.deleted_yn = 0
								AND c.deleted_yn = 0";
			}

			sql += @" ORDER BY c.cmp_id";

			return sql;
		}
	}

	/// <summary>
	/// Class for performing chunked SDF synonyms exports - this is used for Colin's DERA purposes.
	/// It excludes various synonym types and only exports validated english language synonyms.
	/// </summary>
	public class ColinsSynonymsChunkedSDFExportFile : SdfChunkedDataExportFile
	{
		public ColinsSynonymsChunkedSDFExportFile(List<int> dsn_ids, int? chunk_size, string file_label, bool include_substance_links, bool include_csid, bool include_inchis, bool include_deprecated_flag, bool include_datasource_count, bool include_non_validated_synonyms, bool include_database_ids)
			: base(dsn_ids, chunk_size, file_label, include_substance_links, include_csid, include_inchis, include_deprecated_flag, include_datasource_count, include_non_validated_synonyms, include_database_ids)
		{ }

		/// <summary>
		/// Overridden to change the export to include synonyms info.
		/// </summary>
		protected override string GetSQL()
		{
			return @"SELECT s.syn_id, 
									 s.synonym, 
									 cs.cmp_id, 
									 CONVERT(nvarchar,c.SMILES) as SMILES,
									 i.inchi,
									 i2.inchi as std_inchi,
									 sdf.sdf
							 FROM compounds_synonyms cs WITH (NOLOCK)
							 JOIN synonyms s WITH (NOLOCK) ON cs.syn_id = s.syn_id 
							 JOIN compounds c WITH (NOLOCK) ON c.cmp_id = cs.cmp_id
							 JOIN inchis i WITH (NOLOCK) ON i.cmp_id = cs.cmp_id
							 JOIN inchis_std i2 WITH (NOLOCK) ON i2.cmp_id = cs.cmp_id
							 JOIN [ChemSpiderBlobs]..sdfs sdf WITH (NOLOCK) ON sdf.cmp_id = c.cmp_id
							 WHERE LEN(s.synonym) <= 32
							 AND approved_yn = 1
							 AND ISNULL(opinion, '') != 'N'
							 AND lang_id1 = 'en'
							 AND cs.deleted_yn = 0
							 AND s.deleted_yn = 0
							 AND c.deleted_yn = 0
							 AND NOT EXISTS (SELECT *
												FROM v_synonyms_synonyms_flags x
													WHERE x.syn_id = s.syn_id
														AND x.name IN ('EINECS', 'RN', 'DBID', 'WLN', 'Beilstein'))
							ORDER BY c.cmp_id ASC";
		}
	}

	#endregion

	#region RdfDataSourceExport

	/// <summary>
	/// Class for generating Rdf Data Exports for CVSP.
	/// </summary>
	public class RdfCRSDataSourceExport : DataExport
	{
		//The export is specific to a particular data source.
		private string _dsn;

		/// <summary>
		/// Constructor is passed the dsn to generate the export for.
		/// </summary>
		/// <param name="dsn">The dsn to generate the export for.</param>
		public RdfCRSDataSourceExport(string dsn)
		{
			_dsn = dsn;
		}

		/// <summary>
		/// Returns the ExportId after the Export has been created in the database.
		/// </summary>
		/// <returns>ExportID as integer</returns>
		public override int GetExportId()
		{
			int export_id;
			using (SqlConnection conn = new SqlConnection(DBConnection))
			{
				int dsn_id = 0;
				int version_id = 0;

				//Get the latest version_id and dsn_id.
				conn.ExecuteReader(@"SELECT TOP 1 d.dsn_id, dv.version_id FROM datasources d
										JOIN data_versions dv ON dv.dsn_id = d.dsn_id
											WHERE dsn_name = @name
												ORDER BY dv.version_id DESC",
					r =>
					{
						dsn_id = (int)r["dsn_id"];
						version_id = (int)r["version_id"];
					}, new { name = _dsn });

				//Insert the RDF Export and return the ExportId.
				export_id = conn.ExecuteScalar<int>(@"EXEC AddRdfExport @dsn_id, @version_id",
								new { dsn_id = dsn_id, version_id = version_id }
								);
			}
			return export_id;
		}

		/// <summary>
		/// Updates the status and error message of the export - must be implemented by the inherited class.
		/// </summary>
		public override void UpdateExport()
		{
			using (SqlConnection conn = new SqlConnection(DBConnection))
			{
				//Update the RDF Export set the failed and finished flags and the error message.
				conn.ExecuteCommand(@"EXEC UpdateRdfExport @exp_id, @has_failed, @has_finished, @error_message, @export_directory",
									new
									{
										exp_id = ExportId,
										has_failed = ExportFailed,
										has_finished = true,
										error_message = ErrorMessage,
										export_directory = ExportFiles().First().FtpTargetDirectory
									}
									);
			}
		}

		/// <summary>
		/// Executes the Data Export.
		/// </summary>
		public override void Export()
		{
			//Create the DataSnapshots for the export, pass reference to Export as it contains the connection string.
			AddSnapshot(new PropertiesCRSDataSourceSnapshot(this, _dsn)); //Do properties and synonyms first as they call web services.
			AddSnapshot(new SynonymsCRSDataSourceSnapshot(this, _dsn));
			AddSnapshot(new IdentifiersCRSDataSourceSnapshot(this, _dsn));
			AddSnapshot(new ReferencesCRSDataSourceSnapshot(this, _dsn));
			AddSnapshot(new ParentChildCRSDataSourceSnapshot(this, _dsn));
			AddSnapshot(new IssuesCRSDataSourceSnapshot(this, _dsn));

			//Create the DataExportFiles for the export.
			AddFile(new ExactLinksetDataExportFile(_dsn));
			if (Turtle.useSkosRelatedMatchForDSN(_dsn))
			{
				AddFile(new RelatedLinksetDataExportFile(_dsn));
			}

			//Export the Synonyms.
			AddFile(new SynonymsDataExportFile(_dsn));

			//Export the Properties.
			AddFile(new PropertiesDataExportFile(_dsn));

			//Export the Parent-Child linksets depends on the parent_types table - need one file for each parent type.
			getParentChildLinksets(_dsn).ForEach(l => AddFile(l));

			//Export the OPS-ChemSpider linkset.
			AddFile(new ChemSpiderLinksetDataExportFile(_dsn));

			//Export the Issues.
			AddFile(new IssuesDataExportFile(_dsn));

			//Export the data.
			base.Export();
		}

		/// <summary>
		/// Get the Parent-Child Linksets we will need to generate.
		/// </summary>
		/// <returns></returns>
		private List<IDataExportFile> getParentChildLinksets(string dsn)
		{
			List<IDataExportFile> linksets = new List<IDataExportFile>();
			using (SqlConnection conn = new SqlConnection(DBConnection))
			{
				conn.ExecuteReader(@"SELECT * FROM parent_types",
									r =>
									{
										linksets.Add(new ParentChildLinksetDataExportFile(dsn, Convert.ToInt32(r["parent_id"]), Turtle.SkosStringToPredicate(r["rdf_relation"].ToString())));
									});
			}
			return linksets;
		}
	}

	/// <summary>
	/// Class for generating Rdf Data Exports for the OPS project.
	/// </summary>
	public class RdfDataSourceExport : DataExport
	{
		//The export is specific to a particular data source.
		private string _dsn;

		/// <summary>
		/// Constructor is passed the dsn to generate the export for.
		/// </summary>
		/// <param name="dsn">The dsn to generate the export for.</param>
		public RdfDataSourceExport(string dsn)
		{
			_dsn = dsn;
		}

		/// <summary>
		/// Returns the ExportId after the Export has been created in the database.
		/// </summary>
		/// <returns>ExportID as integer</returns>
		public override int GetExportId()
		{
			int export_id;
			using (SqlConnection conn = new SqlConnection(ChemSpiderDB.ConnectionString))
			{
				//Get the dsn_id.
				int dsn_id = conn.ExecuteScalar<int>("SELECT dsn_id FROM ChemUsers..data_sources WHERE name = @name", new { name = _dsn });

				//Insert the RDF Export and return the ExportId.
				export_id = conn.ExecuteScalar<int>(@"EXEC AddRdfExport @dsn_id, @export_file",
								new { dsn_id = dsn_id, export_file = String.Empty } //Don't need an export file as there are more than 1 now.
								);
			}
			return export_id;
		}

		/// <summary>
		/// Executes the Data Export.
		/// </summary>
		public override void Export()
		{
			//Create the DataSnapshots for the export, pass reference to Export as it contains the connection string.
			AddSnapshot(new IdentifiersDataSourceSnapshot(this, _dsn));
			AddSnapshot(new ReferencesDataSourceSnapshot(this, _dsn));
			AddSnapshot(new SynonymsDataSourceSnapshot(this, _dsn));
			AddSnapshot(new PropertiesDataSourceSnapshot(this, _dsn));

			//Create the DataExportFiles for the export.
			AddFile(new ExactLinksetDataExportFile(_dsn));
			if (Turtle.useSkosRelatedMatchForDSN(_dsn))
			{
				AddFile(new RelatedLinksetDataExportFile(_dsn));
			}
			AddFile(new SynonymsDataExportFile(_dsn));
			AddFile(new PropertiesDataExportFile(_dsn));

			//Export the data.
			base.Export();
		}
	}

	/// <summary>
	/// Used for exporting a data export file based on a particular Data Source.
	/// </summary>
	public class DataSourceDataExportFile : DataExportFile
	{
		//The export is specific to a particular data source.
		protected string _dsn;

		//Some Data Sources have names which are not valid in RDF.
		public string DsnLabel
		{
			get { return _dsn.Replace(" ", "_").ToUpper(); }
		}

		/// <summary>
		/// Constructor is passed the dsn to generate the export for.
		/// </summary>
		/// <param name="dsn">The dsn to generate the export for.</param>
		public DataSourceDataExportFile(string dsn)
		{
			_dsn = dsn;
		}

		/// <summary>
		/// Set the FTPTargetDirectory for the Data Source Export file.
		/// </summary>
		public override void Export(IDataExport Exp, Encoding Encoding)
		{
			base.Export(Exp, Encoding);
			FtpTargetDirectory = String.Format("{0}/{1}", Exp.DateTimeExported.ToString("yyyyMMdd"), DsnLabel);
		}

		/// <summary>
		/// Gets the uri prefix from the database, also outputs an alias and whether to use the full URI or not.
		/// </summary>
		/// <param name="dsn">The dsn</param>
		/// <param name="alias">The outputted alias</param>
		/// <param name="use_full_uri">Boolean indicating whether to use the full uri</param>
		/// <returns>The DSN URI prefix</returns>
		protected string getDSNPrefix(IDataExport Exp, string dsn, out string alias, out bool use_full_uri)
		{
			using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
			{
				string dsn_uri = conn.ExecuteScalar<string>("SELECT dsn_uri FROM datasources WHERE dsn_name = @dsn", new { dsn = dsn });

				//Use the lowercase data source name as the alias.
				alias = dsn.ToLower();

				//Use the full uris for everything as we cannot rely on it being correct as an alias.
				use_full_uri = true;
				return dsn_uri;
			}
		}
	}

	/// <summary>
	/// Used for exporting a date export file based on a particular list of Data Sources and their associated ExportIds.
	/// </summary>
	public class DataSourceListDataExportFile : DataExportFile
	{
		//The export is specific to a particular list of data source and their associated ExportIds.
		protected Dictionary<string, int> _dsn_exp_ids;
		protected List<int> _dsn_ids;

		/// <summary>
		/// Constructor is passed  list of data sources and their associated ExportIds.
		/// </summary>
		/// <param name="dsn">The dsn to generate the export for.</param>
		public DataSourceListDataExportFile(Dictionary<string, int> dsn_exp_ids)
		{
			_dsn_exp_ids = dsn_exp_ids;
		}

		/// <summary>
		/// Constructor is passed list of data source ids.
		/// </summary>
		/// <param name="dsn">The dsn to generate the export for.</param>
		public DataSourceListDataExportFile(List<int> dsn_ids)
		{
			_dsn_ids = dsn_ids;
		}
	}

	#endregion

	#region LinksetDataExportFile

	/// <summary>
	/// </summary>
	public class ParentChildLinksetDataExportFile : DataSourceDataExportFile
	{
		/// <summary>
		/// The id of the type of Parent we are referencing in this linkset file.
		/// </summary>
		public int ParentId { get; set; }

		/// <summary>
		/// How we will link items in our linksets.
		/// </summary>
		public Turtle.SkosPredicate SkosPredicate { get; set; }

		/// <summary>
		/// How the linkset is justified.
		/// </summary>
		public string Predicate { get; set; }

		/// <summary>
		/// Description of the linkset justification.
		/// </summary>
		public string Description { get; set; }

		/// <summary>
		/// A label used in filenames - replace spaces and dots and lower-case.
		/// </summary>
		public string Label { get { return Description.Replace(" ", "_").Replace(".", "_").ToLower(); } }

		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public ParentChildLinksetDataExportFile(string dsn, int parent_id, Turtle.SkosPredicate skos_predicate)
			: base(dsn)
		{
			ParentId = parent_id;
			SkosPredicate = skos_predicate;  //E.g. skos:exactMatch
		}

		/// <summary>
		/// Exports an Rdf Parent Child Data Source Export File.
		/// </summary>
		public override void Export(IDataExport Exp, Encoding Encoding)
		{
			//Get the parent_type details.
			using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
			{
				conn.ExecuteReader(String.Format(@"SELECT *
													FROM parent_types
														WHERE parent_id = {0}", ParentId),
									r =>
									{
										Predicate = r["rdf_predicate"].ToString();
										Description = r["parent_name"].ToString();
									});
			}

			//Perform the full export of related matches.
			string file_part = Turtle.SkosPredicateToString(SkosPredicate).Replace("Match", "").ToUpper();
			FileName = String.Format("LINKSET_{0}_PARENT_CHILD_{1}_{2}{3}.ttl", file_part, Label.ToUpper(), DsnLabel.ToUpper(), Exp.DateTimeExported.ToString("yyyyMMdd"));
			base.Export(Exp, Encoding);

			//Get the linkset and parent_child prefixes.
			Dictionary<string, string> prefixes = new Dictionary<string, string>(Turtle.linkset_prefixes);
			foreach (KeyValuePair<string, string> p in Turtle.parent_child_prefixes)
				prefixes.Add(p.Key, p.Value);

			//Add the ops url to the prefixes.
			prefixes.Add(Turtle.RDF_URI_PREFIX.ToLower(), Turtle.RDF_URI + "/");  //Re-use the prefix as the alias.

			//Get the linkset uri.
			string linkset_uri = String.Format("{0}/{1}/{2}", Turtle.FTP_PREFIX, FtpTargetDirectory, FileName);

			//Add the prefix for this void file with the empty prefix.
			prefixes.Add("", String.Format("{0}#", linkset_uri));

			//Write the Parent child turtle export.
			using (TextWriter w = new StreamWriter(FileName, false, Encoding))
			{
				//Output the prefixes.
				foreach (KeyValuePair<string, string> p in prefixes)
					w.WriteLine(String.Format("@prefix {0}: <{1}> .", p.Key, p.Value));

				//Add the predicate to describe which subset this dataset is in.
				string subset_object = string.Format("{0}_parent_child_{1}_{2}", DsnLabel.ToLower(), Label, Turtle.SkosPredicateToString(SkosPredicate)); //E.g. :drugbank_parent_child_isotope_unsensitive_parent_exactMatch
				w.WriteLine(Turtle.getDSNInDatasetPredicate(_dsn, subset_object, linkset_uri));

				using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
				{
					//Write out the predicates:
					conn.ExecuteReader(@"SELECT e.csid_daughter, e.csid_parent
														FROM rdf_exported_parent_child_snapshot e
															JOIN parent_types p ON p.parent_id = e.parent_id
																WHERE e.exp_id = @exp_id AND p.parent_id = @parent_id
														ORDER BY e.csid_parent ASC",
											r =>
											{
												string line;
												/*if (ParentId == 0)
													line = "{0}:{1}{2} {3} {0}:{1}{4} .";  //Invert the Fragment relationship.
												else*/
												line = "{0}:{1}{4} {3} {0}:{1}{2} .";

												w.WriteLine(line
															, Turtle.RDF_URI_PREFIX.ToLower()
															, Turtle.RDF_URI_PREFIX
															, r["csid_parent"]
															, String.Format("{0}:{1}", Turtle.SKOS_PREFIX, Turtle.SkosPredicateToString(SkosPredicate))
															, r["csid_daughter"]);
											}, new { exp_id = Exp.ExportId, parent_id = ParentId });
				}
			}
		}
	}

	/// <summary>
	/// Used for generating export files for linking OPS compounds to ChemSpider compounds for a particular Data Source.
	/// </summary>
	public class ChemSpiderLinksetDataExportFile : DataSourceDataExportFile
	{
		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public ChemSpiderLinksetDataExportFile(string dsn) : base(dsn) { }

		/// <summary>
		/// Exports an Rdf Linkset containing Exact Matches.
		/// </summary>
		public override void Export(IDataExport Exp, Encoding Encoding)
		{
			//Set the file name.
			FileName = String.Format("LINKSET_EXACT_OPS_CHEMSPIDER_{0}{1}.ttl", DsnLabel, Exp.DateTimeExported.ToString("yyyyMMdd"));
			base.Export(Exp, Encoding);

			//Get a copy of the linkset prefixes.
			Dictionary<string, string> prefixes = new Dictionary<string, string>(Turtle.linkset_prefixes);
			string db_alias = string.Empty;
			string dsn_prefix = string.Empty;

			//Perform the full export of exact matches.
			using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
			{
				//Write the linkset containing skos:exactMatch References.
				using (TextWriter w = new StreamWriter(FileName, false, Encoding))
				{
					//Get the URI for Chemspider RDF.
					dsn_prefix = Turtle.CHEMSPIDER_RDF_URI;

					//Get the linkset uri.
					string linkset_uri = String.Format("{0}/{1}/{2}", Turtle.FTP_PREFIX, FtpTargetDirectory, FileName);

					//Add the ops url to the prefixes.
					prefixes.Add(Turtle.RDF_URI_PREFIX.ToLower(), Turtle.RDF_URI + "/");  //Re-use the prefix as the alias.

					//Add the prefix for this void file with the empty prefix.
					prefixes.Add("", String.Format("{0}#", linkset_uri));

					//Output the prefixes for the linkset.
					foreach (KeyValuePair<string, string> p in prefixes)
						w.WriteLine(String.Format("@prefix {0}: <{1}> .", p.Key, p.Value));

					//Add the predicate to describe which subset this dataset is in.
					string subset_object = string.Format("{0}_{1}_{2}", DsnLabel.ToLower(), "ops_chemspider", Turtle.SKOS_EXACT_MATCH); //E.g. :drugbank_ops_chemspider_exactMatch
					w.WriteLine(Turtle.getDSNInDatasetPredicate(_dsn, subset_object, linkset_uri));

					//Get the linkset details from CSID-CMP_ID in exported_identifiers_snapshot table.
					conn.ExecuteReader(@"SELECT r.* FROM rdf_exported_identifiers_snapshot r
											WHERE r.exp_id = @exp_id AND cmp_id IS NOT NULL",
					r =>
					{
						w.WriteLine("{0}:{1}{2} skos:exactMatch <{3}/{4}> ."
							   , Turtle.RDF_URI_PREFIX.ToLower()    //0
							   , Turtle.RDF_URI_PREFIX              //1
							   , r["csid"]                          //2
							   , dsn_prefix                         //3
							   , r["cmp_id"]);                      //4
					}
					, new { exp_id = Exp.ExportId });
				}
			}
		}
	}

	/// <summary>
	/// Used for generating export files for the Exact Match Linkset data export file.
	/// </summary>
	public class ExactLinksetDataExportFile : DataSourceDataExportFile
	{
		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public ExactLinksetDataExportFile(string dsn) : base(dsn) { }

		/// <summary>
		/// Exports an Rdf Linkset containing Exact Matches.
		/// </summary>
		public override void Export(IDataExport Exp, Encoding Encoding)
		{
			//Set the file name.
			FileName = String.Format("LINKSET_EXACT_{0}{1}.ttl", DsnLabel, Exp.DateTimeExported.ToString("yyyyMMdd"));
			base.Export(Exp, Encoding);

			//Get a copy of the linkset prefixes.
			Dictionary<string, string> prefixes = new Dictionary<string, string>(Turtle.linkset_prefixes);
			string db_alias = string.Empty;
			string dsn_prefix = string.Empty;
			bool use_full_uri = false;

			//Perform the full export of exact matches.
			using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
			{
				//Write the linkset containing skos:exactMatch References.
				using (TextWriter w = new StreamWriter(FileName, false, Encoding))
				{
					//Add the dsn prefix if not already added in the above list of prefixes and we must not use the full uri.
					dsn_prefix = getDSNPrefix(Exp, _dsn, out db_alias, out use_full_uri);
					if (!use_full_uri)
					{
						KeyValuePair<string, string> kvp = Turtle.linkset_prefixes.Where(l => (l.Value == dsn_prefix)).FirstOrDefault();
						if (kvp.Value != null)
							//We have already added this item so get the alias.
							db_alias = kvp.Key;
						else
							prefixes.Add(db_alias, dsn_prefix);
					}

					//Get the linkset uri.
					string linkset_uri = String.Format("{0}/{1}/{2}", Turtle.FTP_PREFIX, FtpTargetDirectory, FileName);

					//Add the base uri prefix.
					prefixes.Add(Turtle.RDF_URI_PREFIX.ToLower(), Turtle.RDF_URI + "/");  //Re-use the prefix as the alias.

					//Add the prefix for this void file with the empty prefix.
					prefixes.Add("", String.Format("{0}#", linkset_uri));

					//Output the prefixes for the linkset.
					foreach (KeyValuePair<string, string> p in prefixes)
						w.WriteLine(String.Format("@prefix {0}: <{1}> .", p.Key, p.Value));

					//Add the predicate to describe which subset this dataset is in.
					string subset_object = string.Format("{0}_{1}", DsnLabel.ToLower(), Turtle.SKOS_EXACT_MATCH); //E.g. :chebi_exactMatch
					w.WriteLine(Turtle.getDSNInDatasetPredicate(_dsn, subset_object, linkset_uri));

					//References.
					conn.ExecuteReader(String.Format(@"SELECT r.* FROM rdf_exported_ext_refs_snapshot r
													WHERE r.exp_id = {0}", Exp.ExportId),
					r =>
					{
						if (Turtle.useSkosExactMatchForId(_dsn, r["ext_id"].ToString()))
							w.WriteLine("{0}:{1}{2} skos:exactMatch {3} ."
								, Turtle.RDF_URI_PREFIX.ToLower()
								, Turtle.RDF_URI_PREFIX
								, r["cmp_id"]
								, Turtle.getDSNUri(_dsn, r["ext_id"].ToString(), db_alias, dsn_prefix, use_full_uri));
					});
				}
			}
		}
	}

	/// <summary>
	/// Used for generating the export file for a void.ttl void descriptor for a list of data sources that have been included in the export.
	/// The void descriptors are used to describe the provenance and versioning of the data export and the external data sources we link to.
	/// </summary>
	public class VoidDataExportFile : DataSourceListDataExportFile
	{
		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public VoidDataExportFile(Dictionary<string, int> dsn_exp_ids) : base(dsn_exp_ids) { }

		//Configuration settings.
		private string VOID_TITLE = ConfigurationManager.AppSettings["void_title"].ToString();                                      //E.g. "A VoID Description of the ChemSpider Dataset"
		private string DATASET_TITLE = ConfigurationManager.AppSettings["void_dataset_title"].ToString();                           //E.g. "ChemSpider Dataset"
		private string DATASET_DESCRIPTION = ConfigurationManager.AppSettings["void_dataset_description"].ToString();               //E.g. "ChemSpider's Public Dataset"
		private string DATASET_CREATED_ON = ConfigurationManager.AppSettings["void_dataset_created_on"].ToString();                 //E.g. "2012-10-24T10:49:00Z"
		private string PROVENANCE_CREATED_ON = ConfigurationManager.AppSettings["void_provenance_created_on"].ToString();           //E.g. "2007-03-01T00:00:00Z"
		private string PROVENANCE_AUTHORED_ON = ConfigurationManager.AppSettings["void_provenance_authored_on"].ToString();         //E.g. "2012-10-30T12:16:00Z"
		private string USER_PROFILE_URL_PREFIX = ConfigurationManager.AppSettings["void_user_profile_url_prefix"].ToString();       //E.g. "http://www.chemspider.com/UserProfile.aspx?username={0}"

		private Uri m_provenance_authored_by = new Uri(ConfigurationManager.AppSettings["void_provenance_authored_by"].ToString()); //E.g. http://www.chemspider.com/UserProfile.aspx?username=jonsteele
		private Uri m_home_page = new Uri(ConfigurationManager.AppSettings["void_homepage"].ToString());                            //E.g. http://www.chemspider.com/
		private Uri m_license = new Uri(ConfigurationManager.AppSettings["void_license"].ToString());                               //E.g. http://creativecommons.org/licenses/by-sa/3.0/
		private Uri m_vocabulary_subject = new Uri(ConfigurationManager.AppSettings["void_subject"].ToString());                    //E.g. http://dbpedia.org/resource/Molecule
		private Uri m_example_resource = new Uri(ConfigurationManager.AppSettings["void_example_resource"].ToString());             //E.g. http://rdf.chemspider.com/2157
		private Uri m_chemspider_license = new Uri(ConfigurationManager.AppSettings["void_chemspider_license"].ToString());         //E.g. http://creativecommons.org/licenses/by/3.0/

		//Pages that the chemspider dataset will feature on -removed.
		private Uri m_page_data_hub = new Uri("http://thedatahub.org/dataset/chemspider");
		private Uri m_page_miriam = new Uri("http://www.ebi.ac.uk/miriam/main/collections/MIR:00000138");

		//Prefixes.
		private Dictionary<string, string> m_void_prefixes = new Dictionary<string, string>()
					{
						{ "dcterms", Turtle.ns_dcterms.ToString() },
						{ "dctype", Turtle.ns_dctype.ToString() },
						{ "foaf", Turtle.ns_foaf.ToString() },
						{ "freq", Turtle.ns_freq.ToString() },
						{ "pav", Turtle.ns_pav.ToString() },
						{ "rdf", Turtle.ns_rdf.ToString() },
						{ "rdfs", Turtle.ns_rdfs.ToString() },
						{ "voag", Turtle.ns_voag.ToString() },
						{ "void", Turtle.ns_void.ToString() },
						{ "xsd", Turtle.ns_xsd.ToString() },
						{ "skos", Turtle.ns_skos.ToString() },
						{ "dul",  Turtle.ns_dul.ToString() },
						{ "prov",  Turtle.ns_prov.ToString() },
						{ "obo2",  Turtle.ns_obo2.ToString() },
						{ "cheminf",  Turtle.ns_cheminf.ToString() },
					};

		//Vocabularies - this is the vocabularies we are referencing in the referenced files, not just those used in the VoID file.
		private List<string> m_vocabularies = new List<string>
				{
					Turtle.ns_dcterms.ToString(),
					Turtle.ns_dctype.ToString(),
					Turtle.ns_pav.ToString(),
					Turtle.ns_void.ToString(),
					Turtle.ns_rdf.ToString(),
					Turtle.ns_xsd.ToString(),
					Turtle.ns_skos.ToString(),
					Turtle.ns_dul.ToString(),
					Turtle.ns_foaf.ToString(),
					Turtle.ns_obo.ToString(),
					Turtle.ns_obo_ro.ToString(),
					Turtle.ns_cheminf.ToString(),
					Turtle.ns_qudt.ToString(),
					Turtle.ns_owl.ToString(),
					Turtle.ns_obo2.ToString(),
				};

		/// <summary>
		/// Exports the Void linkset.
		/// </summary>
		/// <param name="Exp">The DataExport object</param>
		/// <param name="Encoding">The output encoding for this export</param>
		public override void Export(IDataExport Exp, Encoding Encoding)
		{
			//Generate the file name.
			FileName = "void_" + Exp.DateTimeExported.ToString("yyyy-MM-dd") + ".ttl";
			base.Export(Exp, Encoding);

			Graph g = new Graph();
			List<string> void_metadata = new List<string>();
			List<string> void_dataset = new List<string>();

			//Modified/updated dates.
			string dataset_last_updated_on = Exp.DateTimeExported.ToString(Turtle.DATE_TIME_FORMAT);
			string provenance_modified_on = dataset_last_updated_on;

			//Put it into the root directory for the export date.
			FtpTargetDirectory = Exp.DateTimeExported.ToString("yyyyMMdd");

			//Get the void uri.
			string void_uri = String.Format("{0}/{1}/{2}", Turtle.FTP_PREFIX, FtpTargetDirectory, FileName);
			Uri ns_void_file = new Uri(void_uri);

			//Add the prefix for this void file with the empty prefix.
			m_void_prefixes.Add("", String.Format("{0}#", ns_void_file));

			//Uri to the chemspiderDataset.
			Uri uri_chemspider_dataset = new Uri(ns_void_file.ToString() + "#" + Turtle.DATASET_LABEL.ToLower() + "Dataset");

			//Populate the graph.

			//Add the namespaces/prefixes we shall refer to.
			foreach (KeyValuePair<string, string> prefix in m_void_prefixes)
			{
				Uri uriPrefix = new Uri(prefix.Value);
				g.NamespaceMap.AddNamespace(prefix.Key, uriPrefix);
			}

			//DatasetDescription info.
			Turtle.AssertTriple(g, ns_void_file, Turtle.has_type, new Uri(Turtle.ns_void.ToString() + "DatasetDescription"));
			Turtle.AssertTriple(g, ns_void_file, Turtle.dcterms_title, VOID_TITLE);
			Turtle.AssertTriple(g, ns_void_file, Turtle.dcterms_description, DATASET_DESCRIPTION);
			Turtle.AssertTriple(g, ns_void_file, Turtle.pav_createdBy, m_home_page);
			Turtle.AssertTriple(g, ns_void_file, Turtle.pav_createdOn, DATASET_CREATED_ON, Turtle.xsd_dateTime);
			Turtle.AssertTriple(g, ns_void_file, Turtle.pav_lastUpdateOn, dataset_last_updated_on, Turtle.xsd_dateTime);
			Turtle.AssertTriple(g, ns_void_file, Turtle.foaf_primaryTopic, uri_chemspider_dataset);

			//Get the date of the last export so we can reference the previous version void file.
			Uri uri_previous_version = getPreviousVoidVersion(Exp, _dsn_exp_ids.Values.Min());
			if (uri_previous_version != null)
				Turtle.AssertTriple(g, ns_void_file, Turtle.pav_previousVersion, uri_previous_version);

			//ChemSpider Dataset info.
			Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.has_type, Turtle.void_DataSet);
			Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.dcterms_title, DATASET_TITLE);
			Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.dcterms_description, DATASET_DESCRIPTION);
			Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.foaf_homepage, new Uri(Turtle.RDF_URI));

			/* TODO: Pages that it features on - removed for the time being.
			Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.foaf_page, m_page_data_hub);
			Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.foaf_page, m_page_miriam);
			 */

			Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.dcterms_license, m_license);
			Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.void_uriSpace, Turtle.RDF_URI, Turtle.xsd_string);

			//Provenance.
			Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.dcterms_publisher, m_home_page);
			Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.dcterms_created, PROVENANCE_CREATED_ON, Turtle.xsd_dateTime);
			Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.dcterms_modified, provenance_modified_on, Turtle.xsd_dateTime);

			//Subsets.
			string dsn_list = string.Join(",", _dsn_exp_ids.Keys);
			foreach (string s in _dsn_exp_ids.Keys.ToList())
			{
				Uri subset = new Uri(ns_void_file.ToString() + String.Format("#{0}-{1}", Turtle.DATASET_LABEL.ToLower(), s.Replace(" ", "_").ToLower()));
				Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.void_subset, subset);
			}

			//Vocabularies, topics, resources.
			m_vocabularies.ForEach(v => Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.void_vocabulary, new Uri(v)));
			Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.dcterms_subject, m_vocabulary_subject);
			Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.void_exampleResource, m_example_resource);

			//Update Frequency.
			Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.voag_frequencyOfChange, Turtle.freq_monthly);

			//Technical features.
			Turtle.AssertTriple(g, uri_chemspider_dataset, Turtle.void_feature, Turtle.ns_turtle);

			//Now write details of each subset.
			foreach (KeyValuePair<string, int> dsn in _dsn_exp_ids)
			{
				//Map the data source name to the dsn_id.
				int dsn_id = Turtle.getDSNId(Exp, dsn.Key);
				getDSNVoidSubsetInfo(Exp, g, dsn.Key, dsn.Key.Replace(" ", "_").ToLower(), dsn_id, Turtle.useSkosRelatedMatchForDSN(dsn.Key), uri_chemspider_dataset, ns_void_file, uri_previous_version, dsn.Value);
			}

			//Write the output to file.
			using (TextWriter w = new StreamWriter(FileName, true, Encoding))
			{
				TurtleWriter turtleWriter = new TurtleWriter();
				turtleWriter.Save(g, w);
			}
		}

		/// <summary>
		/// Returns the uri of the void of the previous export for this dsn_id.
		/// </summary>
		/// <param name="Exp">The Data Export.</param>
		/// <param name="export_id">The lowest Export Id in this batch of exports.</param>
		/// <returns>The URI of the previous export.</returns>
		private Uri getPreviousVoidVersion(IDataExport Exp, int export_id)
		{
			using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
			{
				DateTime now = DateTime.Now;
				string previous = conn.ExecuteScalar<string>("SELECT TOP 1 date_exported FROM rdf_exports WHERE exp_id < @exp_id AND date_exported < @today ORDER BY exp_id DESC", new { exp_id = export_id, today = now.ToString("yyyyMMdd") });
				if (previous != null)
				{
					DateTime date_of_previous_version = DateTime.Parse(previous);
					//Generate the file name.
					string file_name = "void_" + date_of_previous_version.ToString("yyyy-MM-dd") + ".ttl";
					//Put it into the root directory for the export date.
					string target_directory = date_of_previous_version.ToString("yyyyMMdd");
					//Return the previous version void uri.
					return new Uri(String.Format("{0}/{1}/{2}", Turtle.FTP_PREFIX, target_directory, file_name));
				}
				return null;
			}
		}

		/// <summary>
		/// Returns the number of triples for a particular dsn linkset, for this export.
		/// </summary>
		/// <param name="Exp">The Data Export.</param>
		/// <param name="exp_id">The export id we are exporting</param>
		/// <param name="table">The table name</param>
		/// <param name="where_clause">The where clause to further filter the record selection</param>
		/// <returns>The number of Triples</returns>
		private int getExportNoOfTriples(IDataExport Exp, int exp_id, string table, string where_clause)
		{
			int no_of_triples = 0;
			using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
			{
				//Get the number of reference triples 1 triple per reference.
				no_of_triples = conn.ExecuteScalar<int>(String.Format("select count(*) from {0} where exp_id = @id{1}", table, where_clause != string.Empty ? string.Format(" AND {0}", where_clause) : string.Empty), new { id = exp_id });
			}
			return no_of_triples;
		}

		/// <summary>
		/// Returns the number of triples for a particular dsn linkset, for this export.
		/// </summary>
		/// <param name="Exp">The Data Export.</param>
		/// <param name="exp_id">The export id we are exporting</param>
		/// <param name="dsn">The data source</param>
		/// <param name="use_related_match">Whether we are using related and exact match for this data source</param>
		/// <param name="is_related">Whether this is actually a related match export</param>
		/// <returns>The number of triples</returns>
		private int getDSNNoOfTriples(IDataExport Exp, int exp_id, string dsn, bool use_related_match, bool is_related)
		{
			int no_of_triples = 0;
			using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
			{
				if (!use_related_match)
				{
					//Get the number of reference triples 1 triple per reference.
					no_of_triples = conn.ExecuteScalar<int>("select count(*) from rdf_exported_ext_refs_snapshot where exp_id = @id", new { id = exp_id });
				}
				else
				{
					conn.ExecuteReader(String.Format("select ext_id from rdf_exported_ext_refs_snapshot where exp_id = {0}", exp_id),
						r =>
						{
							if (is_related)
							{
								if (Turtle.useSkosRelatedMatchForId(dsn, r["ext_id"].ToString()))
									no_of_triples++;
							}
							else
							{
								if (Turtle.useSkosExactMatchForId(dsn, r["ext_id"].ToString()))
									no_of_triples++;
							}
						});
				}
			}
			return no_of_triples;
		}

		/// <summary>
		/// Where we link to another dataset which has no rdf we must populate the information here.
		/// </summary>
		/// <param name="exp">The data export</param>
		/// <param name="g">The graph</param>
		/// <param name="dsn">The data source</param>
		/// <param name="dsn_id">The data source id</param>
		/// <param name="ns_void_file">The void file uri</param>
		private void getDSNDatasetInfo(IDataExport exp, Graph g, string dsn, string dsn_label, int dsn_id, Uri ns_void_file, Uri dataset_void_uri)
		{
			using (SqlConnection conn = new SqlConnection(exp.DBConnection))
			{
				Uri dsn_dataset = new Uri(ns_void_file.ToString() + String.Format("#{0}", dsn_label));

				//OLD Chemspider SQL - need to work out what to do with this.
				//                conn.ExecuteReader(String.Format(@"SELECT dt.date date_published, u.username, ds.ds_url, ds.name, ds.company_name, d.date_submitted, d.dataset_version, d.dataset_created, d.dataset_imported_from, d.num_records, ds.dataset_license, ds.dataset_uri_space, ds.dataset_void_uri
				//	                                                FROM ChemUsers..data_sources ds
				//	                                                    LEFT JOIN depositions d
				//		                                                    ON ds.dsn_id = d.dsn_id
				//			                                                    AND d.dep_id = (SELECT MAX(dep_id) FROM depositions WHERE dsn_id = ds.dsn_id AND deleted_yn = 0 AND status = 'P')
				//	                                                    LEFT JOIN v_users u 
				//		                                                    ON u.usr_id = d.usr_id
				//	                                                    LEFT JOIN depositions_track dt
				//		                                                    ON dt.dep_id = d.dep_id
				//			                                                    AND dt.dtid = (SELECT MAX(dtid) FROM depositions_track WHERE dep_id = d.dep_id AND status = 'P')
				//	                                                WHERE ds.dsn_id = {0}", dsn_id),

				//Retrieve the Provenance and Versioning information.
				conn.ExecuteReader(String.Format(@"SELECT v.depositeDate date_published
													, d.dsn_url ds_url
													, d.dsn_name name
													, v.dataset_license_url dataset_license
													, '' dataset_uri_space
													, v.version_name dataset_version
													, v.downloadURL dataset_imported_from
													, v.depositedBy username
														FROM datasources d
															LEFT JOIN data_versions v ON v.dsn_id = d.dsn_id WHERE d.dsn_id = {0}", dsn_id),
				r =>
				{
					DateTime imported_on;
					string formatted_imported_on = string.Empty;
					if (DateTime.TryParse(r["date_published"].ToString(), out imported_on))
						formatted_imported_on = imported_on.ToString(Turtle.DATE_TIME_FORMAT);

					Turtle.AssertTriple(g, dsn_dataset, Turtle.has_type, Turtle.dctype_Dataset);

					if (r["ds_url"].ToString() != string.Empty)
						Turtle.AssertTriple(g, dsn_dataset, Turtle.foaf_homepage, new Uri(r["ds_url"].ToString()));

					//Only include this information if we are linking to a dataset with no void uri.
					if (dataset_void_uri == null)
					{
						Turtle.AssertTriple(g, dsn_dataset, Turtle.dcterms_title, String.Format("The {0} Dataset", r["name"]));

						if (r["dataset_license"].ToString() != string.Empty)
							Turtle.AssertTriple(g, dsn_dataset, Turtle.dcterms_license, new Uri(r["dataset_license"].ToString()));

						if (r["dataset_uri_space"].ToString() != string.Empty)
							Turtle.AssertTriple(g, dsn_dataset, Turtle.void_uriSpace, r["dataset_uri_space"].ToString(), Turtle.xsd_string);

						if (r["ds_url"].ToString() != string.Empty)
							Turtle.AssertTriple(g, dsn_dataset, Turtle.dcterms_publisher, new Uri(r["ds_url"].ToString()));
					}

					if (r["dataset_version"].ToString() != string.Empty)
						Turtle.AssertTriple(g, dsn_dataset, Turtle.pav_version, r["dataset_version"].ToString(), Turtle.xsd_string);

					if (r["dataset_imported_from"].ToString() != string.Empty)
						Turtle.AssertTriple(g, dsn_dataset, Turtle.pav_retrievedFrom, new Uri(r["dataset_imported_from"].ToString()));

					if (r["username"].ToString() != string.Empty)
						Turtle.AssertTriple(g, dsn_dataset, Turtle.pav_retrievedBy, new Uri(String.Format(USER_PROFILE_URL_PREFIX, r["username"])));

					if (formatted_imported_on != string.Empty)
						Turtle.AssertTriple(g, dsn_dataset, Turtle.pav_retrievedOn, formatted_imported_on, Turtle.xsd_dateTime);
				});
			}
		}

		/// <summary>
		/// Returns metadata relating to the linkset subsets.
		/// </summary>
		/// <param name="exp">The DataExport</param>
		/// <param name="g">The graph</param>
		/// <param name="subset"></param>
		/// <param name="dsn">The data source name</param>
		/// <param name="dsn_label">The data source label</param>
		/// <param name="predicate_label">The predicate label</param>
		/// <param name="predicate_description">A description of the predicate</param>
		/// <param name="linkset_label">The label for the linkset</param>
		/// <param name="linkset_description">The description of the linkset</param>
		/// <param name="dsn_id">The data source id</param>
		/// <param name="ns_void_file">The void file namespace</param>
		/// <param name="uri_parent_dataset">The uri of the parent dataset</param>
		/// <param name="skos_predicate">The skos predicate used to describe the linkset</param>
		/// <param name="is_parent_child">Whether the linkset is a parent_child linkset</param>
		/// <param name="is_chemspider">Whether the linkset links to ChemSpider records</param>
		/// <param name="no_of_triples">The number of triples in the linkset</param>
		/// <param name="previous_version">The previous version of the linkset</param>
		/// <param name="dataset_void_uri">The void uri of the dataset</param>
		/// <param name="expresses">The predicate describing how the predicate is expressed</param>
		private void getDSNVoidSubsetMatchInfo(IDataExport exp, Graph g, Uri subset
											   , string dsn, string dsn_label, string predicate_label, string predicate_description, string linkset_label
											   , string linkset_description, int dsn_id, Uri ns_void_file, Uri uri_parent_dataset
											   , Turtle.SkosPredicate skos_predicate, bool is_parent_child, bool is_chemspider, int no_of_triples
											   , Uri previous_version, Uri dataset_void_uri, Uri expresses)
		{
			string linkset_title;
			//string linkset_description;
			string match_description;
			string data_dump_url;
			string file_part;

			Uri dsn_match;
			Uri objects_target;
			Uri subjects_target;

			file_part = Turtle.SkosPredicateToString(skos_predicate).Replace("Match", "").ToLower();
			match_description = String.Format("{0}Match", file_part);

			if (is_parent_child)
			{
				match_description = String.Format("{0}_{1}_{2}", linkset_label, predicate_label, match_description);
				file_part = String.Format("{0}_{1}_{2}", file_part, linkset_label, predicate_label);
				linkset_title = String.Format("{0} {1} {2} Parent-Child Linkset", Turtle.DATASET_LABEL, dsn, predicate_description);
				linkset_description = String.Format("{0}: {1}", predicate_description, linkset_description);
			}
			else if (is_chemspider)
			{
				match_description = String.Format("{0}_{1}", linkset_label, match_description);
				file_part = String.Format("{0}_{1}", file_part, linkset_label);
				linkset_title = String.Format("{0} {1} OPS-ChemSpider Linkset", Turtle.DATASET_LABEL, dsn);
				linkset_description = String.Format("{0} linkset of compounds deposited into {1} that {2} ChemSpider compounds.", dsn, Turtle.DATASET_LABEL, Turtle.SkosPredicateToDescription(skos_predicate));
			}
			else
			{
				linkset_title = String.Format("{0} {1} {2} Linkset", Turtle.DATASET_LABEL, dsn, String.Format("{0} {1}", file_part, "match"));
				linkset_description = String.Format("{0} linkset of compounds deposited into {1} that {2} {1} compounds.", dsn, Turtle.DATASET_LABEL, Turtle.SkosPredicateToDescription(skos_predicate));
			}

			dsn_match = new Uri(ns_void_file.ToString() + String.Format("#{0}_{1}", dsn_label, match_description));
			Turtle.AssertTriple(g, subset, Turtle.void_subset, dsn_match);
			Turtle.AssertTriple(g, dsn_match, Turtle.dcterms_title, linkset_title);
			Turtle.AssertTriple(g, dsn_match, Turtle.dcterms_description, linkset_description);
			Turtle.AssertTriple(g, dsn_match, Turtle.has_type, Turtle.void_Linkset);
			Turtle.AssertTriple(g, dsn_match, Turtle.dcterms_license, m_chemspider_license);
			data_dump_url = String.Format("{0}/{1}/{2}/LINKSET_{3}_{2}{1}.ttl.gz", Turtle.FTP_PREFIX, exp.DateTimeExported.ToString("yyyyMMdd"), dsn_label.ToUpper(), file_part.ToUpper());

			if (is_parent_child)
			{
				//Parent-Child Linksets link OPS compounds together.
				objects_target = uri_parent_dataset;
				subjects_target = uri_parent_dataset;
			}
			else if (is_chemspider)
			{
				//OPS-ChemSpider Linksets link OPS compounds to ChemSpider compounds.
				objects_target = new Uri(Turtle.CHEMSPIDER_RDF_URI + "/void.rdf");
				subjects_target = uri_parent_dataset;
			}
			else
			{
				//Data source Linksets.
				objects_target = new Uri(ns_void_file.ToString() + String.Format("#{0}", dsn_label));
				subjects_target = new Uri(ns_void_file.ToString() + String.Format("#{0}-{1}", Turtle.DATASET_LABEL.ToLower(), dsn_label));

				//Use the void file for the dataset if there is one.
				if (dataset_void_uri != null)
					objects_target = dataset_void_uri;
			}

			//Link Information, subjectsTarget, objectsTarget and linkPredicate.
			Turtle.AssertTriple(g, dsn_match, Turtle.void_objectsTarget, objects_target);
			Turtle.AssertTriple(g, dsn_match, Turtle.void_subjectsTarget, subjects_target);

			//void:linkPredicate
			Turtle.AssertTriple(g, dsn_match, Turtle.void_linkPredicate, Turtle.SkosPredicateToUri(skos_predicate));

			//How can we express the relationship between the matches.
			Turtle.AssertTriple(g, dsn_match, Turtle.dul_expresses, expresses);

			//Linkset Provenance.
			Turtle.AssertTriple(g, dsn_match, Turtle.pav_authoredBy, m_provenance_authored_by);
			Turtle.AssertTriple(g, dsn_match, Turtle.pav_authoredOn, PROVENANCE_AUTHORED_ON, Turtle.xsd_dateTime);
			Turtle.AssertTriple(g, dsn_match, Turtle.pav_createdWith, m_home_page);
			Turtle.AssertTriple(g, dsn_match, Turtle.pav_createdBy, m_provenance_authored_by);
			Turtle.AssertTriple(g, dsn_match, Turtle.pav_createdOn, exp.DateTimeExported.ToString(Turtle.DATE_TIME_FORMAT), Turtle.xsd_dateTime);

			//The previous version of this void information.
			if (previous_version != null)
				Turtle.AssertTriple(g, dsn_match, Turtle.pav_previousVersion, new Uri(previous_version.ToString() + String.Format("#{0}_{1}", dsn_label, match_description)));

			//Linkset statistics - no of triples.
			Turtle.AssertTriple(g, dsn_match, Turtle.void_triples, no_of_triples.ToString(), Turtle.xsd_integer);

			//Dataset access.
			Turtle.AssertTriple(g, dsn_match, Turtle.void_dataDump, new Uri(data_dump_url));

			//Add the Dataset info.
			getDSNDatasetInfo(exp, g, dsn, dsn_label, dsn_id, ns_void_file, dataset_void_uri);
		}

		/// <summary>
		/// Adds details of a dsn subset to the Graph.
		/// </summary>
		/// <param name="g">The Graph</param>
		/// <param name="dsn">The data source</param>
		/// /// <param name="dsn">The data source id</param>
		/// <param name="use_skos_related">Whether this is a skos_related predicate</param>
		/// <param name="uri_chemspider_dataset">Uri for the chemspider dataset</param>
		/// <param name="ns_void_file">Uri for the void file</param>
		/// <param name="export_id">Export Id</param>
		private void getDSNVoidSubsetInfo(IDataExport exp, Graph g, string dsn, string dsn_label, int dsn_id, bool use_skos_related, Uri uri_chemspider_dataset, Uri ns_void_file, Uri uri_previous_version, int export_id)
		{
			using (SqlConnection conn = new SqlConnection(exp.DBConnection))
			{
				Uri subset = new Uri(ns_void_file.ToString() + String.Format("#{0}-{1}", Turtle.DATASET_LABEL.ToLower(), dsn_label));
				string formatted_imported_on = String.Empty;

				//Get the case-sensitivity right.
				dsn = Turtle.getDSN(exp, dsn);

				//TODO: This is the old ChemSpider SQL - need to sort this out - so we can have different SQL for multiple databases.
				//                conn.ExecuteReader(String.Format(@"SELECT dt.date date_published, u.username, ds.ds_url, ds.name, ds.company_name, d.date_submitted, d.dataset_version, d.dataset_created, d.dataset_imported_from, d.num_records, ds.dataset_license, ds.dataset_uri_space, ds.dataset_void_uri
				//	                                                FROM ChemUsers..data_sources ds
				//	                                                    LEFT JOIN depositions d
				//		                                                    ON ds.dsn_id = d.dsn_id
				//			                                                    AND d.dep_id = (SELECT MAX(dep_id) FROM depositions WHERE dsn_id = ds.dsn_id AND deleted_yn = 0 AND status = 'P')
				//	                                                    LEFT JOIN v_users u 
				//		                                                    ON u.usr_id = d.usr_id
				//	                                                    LEFT JOIN depositions_track dt
				//		                                                    ON dt.dep_id = d.dep_id
				//			                                                    AND dt.dtid = (SELECT MAX(dtid) FROM depositions_track WHERE dep_id = d.dep_id AND status = 'P')
				//	                                                WHERE ds.dsn_id = {0}", dsn_id),

				//Retrieve the Provenance and Versioning information.
				conn.ExecuteReader(String.Format(@"SELECT v.depositeDate date_published
																		, v.depositedBy username
																		, d.dsn_url ds_url
																		, d.dsn_name name
																		, d.dsn_name company_name
																		, v.depositeDate date_submitted
																		, v.version_name dataset_version
																		, v.dataset_create_date dataset_created
																		, v.downloadURL dataset_imported_from
																		, '' num_records
																		, v.dataset_license_url dataset_license
																		, d.dsn_uri dataset_uri_space
																		, dataset_void_url dataset_void_uri
																	FROM datasources d
																		LEFT JOIN data_versions v ON v.dsn_id = d.dsn_id WHERE d.dsn_id = {0}", dsn_id),
				r =>
				{
					Turtle.AssertTriple(g, subset, Turtle.has_type, Turtle.void_DataSet);

					if (r["name"].ToString() != string.Empty)
					{
						Turtle.AssertTriple(g, subset, Turtle.dcterms_title, String.Format("{0} {1} Subset", Turtle.DATASET_LABEL, r["name"]));
						Turtle.AssertTriple(g, subset, Turtle.dcterms_description, String.Format("The subset of {0} that contains {1} data.", Turtle.DATASET_LABEL, r["name"]));
					}
					if (r["ds_url"].ToString() != string.Empty)
						Turtle.AssertTriple(g, subset, Turtle.foaf_page, new Uri(r["ds_url"].ToString()));

					//Add the links to subsets

					//Provenance.
					Turtle.AssertTriple(g, subset, Turtle.prov_wasDerivedFrom, new Uri(ns_void_file.ToString() + String.Format("#{0}", dsn_label)));

					//Add the locations of the synonyms, properties and issues files.
					Uri synonyms_uri = new Uri(String.Format("{0}/{1}/SYNONYMS_{2}{3}.ttl.gz", Turtle.FTP_PREFIX, exp.DateTimeExported.ToString("yyyyMMdd"), dsn_label.ToUpper(), exp.DateTimeExported.ToString("yyyyMMdd")));
					Uri properties_uri = new Uri(String.Format("{0}/{1}/PROPERTIES_{2}{3}.ttl.gz", Turtle.FTP_PREFIX, exp.DateTimeExported.ToString("yyyyMMdd"), dsn_label.ToUpper(), exp.DateTimeExported.ToString("yyyyMMdd")));
					Uri issues_uri = new Uri(String.Format("{0}/{1}/ISSUES_{2}{3}.ttl.gz", Turtle.FTP_PREFIX, exp.DateTimeExported.ToString("yyyyMMdd"), dsn_label.ToUpper(), exp.DateTimeExported.ToString("yyyyMMdd")));

					Turtle.AssertTriple(g, subset, Turtle.void_dataDump, synonyms_uri);
					Turtle.AssertTriple(g, subset, Turtle.void_dataDump, properties_uri);
					Turtle.AssertTriple(g, subset, Turtle.void_dataDump, issues_uri);

					Uri uri_dataset_void = null;
					if (r["dataset_void_uri"].ToString() != string.Empty)
						uri_dataset_void = new Uri(r["dataset_void_uri"].ToString());

					//All dsns have exactMatch subsets.
					getDSNVoidSubsetMatchInfo(exp, g, subset, dsn, dsn_label, string.Empty, string.Empty, string.Empty, string.Empty, dsn_id, ns_void_file, uri_chemspider_dataset, Turtle.SkosPredicate.EXACT_MATCH, false, false, getDSNNoOfTriples(exp, export_id, dsn, use_skos_related, false), uri_previous_version, uri_dataset_void, getExpressesUri(dsn, use_skos_related));

					//Some dsns have relatedMatch subsets.
					if (use_skos_related)
						getDSNVoidSubsetMatchInfo(exp, g, subset, dsn, dsn_label, string.Empty, string.Empty, string.Empty, string.Empty, dsn_id, ns_void_file, uri_chemspider_dataset, Turtle.SkosPredicate.RELATED_MATCH, false, false, getDSNNoOfTriples(exp, export_id, dsn, use_skos_related, use_skos_related), uri_previous_version, uri_dataset_void, getExpressesUri(dsn, use_skos_related));

					//Parent-child subsets.
					using (SqlConnection conn1 = new SqlConnection(exp.DBConnection))
					{
						conn1.ExecuteReader("SELECT * FROM parent_types WHERE rdf_predicate IS NOT NULL",
											p =>
											{
												//Get the parentChild information.
												getDSNVoidSubsetMatchInfo(exp
																		  , g
																		  , subset
																		  , dsn
																		  , dsn_label
																		  , p["parent_name"].ToString().Replace(" ", "_").Replace(".", "_").ToLower()
																		  , p["parent_name"].ToString()
																		  , "parent_child"
																		  , p["parent_description"].ToString()
																		  , dsn_id
																		  , ns_void_file
																		  , uri_chemspider_dataset
																		  , Turtle.SkosStringToPredicate(p["rdf_relation"].ToString())
																		  , true
																		  , false
																		  , getExportNoOfTriples(exp
																								, export_id
																								, "rdf_exported_parent_child_snapshot"
																								, string.Format("parent_id = {0}", p["parent_id"].ToString()))
																		  , uri_previous_version
																		  , uri_dataset_void
																		  , new Uri(p["rdf_predicate"].ToString()));
											});
					}

					//ChemSpider-OPS exactMatch subset.
					getDSNVoidSubsetMatchInfo(exp, g, subset, dsn, dsn_label, string.Empty, string.Empty, "ops_chemspider", string.Empty, dsn_id, ns_void_file, uri_chemspider_dataset, Turtle.SkosPredicate.EXACT_MATCH, false, true, getDSNNoOfTriples(exp, export_id, dsn, use_skos_related, false), uri_previous_version, uri_dataset_void, getExpressesUri(dsn, use_skos_related));
				});
			}
		}

		//Hard-coded list of what the linkset expresses.
		private Uri getExpressesUri(string dsn, bool use_skos_related)
		{
			switch (dsn.ToLower())
			{
				case "pdb":
					return use_skos_related ? Turtle.dul_expresses_ligates : Turtle.dul_expresses_inchi;
				default:
					//Unsupported dsn.
					return Turtle.dul_expresses_inchi;
			}
		}
	}

	/// <summary>
	/// Used for generating export files for the Related Linkset RDF File.
	/// </summary>
	public class RelatedLinksetDataExportFile : DataSourceDataExportFile
	{
		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public RelatedLinksetDataExportFile(string dsn) : base(dsn) { }

		/// <summary>
		/// Exports an Rdf Linkset containing Exact Matches.
		/// </summary>
		public override void Export(IDataExport Exp, Encoding Encoding)
		{
			FileName = String.Format("LINKSET_RELATED_{0}{1}.ttl", DsnLabel, Exp.DateTimeExported.ToString("yyyyMMdd"));
			base.Export(Exp, Encoding);

			Dictionary<string, string> prefixes = new Dictionary<string, string>(Turtle.linkset_prefixes);
			string db_alias = string.Empty;
			string dsn_prefix = string.Empty;
			bool use_full_uri = false;

			//Perform the full export of related matches.
			using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
			{
				//Write the linkset containing skos:relatedMatch References.
				using (TextWriter w = new StreamWriter(FileName, false, Encoding))
				{
					//Add the dsn prefix if not already added in the above list of prefixes and we must not use the full uri.
					dsn_prefix = getDSNPrefix(Exp, _dsn, out db_alias, out use_full_uri);
					if (!use_full_uri)
					{
						KeyValuePair<string, string> kvp = Turtle.linkset_prefixes.Where(l => (l.Value == dsn_prefix)).FirstOrDefault();
						if (kvp.Value != null)
							//We have already added this item so get the alias.
							db_alias = kvp.Key;
						else
							prefixes.Add(db_alias, dsn_prefix);
					}

					//Get the linkset uri.
					string linkset_uri = String.Format("{0}{1}/{2}", Turtle.FTP_PREFIX, FtpTargetDirectory, FileName);

					//Add the prefix for this void file with the empty prefix.
					prefixes.Add("", String.Format("{0}#", linkset_uri));

					//Add the base uri prefix.
					prefixes.Add(Turtle.RDF_URI_PREFIX.ToLower(), Turtle.RDF_URI + "/");  //Re-use the prefix as the alias.

					//Output the prefixes for the linkset.
					foreach (KeyValuePair<string, string> p in prefixes)
						w.WriteLine(String.Format("@prefix {0}: <{1}> .", p.Key, p.Value));

					//Add the predicate to describe which subset this dataset is in.
					string subset_object = string.Format("{0}_{1}", _dsn.ToLower(), Turtle.SKOS_RELATED_MATCH); //E.g. :chebi_relatedMatch
					w.WriteLine(Turtle.getDSNInDatasetPredicate(_dsn, subset_object, linkset_uri));

					//References.
					conn.ExecuteReader(String.Format(@"SELECT r.* FROM rdf_exported_ext_refs_snapshot r
															WHERE r.exp_id = {0}", Exp.ExportId),
						r =>
						{
							if (Turtle.useSkosRelatedMatchForId(_dsn, r["ext_id"].ToString()))
								w.WriteLine("{0}:{1}{2} skos:relatedMatch {3} ."
									, Turtle.RDF_URI_PREFIX.ToLower()
									, Turtle.RDF_URI_PREFIX
									, r["cmp_id"]
									, Turtle.getDSNUri(_dsn, r["ext_id"].ToString(), db_alias, dsn_prefix, use_full_uri));
						});
				}
			}
		}
	}

	#endregion

	#region SynonymsDataExportFile

	/// <summary>
	/// Used for generating export files for the Synonyms RDF file.
	/// </summary>
	public class SynonymsDataExportFile : DataSourceDataExportFile
	{
		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public SynonymsDataExportFile(string dsn) : base(dsn) { }

		/// <summary>
		/// Exports Rdf Synonyms the full export.
		/// </summary>
		public override void Export(IDataExport Exp, Encoding Encoding)
		{
			//Set the filename.
			FileName = String.Format("SYNONYMS_{0}{1}.ttl", DsnLabel, Exp.DateTimeExported.ToString("yyyyMMdd"));
			base.Export(Exp, Encoding);

			//Get a copy of the synonyms prefixes.
			Dictionary<string, string> prefixes = new Dictionary<string, string>(Turtle.synonyms_prefixes);

			using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
			{
				//Write the Identifiers and Synonyms.
				using (TextWriter w = new StreamWriter(FileName, false, Encoding))
				{
					//Get the synonyms file uri.
					string synonyms_uri = String.Format("{0}/{1}/{2}", Turtle.FTP_PREFIX, FtpTargetDirectory, FileName);

					//Add the base uri prefix.
					prefixes.Add(Turtle.RDF_URI_PREFIX.ToLower(), Turtle.RDF_URI + "/");  //Re-use the prefix as the alias.

					//Add the prefix for this void file with the empty prefix.
					prefixes.Add("", String.Format("{0}#", synonyms_uri));

					//Output the prefixes
					foreach (KeyValuePair<string, string> p in prefixes)
						w.WriteLine(String.Format("@prefix {0}: <{1}> .", p.Key, p.Value));

					//Add the predicate to describe which dataset this is in.
					string dataset_object = string.Format("{0}-{1}", Turtle.DATASET_LABEL.ToLower(), DsnLabel.ToLower()); //E.g. :chemspider-chebi
					w.WriteLine(Turtle.getDSNInDatasetPredicate(_dsn, dataset_object, synonyms_uri));

					//Add the Annotation Property type assignments.
					string annotation_prop = "cheminf:{0} a owl:AnnotationProperty . # {1}";
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_validated_synonym, "Validated Synonym"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_unvalidated_synonym, "Unvalidated Synonym"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_validated_dbid, "Validated Database Identifier"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_unvalidated_dbid, "Unvalidated Database Identifier"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_std_inchi_104, "Standard InChI 1.04"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_std_inchikey_104, "Standard InChIKey 1.04"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_csid, "ChemSpider ID"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_title, "ChemSpider title"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_smiles, "SMILES"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_mf, "Molecular Formula"));

					//Compounds
					conn.ExecuteReader(String.Format(@"SELECT r.* FROM rdf_exported_identifiers_snapshot r
														WHERE r.exp_id = {0}", Exp.ExportId),
						r =>
						{
							string line = String.Format("{0}:{1}{2}"
														, Turtle.RDF_URI_PREFIX.ToLower()   //0
														, Turtle.RDF_URI_PREFIX             //1
														, r["csid"]                         //2
														);

							//If we have a CSID then add the predicate.
							if (!String.IsNullOrEmpty(r["cmp_id"].ToString()))
								line += String.Format(" {0}:{1} {2};"
														, "cheminf"             //0
														, Turtle.cheminf_csid   //1
														, r["cmp_id"]           //2
														);

							//Add the InChI, InChIKey, SMILES and Molecular Formula.
							line += String.Format(" {0}:{1} \"{2}\"; {0}:{3} \"{4}\"; {0}:{5} \"{6}\"; {0}:{7} \"{8}\""
													, "cheminf"                                         //0
													, Turtle.cheminf_std_inchi_104                      //1
													, r["inchi"]                                        //2
													, Turtle.cheminf_std_inchikey_104                   //3
													, r["inchi_key"]                                    //4
													, Turtle.cheminf_smiles                             //5
													, Turtle.RdfEncodedString(r["SMILES"] as string)    //6
													, Turtle.cheminf_mf                                 //7
													, Turtle.FormatMolecularFormula(r["molecular_formula"] as string)  //8
													);

							//If we have a ChemSpider Title then add the predicate.
							if (!String.IsNullOrEmpty(r["name"] as string))
								line += String.Format("; {0}:{1} \"{2}\"@en"
													, "cheminf"                                         //0
													, Turtle.cheminf_title                              //1
													, Turtle.RdfEncodedString(r["name"] as string)      //2
													);

							//Add the dot at the end.
							line += " .";

							//Write the line.
							w.WriteLine(line);
						});

					//Synonyms and DBIDs (validated or unvalidated).
					conn.ExecuteReader(String.Format(@"SELECT r.* FROM rdf_exported_synonyms_snapshot r
														WHERE r.exp_id = {0}", Exp.ExportId),
						r =>
						{
							w.WriteLine("<{0}/{1}{2}> cheminf:{3} \"{4}\"@{5} ."
														, Turtle.RDF_URI
														, Turtle.RDF_URI_PREFIX
														, r["cmp_id"]
														, (bool)r["is_dbid"]
															? (bool)r["validated"] ? Turtle.cheminf_validated_dbid : Turtle.cheminf_unvalidated_dbid           //Dbids
															: (bool)r["validated"] ? Turtle.cheminf_validated_synonym : Turtle.cheminf_unvalidated_synonym     //Synonyms
														, Turtle.RdfEncodedString(r["synonym"].ToString())
														, r["lang_id"]);
						});
				}
			}
		}
	}

	#endregion

	#region PropertiesDataExportFile

	/// <summary>
	/// Export file for Properties.
	/// Used for generating Rdf files, containing Properties.
	/// </summary>
	public class PropertiesDataExportFile : DataSourceDataExportFile
	{
		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public PropertiesDataExportFile(string dsn) : base(dsn) { }

		/// <summary>
		/// Exports Rdf Synonyms the full export.
		/// </summary>
		public override void Export(IDataExport Exp, Encoding Encoding)
		{
			//Set the file name.
			FileName = String.Format("PROPERTIES_{0}{1}.ttl", DsnLabel, Exp.DateTimeExported.ToString("yyyyMMdd"));
			base.Export(Exp, Encoding);

			//Get a copy of the properties prefixes.
			Dictionary<string, string> prefixes = new Dictionary<string, string>(Turtle.props_prefixes);

			using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
			{
				//Write the properties.
				using (TextWriter w = new StreamWriter(FileName, false, Encoding))
				{
					//Get the synonyms file uri.
					string properties_uri = String.Format("{0}/{1}/{2}", Turtle.FTP_PREFIX, FtpTargetDirectory, FileName);

					//Add the base uri prefix.
					prefixes.Add(Turtle.RDF_URI_PREFIX.ToLower(), Turtle.RDF_URI + "/");  //Re-use the prefix as the alias.

					//Add the prefix for this void file with the empty prefix.
					prefixes.Add("", String.Format("{0}#", properties_uri));

					//Output the prefixes.
					foreach (KeyValuePair<string, string> p in prefixes)
						w.WriteLine(String.Format("@prefix {0}: <{1}> .", p.Key, p.Value));

					//Add the predicate to describe which dataset this is in.
					string dataset_object = string.Format("{0}-{1}", Turtle.DATASET_LABEL.ToLower(), _dsn.ToLower()); //E.g. :chemspider-chebi
					w.WriteLine(Turtle.getDSNInDatasetPredicate(_dsn, dataset_object, properties_uri));

					//Output the properties comments.
					Turtle.props_comments.ForEach(l => w.WriteLine(l));

					//Properties
					//Get the list of fields.
					List<string> fields = Turtle.CHEMINFmapping.Keys.ToList();
					fields.AddRange(from c in Turtle.columnsWithQuotedErrors select c + "_Error");

					conn.ExecuteReader(String.Format(@"SELECT r.* FROM rdf_exported_props_snapshot r
														WHERE r.exp_id = {0}", Exp.ExportId),
						r => Turtle.AllTurtleProps(((int)r["cmp_id"]).ToString(), Turtle.SqlDataReaderAsDictionary(r, fields)).ForEach(l => w.WriteLine(l)));
				}
			}
		}
	}

	#endregion

	#region TsvDataExportFile

	public class TsvDataExport : DataExport
	{
		/// <summary>
		/// Constructor is passed the dsns to generate the export for.
		/// </summary>
		/// <param name="dsn">The dsns to generate the export for.</param>
		public TsvDataExport() { }

		/// <summary>
		/// Executes the Data Export.
		/// </summary>
		public override void Export()
		{
			//Create the SdfDataExportFile for the export (if we havent already specified them in a derived class).
			if (ExportFiles().Count == 0)
				AddFile(new TsvDataExportFile());

			//Export the data.
			base.Export();
		}
	}

	public class TsvDataExportFile : DataExportFile
	{
		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public TsvDataExportFile() : base() { }

		/// <summary>
		/// Export the Tsv file.
		/// </summary>
		public override void Export(IDataExport Exp, Encoding Encoding)
		{
			//Set the file name.
			FileName = String.Format("CHEMSPIDER_1M_SMILES_{0}.tsv", Exp.DateTimeExported.ToString("yyyyMMdd"));
			base.Export(Exp, Encoding);

			//Write the first 1M ChemSpider records to the TSV.
			using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
			{
				//Write the properties.
				using (TextWriter w = new StreamWriter(FileName, false, Encoding))
				{
					w.WriteLine(String.Format("chemspider_id\tsmiles"));

					conn.ExecuteReader(String.Format(@"SELECT TOP 1000000 cmp_id, SMILES FROM compounds WITH (NOLOCK) WHERE deleted_yn = 0 ORDER BY cmp_id ASC"),
					r =>
					{
						w.WriteLine(String.Format("{0}\t{1}", r["cmp_id"].ToString(), r["SMILES"].ToString()));
					});
				}
			}
		}
	}

	#endregion

	#region IssuesDataExportFile

	/// <summary>
	/// Export file for Issues.
	/// Used for generating Rdf files, containing Issues with a particular data sources compounds.
	/// </summary>
	public class IssuesDataExportFile : DataSourceDataExportFile
	{
		/// <summary>
		/// Call the base constructor.
		/// </summary>
		public IssuesDataExportFile(string dsn) : base(dsn) { }

		/// <summary>
		/// Exports Rdf Issues for the data source.
		/// </summary>
		public override void Export(IDataExport Exp, Encoding Encoding)
		{
			//Set the file name.
			FileName = String.Format("ISSUES_{0}{1}.ttl", DsnLabel, Exp.DateTimeExported.ToString("yyyyMMdd"));
			base.Export(Exp, Encoding);

			//Get a copy of the issues prefixes.
			Dictionary<string, string> prefixes = new Dictionary<string, string>(Turtle.issues_prefixes);

			//Get the DSN prefix for this data source.
			string db_alias = string.Empty;
			string dsn_prefix = string.Empty;
			bool use_full_uri = false;
			dsn_prefix = getDSNPrefix(Exp, _dsn, out db_alias, out use_full_uri);

			using (SqlConnection conn = new SqlConnection(Exp.DBConnection))
			{
				//Write the properties.
				using (TextWriter w = new StreamWriter(FileName, false, Encoding))
				{
					//Get the issues file uri.
					string issues_uri = String.Format("{0}/{1}/{2}", Turtle.FTP_PREFIX, FtpTargetDirectory, FileName);

					//Add the base uri prefix.
					prefixes.Add(Turtle.RDF_URI_PREFIX.ToLower(), Turtle.RDF_URI + "/");  //Re-use the prefix as the alias.

					//Add the prefix for this void file with the empty prefix.
					prefixes.Add("", String.Format("{0}#", issues_uri));

					//Output the prefixes.
					foreach (KeyValuePair<string, string> p in prefixes)
						w.WriteLine(String.Format("@prefix {0}: <{1}> .", p.Key, p.Value));

					//Add the Annotation Property type assignments.
					string annotation_prop = "cheminf:{0} a owl:AnnotationProperty . # {1}";
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_issue_parsing_info, "connection table interpretation information data item"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_issue_parsing_warning, "connection table interpretation warning"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_issue_parsing_error, "connection table interpretation error"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_issue_validation_info, "structural validation information data item"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_issue_validation_warning, "structural validation warning"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_issue_validation_error, "structural validation error"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_issue_standardization_info, "structural standardization information data item"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_issue_standardization_warning, "structural standardization warning"));
					w.WriteLine(String.Format(annotation_prop, Turtle.cheminf_issue_standardization_error, "structural standardization error"));

					//Issues
					conn.ExecuteReader(String.Format(@"SELECT r.* FROM rdf_exported_issues_snapshot r
														WHERE r.exp_id = {0}", Exp.ExportId),
						r =>
						{
							//Get the dsn uri and the issue predicate.
							string dsn_uri = Turtle.getDSNUri(_dsn, r["ext_regid"].ToString(), db_alias, dsn_prefix, use_full_uri);
							string issue_predicate = Turtle.getIssuePredicate(Convert.ToInt32(r["issue_severity"]), Convert.ToInt32(r["issue_type"]));

							//Add the issue to the export file.
							string line = String.Format("{0} {1} \"{2}{3}\"@en ."
														, dsn_uri                                                       //0
														, issue_predicate                                               //1
														, Turtle.RdfEncodedString(r["issue_description"].ToString())    //2
														, r["issue_additional_description"].ToString() == string.Empty ? string.Empty : String.Format("; {0}", Turtle.RdfEncodedString(r["issue_additional_description"].ToString())) //3
														);
							//Write the line.
							w.WriteLine(line);
						});
				}
			}
		}

	}

	#endregion
}