using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Runtime.Serialization;

namespace RSC.CVSP
{
	[DataContract]
	public class DepositionFile
	{
		[Display(Name = "ID")]
		[DataMember]
		public Guid Id { get; set; }

		[Display(Name = "File Name")]
		[DataMember]
		public string Name { get; set; }
		[DataMember]
		public IEnumerable<Field> Fields { get; set; }
	}

	public class DepositionFileComparer : IEqualityComparer<DepositionFile>
	{
		bool IEqualityComparer<DepositionFile>.Equals(DepositionFile x, DepositionFile y)
		{
			if (Object.ReferenceEquals(x, y)) return true;

			if (Object.ReferenceEquals(x, null) ||
				Object.ReferenceEquals(y, null))
				return false;

			return x.Name == y.Name;
		}

		int IEqualityComparer<DepositionFile>.GetHashCode(DepositionFile file)
		{
			if (Object.ReferenceEquals(file, null)) return 0;

			return file.Name.GetHashCode();
		}
	}
}
