using RSC.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
	public static class CVSPIssueExtensions
	{
		private static Dictionary<string, string> s_issue_pattern = new Dictionary<string, string>();

		static CVSPIssueExtensions()
		{
            s_issue_pattern.Add("non-unique dearomatization", "100.60");
            s_issue_pattern.Add("molfile loader", "200.2");
			s_issue_pattern.Add("molecule", "200.1");
			s_issue_pattern.Add("element", "200.4");
			s_issue_pattern.Add("stereocenters", "200.5");
			s_issue_pattern.Add("indigo-inchi", "200.13");
			s_issue_pattern.Add("SMILES saver", "200.15");
			s_issue_pattern.Add("allene stereo", "200.3");
			s_issue_pattern.Add("SMILES loader", "200.16");
			s_issue_pattern.Add("Dearomatization groups", "200.17");
			s_issue_pattern.Add("Molecule automorphism search timeout", "200.18");
		}

		public static Issue ParseIndigoException(this Exception ex)
		{
            Console.WriteLine(ex);
			if (ex.Message.Contains(":"))
			{
				string[] message = ex.Message.Split(':');
				string title = message[0].Trim();
				string description = message[1].Trim();
				if (s_issue_pattern.ContainsKey(title))
				{
                    if (title.Contains("dearomatization"))
                    {
                        return new Issue { Code = "100.60", Message = description };
                    }
					if (title.Contains("stereocenters"))
					{
						if (String.IsNullOrEmpty(description))
							return new Issue
							{
								Code = "200.5",
								AuxInfo = ex.StackTrace,
								Message = description
							};
						else if (description.Contains("angle between bonds is too small"))
							return new Issue
							{
								Code = "200.6",
								AuxInfo = ex.StackTrace,
								Message = description

							};

						else if (description.Contains("stereo types of the opposite bonds mismatch"))
							return new Issue
							{
								Code = "200.7",
								AuxInfo = ex.StackTrace,
								Message = description
							};

						else if (description.Contains("stereo types of non-opposite bonds match"))
							return new Issue
							{
								Code = "200.9",
								AuxInfo = ex.StackTrace,
								Message = description
							};

						else if (description.Contains("one bond up, one bond down"))
							return new Issue
							{
								Code = "200.8",
								AuxInfo = ex.StackTrace,
								Message = description
							};

						else if (description.Contains("2 hydrogens near stereocenter"))
							return new Issue
							{
								Code = "200.11",
								AuxInfo = ex.StackTrace,
								Message = description
							};

						else if (description.Contains("have hydrogen(s) besides implicit hydrogen near stereocenter"))
						{
							Issue i = new Issue
							{
								Code = "200.10",
								AuxInfo = ex.StackTrace,
								Message = description
							};
							return i;
						}

						else if (description.Contains("degenerate case") && description.Contains("bonds overlap"))
						{
							Issue i = new Issue
							{
								Code = "200.12",
								AuxInfo = ex.StackTrace,
								Message = description
							};
							return i;
						}
						else
							return new Issue
							{
								Code = "200.5",
								AuxInfo = ex.StackTrace,
								Message = description
							};

					}
					else if (title.Contains("molecule"))
					{
						if (description.Contains("getAtomValence() does not work on pseudo-atoms"))
						{
							return new Issue
							{
								Code = "200.1",
								AuxInfo = ex.StackTrace,
								Message = description
							};
						}
						else return new Issue
						{
							Code = "100.69",
							AuxInfo = ex.StackTrace,
							Message = description
						};
					}
					else if (title.Contains("molfile loader") && description.Contains("direction of bond") && description.Contains("makes no sense"))
					{
						//do nothing - adding our own modules to handle these cases
						return null;
					}
                    else if (title.Contains("molfile loader") && description.Contains("queries") && description.Contains("bond"))
                    {
                        return new Issue() { Code = "100.5", AuxInfo = ex.StackTrace, Message = description };
                    }
                    else if (title.Contains("allene stereo"))
                    {
                        //do nothing - adding our own modules to handle these cases
                        return null;
                    }
                    else
                    {
                        return new Issue
                        {
                            Code = s_issue_pattern[title],
                            Message = ex.Message,
                            AuxInfo = ex.StackTrace,
                        };
                    }
				}
				return new Issue
				{
					Code = "100.69",
					Message = ex.Message,
					AuxInfo = ex.StackTrace,
				};
			}
			else
			{
				return new Issue
				{
					Code = "100.69",
					Message = ex.Message,
					AuxInfo = ex.StackTrace,
				};
			}
		}
	}
}
