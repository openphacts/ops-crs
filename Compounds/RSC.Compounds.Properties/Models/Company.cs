using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RSC.Compounds.Properties
{
    public class Company
    {
        public string Name { get; set; }
        public string Address { get; set; }
    }
}
