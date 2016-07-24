using System;
using System.Text;
using RSC.Datasources;

namespace RSC.Compounds.DataExport
{
	public class RelatedLinksetDataExportFile : LinksetDataExportFile
	{
		public RelatedLinksetDataExportFile(IDataSourceExport exp)
			: base(exp)
		{
			FileName = string.Format("LINKSET_RELATED_{0}{1}.ttl", exp.DsnLabel, exp.ExportDate.ToString("yyyyMMdd"));
		}

		protected override SkosPredicate SkosPredicate
		{
			get
			{
				return SkosPredicate.RELATED_MATCH;
			}
		}
	}
}
