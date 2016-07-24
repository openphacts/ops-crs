using System;
using System.Configuration.Install;
using System.Collections.Specialized;
using System.Collections.Generic;
using System.Linq;
using RSC.Compounds.DataExport;
using System.Configuration;
using RSC.Properties.EntityFramework;
using System.Text;
using RSC.Compounds.EntityFramework;
using RSC.Logging.EntityFramework;
using System.Reflection;
using System.IO;
using System.Data.SqlClient;
using ChemSpider.Database;
using ChemSpider.Molecules;
using ChemSpider.Utilities;
using System.Data;
using System.Threading;
using com.ggasoftware.indigo;
using System.Diagnostics;

namespace cscutil
{
	class Program
	{
		private static void Usage(string message = null)
		{
			if (!String.IsNullOrEmpty(message))
				Console.Error.WriteLine(message);
			Console.Error.WriteLine(Resources.usage);
			Environment.Exit(1);
		}

		static void Main(string[] args)
		{
			InstallContext context = new InstallContext(null, args);
			StringDictionary parameters = context.Parameters;

			if (!parameters.ContainsKey("command"))
				Usage();

			var export = new OpsDataExport();
			var options = new OpsDataExportOptions
			{
				ExportDate = String.IsNullOrEmpty(parameters["date"]) ? DateTime.Now : DateTime.Parse(parameters["date"]),
				DataSourceIds = String.IsNullOrEmpty(parameters["dsn_ids"]) ? null : parameters["dsn_ids"].Split(',').Select(s => new Guid(s.Trim())).ToList(),
				ExportIds = String.IsNullOrEmpty(parameters["exp_ids"]) ? new List<int>() : parameters["exp_ids"].Split(',').Select(s => int.Parse(s.Trim())).ToList(),
				FileIds = String.IsNullOrEmpty(parameters["file_ids"]) ? new List<int>() : parameters["file_ids"].Split(',').Select(s => int.Parse(s.Trim())).ToList(),
				Compress = context.IsParameterTrue("compress"),
				Limited = context.IsParameterTrue("limited"),
				UploadUrl = parameters["upload-url"] ?? ConfigurationManager.AppSettings["rdf.upload.url"] ?? Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().CodeBase), "out"),
				DownloadUrl = parameters["download-url"] ?? ConfigurationManager.AppSettings["rdf.download.url"] ?? "http://ops.rsc.org/download/",
				Username = parameters["username"] ?? ConfigurationManager.AppSettings["rdf.upload.username"],
				Password = parameters["password"] ?? ConfigurationManager.AppSettings["rdf.upload.password"],
				TmpDir = parameters["tmpdir"] ?? ConfigurationManager.AppSettings["rdf.tmpdir"],
			};

