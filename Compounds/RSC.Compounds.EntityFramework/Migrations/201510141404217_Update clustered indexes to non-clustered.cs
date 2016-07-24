namespace RSC.Compounds.EntityFramework.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class Updateclusteredindexestononclustered : DbMigration
    {
        public override void Up()
        {
            //README! - Note - There was no migration script in existence when the db went live, so if there's an error, then assume we must migrate the clustered indexes to non-clustered.

            //Assume this is an existing database - so change clustered indexes to non-clustered.

            //Drop the referencing foreign keys.
            DropForeignKey("dbo.Revisions", "CompoundId", "dbo.Compounds");
            DropForeignKey("dbo.CompoundSynonyms", "CompoundId", "dbo.Compounds");
            DropForeignKey("dbo.ExternalReferences", "CompoundId", "dbo.Compounds");
            DropForeignKey("dbo.ParentChildren", "ParentId", "dbo.Compounds");
            DropForeignKey("dbo.ParentChildren", "ChildId", "dbo.Compounds");
            DropForeignKey("dbo.Compounds", "TautomericNonStdInChIId", "dbo.InChIs");
            DropForeignKey("dbo.Compounds", "StandardInChIId", "dbo.InChIs");
            DropForeignKey("dbo.Compounds", "NonStandardInChIId", "dbo.InChIs");
            DropForeignKey("dbo.InChIMD5s", "Id", "dbo.InChIs");
            DropForeignKey("dbo.Issues", "RevisionId", "dbo.Revisions");
            DropForeignKey("dbo.Properties", "RevisionId", "dbo.Revisions");
            DropForeignKey("dbo.Annotations", "Revision_Id", "dbo.Revisions");
            DropForeignKey("dbo.ef_Synonymef_Revision", "ef_Revision_Id", "dbo.Revisions");
            DropForeignKey("dbo.CompoundSynonymHistory", "CompoundSynonymId", "dbo.CompoundSynonyms");
            DropForeignKey("dbo.CompoundSynonymsSynonymFlag", "CompoundSynonymId", "dbo.CompoundSynonyms");
            DropForeignKey("dbo.CompoundSynonymSynonymFlagHistory", "CompoundSynonymHistoryId", "dbo.CompoundSynonymHistory");
            DropForeignKey("dbo.CompoundSynonyms", "SynonymId", "dbo.Synonyms");
            DropForeignKey("dbo.SynonymsSynonymFlag", "SynonymId", "dbo.Synonyms");
            DropForeignKey("dbo.ef_Synonymef_Revision", "ef_Synonym_Id", "dbo.Synonyms");
            DropForeignKey("dbo.SynonymHistory", "SynonymId", "dbo.Synonyms");
            DropForeignKey("dbo.SynonymsSynonymFlagHistory", "SynonymHistoryId", "dbo.SynonymHistory");
            DropForeignKey("dbo.Compounds", "SmilesId", "dbo.Smiles");
            DropForeignKey("dbo.Revisions", "SubstanceId", "dbo.Substances");

            //Drop the old Primary keys.
            DropPrimaryKey("dbo.Compounds");
            DropPrimaryKey("dbo.InChIs");
            DropPrimaryKey("dbo.Revisions");
            DropPrimaryKey("dbo.CompoundSynonyms");
            DropPrimaryKey("dbo.CompoundSynonymsSynonymFlag");
            DropPrimaryKey("dbo.CompoundSynonymHistory");
            DropPrimaryKey("dbo.CompoundSynonymSynonymFlagHistory");
            DropPrimaryKey("dbo.Synonyms");
            DropPrimaryKey("dbo.SynonymHistory");
            DropPrimaryKey("dbo.SynonymsSynonymFlagHistory");
            DropPrimaryKey("dbo.SynonymsSynonymFlag");
            DropPrimaryKey("dbo.ExternalReferences");
            DropPrimaryKey("dbo.InChIMD5s");
            DropPrimaryKey("dbo.Smiles");
            DropPrimaryKey("dbo.Substances");
            DropPrimaryKey("dbo.ef_Synonymef_Revision");

            //Add the new non-clustered indexes.
            AddPrimaryKey("dbo.Compounds", "Id", null, false);
            AddPrimaryKey("dbo.InChIs", "Id", null, false);
            AddPrimaryKey("dbo.Revisions", "Id", null, false);
            AddPrimaryKey("dbo.CompoundSynonyms", "Id", null, false);
            AddPrimaryKey("dbo.CompoundSynonymsSynonymFlag", "Id", null, false);
            AddPrimaryKey("dbo.CompoundSynonymHistory", "Id", null, false);
            AddPrimaryKey("dbo.CompoundSynonymSynonymFlagHistory", "Id", null, false);
            AddPrimaryKey("dbo.Synonyms", "Id", null, false);
            AddPrimaryKey("dbo.SynonymHistory", "Id", null, false);
            AddPrimaryKey("dbo.SynonymsSynonymFlagHistory", "Id", null, false);
            AddPrimaryKey("dbo.SynonymsSynonymFlag", new[] { "SynonymId", "SynonymFlagId" }, null, false);
            AddPrimaryKey("dbo.ExternalReferences", "Id", null, false);
            AddPrimaryKey("dbo.InChIMD5s", "Id", null, false);
            AddPrimaryKey("dbo.Smiles", "Id", null, false);
            AddPrimaryKey("dbo.Substances", "Id", null, false);
            AddPrimaryKey("dbo.ef_Synonymef_Revision", new[] { "ef_Synonym_Id", "ef_Revision_Id" }, null, false);

            //Add the referencing foreign keys.
            AddForeignKey("dbo.Revisions", "CompoundId", "dbo.Compounds");
            AddForeignKey("dbo.CompoundSynonyms", "CompoundId", "dbo.Compounds");
            AddForeignKey("dbo.ExternalReferences", "CompoundId", "dbo.Compounds");
            AddForeignKey("dbo.ParentChildren", "ParentId", "dbo.Compounds");
            AddForeignKey("dbo.ParentChildren", "ChildId", "dbo.Compounds");
            AddForeignKey("dbo.Compounds", "TautomericNonStdInChIId", "dbo.InChIs");
            AddForeignKey("dbo.Compounds", "StandardInChIId", "dbo.InChIs");
            AddForeignKey("dbo.Compounds", "NonStandardInChIId", "dbo.InChIs");
            AddForeignKey("dbo.InChIMD5s", "Id", "dbo.InChIs");
            AddForeignKey("dbo.Issues", "RevisionId", "dbo.Revisions");
            AddForeignKey("dbo.Properties", "RevisionId", "dbo.Revisions");
            AddForeignKey("dbo.Annotations", "Revision_Id", "dbo.Revisions");
            AddForeignKey("dbo.ef_Synonymef_Revision", "ef_Revision_Id", "dbo.Revisions");
            AddForeignKey("dbo.CompoundSynonymHistory", "CompoundSynonymId", "dbo.CompoundSynonyms");
            AddForeignKey("dbo.CompoundSynonymsSynonymFlag", "CompoundSynonymId", "dbo.CompoundSynonyms");
            AddForeignKey("dbo.CompoundSynonymSynonymFlagHistory", "CompoundSynonymHistoryId", "dbo.CompoundSynonymHistory");
            AddForeignKey("dbo.CompoundSynonyms", "SynonymId", "dbo.Synonyms");
            AddForeignKey("dbo.SynonymsSynonymFlag", "SynonymId", "dbo.Synonyms");
            AddForeignKey("dbo.ef_Synonymef_Revision", "ef_Synonym_Id", "dbo.Synonyms");
            AddForeignKey("dbo.SynonymHistory", "SynonymId", "dbo.Synonyms");
            AddForeignKey("dbo.SynonymsSynonymFlagHistory", "SynonymHistoryId", "dbo.SynonymHistory");
            AddForeignKey("dbo.Compounds", "SmilesId", "dbo.Smiles");
            AddForeignKey("dbo.Revisions", "SubstanceId", "dbo.Substances");
        }
        
        public override void Down()
        {
            //there is no rollback as it's a manually written script.
        }
    }
}
