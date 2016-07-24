//using RSC.Properties;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Runtime.Serialization;
//using System.Text;
//using System.Threading.Tasks;

//namespace RSC.CVSP.Compounds
//{
//	[DataContract]
//	[Serializable]
//	public class ChemSpiderProperties
//	{
//		public ChemSpiderProperties()
//		{
//			Properties = new List<Property>();
//		}

//		[DataMember]
//		public ICollection<Property> Properties { get; set; }
//	}

//	public static class CompoundRecordPropertyExtensions
//	{
//		private const string DYNAMIC_PROPERTY_NAME = "ChemSpiderProperties";

//		public static ICollection<Property> GetChemSpiderProperties(this CompoundRecord compound)
//		{
//			if (!compound.HasDynamicMember(DYNAMIC_PROPERTY_NAME))
//				((dynamic)compound).ChemSpiderProperties = new ChemSpiderProperties();

//			return (((dynamic)compound).ChemSpiderProperties as ChemSpiderProperties).Properties;
//		}

//		public static void AddChemSpiderProperty(this CompoundRecord compound, string name, object value)
//		{
//			compound.AddChemSpiderProperties(new Property[] { new Property() { Name = name, Value = value } });
//		}

//		public static void AddChemSpiderProperties(this CompoundRecord compound, IEnumerable<Property> newProperties)
//		{
//			if (!compound.HasDynamicMember(DYNAMIC_PROPERTY_NAME))
//				((dynamic)compound).ChemSpiderProperties = new ChemSpiderProperties();

//			var properties = ((dynamic)compound).ChemSpiderProperties as ChemSpiderProperties;

//			properties.Properties = properties.Properties.Concat(newProperties).ToList();

//			((dynamic)compound).ChemSpiderProperties = properties;
//		}
//	}
//}
