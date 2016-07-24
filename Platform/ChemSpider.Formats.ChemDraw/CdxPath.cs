using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace MoleculeObjects
{
    /// <summary>
    /// This class provides strings for doing regexes and the extension methods
    /// on Cdx and CdxEntity.
    /// 
    /// If it gets too complicated and manky (always a danger) we should use fslex/fsyacc.
    /// </summary>
    public static class CdxPath
    {
        public static string plain = "^(?<obj>[A-Za-z0-9_]*)$";
        public static string predicateTest = @"^(?<obj>[A-Za-z0-9_]*)\[(?<pred>.*)\]$";
        public static string negativePredicate = @"^not\((?<pred>.*)\)$";
        public static string booleanPredicate = @"^(?<pred1>.*)\s(?<op>and|or)\s(?<pred2>.*)$";
        public static string axisPredicate = @"^(?<axis>(parent|preceding-sibling|following-sibling)::)(?<pred>.*)$";
        public static string propertyExistsPredicate = @"^@(?<propname>[A-Za-z0-9_]*)$";
        public static string propertyValuePredicate = @"^@(?<propname>[A-Za-z0-9_]*)=(?<propvalue>[A-Za-z0-9]+)$";

        public static List<CdxObject> CdxPathSelectObjects(this Cdx cdx, string cdxpath)
        {
            return cdx.Root.CdxPathSelectObjects(cdxpath, 0);
        }

        public static List<CdxObject> CdxPathSelectObjects(this CdxObject o, string cdxpath)
        {
            return o.CdxPathSelectObjects(cdxpath, 0);
        }

        public static bool CdxPath_SelfMatchesPredicate(this CdxObject o, string predicate)
        {
            Match mBool = Regex.Match(predicate, CdxPath.booleanPredicate);
            if (mBool.Success)
            {
                bool firstResult = o.CdxPath_SelfMatchesPredicate(mBool.Groups["pred1"].Value);
                bool secondResult = o.CdxPath_SelfMatchesPredicate(mBool.Groups["pred2"].Value);
                switch (mBool.Groups["op"].Value)
                {
                    case "and":
                        return firstResult && secondResult;
                    case "or":
                        return firstResult || secondResult;
                    default:
                        throw new MoleculeException("unimplemented boolean operator " + mBool.Groups["op"].Value);
                }
            }
            else
            {
                Match mNeg = Regex.Match(predicate, CdxPath.negativePredicate);
                if (mNeg.Success)
                {
                    return !o.CdxPath_SelfMatchesPredicate(mNeg.Groups["pred"].Value);
                }
                else
                {
                    Match mChildExists = Regex.Match(predicate, CdxPath.plain, RegexOptions.ExplicitCapture);
                    Match mPropertyExists = Regex.Match(predicate, CdxPath.propertyExistsPredicate, RegexOptions.ExplicitCapture);
                    Match mPropertyTest = Regex.Match(predicate, CdxPath.propertyValuePredicate, RegexOptions.ExplicitCapture);
                    Match mAxisTest = Regex.Match(predicate, CdxPath.axisPredicate, RegexOptions.ExplicitCapture);

                    if (mChildExists.Success)
                    {
                        return (o.Objects.Where(oo => oo.TagName == mChildExists.Groups["obj"].Value).Count() > 0);
                    }
                    else if (mPropertyExists.Success)
                    {
                        return o.HasProperty(mPropertyExists.Groups["propname"].Value);
                    }
                    else if (mPropertyTest.Success)
                    {
                        string propname = mPropertyTest.Groups["propname"].Value;
                        string propvalue = mPropertyTest.Groups["propvalue"].Value;
                        return o.MatchesProperty(propname, propvalue);
                    }
                    else if (mAxisTest.Success)
                    {
                        string axis = mAxisTest.Groups["axis"].Value;
                        predicate = mAxisTest.Groups["pred"].Value;
                        switch (axis)
                        {
                            case "parent::":
                                return (predicate == "*") ? true : (predicate == o.Parent.TagName);
                            case "preceding-sibling::":
                                if (predicate == "*")
                                {
                                    return (o.CdxPathIndex > 1);
                                }
                                else
                                {
                                    IEnumerable<CdxObject> precedings = o.Parent.Objects.Where(oo => oo.CdxPathIndex < o.CdxPathIndex);
                                    return (precedings.Where(p => p.TagName == predicate).Count() > 0);
                                }
                            case "following-sibling::":
                                IEnumerable<CdxObject> followings = o.Parent.Objects.Where(oo => oo.CdxPathIndex > o.CdxPathIndex);
                                if (predicate == "*")
                                {
                                    return followings.Count() > 0;
                                }
                                else
                                {
                                    return (followings.Where(p => p.TagName == predicate).Count() > 0);
                                }
                            default:
                                throw new MoleculeException("Unsupported axis " + axis);
                        }
                    }
                    else
                    {
                        throw new MoleculeException("Unsupported predicate " + predicate);
                    }
                }
            }
        }

        public static List<CdxObject> CdxPathSelectObjects(this CdxObject o, string cdxpath, int indentationDepth)
        {
            string indent = "".PadLeft(indentationDepth);
            List<CdxObject> result = new List<CdxObject>();
            if (cdxpath.StartsWith("//"))
            {
                // search children for matches
                o.Objects.ForEach(oo => result.AddRange(oo.CdxPathSelectObjects(cdxpath, indentationDepth + 1)));
                cdxpath = cdxpath.Substring(2);
            }

            if (cdxpath.Contains("/"))
            {
                string thisNode = cdxpath.Substring(0, cdxpath.IndexOf("/"));

                if (o.CdxPath_NodeTestSucceeds(thisNode) || (thisNode.Length == 0))
                {
                    cdxpath = cdxpath.Substring(cdxpath.IndexOf("/") + 1);
                    o.Objects.ForEach(oo => result.AddRange(oo.CdxPathSelectObjects(cdxpath, indentationDepth + 1)));
                }
            }
            else
            {
                if (o.CdxPath_NodeTestSucceeds(cdxpath))
                    result.Add(o);
            }
            return result;
        }

        public static bool CdxPath_NodeTestSucceeds(this CdxObject o, string cdxpath)
        {
            Match mPlain = Regex.Match(cdxpath, CdxPath.plain, RegexOptions.ExplicitCapture);
            Match mPredicate = Regex.Match(cdxpath, CdxPath.predicateTest, RegexOptions.ExplicitCapture);

            if ( mPlain.Success ) {
                string objectName = mPlain.Groups["obj"].Value;
                return (o.TagName == objectName);
            }
            else if ( mPredicate.Success ) {
                string objectName = mPredicate.Groups["obj"].Value;
                string predicate = mPredicate.Groups["pred"].Value;
                return (o.TagName == objectName) && o.CdxPath_SelfMatchesPredicate(predicate);
            }
            else {
                throw new Exception("CdxPath " + cdxpath + " not interpretable (yet!)");
            }
        }
    }
}
