namespace RSC.Compounds.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class AddindextoExternalIdentiferinSubstances : DbMigration
    {
        public override void Up()
        {
            DropIndex("dbo.Substances", new[] { "DataSourceId" });
            AlterColumn("dbo.Substances", "ExternalIdentifier", c => c.String(nullable: false, maxLength: 200));
            CreateIndex("dbo.Substances", new[] { "DataSourceId", "ExternalIdentifier" }, unique: true, name: "ExternalIdentifier_DataSourceId_idx");
        }
        
        public override void Down()
        {
            DropIndex("dbo.Substances", "ExternalIdentifier_DataSourceId_idx");
            AlterColumn("dbo.Substances", "ExternalIdentifier", c => c.String(nullable: false));
            CreateIndex("dbo.Substances", "DataSourceId");
        }
    }
}
