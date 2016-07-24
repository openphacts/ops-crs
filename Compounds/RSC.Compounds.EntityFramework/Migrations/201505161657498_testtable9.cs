namespace RSC.Compounds.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class testtable9 : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.TestTables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        One = c.Int(nullable: false),
                        Two = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
        
        public override void Down()
        {
            DropTable("dbo.TestTables");
        }
    }
}
