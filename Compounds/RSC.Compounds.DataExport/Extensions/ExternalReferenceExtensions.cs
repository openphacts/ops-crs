namespace RSC.Compounds.DataExport
{
	public static class ExternalReferenceExtensions
	{
		/// <summary>
		///  Converts a External Reference to the Url required by the RDF Export (requires OPS prefix inserted for OPS IDS).
		/// </summary>
		/// <param name="externalReference">External Reference</param>
		/// <returns>Uri as a string.</returns>
		public static string ToOpsUri(this ExternalReference externalReference)
		{
			return string.Format(@"{0}{1}", externalReference.Type.UriSpace
										  , externalReference.Type.UriSpace == Constants.OPSUriSpace ? externalReference.ToOpsId() : externalReference.Value);
		}

		/// <summary>
		///  Converts an OPS External Reference to the Url required by the RDF Export (requires OPS prefix inserted).
		/// </summary>
		/// <param name="externalReference">External Reference</param>
		/// <returns>Uri as a string.</returns>
		public static string ToOpsId(this ExternalReference externalReference)
		{
			return string.Format(@"OPS{0}", externalReference.Value);
		}      
	}
}