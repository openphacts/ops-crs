using ChemSpider.Molecules;
using OpenEyeNet;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Web.Http;
using System.Web.Http.Cors;

namespace RSC.Compounds.WebApi
{
	[EnableCors("*", "*", "*")]
	public class ConvertController : ApiController
	{
		private readonly ICompoundConvert convertService;

		public ConvertController(ICompoundConvert convertService)
		{
			if (convertService == null)
			{
				throw new ArgumentNullException("convertService");
			}

			this.convertService = convertService;
		}

		[HttpPost]
		[Route("api/convert")]
		public IHttpActionResult ConvertToPOST([FromBody] ChemIdUtils.ConvertOptions convertOptions)
		{
			return Ok(ConvertTo(convertOptions));
		}

		[HttpGet]
		[Route("api/convert")]
		public IHttpActionResult ConvertToGET([FromUri] ChemIdUtils.ConvertOptions convertOptions)
		{
			return Ok(ConvertTo(convertOptions));
		}

		private ChemIdUtils.N2SResult ConvertTo(ChemIdUtils.ConvertOptions convertOptions)
		{
			ChemIdUtils.N2SResult res = new ChemIdUtils.N2SResult
			{
				confidence = 0,
				message = string.Empty,
				mol = string.Empty
			};

			try
			{
				string mol = string.Empty;
				Guid? compoundId = null;
				IEnumerable<Guid> compoundIds = null;
				var serializer = new System.Web.Script.Serialization.JavaScriptSerializer();

				switch (convertOptions.Direction)
				{
					case ChemIdUtils.ConvertOptions.EDirection.InChi2CSID:
						compoundIds = convertService.InChIToCompoundId(convertOptions.Text);
						if (compoundIds.Any())
						{
							res.mol = serializer.Serialize(compoundIds);
							res.confidence = 100;
						}
						break;
					case ChemIdUtils.ConvertOptions.EDirection.InChiKey2CSID:
						compoundIds = convertService.InChIKeyToCompoundId(convertOptions.Text);
						if (compoundIds.Any())
						{
							res.mol = serializer.Serialize(compoundIds);
							res.confidence = 100;
						}
						break;
					case ChemIdUtils.ConvertOptions.EDirection.SMILES2CSID:
						compoundIds = convertService.SMILESToCompoundId(convertOptions.Text);
						if (compoundIds.Any())
						{
							res.mol = serializer.Serialize(compoundIds);
							res.confidence = 100;
						}
						break;
					//case ChemIdUtils.ConvertOptions.EDirection.Mol2CSID:
					//	compoundId = convertService.MolToCompoundId(convertOptions.Text);
					//	if (compoundId != null)
					//	{
					//		res.mol = compoundId.ToString();
					//		res.confidence = 100;
					//	}
					//	break;
					case ChemIdUtils.ConvertOptions.EDirection.Smiles2InChi:
						mol = MolUtils.SMILESToMol(convertOptions.Text);
						if (string.IsNullOrEmpty(mol))
							res.message = string.Format("Cannot parse SMILES: {0}", convertOptions.Text);
						else
						{
							res.mol = InChINet.InChIUtils.mol2inchiinfo(mol, InChINet.InChIFlags.Standard)[0];
							res.confidence = 100;
						}
						break;
					case ChemIdUtils.ConvertOptions.EDirection.Smiles2Mol:
						res.mol = ChemIdUtils.smiles2mol(convertOptions.Text);
						res.confidence = 100;
						break;
					case ChemIdUtils.ConvertOptions.EDirection.Mol2SMILES:
						res.mol = OpenEyeUtility.GetInstance().MolToSMILES(convertOptions.Text);
						res.confidence = 100;
						break;
					case ChemIdUtils.ConvertOptions.EDirection.InChI2Mol:
						mol = ChemSpider.Molecules.InChI.InChIToMol(convertOptions.Text);
						if (!string.IsNullOrEmpty(mol))
						{
							res.mol = mol;
							res.confidence = 100;
						}
						break;
					default:
						var parts = new ChemSpider.Parts.Client.JSONClient();
						res = parts.ConvertTo(convertOptions);
						break;
				}

				if (string.IsNullOrEmpty(res.mol))
				{
					res.confidence = 0;
					res.message = "Cannot do the convertion. Sorry!";
				}
			}
			catch (Exception ex)
			{
				res.confidence = 0;
				res.message = string.Format("Cannot convert chemical structure: {0}", ex.Message);
			}

			return res;
		}
	}
}
