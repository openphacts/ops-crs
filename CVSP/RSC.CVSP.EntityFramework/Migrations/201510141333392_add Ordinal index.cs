namespace RSC.CVSP.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addOrdinalindex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Records", "Ordinal");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Records", new[] { "Ordinal" });
        }
    }
}
