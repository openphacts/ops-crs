using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections;
using System.Diagnostics;
using System.Runtime.Serialization;
using System.ComponentModel.DataAnnotations;

namespace ChemSpider.ObjectModel
{
	public enum Severity { Information = 0, Warning = 1, Error = 2, All = 3 }
	
	[DataContract]
	public class BasicIssue
	{
		[Display(Name = "Severity")]
		[DataMember]
		public Severity Severity { get; set; }

		[Display(Name = "Message")]
		[DataMember]
		public string Message { get; set; }
	}

	public enum IssueType
	{
		parsing = 0,//please do not change numbers as in OPS database these are the numbers. Will change on next reprocessing of OPS
		validation = 2,
		standardization = 3
	};

	[DataContract]
	public class Issue : BasicIssue
	{
		[Display(Name = "Type")]
		[DataMember]
		public IssueType IssueType { get; set; }

		[Display(Name = "Code")]
		[DataMember]
		public int Code { get; set; }

		[Display(Name = "Description")]
		[DataMember]
		public string Description { get; set; }

		[Display(Name = "Exception")]
		[DataMember]
		public string StackTrace { get; set; }

		public override bool Equals(Object obj)
		{
			if ( obj == null || GetType() != obj.GetType() )
				return false;
			Issue issue = (Issue)obj;
			return Code == issue.Code && Message == issue.Message;
		}
	}

	public class IssuesCollection : IEnumerable, IEnumerable<BasicIssue>
	{
		private List<BasicIssue> _issues = new List<BasicIssue>();

		public void Add(Severity severity, string format, params object[] args)
		{
			_issues.Add(new BasicIssue { Severity = severity, Message = String.Format(format, args) });
		}

		public bool IsError
		{
			get { return _issues.Exists(i => i.Severity == Severity.Error); }
		}

		public bool IsWarning
		{
			get { return _issues.Exists(i => i.Severity == Severity.Warning); }
		}

		public int Count
		{
			get { return _issues.Count; }
		}

		public IEnumerator GetEnumerator()
		{
			return _issues.GetEnumerator();
		}

		IEnumerator<BasicIssue> IEnumerable<BasicIssue>.GetEnumerator()
		{
			return _issues.GetEnumerator();
		}

		public override string ToString()
		{
			StringBuilder sb = new StringBuilder();
			foreach ( var i in _issues ) {
				sb.AppendFormat("{0}: {1}", i.Severity, i.Message);
				sb.AppendLine();
			}
			return sb.ToString();
		}
	}
	
}
