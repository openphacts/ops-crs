using System.Linq;

namespace RSC.CVSP.EntityFramework
{
	public static class DepositionExtensions
	{
		public static Deposition ToDeposition(this ef_Deposition ef)
		{
			return new RSC.CVSP.Deposition()
			{
				Id = ef.Guid,
				DateSubmitted = ef.DateSubmitted,
				DateReprocessed = ef.DateReprocessed,
				Status = (DepositionStatus)ef.Status,
				IsPublic = ef.IsPublic,
				UserId = ef.UserProfile.Guid,
				DataDomain = (DataDomain)ef.DataDomain,
				DatasourceId = ef.DatasourceId,
				Parameters = ef.ProcessingParameters.Select(p => p.ToProcessingParameter()).ToList(),
				DepositionFiles = ef.Files.Select(f => f.ToDepositionFile()).ToList()
			};
		}
	}
}