			switch (parameters["command"].ToLower())
			{
				// E.g. /command=exp-ops /dsn_ids="73834460-e542-46d1-8aa5-b034ea2bf34a,8dfae34b-fa65-4c71-88fb-79a9ed55b09b"
				case "exp-ops":
					if (options.DataSourceIds != null && options.ExportIds.Any())
						Usage("Either /dsn_ids or /exp_ids can be specified at the same time");
					export.Export(options);
					break;
				case "exp-ops-dsn":
					if (options.DataSourceIds != null && options.ExportIds.Any())
						Usage("Either /dsn_ids or /exp_ids can be specified at the same time");
					if (options.ExportIds.Any())
						export.ExportDatasourcesByExportIds(options);
					else
						export.ExportDatasources(options);
					break;
				case "exp-ops-map":
					export.ExportOpsToCsidSdf(options);
					break;
				case "exp-ops-void":
					export.ExportVoID(options);
					break;

				case "check-all-present":
					CheckExport.AllFilesPresent(parameters["folder"]);
					break;
				case "check-all-validate":
					CheckExport.AllFilesValidate(parameters["folder"]);
					break;
				case "check-void":
					CheckExport.ValidateVoidAgainstFolder(parameters["folder"], parameters["base"]);
					break;
				case "compare-exports":
					CheckExport.CompareExports(parameters["folder1"], parameters["folder2"]);
					break;
				case "provoke-updates":
					provokeUpdates();
					break;

				case "load-surechembl":
					loadSureChEMBL((parameters["files"] ?? "").Split(',').Select(s => s.Trim()));
					break;

				case "load-bingodb":
					loadBingoDb(parameters["bingo-db"], (parameters["files"] ?? "").Split(',').Select(s => s.Trim()), context.IsParameterTrue("update"), context.IsParameterTrue("optimize"));
					break;

				case "merge-bingodb":
					mergeBingoDb(parameters["src-db"], parameters["dst-db"], context.IsParameterTrue("optimize"));
					break;

				case "calc-surechembl":
					calcSureChEMBL((parameters["files"] ?? "").Split(',').Select(s => s.Trim()));
					break;

				case "missed-smiles":
					calcMissedSmiles();
					break;

				default:
					Usage(String.Format("Unknown command: {0}", parameters["command"]));
					break;
			}
		}

		private static void mergeBingoDb(string src_db, string dst_db, bool optimize)
		{
			int nRecords = 0;
			using (Indigo indigo1 = new Indigo())
			// using ( Indigo indigo2 = new Indigo() )
			using (Bingo srcBingo = Bingo.loadDatabaseFile(indigo1, src_db))
			using (Bingo dstBingo = Directory.Exists(dst_db) ? Bingo.loadDatabaseFile(indigo1, dst_db) : Bingo.createDatabaseFile(indigo1, dst_db, "molecule", ""))
			{
				List<int> ids = new List<int>();

				using (var query = indigo1.loadMolecule("C"))
				using (var sim_matcher = srcBingo.searchSim(query, 0, 1))
				{
					while (sim_matcher.next())
						ids.Add(sim_matcher.getCurrentId());
				}

				Console.Out.WriteLine("Merging {0} ({1} records) into {2}", src_db, ids.Count(), dst_db);

				foreach (var id in ids)
				{
					dstBingo.insert(srcBingo.getRecordById(id), id);

					Interlocked.Increment(ref nRecords);
					if (nRecords % 1000 == 0)
						Console.Out.Write(".");
					else if (nRecords % 100000 == 0)
						Console.Out.Write("{0} ", nRecords);
				}
			}
		}

		private static void loadBingoDb(string location, IEnumerable<string> files, bool update, bool optimize)
		{
			int nRecords = 0;

			Action<string> a = file =>
			{
				using (SdfReader sdf = new SdfReader(file))
				using (Indigo indigo = new Indigo())
				{
					string dir = location;
					if (String.IsNullOrEmpty(dir))
						dir = Path.GetFileNameWithoutExtension(file);

					Console.Out.WriteLine("Location: {0}", dir);

					using (var db = openOrCreateBingoDb(indigo, dir, optimize))
					{
						sdf.Records
							.ForAll(r =>
							{
								int id = int.Parse(r["ID"].First().Substring(7));
								if (!update || !recordExists(db, id))
								{
									string smiles = r.Molecule.SMILES;
									if (!String.IsNullOrEmpty(smiles))
									{
										try
										{
											IndigoObject mol = indigo.loadMolecule(smiles);
											db.insert(mol, id);
										}
										catch (IndigoException ex)
										{
											Trace.TraceWarning(ex.Message);
										}
										catch (BingoException ex)
										{
											Trace.TraceWarning(ex.Message);
										}
									}

									Interlocked.Increment(ref nRecords);
									if (nRecords % 1000 == 0)
										Console.Out.Write("{0} ", nRecords);
								}
							});
						if (optimize)
							db.optimize();
						db.close();
					}
				}
			};

			if (String.IsNullOrEmpty(location) && files.Count() > 1)
				files.AsParallel().ForAll(a);
			else
				files.ForAll(a);
		}

		private static bool recordExists(Bingo bingo, int id)
		{
			IndigoObject rec = null;
			try
			{
				rec = bingo.getRecordById(id);
				return rec != null;
			}
			catch (BingoException)
			{

			}
			catch (IndigoException)
			{

			}
			return false;
		}

		private static Bingo openOrCreateBingoDb(Indigo indigo, string location, bool optimize)
		{
			Bingo bingo = null;
			if (!Directory.Exists(location))
				bingo = Bingo.createDatabaseFile(indigo, location, "molecule", "");
			else
			{
				bingo = Bingo.loadDatabaseFile(indigo, location);
				if (optimize)
					bingo.optimize();
			}
			return bingo;
		}

		private static void calcSureChEMBL(IEnumerable<string> files)
		{
			int nRecords = 0, nThreads = 0;

			files
				.AsParallel()
				.ForAll(file =>
				{
					int nThread;
					lock (_mutex)
						nThread = nThreads++;

					string csvfile = Path.Combine(Path.GetDirectoryName(file), Path.GetFileNameWithoutExtension(file)) + ".csv";

					using (SdfReader sdf = new SdfReader(file))
					using (StreamWriter csv = new StreamWriter(csvfile))
					{

						Console.Out.WriteLine("Thread: {0}: {1} => {2}", nThread, file, csvfile);

						sdf.Records
							.ForAll(r =>
							{
								csv.WriteLine("{0},{1},{2}", r["ID"].First(), r["InChIKey"].First(), r.Molecule.SMILES);

								Interlocked.Increment(ref nRecords);
								if (nRecords % 1000 == 0)
									Console.Out.Write("{0} ", nRecords);
							});
					}
				});
		}

		private static object _mutex = new object();

		private static void loadSureChEMBL(IEnumerable<string> files)
		{
			int nRecords = 0, nThreads = 0;

			files
				.AsParallel()
				.ForAll(file =>
				{
					int nThread;
					lock (_mutex)
						nThread = nThreads++;

					Console.Out.WriteLine("File: {0}, Thread: {1}", file, nThread);

					using (SdfReader sdf = new SdfReader(file))
					using (SqlConnection conn = new SqlConnection(ConfigurationManager.ConnectionStrings["SureConnection"].ConnectionString))
					{
						conn.Open();
						string table = String.Format("compounds_{0}", nThread);
						Console.Out.WriteLine("Table: {0}", table);

						using (SqlCommand cmd = new SqlCommand(String.Format(
							@"if not exists (select * from sysobjects where name='{0}' and xtype='U')
								create table {0} (
									cmp_id int primary key identity,
									ext_id varchar(20) not null unique,
									smiles varchar(1000),
									inchi_key varchar(30) not null,
								)", table), conn))
						{
							cmd.ExecuteNonQuery();
						}

						using (SqlCommand cmd = new SqlCommand(String.Format(
							@"with params as (select @ext_id as ext_id, @smiles as smiles, @inchi_key as inchi_key)
							merge {0} c
							using params p
							on c.ext_id = p.ext_id
							when not matched by target
							then insert (ext_id, smiles, inchi_key) values (p.ext_id, p.smiles, p.inchi_key);", table), conn))
						{
							cmd.Parameters.Add("@ext_id", SqlDbType.VarChar, 20);
							cmd.Parameters.Add("@smiles", SqlDbType.VarChar, 1000);
							cmd.Parameters.Add("@inchi_key", SqlDbType.VarChar, 30);

							sdf.Records
								// .AsParallel()
								.ForAll(r =>
								{
									cmd.Parameters["@ext_id"].Value = r["ID"].First();
									if (String.IsNullOrEmpty(r.Molecule.SMILES))
										cmd.Parameters["@smiles"].Value = DBNull.Value;
									else
										cmd.Parameters["@smiles"].Value = r.Molecule.SMILES;
									cmd.Parameters["@inchi_key"].Value = r["InChIKey"].First();
									cmd.ExecuteNonQuery();

									Interlocked.Increment(ref nRecords);
									if (nRecords % 1000 == 0)
										Console.Out.Write("{0} ", nRecords);
								});
						}
					}
				});
		}

		private static void calcMissedSmiles()
		{
			var problemCompounds = new List<Guid>();

			IEnumerable<Guid> compoundIds = null;

			using (var db = new CompoundsContext())
			{
				db.Configuration.ValidateOnSaveEnabled = false;

				Trace.WriteLine("Get problem compounds... please be patient!");
				compoundIds = db.Compounds.Where(c => c.SmilesId == null && !string.IsNullOrEmpty(c.Mol)).Select(c => c.Id).ToList();
				Trace.WriteLine(string.Format("Done: {0}", compoundIds.Count()));
			}

			Trace.WriteLine("Calculating...");

			compoundIds.AsParallel().ForAll(id =>
			{
				using (var db = new CompoundsContext())
				{
					try
					{
						var compound = db.Compounds.Find(id);

						if (compound != null)
						{
							using (var indigo = new Indigo())
							{
								indigo.setOption("ignore-stereochemistry-errors", "true");
								indigo.setOption("unique-dearomatization", "false");
								indigo.setOption("ignore-noncritical-query-features", "true");
								indigo.setOption("timeout", "600000");

								IndigoObject original = indigo.loadMolecule(compound.Mol);

								var smiles = new RSC.Compounds.Smiles(original.canonicalSmiles());

								var md5 = smiles.IndigoSmilesHash;

								var efSmiles = db.Smiles.Where(s => s.IndigoSmilesMd5 == md5).FirstOrDefault();

								if (efSmiles == null)
								{
									efSmiles = new ef_Smiles()
									{
										Id = Guid.NewGuid(),
										IndigoSmiles = smiles.IndigoSmiles,
										IndigoSmilesMd5 = smiles.IndigoSmilesHash
									};
								}

								compound.Smiles = efSmiles;

								db.SaveChanges();

								Trace.WriteLine(string.Format("{0}: {1}", id, smiles.IndigoSmiles));
							}
						}
					}
					catch (Exception ex)
					{
						Trace.WriteLine(string.Format("Can't generate SMILES for {0}\n{1}", id, ex.ToString()));
					}
				}
			});
		}

		private static void provokeUpdates()
		{
			var dataExportStore = new EFDataExportStore();
			var log = dataExportStore.GetDataExportLog(1);
			Console.Out.WriteLine(log);

			var propertiesStore = new EFPropertyStore();
			var provs = propertiesStore.ProvenancesList();
			foreach (var p in provs)
				Console.Out.WriteLine(p);

			var compoundsStore = new EFCompoundStore(propertiesStore);
			var count = compoundsStore.GetCompoundsCount();
			Console.Out.WriteLine(count);

			var logStore = new EFLogStore(null);
			var sources = logStore.GetLogSources();
			foreach (var p in sources)
				Console.Out.WriteLine(p);

			var datasourcesStore = new EFDatasourceStore();
			var ids = datasourcesStore.GetDataSourceIds();
			foreach (var p in ids)
				Console.Out.WriteLine(p);
		}
	}
}
