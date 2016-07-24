namespace RSC.Compounds.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ChangedExternalReferencesPKtoint : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.ExternalReferences");
            DropColumn("dbo.ExternalReferences", "Id");
            AddColumn("dbo.ExternalReferences", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.ExternalReferences", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.ExternalReferences");
            AlterColumn("dbo.ExternalReferences", "Id", c => c.Guid(nullable: false));
            AddPrimaryKey("dbo.ExternalReferences", "Id");
        }
    }
}
