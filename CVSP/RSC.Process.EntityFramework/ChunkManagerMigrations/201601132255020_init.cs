namespace RSC.Process.EntityFramework.ChunkManagerMigrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class init : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.ChunkBlobs",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Data = c.Binary(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Chunks", t => t.Id)
                .Index(t => t.Id);
            
            CreateTable(
                "dbo.Chunks",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        ExternalId = c.Guid(nullable: false),
                        Status = c.Int(nullable: false),
                        NumberOfRecords = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .Index(t => t.ExternalId);
            
            CreateTable(
                "dbo.ChunkParameters",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Name = c.String(nullable: false, maxLength: 50),
                        Value = c.String(nullable: false),
                        ChunkId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Chunks", t => t.ChunkId, cascadeDelete: true)
                .Index(t => t.ChunkId);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.ChunkBlobs", "Id", "dbo.Chunks");
            DropForeignKey("dbo.ChunkParameters", "ChunkId", "dbo.Chunks");
            DropIndex("dbo.ChunkParameters", new[] { "ChunkId" });
            DropIndex("dbo.Chunks", new[] { "ExternalId" });
            DropIndex("dbo.ChunkBlobs", new[] { "Id" });
            DropTable("dbo.ChunkParameters");
            DropTable("dbo.Chunks");
            DropTable("dbo.ChunkBlobs");
        }
    }
}
