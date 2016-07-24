using System.Linq;
using RSC.Logging;

namespace RSC.Compounds.EntityFramework.Extensions
{
    public static class RevisionExtensions
    {
        /// <summary>
        /// Converts an ef_Revision to a core Revision.
        /// </summary>
        /// <param name="ef">ef_Revision for conversion.</param>
        /// <returns>The converted core Revision.</returns>
        public static Revision ToRevision(this ef_Revision ef)
        {
            return new Revision
            {
                Id = ef.Id,
                Sdf = ef.Sdf,
                Issues = ef.Issues == null ? null :
                    ef.Issues.Select(i => new Issue
                    {
                        Code = i.Code,
                        Id = i.LogId
                    }).ToList(),
                Annotations = ef.Annotations == null ? null :
                    ef.Annotations
                    .Select(i => new Annotation
                    {
                        Type = i.Type,
                        Value = i.Value
                    }).ToList(),
                Substance = ef.Substance == null ? null :
                new Substance
                {
                    DataSourceId = ef.Substance.DataSourceId,
                    ExternalIdentifier = ef.Substance.ExternalIdentifier,
                    Id = ef.Substance.Id
                },
                CompoundId = ef.CompoundId,
                DateCreated = ef.DateCreated,
                EmbargoDate = ef.EmbargoDate,
                Revoked = ef.Revoked,
                Version = ef.Version
            };
        }
    }
}
