namespace RSC.Compounds.EntityFramework.Migrations
{
	using System;
	using System.Data.Entity;
	using System.Data.Entity.Migrations;
	using System.Linq;
	using RSC.Compounds.EntityFramework;

	internal sealed class Configuration : DbMigrationsConfiguration<RSC.Compounds.EntityFramework.CompoundsContext>
	{
		public Configuration()
		{
			AutomaticMigrationsEnabled = true;
			AutomaticMigrationDataLossAllowed = true;

			CommandTimeout = 60 * 30;	//	30 mins
		}

		protected override void Seed(RSC.Compounds.EntityFramework.CompoundsContext context)
		{
			//See the database with Synonym Flags we use on ChemSpider.
			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 1,
										Name = "DBID",
										Description = "Database ID",
										ExcludeFromTitle = true,
										IsUniquePerLanguage = false,
										Rank = 0,
										RegEx = null,
										Type = SynonymFlagType.SynonymAssertion,
										Url = null
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 2,
										Name = "Wiki",
										Description = "Wikipedia",
										ExcludeFromTitle = false,
										IsUniquePerLanguage = false,
										Rank = 6,
										RegEx = null,
										Type = SynonymFlagType.SynonymAssertion,
										Url = "http://en.wikipedia.org/wiki/$synonym"
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 3,
										Name = "EINECS",
										Description = "European Chemical Substances Information System",
										ExcludeFromTitle = false,
										IsUniquePerLanguage = false,
										Rank = 3,
										RegEx = null,
										Type = SynonymFlagType.SynonymType,
										Url = "http://echa.europa.eu/web/guest/information-on-chemicals/ec-inventory"
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 4,
										Name = "RN",
										Description = "Registry Number",
										ExcludeFromTitle = false,
										IsUniquePerLanguage = false,
										Rank = 5,
										RegEx = null,
										Type = SynonymFlagType.SynonymType,
										Url = "http://www.ncbi.nlm.nih.gov/sites/entrez?db=pccompound&amp;term=\"$synonym\"[CompleteSynonym]"
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 5,
										Name = "WLN",
										Description = "Wiswesser Line Notation",
										ExcludeFromTitle = true,
										IsUniquePerLanguage = false,
										Rank = 0,
										RegEx = null,
										Type = SynonymFlagType.SynonymType,
										Url = "http://en.wikipedia.org/wiki/Wiswesser_Line_Notation"
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 6,
										Name = "Beilstein",
										Description = "Beilstein database",
										ExcludeFromTitle = true,
										IsUniquePerLanguage = false,
										Rank = 0,
										RegEx = null,
										Type = SynonymFlagType.SynonymAssertion,
										Url = "http://en.wikipedia.org/wiki/Beilstein_database"
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 7,
										Name = "USP",
										Description = "United States Pharmacopeia",
										ExcludeFromTitle = false,
										IsUniquePerLanguage = true,
										Rank = 0,
										RegEx = null,
										Type = SynonymFlagType.SynonymAssertion,
										Url = "http://en.wikipedia.org/wiki/United_States_Pharmacopeia"
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 8,
										Name = "INN",
										Description = "International Nonproprietary Name",
										ExcludeFromTitle = false,
										IsUniquePerLanguage = true,
										Rank = 4,
										RegEx = null,
										Type = SynonymFlagType.SynonymAssertion,
										Url = "http://en.wikipedia.org/wiki/International_Nonproprietary_Name"
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 9,
										Name = "BAN",
										Description = "British Approved Name",
										ExcludeFromTitle = false,
										IsUniquePerLanguage = true,
										Rank = 4,
										RegEx = null,
										Type = SynonymFlagType.SynonymAssertion,
										Url = "http://en.wikipedia.org/wiki/British_Approved_Name"
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 10,
										Name = "JAN",
										Description = "Japanese Approved Name",
										ExcludeFromTitle = false,
										IsUniquePerLanguage = true,
										Rank = 4,
										RegEx = null,
										Type = SynonymFlagType.SynonymAssertion,
										Url = null
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 11,
										Name = "USAN",
										Description = "United States Adopted Name",
										ExcludeFromTitle = false,
										IsUniquePerLanguage = true,
										Rank = 4,
										RegEx = null,
										Type = SynonymFlagType.SynonymAssertion,
										Url = "http://www.ama-assn.org/ama/pub/physician-resources/medical-science/united-states-adopted-names-council/generic-drug-naming-explained.page"
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 12,
										Name = "NF",
										Description = "National Formulary drug name",
										ExcludeFromTitle = false,
										IsUniquePerLanguage = false,
										Rank = 4,
										RegEx = null,
										Type = SynonymFlagType.SynonymAssertion,
										Url = null
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 13,
										Name = "JP15",
										Description = "Japanese Pharmacopoeia, 15th edition",
										ExcludeFromTitle = false,
										IsUniquePerLanguage = true,
										Rank = 4,
										RegEx = null,
										Type = SynonymFlagType.SynonymAssertion,
										Url = null
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 14,
										Name = "Formula",
										Description = "Shorthand Formula",
										ExcludeFromTitle = true,
										IsUniquePerLanguage = false,
										Rank = 2,
										RegEx = null,
										Type = SynonymFlagType.SynonymAssertion,
										Url = null
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 15,
										Name = "ISO",
										Description = "ISO",
										ExcludeFromTitle = false,
										IsUniquePerLanguage = true,
										Rank = 4,
										RegEx = null,
										Type = SynonymFlagType.SynonymAssertion,
										Url = null
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 16,
										Name = "BSI",
										Description = "BSI",
										ExcludeFromTitle = false,
										IsUniquePerLanguage = true,
										Rank = 4,
										RegEx = null,
										Type = SynonymFlagType.SynonymAssertion,
										Url = null
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 17,
										Name = "IUPAC",
										Description = "ACD/IUPAC Name",
										ExcludeFromTitle = false,
										IsUniquePerLanguage = true,
										Rank = 1,
										RegEx = null,
										Type = SynonymFlagType.SynonymAssertion,
										Url = null
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 18,
										Name = "Index",
										Description = "ACD/Index Name",
										ExcludeFromTitle = true,
										IsUniquePerLanguage = true,
										Rank = 1,
										RegEx = null,
										Type = SynonymFlagType.SynonymAssertion,
										Url = null
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 19,
										Name = "Trade name",
										Description = "Trade name",
										ExcludeFromTitle = true,
										IsUniquePerLanguage = false,
										Rank = 0,
										RegEx = null,
										Type = SynonymFlagType.SynonymAssertion,
										Url = null
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 20,
										Name = "MFCD",
										Description = "MDL number",
										ExcludeFromTitle = true,
										IsUniquePerLanguage = false,
										Rank = 0,
										RegEx = null,
										Type = SynonymFlagType.SynonymType,
										Url = null
									});

			context.SynonymFlags.AddOrUpdate(i => i.Id,
									new ef_SynonymFlag()
									{
										Id = 21,
										Name = "UNII",
										Description = "Unique Ingredient Identifier",
										ExcludeFromTitle = true,
										IsUniquePerLanguage = false,
										Rank = 0,
										RegEx = null,
										Type = SynonymFlagType.SynonymType,
										Url = "https://en.wikipedia.org/wiki/Unique_Ingredient_Identifier"
									});

			//Add the ChemSpider Id and Ops Id Identifier Types.
			context.ExternalReferenceTypes.AddOrUpdate(i => i.Id,
								   new ef_ExternalReferenceType()
								   {
									   Id = 1,
									   Description = "ChemSpider Id",
									   UriSpace = Constants.CSUriSpace
								   });

			context.ExternalReferenceTypes.AddOrUpdate(i => i.Id,
								   new ef_ExternalReferenceType()
								   {
									   Id = 2,
									   Description = "Ops Id",
									   UriSpace = Constants.OPSUriSpace
								   });
		}
	}
}
