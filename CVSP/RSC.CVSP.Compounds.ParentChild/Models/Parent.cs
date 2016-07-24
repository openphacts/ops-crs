using RSC.Compounds;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RSC.CVSP.Compounds
{
	[DataContract]
	[Serializable]
	public class Parent
	{
		[DataMember]
		public ParentChildRelationship Relationship { get; set; }

		[DataMember]
		public string MolFile { get; set; }

		[DataMember]
		public InChI StdInChI { get; set; }

		[DataMember]
		public InChI NonStdInChI { get; set; }

		[DataMember]
		public InChI TautomericInChI { get; set; }

		[DataMember]
		public string Smiles { get; set; }
	}

	public static class CompoundRecordExtensions
	{
		public static ICollection<Parent> GetParents(this CompoundRecord compound)
		{
			if (!compound.HasDynamicMember("Parents"))
				((dynamic)compound).Parents = new List<Parent>();

			return ((dynamic)compound).Parents as List<Parent>;
		}

		public static void AddParent(this CompoundRecord compound, Parent parent)
		{
			compound.AddParents(new Parent[] { parent });
		}

		public static void AddParents(this CompoundRecord compound, IEnumerable<Parent> newParents)
		{
			if (!compound.HasDynamicMember("Parents"))
				((dynamic)compound).Parents = new List<Parent>();

			var parents = ((dynamic)compound).Parents as List<Parent>;

			parents.AddRange(newParents);

			((dynamic)compound).Parents = parents;
		}
	}
}
