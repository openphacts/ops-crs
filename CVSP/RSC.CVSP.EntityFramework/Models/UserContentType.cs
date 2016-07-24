namespace CVSPEntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

	[Table("user_content_types")]
    public partial class UserContentType
    {
        public UserContentType()
        {
            
        }

		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		public int Type { get; set; }
        
        public string Description { get; set; }
        
    }
}
