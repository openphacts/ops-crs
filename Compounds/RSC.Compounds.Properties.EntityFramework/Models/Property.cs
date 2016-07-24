using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace RSC.Compounds.Properties.EntityFramework
{
	public enum PropertyType
	{
		Predicted,
		Experimental
	}
    public class Property
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public PropertyType Type { get; set; }
        public virtual ICollection<PropertyValue> Values { get; set; }
    }
}
