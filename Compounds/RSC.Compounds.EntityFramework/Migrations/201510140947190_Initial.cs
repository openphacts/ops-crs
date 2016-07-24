namespace RSC.Compounds.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;

    public partial class Initial : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Annotations",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Type = c.Int(nullable: false),
                        Value = c.String(nullable: false),
                        Revision_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Revisions", t => t.Revision_Id, cascadeDelete: true)
                .Index(t => t.Revision_Id);

            CreateTable(
                "dbo.Revisions",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DepositionId = c.Guid(nullable: false),
                        Version = c.Int(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        DateModified = c.DateTime(),
                        EmbargoDate = c.DateTime(),
                        Revoked = c.Boolean(nullable: false),
                        Sdf = c.String(),
                        SubstanceId = c.Guid(nullable: false),
                        CompoundId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id, clustered: false) // <-- Added "clustered: false" here
                .ForeignKey("dbo.Compounds", t => t.CompoundId, cascadeDelete: true)
                .ForeignKey("dbo.Substances", t => t.SubstanceId, cascadeDelete: true)
                .Index(t => t.DepositionId)
                .Index(t => t.SubstanceId)
                .Index(t => t.CompoundId);

            CreateTable(
                "dbo.Compounds",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        Mol = c.String(),
                        NonStandardInChIId = c.Guid(),
                        TautomericNonStdInChIId = c.Guid(),
                        StandardInChIId = c.Guid(),
                        SmilesId = c.Guid(),
                    })
                .PrimaryKey(t => t.Id, clustered: false) // <-- Added "clustered: false" here
                .ForeignKey("dbo.InChIs", t => t.NonStandardInChIId)
                .ForeignKey("dbo.Smiles", t => t.SmilesId)
                .ForeignKey("dbo.InChIs", t => t.StandardInChIId)
                .ForeignKey("dbo.InChIs", t => t.TautomericNonStdInChIId)
                .Index(t => t.NonStandardInChIId)
                .Index(t => t.TautomericNonStdInChIId)
                .Index(t => t.StandardInChIId)
                .Index(t => t.SmilesId);

            CreateTable(
                "dbo.ParentChildren",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Type = c.Int(nullable: false),
                        ParentId = c.Guid(nullable: false),
                        ChildId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id, clustered: false) // <-- Added "clustered: false" here
                .ForeignKey("dbo.Compounds", t => t.ChildId)
                .ForeignKey("dbo.Compounds", t => t.ParentId)
                .Index(t => t.Type)
                .Index(t => t.ParentId)
                .Index(t => t.ChildId);

            CreateTable(
                "dbo.CompoundSynonyms",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CompoundId = c.Guid(nullable: false),
                        SynonymId = c.Guid(nullable: false),
                        SynonymState = c.Int(nullable: false),
                        IsTitle = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id, clustered: false) // <-- Added "clustered: false" here
                .ForeignKey("dbo.Synonyms", t => t.SynonymId, cascadeDelete: true)
                .ForeignKey("dbo.Compounds", t => t.CompoundId, cascadeDelete: true)
                .Index(t => new { t.CompoundId, t.SynonymState }, name: "IX_CompoundIdAndSynonymState")
                .Index(t => t.SynonymId);

            CreateTable(
                "dbo.CompoundSynonymsSynonymFlag",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CompoundSynonymId = c.Guid(nullable: false),
                        SynonymFlagId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id, clustered: false) // <-- Added "clustered: false" here
                .ForeignKey("dbo.SynonymFlags", t => t.SynonymFlagId, cascadeDelete: true)
                .ForeignKey("dbo.CompoundSynonyms", t => t.CompoundSynonymId, cascadeDelete: true)
                .Index(t => t.CompoundSynonymId)
                .Index(t => t.SynonymFlagId);

            CreateTable(
                "dbo.SynonymFlags",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Name = c.String(nullable: false, maxLength: 20),
                        Description = c.String(maxLength: 200),
                        Url = c.String(maxLength: 200),
                        Rank = c.Int(nullable: false),
                        ExcludeFromTitle = c.Boolean(nullable: false),
                        Type = c.Int(nullable: false),
                        IsUniquePerLanguage = c.Boolean(nullable: false),
                        RegEx = c.String(),
                        IsRestricted = c.Boolean(nullable: false),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.CompoundSynonymHistory",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CompoundSynonymId = c.Guid(nullable: false),
                        SynonymState = c.Int(nullable: false),
                        IsTitle = c.Boolean(nullable: false),
                        DateChanged = c.DateTime(nullable: false),
                        CuratorId = c.Guid(),
                        RevisionId = c.Int(),
                    })
                .PrimaryKey(t => t.Id, clustered: false) // <-- Added "clustered: false" here
                .ForeignKey("dbo.CompoundSynonyms", t => t.CompoundSynonymId, cascadeDelete: true)
                .Index(t => t.CompoundSynonymId);

            CreateTable(
                "dbo.CompoundSynonymSynonymFlagHistory",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CompoundSynonymHistoryId = c.Guid(nullable: false),
                        SynonymFlagId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id, clustered: false) // <-- Added "clustered: false" here
                .ForeignKey("dbo.SynonymFlags", t => t.SynonymFlagId, cascadeDelete: true)
                .ForeignKey("dbo.CompoundSynonymHistory", t => t.CompoundSynonymHistoryId, cascadeDelete: true)
                .Index(t => t.CompoundSynonymHistoryId)
                .Index(t => t.SynonymFlagId);

            CreateTable(
                "dbo.Synonyms",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        Synonym = c.String(nullable: false, maxLength: 448),
                        LanguageId = c.String(nullable: false, maxLength: 2),
                        DateCreated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id, clustered: false) // <-- Added "clustered: false" here
                .Index(t => new { t.Synonym, t.LanguageId }, unique: true, name: "synonym_langid_idx");

            CreateTable(
                "dbo.SynonymHistory",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        SynonymId = c.Guid(nullable: false),
                        CuratorId = c.Guid(),
                        DateChanged = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id, clustered: false) // <-- Added "clustered: false" here
                .ForeignKey("dbo.Synonyms", t => t.SynonymId, cascadeDelete: true)
                .Index(t => t.SynonymId);

            CreateTable(
                "dbo.SynonymsSynonymFlagHistory",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        SynonymHistoryId = c.Guid(nullable: false),
                        SynonymFlagId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => t.Id, clustered: false) // <-- Added "clustered: false" here
                .ForeignKey("dbo.SynonymFlags", t => t.SynonymFlagId, cascadeDelete: true)
                .ForeignKey("dbo.SynonymHistory", t => t.SynonymHistoryId, cascadeDelete: true)
                .Index(t => t.SynonymHistoryId)
                .Index(t => t.SynonymFlagId);

            CreateTable(
                "dbo.SynonymsSynonymFlag",
                c => new
                    {
                        SynonymId = c.Guid(nullable: false),
                        SynonymFlagId = c.Int(nullable: false),
                    })
                .PrimaryKey(t => new { t.SynonymId, t.SynonymFlagId }, clustered: false) // <-- Added "clustered: false" here
                .ForeignKey("dbo.SynonymFlags", t => t.SynonymFlagId, cascadeDelete: true)
                .ForeignKey("dbo.Synonyms", t => t.SynonymId, cascadeDelete: true)
                .Index(t => t.SynonymId)
                .Index(t => t.SynonymFlagId);

            CreateTable(
                "dbo.ExternalReferences",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        CompoundId = c.Guid(nullable: false),
                        ExternalReferenceTypeId = c.Int(nullable: false),
                        Value = c.String(maxLength: 100),
                    })
                .PrimaryKey(t => t.Id, clustered: false) // <-- Added "clustered: false" here
                .ForeignKey("dbo.ExternalReferenceTypes", t => t.ExternalReferenceTypeId, cascadeDelete: true)
                .ForeignKey("dbo.Compounds", t => t.CompoundId, cascadeDelete: true)
                .Index(t => new { t.CompoundId, t.ExternalReferenceTypeId, t.Value }, unique: true, name: "CompoundId_ExternalReferenceTypeId_Value_idx");

            CreateTable(
                "dbo.ExternalReferenceTypes",
                c => new
                    {
                        Id = c.Int(nullable: false),
                        Description = c.String(),
                        UriSpace = c.String(),
                    })
                .PrimaryKey(t => t.Id);

            CreateTable(
                "dbo.InChIs",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        InChI = c.String(nullable: false),
                        InChIKey = c.String(nullable: false, maxLength: 27),
                    })
                .PrimaryKey(t => t.Id, clustered: false) // <-- Added "clustered: false" here
                .Index(t => t.InChIKey, unique: true, name: "InChIKey_idx");

            CreateTable(
                "dbo.InChIMD5s",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        InChIKey_A = c.String(nullable: false, maxLength: 14),
                        InChIKey_B = c.String(nullable: false, maxLength: 23),
                        InChI_MD5 = c.Binary(nullable: false, maxLength: 16),
                        InChI_MF_MD5 = c.Binary(nullable: false, maxLength: 16),
                        InChI_C_MD5 = c.Binary(nullable: false, maxLength: 16),
                        InChI_CH_MD5 = c.Binary(nullable: false, maxLength: 16),
                        InChI_CHSI_MD5 = c.Binary(nullable: false, maxLength: 16),
                    })
                .PrimaryKey(t => t.Id, clustered: false) // <-- Added "clustered: false" here
                .ForeignKey("dbo.InChIs", t => t.Id)
                .Index(t => t.Id)
                .Index(t => t.InChIKey_A)
                .Index(t => t.InChIKey_B)
                .Index(t => t.InChI_MD5, unique: true)
                .Index(t => t.InChI_MF_MD5)
                .Index(t => t.InChI_C_MD5)
                .Index(t => t.InChI_CH_MD5)
                .Index(t => t.InChI_CHSI_MD5);

            CreateTable(
                "dbo.Smiles",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        IndigoSmiles = c.String(nullable: false),
                        IndigoSmilesMd5 = c.String(nullable: false, maxLength: 32),
                    })
                .PrimaryKey(t => t.Id, clustered: false) // <-- Added "clustered: false" here
                .Index(t => t.IndigoSmilesMd5, unique: true, name: "IndigoSmilesMd5_idx");

            CreateTable(
                "dbo.Issues",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        Code = c.String(nullable: false, maxLength: 10),
                        LogId = c.Guid(nullable: false),
                        RevisionId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Revisions", t => t.RevisionId, cascadeDelete: true)
                .Index(t => t.Code)
                .Index(t => t.RevisionId);

            CreateTable(
                "dbo.Substances",
                c => new
                    {
                        Id = c.Guid(nullable: false),
                        ExternalIdentifier = c.String(nullable: false),
                        DataSourceId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id, clustered: false) // <-- Added "clustered: false" here
                .Index(t => t.DataSourceId);

            CreateTable(
                "dbo.Properties",
                c => new
                    {
                        Id = c.Int(nullable: false, identity: true),
                        PropertyId = c.Guid(nullable: false),
                        RevisionId = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Revisions", t => t.RevisionId, cascadeDelete: true)
                .Index(t => t.RevisionId);

            CreateTable(
                "dbo.ef_Synonymef_Revision",
                c => new
                    {
                        ef_Synonym_Id = c.Guid(nullable: false),
                        ef_Revision_Id = c.Guid(nullable: false),
                    })
                .PrimaryKey(t => new { t.ef_Synonym_Id, t.ef_Revision_Id }, clustered: false)  // <-- Added "clustered: false" here
                .ForeignKey("dbo.Synonyms", t => t.ef_Synonym_Id, cascadeDelete: true)
                .ForeignKey("dbo.Revisions", t => t.ef_Revision_Id, cascadeDelete: true)
                .Index(t => t.ef_Synonym_Id)
                .Index(t => t.ef_Revision_Id);

        }

        public override void Down()
        {
            DropForeignKey("dbo.Properties", "RevisionId", "dbo.Revisions");
            DropForeignKey("dbo.Annotations", "Revision_Id", "dbo.Revisions");
            DropForeignKey("dbo.Revisions", "SubstanceId", "dbo.Substances");
            DropForeignKey("dbo.Issues", "RevisionId", "dbo.Revisions");
            DropForeignKey("dbo.Revisions", "CompoundId", "dbo.Compounds");
            DropForeignKey("dbo.Compounds", "TautomericNonStdInChIId", "dbo.InChIs");
            DropForeignKey("dbo.Compounds", "StandardInChIId", "dbo.InChIs");
            DropForeignKey("dbo.Compounds", "SmilesId", "dbo.Smiles");
            DropForeignKey("dbo.ParentChildren", "ParentId", "dbo.Compounds");
            DropForeignKey("dbo.Compounds", "NonStandardInChIId", "dbo.InChIs");
            DropForeignKey("dbo.InChIMD5s", "Id", "dbo.InChIs");
            DropForeignKey("dbo.ExternalReferences", "CompoundId", "dbo.Compounds");
            DropForeignKey("dbo.ExternalReferences", "ExternalReferenceTypeId", "dbo.ExternalReferenceTypes");
            DropForeignKey("dbo.CompoundSynonyms", "CompoundId", "dbo.Compounds");
            DropForeignKey("dbo.CompoundSynonyms", "SynonymId", "dbo.Synonyms");
            DropForeignKey("dbo.SynonymsSynonymFlag", "SynonymId", "dbo.Synonyms");
            DropForeignKey("dbo.SynonymsSynonymFlag", "SynonymFlagId", "dbo.SynonymFlags");
            DropForeignKey("dbo.ef_Synonymef_Revision", "ef_Revision_Id", "dbo.Revisions");
            DropForeignKey("dbo.ef_Synonymef_Revision", "ef_Synonym_Id", "dbo.Synonyms");
            DropForeignKey("dbo.SynonymsSynonymFlagHistory", "SynonymHistoryId", "dbo.SynonymHistory");
            DropForeignKey("dbo.SynonymsSynonymFlagHistory", "SynonymFlagId", "dbo.SynonymFlags");
            DropForeignKey("dbo.SynonymHistory", "SynonymId", "dbo.Synonyms");
            DropForeignKey("dbo.CompoundSynonymSynonymFlagHistory", "CompoundSynonymHistoryId", "dbo.CompoundSynonymHistory");
            DropForeignKey("dbo.CompoundSynonymSynonymFlagHistory", "SynonymFlagId", "dbo.SynonymFlags");
            DropForeignKey("dbo.CompoundSynonymHistory", "CompoundSynonymId", "dbo.CompoundSynonyms");
            DropForeignKey("dbo.CompoundSynonymsSynonymFlag", "CompoundSynonymId", "dbo.CompoundSynonyms");
            DropForeignKey("dbo.CompoundSynonymsSynonymFlag", "SynonymFlagId", "dbo.SynonymFlags");
            DropForeignKey("dbo.ParentChildren", "ChildId", "dbo.Compounds");
            DropIndex("dbo.ef_Synonymef_Revision", new[] { "ef_Revision_Id" });
            DropIndex("dbo.ef_Synonymef_Revision", new[] { "ef_Synonym_Id" });
            DropIndex("dbo.Properties", new[] { "RevisionId" });
            DropIndex("dbo.Substances", new[] { "DataSourceId" });
            DropIndex("dbo.Issues", new[] { "RevisionId" });
            DropIndex("dbo.Issues", new[] { "Code" });
            DropIndex("dbo.Smiles", "IndigoSmilesMd5_idx");
            DropIndex("dbo.InChIMD5s", new[] { "InChI_CHSI_MD5" });
            DropIndex("dbo.InChIMD5s", new[] { "InChI_CH_MD5" });
            DropIndex("dbo.InChIMD5s", new[] { "InChI_C_MD5" });
            DropIndex("dbo.InChIMD5s", new[] { "InChI_MF_MD5" });
            DropIndex("dbo.InChIMD5s", new[] { "InChI_MD5" });
            DropIndex("dbo.InChIMD5s", new[] { "InChIKey_B" });
            DropIndex("dbo.InChIMD5s", new[] { "InChIKey_A" });
            DropIndex("dbo.InChIMD5s", new[] { "Id" });
            DropIndex("dbo.InChIs", "InChIKey_idx");
            DropIndex("dbo.ExternalReferences", "CompoundId_ExternalReferenceTypeId_Value_idx");
            DropIndex("dbo.SynonymsSynonymFlag", new[] { "SynonymFlagId" });
            DropIndex("dbo.SynonymsSynonymFlag", new[] { "SynonymId" });
            DropIndex("dbo.SynonymsSynonymFlagHistory", new[] { "SynonymFlagId" });
            DropIndex("dbo.SynonymsSynonymFlagHistory", new[] { "SynonymHistoryId" });
            DropIndex("dbo.SynonymHistory", new[] { "SynonymId" });
            DropIndex("dbo.Synonyms", "synonym_langid_idx");
            DropIndex("dbo.CompoundSynonymSynonymFlagHistory", new[] { "SynonymFlagId" });
            DropIndex("dbo.CompoundSynonymSynonymFlagHistory", new[] { "CompoundSynonymHistoryId" });
            DropIndex("dbo.CompoundSynonymHistory", new[] { "CompoundSynonymId" });
            DropIndex("dbo.CompoundSynonymsSynonymFlag", new[] { "SynonymFlagId" });
            DropIndex("dbo.CompoundSynonymsSynonymFlag", new[] { "CompoundSynonymId" });
            DropIndex("dbo.CompoundSynonyms", new[] { "SynonymId" });
            DropIndex("dbo.CompoundSynonyms", "IX_CompoundIdAndSynonymState");
            DropIndex("dbo.ParentChildren", new[] { "ChildId" });
            DropIndex("dbo.ParentChildren", new[] { "ParentId" });
            DropIndex("dbo.ParentChildren", new[] { "Type" });
            DropIndex("dbo.Compounds", new[] { "SmilesId" });
            DropIndex("dbo.Compounds", new[] { "StandardInChIId" });
            DropIndex("dbo.Compounds", new[] { "TautomericNonStdInChIId" });
            DropIndex("dbo.Compounds", new[] { "NonStandardInChIId" });
            DropIndex("dbo.Revisions", new[] { "CompoundId" });
            DropIndex("dbo.Revisions", new[] { "SubstanceId" });
            DropIndex("dbo.Revisions", new[] { "DepositionId" });
            DropIndex("dbo.Annotations", new[] { "Revision_Id" });
            DropTable("dbo.ef_Synonymef_Revision");
            DropTable("dbo.Properties");
            DropTable("dbo.Substances");
            DropTable("dbo.Issues");
            DropTable("dbo.Smiles");
            DropTable("dbo.InChIMD5s");
            DropTable("dbo.InChIs");
            DropTable("dbo.ExternalReferenceTypes");
            DropTable("dbo.ExternalReferences");
            DropTable("dbo.SynonymsSynonymFlag");
            DropTable("dbo.SynonymsSynonymFlagHistory");
            DropTable("dbo.SynonymHistory");
            DropTable("dbo.Synonyms");
            DropTable("dbo.CompoundSynonymSynonymFlagHistory");
            DropTable("dbo.CompoundSynonymHistory");
            DropTable("dbo.SynonymFlags");
            DropTable("dbo.CompoundSynonymsSynonymFlag");
            DropTable("dbo.CompoundSynonyms");
            DropTable("dbo.ParentChildren");
            DropTable("dbo.Compounds");
            DropTable("dbo.Revisions");
            DropTable("dbo.Annotations");
        }
    }
}
