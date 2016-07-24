using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.Properties
{
	public enum PropertyType {
		Predicted = 0,
		Experimental
	}

    public class Property
    {
        public string Name { get; set; }
        public PropertyType Type { get; set; }
		public object Value { get; set; }
		public object Error { get; set; }
		public Unit Unit { get; set; }
		public Company Company { get; set; }
		public SoftwareInstrument SoftwareInstrument { get; set; }
	}
}
