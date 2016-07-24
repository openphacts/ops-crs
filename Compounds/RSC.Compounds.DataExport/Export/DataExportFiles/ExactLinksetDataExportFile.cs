using System;
using System.Text;
using RSC.Datasources;

namespace RSC.Compounds.DataExport
{
	public class ExactLinksetDataExportFile : LinksetDataExportFile
	{
		public ExactLinksetDataExportFile(IDataSourceExport exp)
			: base(exp)
		{
			FileName = String.Format("LINKSET_EXACT_{0}{1}.ttl", exp.DsnLabel, exp.ExportDate.ToString("yyyyMMdd"));
		}

		protected override SkosPredicate SkosPredicate
		{
			get
			{
				return SkosPredicate.EXACT_MATCH;
			}
		}
	}
}
