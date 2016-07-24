namespace RSC.Compounds.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class testtable8 : DbMigration
    {
        public override void Up()
        {
            DropTable("dbo.TestTable2");
        }
        
        public override void Down()
        {
            CreateTable(
                "dbo.TestTable2",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        One = c.Int(nullable: false),
                        Two = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
        }
    }
}
