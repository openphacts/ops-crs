using System.Text;
using System.IO;
using System.Linq;
using ChemSpider.Molecules;
using RSC.Collections;
using System;
using System.Threading;

namespace RSC.Compounds.DataExport
{
	/// <summary>
	/// Exports an SDF containing chemspider_id, ops_id, chemspider_url, ops_url.
	/// 
	/// Note 2015-10-12: This is different from the other implementations of DataExportFile and a bit of a special case.
	/// </summary>
	public class OpsSdfMapDataExportFile : DataExportFile
	{
		private static readonly int chunkSize = 1000;

		public OpsSdfMapDataExportFile(IDataExport exp, string tmpDir)
		{
			FileName = "OPS_CHEMSPIDER_" + exp.ExportDate.ToString("yyyy-MM-dd") + ".sdf";
			base.TmpDir = tmpDir;
		}

		public override void Export(IDataExport exp, Encoding encoding)
		{
			base.Export(exp, encoding);

			//Write the SDF file containing supplementary info.
			using ( TextWriter sdf = new StreamWriter(FileFullPath, false, encoding) )
			using ( TextWriter w = TextWriter.Synchronized(sdf) ) {
				var cmpIds = exp.CompoundsStore.GetCompoundIds();
				cmpIds
					.ToChunks(chunkSize)
					.AsParallel()
					.ForAll(cmpIdsChunk => {
						foreach (
							var compound in
								exp.CompoundsStore.GetCompounds(cmpIdsChunk, new[] { "Id", "ExternalReferences", "Mol" })
									.Where(compound => compound.Mol != null) ) {

							var opsIdentifier =
								compound.ExternalReferences.FirstOrDefault(e => e.Type.UriSpace == Constants.OPSUriSpace);
							var csIdentifier =
								compound.ExternalReferences.FirstOrDefault(e => e.Type.UriSpace == Constants.CSUriSpace);

							Interlocked.Increment(ref recordCount);

							if ( RecordCount % chunkSize == 0 )
								Console.Out.Write(".");
							if ( RecordCount % 10000 == 0 )
								Console.Out.Write(RecordCount);

							//Create a new SDF record.
							var rec = new SdfRecord();
							rec.Mol = compound.Mol;

							//Add the Ops Id.
							if ( opsIdentifier != null ) {
								rec.AddField("OPS_ID", opsIdentifier.ToOpsId());
								rec.AddField("OPS_URL", opsIdentifier.ToOpsUri());
							}

							//Add the ChemSpider id.
							if ( csIdentifier != null ) {
								rec.AddField("CHEMSPIDER_ID", csIdentifier.Value);
								rec.AddField("CHEMSPIDER_URL", csIdentifier.ToOpsUri());
							}

							w.Write(rec.ToString());
						}

						Console.Out.Write(".");
					});
			}
		}
	}
}
