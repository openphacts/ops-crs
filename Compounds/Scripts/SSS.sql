USE [RSCCompounds]
GO

CREATE TABLE [dbo].[compounds_sss](
	[id] [bigint] NOT NULL IDENTITY,
	[cmp_id] [uniqueidentifier] NOT NULL,
	[smiles] [varchar](max) NOT NULL,
	PRIMARY KEY CLUSTERED ([id] ASC)
) ON [PRIMARY] TEXTIMAGE_ON [PRIMARY]
GO

CREATE NONCLUSTERED INDEX [IX_CMP_ID] ON [dbo].[compounds_sss]
(
	[cmp_id] ASC
)
GO

CREATE PROCEDURE [dbo].[SearchByMolSub](@smiles varchar(3000), @parameters varchar(20))
AS
BEGIN
	select c.cmp_id as id, Bingo.Sim(smiles, @smiles, 'tanimoto') as similarity
	from bingo.SearchSub('compounds_sss', @smiles, @parameters) t 
		inner join compounds_sss c on c.id = t.id
	order by 2 desc
END
GO

CREATE PROCEDURE [dbo].[SearchByMolSim](@smiles varchar(3000), @metrics_type varchar(100), @metrics_value float)
AS
BEGIN
	select c.cmp_id as id, t.similarity
	from bingo.SearchSim('compounds_sss', @smiles, @metrics_type, @metrics_value, null) t
		inner join compounds_sss c on c.id = t.id
	order by 2 desc
END
GO

CREATE PROCEDURE [dbo].[SearchBySMARTSSub](@smarts varchar(3000), @parameters varchar(20))
AS
BEGIN
	select c.cmp_id as id, Bingo.Sim(smiles, @smarts, 'tanimoto') as similarity
	from bingo.SearchSMARTS('compounds_sss', @smarts, @parameters) t 
		inner join compounds_sss c on c.id = t.id
	order by 2 desc
END
GO

alter PROCEDURE [dbo].[RebuildBingo]
AS
BEGIN
	MERGE compounds_sss AS target
	USING (select c.Id as cmp_id, s.IndigoSmiles as smiles from Compounds c inner join Smiles s on c.SmilesId = s.Id) AS source (cmp_id, smiles)
	ON (target.smiles = source.smiles)
	WHEN NOT MATCHED THEN
		INSERT (cmp_id, smiles)
		VALUES (source.cmp_id, source.smiles);

	exec bingo.CreateMoleculeIndex 'compounds_sss', 'id', 'smiles'
END
GO

-- copy SMILES
--insert into compounds_sss(cmp_id, smiles)
--select c.Id, s.IndigoSmiles
--from Compounds c inner join Smiles s on c.SmilesId = s.Id

--exec bingo.CreateMoleculeIndex 'compounds_sss', 'id', 'smiles'

exec RebuildBingo
go
