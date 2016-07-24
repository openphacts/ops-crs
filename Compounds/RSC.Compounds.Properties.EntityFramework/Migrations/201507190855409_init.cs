namespace RSC.Compounds.Properties.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Companies",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Address = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.PropertyValues",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        RecordId = c.Int(nullable: false),
                        PropertyId = c.Int(nullable: false),
                        IntValue = c.Int(),
                        FloatValue = c.Single(),
                        TextValue = c.String(),
                        ErrorValue = c.Single(),
                        UnitId = c.Int(),
                        CompanyId = c.Int(),
                        SoftwareInstrumentId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Companies", t => t.CompanyId)
                .ForeignKey("dbo.Properties", t => t.PropertyId, cascadeDelete: true)
                .ForeignKey("dbo.SoftwareInstruments", t => t.SoftwareInstrumentId)
                .ForeignKey("dbo.Units", t => t.UnitId)
                .Index(t => t.PropertyId)
                .Index(t => t.UnitId)
                .Index(t => t.CompanyId)
                .Index(t => t.SoftwareInstrumentId);
            
            CreateTable(
                "dbo.Properties",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Type = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.SoftwareInstruments",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        Version = c.String(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Units",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(),
                        Description = c.String(),
                        BaseUnitId = c.Int(),
                        BaseUnitConversion = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Units", t => t.BaseUnitId)
                .Index(t => t.BaseUnitId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.PropertyValues", "UnitId", "dbo.Units");
            DropForeignKey("dbo.Units", "BaseUnitId", "dbo.Units");
            DropForeignKey("dbo.PropertyValues", "SoftwareInstrumentId", "dbo.SoftwareInstruments");
            DropForeignKey("dbo.PropertyValues", "PropertyId", "dbo.Properties");
            DropForeignKey("dbo.PropertyValues", "CompanyId", "dbo.Companies");
            DropIndex("dbo.Units", new[] { "BaseUnitId" });
            DropIndex("dbo.PropertyValues", new[] { "SoftwareInstrumentId" });
            DropIndex("dbo.PropertyValues", new[] { "CompanyId" });
            DropIndex("dbo.PropertyValues", new[] { "UnitId" });
            DropIndex("dbo.PropertyValues", new[] { "PropertyId" });
            DropTable("dbo.Units");
            DropTable("dbo.SoftwareInstruments");
            DropTable("dbo.Properties");
            DropTable("dbo.PropertyValues");
            DropTable("dbo.Companies");
        }
    }
}
