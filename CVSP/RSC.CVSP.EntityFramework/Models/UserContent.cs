namespace CVSPEntityFramework
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Spatial;

	[Table("user_content")]
    public partial class EFUserContent
    {
        public EFUserContent()
        {
            
            
        }

		[DatabaseGenerated(DatabaseGeneratedOption.Identity), Key()]
		public int Id { get; set; }

		[DatabaseGenerated(DatabaseGeneratedOption.Computed)]
		public Guid Guid { get; set; }

		public UserContentType ContentType { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime? DateRevised { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string ContentBody { get; set; }

        public bool? IsPlatformDefault { get; set; }

        public bool? IsValidated { get; set; }

        public string XmlValidationMessages { get; set; }

        public bool? IsApproved { get; set; }

        public bool? IsSharedWithEverybody { get; set; }

        public int? CountOfCloned { get; set; }

		public virtual ICollection<ContentCollaborator> Collaboraters { get; set; }
		
    }
}
