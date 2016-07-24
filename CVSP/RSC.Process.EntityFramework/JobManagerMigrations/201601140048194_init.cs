namespace RSC.Process.EntityFramework.JobManagerMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Jobs",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Created = c.DateTime(nullable: false),
                        Started = c.DateTime(),
                        Finished = c.DateTime(),
                        Status = c.Int(nullable: false),
                        Error = c.String(),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.ExternalId);
            
            CreateTable(
                "dbo.JobParameters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Value = c.String(nullable: false),
                        JobId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Jobs", t => t.JobId, cascadeDelete: true)
                .Index(t => t.JobId);
            
            CreateTable(
                "dbo.JobWatches",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Begin = c.DateTime(nullable: false),
                        End = c.DateTime(nullable: false),
                        JobId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Jobs", t => t.JobId, cascadeDelete: true)
                .Index(t => t.JobId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.JobWatches", "JobId", "dbo.Jobs");
            DropForeignKey("dbo.JobParameters", "JobId", "dbo.Jobs");
            DropIndex("dbo.JobWatches", new[] { "JobId" });
            DropIndex("dbo.JobParameters", new[] { "JobId" });
            DropIndex("dbo.Jobs", new[] { "ExternalId" });
            DropTable("dbo.JobWatches");
            DropTable("dbo.JobParameters");
            DropTable("dbo.Jobs");
        }
    }
}
