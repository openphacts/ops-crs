namespace RSC.Compounds.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class testtable7 : DbMigration
    {
        public override void Up()
        {
            DropPrimaryKey("dbo.TestTable2");
            AlterColumn("dbo.TestTable2", "Id", c => c.Int(nullable: false, identity: true));
            AddPrimaryKey("dbo.TestTable2", "Id");
        }
        
        public override void Down()
        {
            DropPrimaryKey("dbo.TestTable2");
            AlterColumn("dbo.TestTable2", "Id", c => c.Int(nullable: false));
            AddPrimaryKey("dbo.TestTable2", "Id");
        }
    }
}
