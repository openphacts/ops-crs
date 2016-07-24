using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace RSC.CVSP
{
	[Serializable]
	[DataContract]
	public class Field
	{
		[DataMember]
		public string Name { get; set; }

		[DataMember]
		public Annotation Annotaition { get; set; }
	}

	public class FieldComparer : IEqualityComparer<Field>
	{
		bool IEqualityComparer<Field>.Equals(Field x, Field y)
		{
			if (Object.ReferenceEquals(x, y)) return true;

			if (Object.ReferenceEquals(x, null) ||
				Object.ReferenceEquals(y, null))
				return false;

			return x.Name == y.Name;
		}

		int IEqualityComparer<Field>.GetHashCode(Field property)
		{
			if (Object.ReferenceEquals(property, null)) return 0;

			return property.Name.GetHashCode();
		}
	}
}
