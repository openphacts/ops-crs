using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using RSC.Logging;

namespace RSC.Compounds.DataExport
{
    public static class LogEntryExtensions
    {
        /// <summary>
        /// Translation of the issue severity and the issue type to the Issue predicate.
        /// </summary>
        /// <returns>A string representing the Issue Predicate.</returns>
        public static string GetIssuePredicate(this LogEntry issue, LogEntryType logEntryType, IEnumerable<LogCategory> logCategories)
        {
            Severity issueSeverity = issue.Severity ?? Severity.Information;
            LogCategory issueCategory = logCategories.Single(c => c.Id == logEntryType.CategoryId);
            //Uses the cheminf ontology.
            string predicate;
            switch (issueCategory.ShortName)
            {
                case "vld.fmt":
                    //Parsing.
                    switch (issueSeverity)
                    {
                        case Severity.Information:
                            predicate = Turtle.cheminf_issueParsingInfo;
                            break;
                        case Severity.Warning:
                            predicate = Turtle.cheminf_issueParsingWarning;
                            break;
                        case Severity.Fatal:
                        case Severity.Error:
                            predicate = Turtle.cheminf_issueParsingError;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("issueSeverity", issueSeverity, null);
                    }
                    break;
                case "prc.pred":
                case "prc.chm":
                    //Processing.
                    switch (issueSeverity)
                    {
                        case Severity.Information:
                            predicate = Turtle.cheminf_issueProcessingInfo;
                            break;
                        case Severity.Warning:
                            predicate = Turtle.cheminf_issueProcessingWarning;
                            break;
                        case Severity.Fatal:
                        case Severity.Error:
                            predicate = Turtle.cheminf_issueProcessingError;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("issueSeverity", issueSeverity, null);
                    }
                    break;
                case "indigo":
                case "vld.chm":
                    //Validation.
                    switch (issueSeverity)
                    {
                        case Severity.Information:
                            predicate = Turtle.cheminf_issueValidationInfo;
                            break;
                        case Severity.Warning:
                            predicate = Turtle.cheminf_issueValidationWarning;
                            break;
                        case Severity.Error:
                        case Severity.Fatal:
                            predicate = Turtle.cheminf_issueValidationError;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("issueSeverity", issueSeverity, null);
                    }
                    break;
                case "std.charge":
                case "std.chm":
                case "std.layout":
                    //Standardization.
                    switch (issueSeverity)
                    {
                        case Severity.Information:
                            predicate = Turtle.cheminf_issueStandardizationInfo;
                            break;
                        case Severity.Warning:
                            predicate = Turtle.cheminf_issueStandardizationWarning;
                            break;
                        case Severity.Error:
                        case Severity.Fatal:
                            predicate = Turtle.cheminf_issueStandardizationError;
                            break;
                        default:
                            throw new ArgumentOutOfRangeException("issueSeverity", issueSeverity, null);
                    }
                    break;
                default:
                    predicate = Turtle.cheminf_issueDefault;
                    break;
            }

            //Append the cheminf ontology prefix.
            if (!string.IsNullOrEmpty(predicate))
            {
                predicate = string.Format("{0}:{1}", "cheminf", predicate);
            }
            return predicate;
        }

        public static string ToTurtleLine(this LogEntry le, IEnumerable<LogEntryType> logEntryTypes, IEnumerable<LogCategory> logCategories, string recordUri)
        {
            var logEntryType = logEntryTypes.Single(l => l.Id == le.TypeId);
            string issuePredicate = le.GetIssuePredicate(logEntryType, logCategories);
            return String.Format("{0} {1} \"{2}{3}\"@en ."
                            , recordUri                                                                //0
                            , issuePredicate                                                        //1
                            , logEntryType.Title.RdfEncode()                           //2
                            , string.IsNullOrEmpty(le.Message)
                                ? string.Empty
                                : string.Format("; {0}", le.Message.RdfEncode()));  //3
        }
    }
}
