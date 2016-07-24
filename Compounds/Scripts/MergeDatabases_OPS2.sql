use RSCCompounds_OPS2

--	sync SMILES
select count(1) from Smiles

select count(1) from RSCCompounds_TMPSQL..Smiles

select count(1) from RSCCompounds_TMPSQL..Smiles s
where s.IndigoSmilesMd5 not in (select IndigoSmilesMd5 from Smiles)

MERGE Smiles AS target
USING (select * from RSCCompounds_TMPSQL..Smiles) AS source (id, IndigoSmiles, IndigoSmilesMd5)
ON (target.IndigoSmilesMd5 = source.IndigoSmilesMd5)
WHEN NOT MATCHED THEN
    INSERT (id, IndigoSmiles, IndigoSmilesMd5)
    VALUES (source.id, source.IndigoSmiles, source.IndigoSmilesMd5);
--	running time: 28:00

--	sync InChIs
select count(1) from InChIMD5s

select count(1) from RSCCompounds_TMPSQL..InChIMD5s

select count(1) from RSCCompounds_TMPSQL..InChIMD5s
where InChI_MD5 not in (select InChI_MD5 from InChIMD5s)

MERGE InChIs AS target
USING (select * from RSCCompounds_TMPSQL..InChIs) AS source (id, InChI, InChIKey)
ON (target.InChIKey = source.InChIKey)
WHEN NOT MATCHED THEN
    INSERT (id, InChI, InChIKey)
    VALUES (source.id, source.InChI, source.InChIKey);
--	running time: 1:42:00

MERGE InChIMD5s AS target
USING (select * from RSCCompounds_TMPSQL..InChIMD5s) AS source (id, InChIKey_A, InChIKey_B, InChI_MD5, InChI_MF_MD5, InChI_C_MD5, InChI_CH_MD5, InChI_CHSI_MD5)
ON (target.InChI_MD5 = source.InChI_MD5)
WHEN NOT MATCHED THEN
    INSERT (id, InChIKey_A, InChIKey_B, InChI_MD5, InChI_MF_MD5, InChI_C_MD5, InChI_CH_MD5, InChI_CHSI_MD5)
    VALUES (source.id, source.InChIKey_A, source.InChIKey_B, source.InChI_MD5, source.InChI_MF_MD5, source.InChI_C_MD5, source.InChI_CH_MD5, source.InChI_CHSI_MD5);

--	running time: 2:46:51 (42788003 row(s) affected)

--	sync Substances
MERGE Substances AS target
USING (select * from RSCCompounds_TMPSQL..Substances) AS source (id, ExternalIdentifier, DataSourceId)
ON (target.DataSourceId = source.DataSourceId and target.ExternalIdentifier = source.ExternalIdentifier)
WHEN NOT MATCHED THEN
    INSERT (id, ExternalIdentifier, DataSourceId)
    VALUES (source.id, source.ExternalIdentifier, source.DataSourceId);
--	running time: 0:09:04 (14154160 row(s) affected)
-- running time: 00:02:44 (74139 row(s) affected)

select count(1) from RSCCompounds_TMPSQL..Substances

select count(1) from Substances 

/*
--due to the conflict with already loaded SureChEMBL small subset of data we should clean it first...

delete from Properties
	where RevisionId in (
	select Id from Revisions
	where SubstanceId in (
		select Id from Substances
		where id not in (
			select Id from RSCCompounds_TMPSQL..Substances
		)
		and DataSourceId = '52E6277A-3728-4E73-9C55-AEAAF5EAADC0'
	)
)
--(996588 row(s) affected)

delete from Issues
	where RevisionId in (
	select Id from Revisions
	where SubstanceId in (
		select Id from Substances
		where id not in (
			select Id from RSCCompounds_TMPSQL..Substances
		)
		and DataSourceId = '52E6277A-3728-4E73-9C55-AEAAF5EAADC0'
	)
)
--	(48582 row(s) affected)

delete from Revisions
where SubstanceId in (
	select Id from Substances
	where id not in (
		select Id from RSCCompounds_TMPSQL..Substances
	)
	and DataSourceId = '52E6277A-3728-4E73-9C55-AEAAF5EAADC0'
)
--	(74250 row(s) affected)

delete from Substances
where id not in (
	select Id from RSCCompounds_TMPSQL..Substances
)
and DataSourceId = '52E6277A-3728-4E73-9C55-AEAAF5EAADC0'
--	(74250 row(s) affected)

*/

