namespace RSC.CVSP.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class RemoveindexfromPropertyId : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Properties", new[] { "PropertyId" });
        }
        
        public override void Down()
        {
            CreateIndex("dbo.Properties", "PropertyId");
        }
    }
}
