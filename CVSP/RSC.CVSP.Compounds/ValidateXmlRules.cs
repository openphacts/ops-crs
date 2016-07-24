using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using System.Xml.Linq;
using com.ggasoftware.indigo;


namespace RSC.CVSP.Compounds
{/*
	public class XmlRules
	{
		private static Indigo s_indigo;
		static XmlRules()
		{
			s_indigo = new Indigo();
			s_indigo.setOption("ignore-stereochemistry-errors", true);
			s_indigo.setOption("ignore-noncritical-query-features", true);
			s_indigo.setOption("unique-dearomatization", false);
			s_indigo.setOption("timeout", "60000");
		}

		public static bool Validate(string xml, RuleType ct, out List<Tuple<string, string>> messages)
		{
			messages = new List<Tuple<string, string>>();
			if (ct == RuleType.AcidBase)
			{
				//acid-base validation
				return ValidateAcidBaseRules(xml, out messages);

			}
			else if (ct == RuleType.Validation)
			{
				//validation rules
				return ValidateValidationRules(xml, out messages);
			}
			else if (ct == RuleType.Standardization)
			{
				return ValidateStandardizationRules(xml, out messages);
			}
			return true;

		}

		private static bool ValidateStandardizationRules(string xml, out List<Tuple<string, string>> messages)
		{
			bool res = true;
			messages = new List<Tuple<string, string>>();
			try
			{
				XElement x = XElement.Parse(xml, LoadOptions.None);
				var rules = x.XPathSelectElements("//rule").ToList();
				foreach(var rule in rules)
				{
					try
					{
						if (rule.Attribute("type") == null || (!rule.Attribute("type").Value.Equals("module") && !rule.Attribute("type").Value.Equals("SMIRKS")))
						{
							messages.Add(new Tuple<string, string>("attribute 'type' not found or its value is not 'module' or 'SMIRKS'", rule.ToString()));
							res = false;
						}
						if (rule.Attribute("value") == null || String.IsNullOrEmpty(rule.Attribute("value").Value))
						{
							messages.Add(new Tuple<string, string>("attribute 'value' not found", rule.ToString()));
							res = false;
						}
						if (rule.Attribute("type").Value.Equals("SMIRKS"))
						{
							//validate SMIRKS
							s_indigo.loadReactionSmarts(rule.Attribute("value").Value);
						}
						else if(rule.Attribute("type").Value.Equals("module"))
						{
							Enums.ProcessingModule module;
							bool result = Enum.TryParse(rule.Attribute("value").Value, out module);
							if (!result)
							{
								messages.Add(new Tuple<string, string>("module not recognized", rule.ToString()));
								res = false;
							}
						}
					}
					catch(Exception ex)
					{
						messages.Add(new Tuple<string, string>("rule failed: " + rule.ToString(), ex.Message));
						res = false;
					}
				}
			}
			catch(Exception ex)
			{
				messages.Add(new Tuple<string, string>("exception", ex.Message));
				res = false;
			}
			return res;
		}

		private static bool ValidateValidationRules(string xml, out List<Tuple<string, string>> messages)
		{
			bool returnRes = true;
			messages = new List<Tuple<string, string>>();

			XElement x = XElement.Parse(xml, LoadOptions.None);
			var moleculerules = x.XPathSelectElements("//moleculerules/*").ToList();
			foreach (var moleculerule in moleculerules)
			{
				if (moleculerule.Name == null || String.IsNullOrEmpty(moleculerule.Name.ToString()))
				{
					messages.Add(new Tuple<string, string>("rule has to has name", moleculerule.ToString()));
					return false;
				}
				RSC.Logging.Severity sev;
				bool res = Enum.TryParse(moleculerule.Name.ToString(), out sev);
				if (!res)
				{
					messages.Add(new Tuple<string, string>("rule name should be Information,Warning, or Error", moleculerule.ToString()));
					return false;
				}

				if (moleculerule.Attribute("message") == null)
				{
					messages.Add(new Tuple<string, string>("attribute 'message' not found for the rule", moleculerule.ToString()));
					return false;
				}
				if (moleculerule.Attribute("description") == null)
				{
					messages.Add(new Tuple<string, string>("attribute 'description' not found for the rule", moleculerule.ToString()));
					return false;
				}

				var op = moleculerule.XPathSelectElement("or|and");
				if (op == null)
				{
					var test = moleculerule.XPathSelectElement("test");
					if(!validateValidationTest(test, messages))
						return false;
				}
				else
				{
					var tests = op.XPathSelectElements("test");
					foreach (XElement test in tests)
						if (!validateValidationTest(test, messages))
							return false;
				}
			}
			return returnRes;
		}

		private static bool validateValidationTest(XElement test, List<Tuple<string, string>> messages)
		{
			
				if (test.Attribute("name") == null)
				{
					messages.Add(new Tuple<string, string>("test has to have attribute 'name'", test.ToString()));
					return false;
				}

				if (!test.Attribute("name").Value.Equals("SMARTStest"))
				{
					messages.Add(new Tuple<string, string>("attribute 'name' of test shold be 'SMARTStest'", test.ToString()));
					return false;
				}

				if (test.Attribute("param") == null || String.IsNullOrEmpty(test.Attribute("param").Value))
				{
					messages.Add(new Tuple<string, string>("attribute 'param' of test shold be valid SMARTS string", test.ToString()));
					return false;
				}
				try
				{
					string expandSmarts = MoleculeObjects.Molecule.ExpandSMARTSAbbn(test.Attribute("param").Value);
					s_indigo.loadSmarts(expandSmarts);
				}
				catch (Exception ex)
				{
					messages.Add(new Tuple<string, string>("Smarts did not load: " + test.Attribute("param").Value, ex.Message));
					return false;
				}
			
			return true;
		}

		private static bool ValidateAcidBaseRules(string xml, out List<Tuple<string, string>> messages)
		{

			bool returnRes = true;
			messages = new List<Tuple<string, string>>();
			try
			{
				XElement x = XElement.Parse(xml, LoadOptions.None);
				var acidgroups = x.XPathSelectElements("//acidgroup");
				List<int> ranks = new List<int>();
				List<string> smarts_l = new System.Collections.Generic.List<string>();
				List<string> smirks_l = new System.Collections.Generic.List<string>();
				foreach (var acidgroup in acidgroups)
				{
					if (acidgroup.Attribute("rank") == null)
					{
						messages.Add(new Tuple<string, string>("attribute 'rank' not found", acidgroup.ToString()));
						return false;
					}

					if (acidgroup.Attribute("acid") == null)
					{
						messages.Add(new Tuple<string, string>("attribute 'acid' not found", acidgroup.ToString()));
						return false;
					}
					if (acidgroup.Attribute("base") == null)
					{
						messages.Add(new Tuple<string, string>("attribute 'base' not found", acidgroup.ToString()));
						return false;
					}
					if (acidgroup.Attribute("acid2base") == null)
					{
						messages.Add(new Tuple<string, string>("attribute 'acid2base' not found", acidgroup.ToString()));
						return false;
					}
					if (acidgroup.Attribute("base2acid") == null)
					{
						messages.Add(new Tuple<string, string>("attribute 'base2acid' not found", acidgroup.ToString()));
						return false;
					}

					string rank = acidgroup.Attribute("rank").Value;
					int i_rank;
					bool res = Int32.TryParse(rank, out i_rank);
					if (!res)
					{
						messages.Add(new Tuple<string, string>("rank " + rank + " should be integer", null));
						returnRes = false;
					}

					if (ranks.Contains(i_rank))
					{
						messages.Add(new Tuple<string, string>("rank " + rank + " was found twice", null));
						returnRes = false;
					}
					else
						ranks.Add(i_rank);

					smarts_l.AddRange(new List<string>() { acidgroup.Attribute("acid").Value, acidgroup.Attribute("base").Value });
					smirks_l.AddRange(new List<string>() { acidgroup.Attribute("acid2base").Value, acidgroup.Attribute("base2acid").Value });

				}
				foreach (string smarts in smarts_l)
				{
					try
					{
						s_indigo.loadSmarts(smarts);
					}
					catch (Exception ex)
					{
						messages.Add(new Tuple<string, string>("Smarts did not load: " + smarts, ex.Message));
						returnRes = false;
					}
				}
				foreach (string smirks in smirks_l)
				{
					try
					{
						s_indigo.loadReactionSmarts(smirks);
					}
					catch (Exception ex)
					{
						messages.Add(new Tuple<string, string>("Reaction Smarts did not load: " + smirks, ex.Message));
						returnRes = false;
					}
				}
				return returnRes;

			}
			catch (Exception ex)
			{
				messages.Add(new Tuple<string, string>(ex.Message, null));
				return false;
			}
		}
	}
*/}
