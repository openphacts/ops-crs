using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.Properties
{
    public class Unit
    {
        public string Name { get; set; }
        public string Description { get; set; }
        public Unit BaseUnit { get; set; }
        public string BaseUnitConversion { get; set; }
    }
}