--	sync Compounds
;WITH NonMatchedCompounds AS
(
   SELECT c.Id, c.DateCreated, c.Mol, non_std_i_merged.Id as NonStandardInChIId, std_i_merged.Id as StandardInChIId, taut_i_merged.Id as TautomericNonStdInChIId, s_merged.Id as SmilesId
   FROM RSCCompounds_TMPSQL..Compounds c 
    inner join RSCCompounds_TMPSQL..InChIMD5s non_std_i on c.NonStandardInChIId = non_std_i.Id
	left join InChIMD5s non_std_i_merged on non_std_i.InChI_MD5 = non_std_i_merged.InChI_MD5
	left outer join RSCCompounds_TMPSQL..Smiles s on c.SmilesId = s.Id
	left join Smiles s_merged on s.IndigoSmilesMd5 = s_merged.IndigoSmilesMd5
	left join RSCCompounds_TMPSQL..InChIMD5s std_i on c.StandardInChIId = std_i.Id
	left join InChIMD5s std_i_merged on std_i.InChI_MD5 = std_i_merged.InChI_MD5
	left join RSCCompounds_TMPSQL..InChIMD5s taut_i on c.TautomericNonStdInChIId = taut_i.Id
	left join InChIMD5s taut_i_merged on taut_i.InChI_MD5 = taut_i_merged.InChI_MD5
   WHERE non_std_i.InChI_MD5 NOT IN (
		SELECT InChI_MD5
		FROM InChIMD5s md5
		inner join Compounds c ON md5.Id = c.NonStandardInChIId
   )
)
--select top 10 * from NonMatchedCompounds

INSERT INTO Compounds(Id, DateCreated, Mol, NonStandardInChIId, TautomericNonStdInChIId, StandardInChIId, SmilesId)
SELECT Id, DateCreated, Mol, NonStandardInChIId, TautomericNonStdInChIId, StandardInChIId, SmilesId
FROM NonMatchedCompounds
go
--	running time: 24:49:19 (23788847 row(s) affected)

--	sync Revisions
MERGE Revisions AS target
USING (	select r.Id, r.DepositionId, r.Version, r.DateCreated, r.DateModified, r.EmbargoDate, r.Revoked, r.Sdf, r.SubstanceId, c_merged.Id as CompoundId
		from RSCCompounds_TMPSQL..Revisions r 
			inner join RSCCompounds_TMPSQL..Compounds c on r.CompoundId = c.Id
			inner join RSCCompounds_TMPSQL..InChIMd5s md5 on c.NonStandardInChIId = md5.Id
			inner join InChIMd5s md5_merged on md5.InChI_MD5 = md5_merged.InChI_MD5
			inner join Compounds c_merged on c_merged.NonStandardInChIId = md5_merged.Id
) AS source (Id, DepositionId, Version, DateCreated, DateModified, EmbargoDate, Revoked, Sdf, SubstanceId, CompoundId)
ON (target.Id = source.Id)
WHEN NOT MATCHED THEN
    INSERT (Id, DepositionId, Version, DateCreated, DateModified, EmbargoDate, Revoked, Sdf, SubstanceId, CompoundId)
    VALUES (source.Id, source.DepositionId, source.Version, source.DateCreated, source.DateModified, source.EmbargoDate, source.Revoked, source.Sdf, source.SubstanceId, source.CompoundId);
go
-- 13:58:23 (14228299 row(s) affected)

--	sync Properties
MERGE Properties AS target
USING (select * from RSCCompounds_TMPSQL..Properties) AS source (Id, PropertyId, RevisionId)
ON (target.RevisionId = source.RevisionId and target.PropertyId = source.PropertyId)
WHEN NOT MATCHED THEN
    INSERT (RevisionId, PropertyId)
    VALUES (source.RevisionId, source.PropertyId);
-- 00:58:03	(181426979 row(s) affected)

--	sync Issues
MERGE Issues AS target
USING (select * from RSCCompounds_TMPSQL..Issues) AS source (Id, Code, LogId, RevisionId)
ON (target.RevisionId = source.RevisionId and target.LogId = source.LogId)
WHEN NOT MATCHED THEN
    INSERT (Code, RevisionId, LogId)
    VALUES (source.Code, source.RevisionId, source.LogId);
