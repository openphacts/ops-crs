using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
    public static class IssueFactory
    {
        static Dictionary<string, int> s_issue_pattern;

        static IssueFactory()
        {
            s_issue_pattern = new Dictionary<string, int>();
            s_issue_pattern.Add("molfile loader", Convert.ToInt32(Issues.Code.indigo_molfile_loader));
            s_issue_pattern.Add("molecule", Convert.ToInt32(Issues.Code.indigo_molecule));
            s_issue_pattern.Add("element", Convert.ToInt32(Issues.Code.indigo_element));
            s_issue_pattern.Add("stereocenters", Convert.ToInt32(Issues.Code.indigo_stereocenters_other));
            s_issue_pattern.Add("indigo-inchi", Convert.ToInt32(Issues.Code.indigo_inchi));
            s_issue_pattern.Add("non-unique dearomatization", Convert.ToInt32(Issues.Code.non_unique_dearomatization));
            s_issue_pattern.Add("SMILES saver", Convert.ToInt32(Issues.Code.indigo_smiles_saver));
            s_issue_pattern.Add("allene stereo", Convert.ToInt32(Issues.Code.indigo_allene_stereo));

            s_issue_pattern.Add("SMILES loader", Convert.ToInt32(Issues.Code.indigo_smiles_loader));
            s_issue_pattern.Add("Dearomatization groups", Convert.ToInt32(Issues.Code.indigo_dearomatization_groups));
            s_issue_pattern.Add("Molecule automorphism search timeout", Convert.ToInt32(Issues.Code.indigo_automorphism_search_timeout));
        }

        public static List<Issue> FromCode(Issues.Code code)
        {
            return new List<Issue>() { new Issue()
            {
                Code = (int)code,
                Severity = Issues.CodeSeverityIssue[code].Item2,
                Type = Issues.CodeSeverityIssue[code].Item1,
                Message = Issues.CodeSeverityIssue[code].Item3
            } };
        }

        public static List<Issue> FromCode(Issues.Code code, string description)
        {
            return new List<Issue>() { new Issue()
            {
                Code = (int)code,
                Description = description,
                Severity = Issues.CodeSeverityIssue[code].Item2,
                Type = Issues.CodeSeverityIssue[code].Item1,
                Message = Issues.CodeSeverityIssue[code].Item3
            } };
        }

        public static List<Issue> FromCode(Issues.Code code, string description, Exception exception)
        {
            return new List<Issue>() { new Issue()
            {
                Code = (int)code,
                Description = description,
                Exception = exception.StackTrace,
                Severity = Issues.CodeSeverityIssue[code].Item2,
                Type = Issues.CodeSeverityIssue[code].Item1,
                Message = Issues.CodeSeverityIssue[code].Item3
            } };
        }

        public static List<Issue> FromCode(Issues.Code code, Exception exception)
        {
            return new List<Issue>() { new Issue()
            {
                Code = (int)code,
                Description = exception.Message,
                Exception = exception.StackTrace,
                Severity = Issues.CodeSeverityIssue[code].Item2,
                Type = Issues.CodeSeverityIssue[code].Item1,
                Message = Issues.CodeSeverityIssue[code].Item3
            } };
        }

        public static List<Issue> FromException(Exception exception)
        {
            return new List<Issue>() { new Issue()
            {
                Code = (int)Issues.Code.validationExceptionCodes,
                Message = exception.Message,
                Exception = exception.StackTrace,
                Severity = Severity.Error,
                Type = IssueType.Validation
            } };
        }

        public static List<Issue> FromIndigoException(Exception ex)
        {
            if (ex.Message.Contains(":"))
            {
                string[] message = ex.Message.Split(':');
                string title = message[0].Trim();
                string description = message[1].Trim();
                if (s_issue_pattern.ContainsKey(title))
                {
                    if (title.Contains("stereocenters"))
                    {
                        if (String.IsNullOrEmpty(description))
                            return IssueFactory.FromCode(Issues.Code.indigo_stereocenters_other,
                                description, ex);
                        else if (description.Contains("angle between bonds is too small"))
                            return IssueFactory.FromCode(Issues.Code.indigo_stereocenters_angle_too_small,
                                description, ex);
                        else if (description.Contains("stereo types of the opposite bonds mismatch"))
                            return IssueFactory.FromCode(Issues.Code.indigo_stereocenters_stereotypes_of_the_opposite_bonds_mismatch,
                                description, ex);
                        else if (description.Contains("stereo types of non-opposite bonds match"))
                            return IssueFactory.FromCode(Issues.Code.indigo_stereocenters_stereotypes_of_non_opposite_bonds_match,
                                description, ex);
                        else if (description.Contains("one bond up, one bond down"))
                            return IssueFactory.FromCode(Issues.Code.indigo_stereocenters_one_bond_up_one_bond_down,
                                description, ex);

                        else if (description.Contains("2 hydrogens near stereocenter"))
                            return IssueFactory.FromCode(Issues.Code.indigo_stereocenters_2_hydrogens_near_stereocenters,
                                description, ex);
                        else if (description.Contains("have hydrogen(s) besides implicit hydrogen near stereocenter"))
                        {
                            return IssueFactory.FromCode(Issues.Code.indigo_stereocenters_have_hydrogen_besides_implicit_hydrogen_near_stereocenter,
                                description, ex);
                        }
                        else if (description.Contains("degenerate case") && description.Contains("bonds overlap"))
                        {
                            return IssueFactory.FromCode(Issues.Code.indigo_stereocenters_DegenerateCase,
                                description, ex);
                        }
                        else
                            return IssueFactory.FromCode(Issues.Code.indigo_stereocenters_other,
                                description, ex);
                    }
                    else if (title.Contains("molecule"))
                    {
                        if (description.Contains("getAtomValence() does not work on pseudo-atoms"))
                        {
                            return IssueFactory.FromCode(Issues.Code.indigo_molecule, description, ex);
                        }
                        else return IssueFactory.FromCode(Issues.Code.validationExceptionCodes, description, ex);
                    }
                    else if (title.Contains("molfile loader") && description.Contains("direction of bond") && description.Contains("makes no sense"))
                    {
                        //do nothing - adding our own modules to handle these cases
                        return new List<Issue>();
                    }
                    else if (title.Contains("allene stereo"))
                    {
                        //do nothing - adding our own modules to handle these cases
                        return new List<Issue>();
                    }
                    else if (title.Contains("SMILES saver")) return IssueFactory.FromCode(Issues.Code.indigo_smiles_saver);
                    else if (title.Contains("non-unique dearomatization")) return IssueFactory.FromCode(Issues.Code.non_unique_dearomatization);
                    else
                    {
                        Issues.Code issue_code;
                        Enum.TryParse(s_issue_pattern[title].ToString(), out issue_code);
                        return IssueFactory.FromException(ex);
                    }
                }
                return IssueFactory.FromException(ex);
            }
            else
            {
                return IssueFactory.FromException(ex);
            }
        }
    }
}
