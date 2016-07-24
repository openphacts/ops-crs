using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ChemSpider.Profile.Data.Models
{
    [Serializable]
    public class ChemSpiderUser
    {
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string DisplayName { get; set; }
        public DateTime CreatedDate { get; set; }
        public bool IsApproved { get; set; }
    }
}