-- 00:08:24	(12871313 row(s) affected)

--	sync Parents
;WITH NonMatchedParents AS
(
   SELECT pc.Id, pc.Type, child_c_merged.Id as ChildId, parent_c_merged.Id as ParentId
   FROM RSCCompounds_TMPSQL..ParentChildren pc
    inner join RSCCompounds_TMPSQL..Compounds child_c on pc.ChildId = child_c.Id
    inner join RSCCompounds_TMPSQL..Compounds parent_c on pc.ParentId = parent_c.Id
    inner join RSCCompounds_TMPSQL..InChIMD5s child_non_std_i on child_c.NonStandardInChIId = child_non_std_i.Id
    inner join RSCCompounds_TMPSQL..InChIMD5s parent_non_std_i on parent_c.NonStandardInChIId = parent_non_std_i.Id
	inner join InChIMD5s child_non_std_i_merged on child_non_std_i.InChI_MD5 = child_non_std_i_merged.InChI_MD5
	inner join InChIMD5s parent_non_std_i_merged on parent_non_std_i.InChI_MD5 = parent_non_std_i_merged.InChI_MD5
    inner join Compounds child_c_merged on child_c_merged.NonStandardInChIId = child_non_std_i_merged.Id
    inner join Compounds parent_c_merged on parent_c_merged.NonStandardInChIId = parent_non_std_i_merged.Id
   WHERE pc.Id NOT IN (
	   SELECT pc.Id
	   FROM RSCCompounds_TMPSQL..ParentChildren pc
		inner join RSCCompounds_TMPSQL..Compounds child_c on pc.ChildId = child_c.Id
		inner join RSCCompounds_TMPSQL..Compounds parent_c on pc.ParentId = parent_c.Id
		inner join RSCCompounds_TMPSQL..InChIMD5s child_non_std_i on child_c.NonStandardInChIId = child_non_std_i.Id
		inner join RSCCompounds_TMPSQL..InChIMD5s parent_non_std_i on parent_c.NonStandardInChIId = parent_non_std_i.Id
		inner join InChIMD5s child_non_std_i_merged on child_non_std_i.InChI_MD5 = child_non_std_i_merged.InChI_MD5
		inner join InChIMD5s parent_non_std_i_merged on parent_non_std_i.InChI_MD5 = parent_non_std_i_merged.InChI_MD5
		inner join Compounds child_c_merged on child_c_merged.NonStandardInChIId = child_non_std_i_merged.Id
		inner join Compounds parent_c_merged on parent_c_merged.NonStandardInChIId = parent_non_std_i_merged.Id
		inner join ParentChildren pc_merged on pc_merged.ChildId = child_c_merged.Id and pc_merged.ParentId = parent_c_merged.Id
   )
)
--select count(*) from NonMatchedParents

INSERT INTO ParentChildren(Id, Type, ParentId, ChildId)
SELECT Id, Type, ParentId, ChildId
FROM NonMatchedParents
go

-- 00:29:33 (10651263 row(s) affected)

--	sync ExternalReferences
MERGE ExternalReferences AS target
USING (	select c_merged.Id as CompoundId, r.ExternalReferenceTypeId, r.Value
		from RSCCompounds_TMPSQL..ExternalReferences r
			inner join RSCCompounds_TMPSQL..Compounds c on r.CompoundId = c.Id
			inner join RSCCompounds_TMPSQL..InChIMd5s i on c.NonStandardInChIId = i.Id
			inner join InChIMd5s i_merged on i.InChI_MD5 = i_merged.InChI_MD5
			inner join Compounds c_merged on c_merged.NonStandardInChIId = i_merged.Id
) AS source (CompoundId, ExternalReferenceTypeId, Value)
ON (target.CompoundId = source.CompoundId)
WHEN NOT MATCHED THEN
    INSERT (CompoundId, ExternalReferenceTypeId, Value)
    VALUES (source.CompoundId, source.ExternalReferenceTypeId, source.Value);
-- 00:24:33	(25014322 row(s) affected)

select count(1) from ExternalReferences
select count(1) from RSCCompounds_TMPSQL..ExternalReferences
select count(1) from Compounds where Id not in (select CompoundId from ExternalReferences)
