using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds
{
	[DataContract]
	public class Statistics
	{
		/// <summary>
		/// Total number of compounds in the system
		/// </summary>
		[DataMember]
		public int CompoundsNumber { get; set; }

		/// <summary>
		/// Total number of datasources in the system
		/// </summary>
		[DataMember]
		public int DatasourcesNumber { get; set; }
	}
}
