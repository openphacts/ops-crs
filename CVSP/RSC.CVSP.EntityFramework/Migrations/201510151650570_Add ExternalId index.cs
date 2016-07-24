namespace RSC.CVSP.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddExternalIdindex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Records", "ExternalId_ObjectId");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Records", new[] { "ExternalId_ObjectId" });
        }
    }
}
