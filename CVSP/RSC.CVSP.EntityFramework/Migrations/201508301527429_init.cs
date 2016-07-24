namespace RSC.CVSP.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Annotations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Title = c.String(nullable: false),
                        IsRequired = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.Name, unique: true);
            
            CreateTable(
                "dbo.Depositions",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Guid = c.Guid(nullable: false),
                        DatasourceId = c.Guid(nullable: false),
                        DateSubmitted = c.DateTime(nullable: false),
                        DateReprocessed = c.DateTime(),
                        Status = c.Int(nullable: false),
                        DataDomain = c.Int(nullable: false),
                        IsPublic = c.Boolean(nullable: false),
                        UserProfileId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserProfiles", t => t.UserProfileId, cascadeDelete: true)
                .Index(t => t.Guid, unique: true)
                .Index(t => t.UserProfileId);
            
            CreateTable(
                "dbo.DepositionFiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Guid = c.Guid(nullable: false),
                        Name = c.String(nullable: false, maxLength: 100),
                        DepositionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Depositions", t => t.DepositionId, cascadeDelete: true)
                .Index(t => t.DepositionId);
            
            CreateTable(
                "dbo.Fields",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 200),
                        FileId = c.Int(nullable: false),
                        AnnotationId = c.Int(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Annotations", t => t.AnnotationId)
                .ForeignKey("dbo.DepositionFiles", t => t.FileId, cascadeDelete: true)
                .Index(t => t.Name)
                .Index(t => t.FileId)
                .Index(t => t.AnnotationId);
            
            CreateTable(
                "dbo.RecordFields",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        FieldId = c.Int(nullable: false),
                        Value = c.String(),
                        RecordId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Records", t => t.RecordId, cascadeDelete: true)
                .ForeignKey("dbo.Fields", t => t.FieldId)
                .Index(t => t.FieldId)
                .Index(t => t.RecordId);
            
            CreateTable(
                "dbo.Records",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExternalId_DomainId = c.Int(),
                        ExternalId_ObjectId = c.Guid(),
                        Ordinal = c.Int(nullable: false),
                        SubmissionDate = c.DateTime(nullable: false),
                        RevisionDate = c.DateTime(),
                        DataDomain = c.Int(nullable: false),
                        Original = c.String(nullable: false),
                        Standardized = c.String(),
                        DepositionId = c.Int(nullable: false),
                        FileId = c.Int(nullable: false),
                        Dynamic = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.DepositionFiles", t => t.FileId, cascadeDelete: true)
                .ForeignKey("dbo.Depositions", t => t.DepositionId)
                .Index(t => t.DepositionId)
                .Index(t => t.FileId);
            
            CreateTable(
                "dbo.Issues",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 10),
                        LogId = c.Guid(nullable: false),
                        RecordId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Records", t => t.RecordId, cascadeDelete: true)
                .Index(t => t.Code)
                .Index(t => t.RecordId);
            
            CreateTable(
                "dbo.Properties",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PropertyId = c.Guid(nullable: false),
                        RecordId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Records", t => t.RecordId, cascadeDelete: true)
                .Index(t => t.PropertyId)
                .Index(t => t.RecordId);
            
            CreateTable(
                "dbo.ProcessingParameters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Value = c.String(nullable: false),
                        DepositionId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Depositions", t => t.DepositionId, cascadeDelete: true)
                .Index(t => t.Name)
                .Index(t => t.DepositionId);
            
            CreateTable(
                "dbo.UserProfiles",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Guid = c.Guid(nullable: false),
                        SendEmail = c.Boolean(nullable: false),
                        FtpDirectory = c.String(),
                        Datasource = c.Guid(),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.RuleSets",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Guid = c.Guid(nullable: false),
                        RuleType = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        DateRevised = c.DateTime(),
                        Title = c.String(nullable: false, maxLength: 100),
                        Description = c.String(nullable: false),
                        Body = c.String(nullable: false),
                        IsDefault = c.Boolean(nullable: false),
                        IsApproved = c.Boolean(nullable: false),
                        IsPublic = c.Boolean(nullable: false),
                        CountOfCloned = c.Int(nullable: false),
                        UserProfile_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserProfiles", t => t.UserProfile_Id, cascadeDelete: true)
                .Index(t => t.UserProfile_Id);
            
            CreateTable(
                "dbo.Collaborators",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        UserGuid = c.Guid(nullable: false),
                        RuleSet_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.RuleSets", t => t.RuleSet_Id, cascadeDelete: true)
                .Index(t => t.RuleSet_Id);
            
            CreateTable(
                "dbo.UserVariables",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 100),
                        Value = c.String(nullable: false, maxLength: 4000),
                        Description = c.String(nullable: false, maxLength: 4000),
                        UserProfile_Id = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.UserProfiles", t => t.UserProfile_Id, cascadeDelete: true)
                .Index(t => t.UserProfile_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Depositions", "UserProfileId", "dbo.UserProfiles");
            DropForeignKey("dbo.UserVariables", "UserProfile_Id", "dbo.UserProfiles");
            DropForeignKey("dbo.RuleSets", "UserProfile_Id", "dbo.UserProfiles");
            DropForeignKey("dbo.Collaborators", "RuleSet_Id", "dbo.RuleSets");
            DropForeignKey("dbo.Records", "DepositionId", "dbo.Depositions");
            DropForeignKey("dbo.ProcessingParameters", "DepositionId", "dbo.Depositions");
            DropForeignKey("dbo.DepositionFiles", "DepositionId", "dbo.Depositions");
            DropForeignKey("dbo.Records", "FileId", "dbo.DepositionFiles");
            DropForeignKey("dbo.RecordFields", "FieldId", "dbo.Fields");
            DropForeignKey("dbo.RecordFields", "RecordId", "dbo.Records");
            DropForeignKey("dbo.Properties", "RecordId", "dbo.Records");
            DropForeignKey("dbo.Issues", "RecordId", "dbo.Records");
            DropForeignKey("dbo.Fields", "FileId", "dbo.DepositionFiles");
            DropForeignKey("dbo.Fields", "AnnotationId", "dbo.Annotations");
            DropIndex("dbo.UserVariables", new[] { "UserProfile_Id" });
            DropIndex("dbo.Collaborators", new[] { "RuleSet_Id" });
            DropIndex("dbo.RuleSets", new[] { "UserProfile_Id" });
            DropIndex("dbo.ProcessingParameters", new[] { "DepositionId" });
            DropIndex("dbo.ProcessingParameters", new[] { "Name" });
            DropIndex("dbo.Properties", new[] { "RecordId" });
            DropIndex("dbo.Properties", new[] { "PropertyId" });
            DropIndex("dbo.Issues", new[] { "RecordId" });
            DropIndex("dbo.Issues", new[] { "Code" });
            DropIndex("dbo.Records", new[] { "FileId" });
            DropIndex("dbo.Records", new[] { "DepositionId" });
            DropIndex("dbo.RecordFields", new[] { "RecordId" });
            DropIndex("dbo.RecordFields", new[] { "FieldId" });
            DropIndex("dbo.Fields", new[] { "AnnotationId" });
            DropIndex("dbo.Fields", new[] { "FileId" });
            DropIndex("dbo.Fields", new[] { "Name" });
            DropIndex("dbo.DepositionFiles", new[] { "DepositionId" });
            DropIndex("dbo.Depositions", new[] { "UserProfileId" });
            DropIndex("dbo.Depositions", new[] { "Guid" });
            DropIndex("dbo.Annotations", new[] { "Name" });
            DropTable("dbo.UserVariables");
            DropTable("dbo.Collaborators");
            DropTable("dbo.RuleSets");
            DropTable("dbo.UserProfiles");
            DropTable("dbo.ProcessingParameters");
            DropTable("dbo.Properties");
            DropTable("dbo.Issues");
            DropTable("dbo.Records");
            DropTable("dbo.RecordFields");
            DropTable("dbo.Fields");
            DropTable("dbo.DepositionFiles");
            DropTable("dbo.Depositions");
            DropTable("dbo.Annotations");
        }
    }
}
