namespace RSC.Compounds.EntityFramework
{
    public static class ExternalReferenceExtensions
    {
        /// <summary>
        /// Converts an ef_ExternalIdentifier to a core ExternalIdentifier.
        /// </summary>
        /// <param name="ef">ef_ExternalIdentifier for conversion.</param>
        /// <returns>The converted core External Identifier.</returns>
        public static ExternalReference ToExternalReference(this ef_ExternalReference ef)
        {
            return new ExternalReference()
            {
                Value = ef.Value,
                Type = ef.Type.ToExternalReferenceType(),
            };
        }

        /// <summary>
        /// Converts an ef_ExternalReferenceType to a core ExternalReferenceType.
        /// </summary>
        /// <param name="ef">ef_ExternalReferenceType for conversion.</param>
        /// <returns>The converted core External Id Type.</returns>
        public static ExternalReferenceType ToExternalReferenceType(this ef_ExternalReferenceType ef)
        {
            return new ExternalReferenceType()
            {
                Description = ef.Description,
                UriSpace = ef.UriSpace
            };
        }
    }
}
