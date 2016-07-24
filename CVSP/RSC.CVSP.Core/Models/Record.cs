using RSC.Compression;
using RSC.Logging;
using RSC.Properties;
using System;
using System.Collections.Generic;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;

namespace RSC.CVSP
{
	[Serializable]
	[DataContract]
	public class Record : DynamicObject
	{
		[DataMember]
		public ExternalId Id { get; set; }

		[DataMember]
		public int Ordinal { get; set; }

		[DataMember]
		public Guid DepositionId { get; set; }

		[DataMember]
		public DepositionFile File { get; set; }

		[DataMember]
		public DataDomain DataDomain { get; set; }

		/// <summary>
		/// Depositor specified annotations
		/// </summary>
		[IgnoreDataMember]
		public string RegId
		{
			get
			{
				//	try to find 
				if (File != null && File.Fields != null && File.Fields.Any(f => f.Annotaition != null && f.Annotaition.Name == "ExtId"))
				{
					var field = File.Fields.Where(f => f.Annotaition != null && f.Annotaition.Name == "ExtId").Single();

					if (Fields == null || !Fields.Any(f => f.Name == field.Name))
						return null;

					return Fields.Where(f => f.Name == field.Name).Single().Value;
				}

				return null;
			}
		}

		[DataMember]
		public DateTime SubmissionDate { get; set; }

		[DataMember]
		public DateTime? RevisionDate { get; set; }

		[DataMember]
		public string Original { get; set; }

        /// <summary>
        /// This is a connection table.
        /// </summary>
		[DataMember]
		public string Standardized { get; set; }

		[DataMember]
		public IEnumerable<RecordField> Fields { get; set; }

		[DataMember]
		public IEnumerable<Issue> Issues { get; set; }

		[DataMember]
		public IEnumerable<Property> Properties { get; set; }

		[DataMember]
		public IEnumerable<Guid> PropertyIDs { get; set; }

		public Record()
		{
			Issues = new List<Issue>();
			DataDomain = DataDomain.Unidentified;
			File = new DepositionFile();
			Dynamic = new List<DynamicMember>();
		}

		#region Dynamic Part

		[DataMember]
		public ICollection<DynamicMember> Dynamic { get; set; }

		[OnDeserializing]
		void OnDeserializing(StreamingContext c)
		{
			Dynamic = new List<DynamicMember>();
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			if (Dynamic.Where(m => m.Name == binder.Name).Any())
				Dynamic.Where(m => m.Name == binder.Name).Single().Member = value;
			else
				Dynamic.Add(new DynamicMember() { Name = binder.Name, Member = value });

			return true;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			if (HasDynamicMember(binder.Name))
			{
				result = Dynamic.Where(m => m.Name == binder.Name).Single().Member;
				return true;
			}

			return base.TryGetMember(binder, out result);
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return Dynamic.Select(m => m.Name);
		}

		public bool HasDynamicMember(string name)
		{
			return GetDynamicMemberNames().Contains(name);
		}

		#endregion

		public byte[] Serialize()
		{
			BinaryFormatter bf = new BinaryFormatter();
			using (MemoryStream ms = new MemoryStream())
			{
				bf.Serialize(ms, this);

				return ms.ToArray().GZipCompress();
			}
		}

		public static Record Deserialize(byte[] bytes)
		{
			var decompressed = bytes.GZipDecompress();

			using (MemoryStream ms = new MemoryStream())
			{
				BinaryFormatter bf = new BinaryFormatter();
				ms.Write(decompressed, 0, decompressed.Length);
				ms.Seek(0, SeekOrigin.Begin);
				return bf.Deserialize(ms) as Record;
			}
		}
	}
}
