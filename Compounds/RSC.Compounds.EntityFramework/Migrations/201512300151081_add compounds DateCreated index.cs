namespace RSC.Compounds.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class addcompoundsDateCreatedindex : DbMigration
    {
        public override void Up()
        {
            CreateIndex("dbo.Compounds", "DateCreated");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Compounds", new[] { "DateCreated" });
        }
    }
}
