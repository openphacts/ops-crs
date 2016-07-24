using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.SqlClient;
using InChINet;
using ChemSpider.Molecules;
using ChemSpider.Utilities;
using RSC.Properties;
using EntityFramework.BulkInsert.Extensions;
using System.Transactions;
using System.Data.Entity;
using System.Diagnostics;
using RSC.CVSP;
using RSC.Collections;

namespace RSC.Compounds.EntityFramework
{
    public class EFSubstanceBulkUpload : ISubstanceBulkUpload
    {
        private IDictionary<string, Guid> _inChIKeysDictionary = null;
        private IDictionary<string, Guid> _smilesDictionary = null;
        private IDictionary<string, Guid> _substancesDictionary = null;
        private IDictionary<Guid, Guid> _compoundsDictionary = null;

        //private readonly IPropertyStore _propertyStore = null;
        private readonly ICVSPStore _cvspStore = null;

        public EFSubstanceBulkUpload(/*IPropertyStore propertyStore,*/ ICVSPStore cvspStore)
        {
			//if (propertyStore == null)
			//	throw new ArgumentNullException("propertyStore");

            if (cvspStore == null)
                throw new ArgumentNullException("cvspStore");

            //_propertyStore = propertyStore;
            _cvspStore = cvspStore;
        }

        private CompoundsContext GetCompoundsContext()
        {
            var db = new CompoundsContext();
            db.Configuration.AutoDetectChangesEnabled = false;
            db.Configuration.ValidateOnSaveEnabled = false;

            return db;
        }

        public bool BulkUpload(Guid depositionId, IEnumerable<RecordData> data)
        {
            if (!data.Any())
                return false;

            //	get detailed deposition's information...
            var deposition = _cvspStore.GetDeposition(depositionId);
            if (deposition == null)
                throw new DepositionNotFoundException();

            //TODO: We need to check each step for success and do some logging of what happened i.e. counts and failure messages.

            //	+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            //	Create all InChIs
            //	+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            var allCompounds = data.Select(d => d.Compound).Concat(data.Where(d => d.Parents != null).SelectMany(d => d.Parents));

            var watch = Stopwatch.StartNew();
            Trace.WriteLine("Creating InChis.");

            CreateInChIs(allCompounds);

            Trace.WriteLine(string.Format("Done: {0}", watch.Elapsed.ToString()));
            watch.Stop();

            //	+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            //	Create all SMILES
            //	+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            watch = Stopwatch.StartNew();
            Trace.WriteLine("Creating Smiles.");

            CreateSmiles(allCompounds);

            Trace.WriteLine(string.Format("Done: {0}", watch.Elapsed.ToString()));
            watch.Stop();


            //	+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            //	Create substances
            //	+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            watch = Stopwatch.StartNew();
            Trace.WriteLine("Creating Substances.");

            CreateSubstances(deposition.DatasourceId, data);

            Trace.WriteLine(string.Format("Done: {0}", watch.Elapsed.ToString()));
            watch.Stop();

            //	+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            //	Create compounds and revisions
            //	+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            watch = Stopwatch.StartNew();
            Trace.WriteLine("Creating Compounds.");

            CreateCompounds(deposition.DatasourceId, deposition.Id, data);

            Trace.WriteLine(string.Format("Done: {0}", watch.Elapsed.ToString()));
            watch.Stop();

            //	+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            //	Create compounds' synonyms
            //	+++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
            watch = Stopwatch.StartNew();
            Trace.WriteLine("Creating Synonyms.");

            CreateSynonyms(data.Select(d => d.Compound), deposition.Id, deposition.UserId);

            Trace.WriteLine(string.Format("Done: {0}", watch.Elapsed.ToString()));
            watch.Stop();

            return true;
        }

        private bool CreateInChIs(IEnumerable<RSC.Compounds.Compound> compounds)
        {
            using (var db = GetCompoundsContext())
            {
                //	collect all InChI types... 
                var allInchis = compounds.Where(c => c.StandardInChI != null).Select(c => c.StandardInChI);
                allInchis = allInchis.Concat(compounds.Where(c => c.NonStandardInChI != null).Select(c => c.NonStandardInChI));
                allInchis = allInchis.Concat(compounds.Where(c => c.TautomericNonStdInChI != null).Select(c => c.TautomericNonStdInChI));

                //	remove empty InChIs and duplicates...
                allInchis = allInchis.Where(i => !string.IsNullOrEmpty(i.Inchi)).Distinct(new InChIComparer());

                if (!allInchis.Any())
                    return false;

                var allKeys = allInchis.Select(i => i.InChIKey).Distinct();

                using (DbContextTransaction scope = db.Database.BeginTransaction())
                {
                    //	Lock the table during this transaction in order to ristrict access from diffrent threads...
                    db.Database.ExecuteSqlCommand("SELECT TOP 1 Id FROM InChIs WITH (TABLOCKX, HOLDLOCK)");

                    //	get the list of InChIKeys that already registered in the system...
                    var existingInChIKeys = (from i in db.InChIs.AsNoTracking()
                                             where allKeys.Contains(i.InChIKey)
                                             select i.InChIKey).ToList();

                    //	get list of InChIs that should be created...
                    var inchisToCreate = allInchis.Where(i => !existingInChIKeys.Any(k => k == i.InChIKey)).ToList();

                    //	create new InChIs...
                    if (inchisToCreate.Any())
                    {
                        var newInChIs = new List<ef_InChI>();
                        var newInChIMD5 = new List<ef_InChIMD5>();

                        foreach (var i in inchisToCreate)
                        {
                            Guid id = Guid.NewGuid();
                            newInChIs.Add(new ef_InChI() { Id = id, InChI = i.Inchi, InChIKey = i.InChIKey });

                            var md5 = CalculateInChIMD5(i.Inchi, i.InChIKey);
                            if (md5 != null)
                            {
                                md5.Id = id;
                                newInChIMD5.Add(md5);
                            }
                        }

                        db.BulkInsert(newInChIs, scope.UnderlyingTransaction, SqlBulkCopyOptions.KeepIdentity);
                        db.BulkInsert(newInChIMD5, new BulkInsertOptions() { EnableStreaming = true });

                        db.SaveChanges();
                        scope.Commit();
                    }
                }

                //	cache InChIKey dictionary in order to use it later... 
                _inChIKeysDictionary = (from i in db.InChIs.AsNoTracking()
                                        where allKeys.Contains(i.InChIKey)
                                        select new { i.InChIKey, i.Id })
                                       .ToDictionary(kvp => kvp.InChIKey, kvp => kvp.Id);
            }

            return true;
        }

        private ef_InChIMD5 CalculateInChIMD5(string inchi, string inchikey)
        {
            string[] inchiLayers = null;
            if (inchi.StartsWith("InChI="))
            {
                inchiLayers = InChIUtils.getInChILayers(inchi);
            }
            else if (InChIUtils.isValidInChI(inchi))
            {
                string message;
                inchi = ChemIdUtils.anyId2InChI(inchi, out message, InChIFlags.v104);
                if (!string.IsNullOrEmpty(inchi))
                    inchiLayers = InChIUtils.getInChILayers(inchi);
            }

            if (inchiLayers != null)
            {
                return new ef_InChIMD5()
                {
                    Id = Guid.NewGuid(),
                    InChIKey_A = inchikey.Substring(0, 14),
                    InChIKey_B = inchikey.Substring(0, 23),
					InChI_MD5 = inchiLayers[(int)InChILayers.ALL].GetMD5Hash(),
					InChI_C_MD5 = inchiLayers[(int)InChILayers.C].GetMD5Hash(),
					InChI_CH_MD5 = inchiLayers[(int)InChILayers.CH].GetMD5Hash(),
					InChI_MF_MD5 = inchiLayers[(int)InChILayers.MF].GetMD5Hash(),
					InChI_CHSI_MD5 = inchiLayers[(int)InChILayers.CHSI].GetMD5Hash()
                };
            }

            return null;
        }

        private bool CreateSmiles(IEnumerable<RSC.Compounds.Compound> compounds)
        {
            using (var db = GetCompoundsContext())
            {
                //	collect all SMILES...
                var allSmiles = compounds.Where(c => c.Smiles != null && !string.IsNullOrEmpty(c.Smiles.IndigoSmiles)).Select(c => c.Smiles).Distinct(new SmilesComparer());

                if (!allSmiles.Any())
                    return false;

                var smilesHash = allSmiles.Select(s => s.IndigoSmilesHash);

                using (DbContextTransaction scope = db.Database.BeginTransaction())
                {
                    //	Lock the table during this transaction in order to ristrict access from diffrent threads...
                    db.Database.ExecuteSqlCommand("SELECT TOP 1 Id FROM Smiles WITH (TABLOCKX, HOLDLOCK)");

                    //	get list of SMILES registered in the system...
                    var existingHash = (from s in db.Smiles.AsNoTracking()
                                        where smilesHash.Contains(s.IndigoSmilesMd5)
                                        select s.IndigoSmilesMd5).ToList();

                    //	get list of SMILES to create...
                    var smilesToCreate = allSmiles.Where(s => !existingHash.Any(h => h == s.IndigoSmilesHash)).ToList();

                    //	create new SMILES...
                    if (smilesToCreate.Any())
                    {
                        db.BulkInsert(smilesToCreate.Select(s => new ef_Smiles()
                        {
                            Id = Guid.NewGuid(),
                            IndigoSmiles = s.IndigoSmiles,
                            IndigoSmilesMd5 = s.IndigoSmilesHash
                        }), scope.UnderlyingTransaction);

                        db.SaveChanges();
                        scope.Commit();
                    }
                }

                //	cache SMILES dictionary in order to use it later... 
                _smilesDictionary = (from s in db.Smiles.AsNoTracking()
                                     where smilesHash.Contains(s.IndigoSmilesMd5)
                                     select new { s.IndigoSmilesMd5, s.Id }).ToDictionary(kvp => kvp.IndigoSmilesMd5, kvp => kvp.Id);

                return true;
            }
        }

        private bool CreateSubstances(Guid dataSource, IEnumerable<RecordData> data)
        {
            using (var db = GetCompoundsContext())
            {
                var externalIds = data.Select(d => d.ExternalId).Distinct();

                using (DbContextTransaction scope = db.Database.BeginTransaction())
                {
                    //	Lock the table during this transaction in order to ristrict access from diffrent threads...
                    db.Database.ExecuteSqlCommand("SELECT TOP 1 Id FROM Substances WITH (TABLOCKX, HOLDLOCK)");

                    // find out which externalIds do not have substances
                    var existingExternalIds = db.Substances.Where(s => externalIds.Any(id => id == s.ExternalIdentifier)).Select(s => s.ExternalIdentifier).ToList();

                    var newExternalIds = externalIds.Except(existingExternalIds).ToList();

                    if (newExternalIds.Any())
                    {
                        db.BulkInsert(newExternalIds.Select(extId => new ef_Substance()
                        {
                            Id = Guid.NewGuid(),
                            ExternalIdentifier = extId,
                            DataSourceId = dataSource
                        }), scope.UnderlyingTransaction);

                        db.SaveChanges();
                        scope.Commit();
                    }
                }

                //	cache SMILES dictionary in order to use it later... 
                _substancesDictionary = (from s in db.Substances.AsNoTracking()
                                         where externalIds.Contains(s.ExternalIdentifier) && s.DataSourceId == dataSource
                                         select new { s.ExternalIdentifier, s.Id }).ToDictionary(kvp => kvp.ExternalIdentifier, kvp => kvp.Id);

                return true;
            }
        }

        private bool CreateCompounds(Guid dataSource, Guid deposition, IEnumerable<RecordData> data)
        {
            using (var db = GetCompoundsContext())
            {
                var newCompounds = new List<ef_Compound>();
                var newRevisions = new List<ef_Revision>();
                var newParentChildren = new List<ef_ParentChild>();
                var newIssues = new List<ef_Issue>();
                var newProperties = new List<ef_Property>();
                var newExternalIdentifiers = new List<ef_ExternalReference>();

                using (var transaction = db.Database.BeginTransaction())
                {
                    //	cache COMPOUNDS dictionary {InChI Id, Compound Id}  in order to use it later... 
                    _compoundsDictionary = (from c in db.Compounds.AsNoTracking()
                                            where c.NonStandardInChIId != null && _inChIKeysDictionary.Values.Contains((Guid)c.NonStandardInChIId)
                                            select new { c.NonStandardInChIId, c.Id }).ToDictionary(kvp => (Guid)kvp.NonStandardInChIId, kvp => kvp.Id);

                    _compoundsDictionary.Merge((from c in db.Compounds.AsNoTracking()
                                                where c.TautomericNonStdInChIId != null && c.NonStandardInChIId == null && _inChIKeysDictionary.Values.Contains((Guid)c.TautomericNonStdInChIId)
                                                select new { c.TautomericNonStdInChIId, c.Id }).ToDictionary(kvp => (Guid)kvp.TautomericNonStdInChIId, kvp => kvp.Id));

                    var allRevisionIds = data.Select(r => r.Compound.Id).ToList();
                    var existingRevisionIds = db.Revisions.Where(r => allRevisionIds.Contains(r.Id)).Select(r => r.Id).ToList();
                    var newRevisionIds = allRevisionIds.Where(r => !existingRevisionIds.Any(id => id == r)).ToList();
                    var allSubstances = _substancesDictionary.Values.ToList();

                    var substanceVersions = db.Revisions.Where(r => allSubstances.Contains(r.SubstanceId))
                        .OrderByDescending(s => new { s.SubstanceId, s.Version })
                        .GroupBy(s => s.SubstanceId)
                        .Select(g => new { SubstanceId = g.Key, Vsersion = g.FirstOrDefault().Version })
                        .Distinct()
                        .ToDictionary(i => i.SubstanceId, i => i.Vsersion);

                    //Cache the list of External Id Types.
                    var externalIdTypes = db.ExternalReferenceTypes.AsNoTracking().ToList();

                    foreach (var record in data)
                    {
                        var substanceId = _substancesDictionary[record.ExternalId];

                        //Either the existing Compound Id or a new one if the compound doesn't already exist.
                        Guid compoundId;

                        //	now trying to figure out if compounds with the specific Non-Standard InChI already registered...
                        if (_compoundsDictionary.ContainsKey(_inChIKeysDictionary[record.Compound.NonStandardInChI.InChIKey]))
                        {
                            //	compound already registered so we just need compound's Id...
                            compoundId = _compoundsDictionary[_inChIKeysDictionary[record.Compound.NonStandardInChI.InChIKey]];
                        }
                        else
                        {
                            //	compound is not registered yet... so, register new one...
                            compoundId = Guid.NewGuid();

                            var smilesId = record.Compound.Smiles == null || string.IsNullOrEmpty(record.Compound.Smiles.IndigoSmilesHash) ? (Guid?)null : _smilesDictionary[record.Compound.Smiles.IndigoSmilesHash];
                            var nonStdInChIId = record.Compound.NonStandardInChI == null || string.IsNullOrEmpty(record.Compound.NonStandardInChI.InChIKey) ? (Guid?)null : _inChIKeysDictionary[record.Compound.NonStandardInChI.InChIKey];
                            var stdInChIId = record.Compound.StandardInChI == null || string.IsNullOrEmpty(record.Compound.StandardInChI.InChIKey) ? (Guid?)null : _inChIKeysDictionary[record.Compound.StandardInChI.InChIKey];
                            var tautNonStdInChIId = record.Compound.TautomericNonStdInChI == null || string.IsNullOrEmpty(record.Compound.TautomericNonStdInChI.InChIKey) ? (Guid?)null : _inChIKeysDictionary[record.Compound.TautomericNonStdInChI.InChIKey];

                            _compoundsDictionary.Add((Guid)nonStdInChIId, compoundId);

                            var compound = new ef_Compound()
                            {
                                Id = compoundId,
                                Mol = record.Compound.Mol,
                                DateCreated = DateTime.Now,
                                NonStandardInChIId = nonStdInChIId,
                                StandardInChIId = stdInChIId,
                                TautomericNonStdInChIId = tautNonStdInChIId,
                                SmilesId = smilesId
                            };

                            //	process compound's parents...
                            if (record.Parents != null)
                            {
                                foreach (var parent in record.Parents)
                                {
                                    Guid parentCompoundId = new Guid();

                                    if (parent.NonStandardInChI != null && _compoundsDictionary.ContainsKey(_inChIKeysDictionary[parent.NonStandardInChI.InChIKey]))
                                    {
                                        parentCompoundId = _compoundsDictionary[_inChIKeysDictionary[parent.NonStandardInChI.InChIKey]];
                                    }
                                    else if (parent.TautomericNonStdInChI != null && _compoundsDictionary.ContainsKey(_inChIKeysDictionary[parent.TautomericNonStdInChI.InChIKey]))
                                    {
                                        parentCompoundId = _compoundsDictionary[_inChIKeysDictionary[parent.TautomericNonStdInChI.InChIKey]];
                                    }

                                    if (parentCompoundId == Guid.Empty)
                                    {
                                        parentCompoundId = Guid.NewGuid();

                                        smilesId = parent.Smiles == null || string.IsNullOrEmpty(parent.Smiles.IndigoSmilesHash) ? (Guid?)null : _smilesDictionary[parent.Smiles.IndigoSmilesHash];
                                        nonStdInChIId = parent.NonStandardInChI == null || string.IsNullOrEmpty(parent.NonStandardInChI.InChIKey) ? (Guid?)null : _inChIKeysDictionary[parent.NonStandardInChI.InChIKey];
                                        stdInChIId = parent.StandardInChI == null || string.IsNullOrEmpty(parent.StandardInChI.InChIKey) ? (Guid?)null : _inChIKeysDictionary[parent.StandardInChI.InChIKey];
                                        tautNonStdInChIId = parent.TautomericNonStdInChI == null || string.IsNullOrEmpty(parent.TautomericNonStdInChI.InChIKey) ? (Guid?)null : _inChIKeysDictionary[parent.TautomericNonStdInChI.InChIKey];

                                        if (nonStdInChIId != null)
                                            _compoundsDictionary.Add((Guid)nonStdInChIId, parentCompoundId);
                                        else if (tautNonStdInChIId != null)
                                            _compoundsDictionary.Add((Guid)tautNonStdInChIId, parentCompoundId);

                                        var parentCompound = new ef_Compound()
                                        {
                                            Id = parentCompoundId,
                                            Mol = parent.Mol,
                                            DateCreated = DateTime.Now,
                                            NonStandardInChIId = nonStdInChIId,
                                            StandardInChIId = stdInChIId,
                                            TautomericNonStdInChIId = tautNonStdInChIId,
                                            SmilesId = smilesId
                                        };

                                        newCompounds.Add(parentCompound);

                                        newParentChildren.Add(new ef_ParentChild()
                                        {
                                            Id = Guid.NewGuid(),
                                            Type = parent.Relationship,
                                            ParentId = parentCompoundId,
                                            ChildId = compoundId
                                        });
                                    }
                                }
                            }

                            //Add the External Identifiers for new compounds. Note* - at the moment only for new compounds, but in the future we would need to check existing references for any changes.
                            if (record.Compound.ExternalReferences != null)
                            {
                                newExternalIdentifiers.AddRange(
                                    record.Compound.ExternalReferences.Select(e => new ef_ExternalReference()
                                    {
                                        ExternalReferenceTypeId = externalIdTypes.FirstOrDefault(i => i.UriSpace == e.Type.UriSpace).Id,
                                        Value = e.Value,
                                        CompoundId = compound.Id
                                    })
                                    .ToList());
                            }

                            newCompounds.Add(compound);
                        }

                        ef_Revision revision = null;

                        //	check if we have to register new revision...
                        if (newRevisionIds.Contains(record.Compound.Id))
                        {
                            //	revision wasn't found, so create the new one...
                            revision = new ef_Revision()
                            {
                                Id = record.Compound.Id, //	We should use the CVSP Compound Id here as this is linked to the Properties.
                                Version = substanceVersions.ContainsKey(substanceId) ? substanceVersions[substanceId] + 1 : 0,
                                DateCreated = DateTime.Now,
                                SubstanceId = substanceId,
                                CompoundId = compoundId,
                                Sdf = record.Revision.Sdf,
                                Revoked = record.Revision.Revoked,
                                EmbargoDate = record.Revision.EmbargoDate,
                                DepositionId = deposition
                            };

                            newRevisions.Add(revision);
                        }
                        else
                        {
                            //	revision was found, so we just need to update some fields...
                            revision = db.Revisions.Single(r => r.Id == record.Compound.Id);

                            //revision.DateModified = DateTime.Now;
                            //revision.Sdf = record.Revision.Sdf;
                            //revision.Revoked = record.Revision.Revoked;
                            //revision.EmbargoDate = record.Revision.EmbargoDate;
                            //revision.Compound = compound;
                            //revision.CompoundId = compound.Id;
                            //db.Issues.RemoveRange(revision.Issues);

                            //	TODO: check how properly update Compound properly... 
                            //var entity = db.Entry(revision);
                            //entity.State = System.Data.Entity.EntityState.Modified;
                        }

                        if (record.Revision.Issues != null)
                        {
                            newIssues.AddRange(record.Revision.Issues.Select(i => new ef_Issue()
                            {
                                Code = i.Code,
                                LogId = i.Id,
                                RevisionId = revision.Id,
                            }));
                        }

                        if (record.Revision.Properties != null)
                        {
                            newProperties.AddRange(record.Revision.Properties.Select(p => new ef_Property()
                            {
                                RevisionId = revision.Id,
                                PropertyId = p
                            }).ToList());
                        }
                    }
                }

                using (var transactionScope = new TransactionScope())
                {
                    var bulkInsertOptions = new BulkInsertOptions()
                    {
                        EnableStreaming = true,
                        TimeOut = (int)db.Database.CommandTimeout
                    };
                    db.BulkInsert(newCompounds, bulkInsertOptions);
                    db.BulkInsert(newRevisions, bulkInsertOptions);
                    db.BulkInsert(newParentChildren, bulkInsertOptions);
                    db.BulkInsert(newIssues, bulkInsertOptions);
                    db.BulkInsert(newProperties, bulkInsertOptions);
                    db.BulkInsert(newExternalIdentifiers, bulkInsertOptions);

                    db.SaveChanges();
                    transactionScope.Complete();
                }

                return true;
            }
        }

        private bool CreateSynonyms(IEnumerable<RSC.Compounds.Compound> compounds, Guid deposition, Guid user)
        {
            try
            {
                using (var db = new CompoundsContext())
                {
                    //For testing purposes - uncomment to view SQL statements in the trace window.
                    //db.Database.Log = s => { System.Diagnostics.Debug.WriteLine(s); };

                    using (var transactionScope = new TransactionScope())
                    {
                        var bSaveRequired = false;

                        var newCompoundSynonyms = new List<ef_CompoundSynonym>();
                        var newCompoundSynonymHistory = new List<ef_CompoundSynonymHistory>();
                        var newCompoundSynonymSynonymFlagHistory = new List<ef_CompoundSynonymSynonymFlagHistory>();

                        var newSynonyms = new List<ef_Synonym>();
                        var newSynonymHistory = new List<ef_SynonymHistory>();
                        var newSynonymSynonymFlagHistory = new List<ef_SynonymSynonymFlagHistory>();

                        //We can add flags at the Synonym or CompoundSynonym level.
                        var newSynonymSynonymFlags = new List<ef_SynonymSynonymFlag>();
                        var newCompoundSynonymSynonymFlags = new List<ef_CompoundSynonymSynonymFlag>();

                        //Some Synonyms can be deprecated on the basis of the IsUniquePerLanguage flag.
                        var forDeprecationCompoundSynonyms = new List<ef_CompoundSynonym>();

                        //Identifiers that we mint ourselves.
                        var nextSynonymId = new Guid();
                        var nextCompoundSynonymId = new Guid();
                        var nextCompoundSynonymHistoryId = new Guid();
                        var nextCompoundSynonymSynonymFlagId = new Guid();
                        var nextCompoundSynonymSynonymFlagHistoryId = new Guid();
                        var nextSynonymHistoryId = new Guid();
                        var nextSynonymSynonymFlagHistoryId = new Guid();

                        //Get the list of compounds.
                        var compoundsList = _compoundsDictionary.Values.ToList().Distinct();

                        //Get the list of Synonym flags.
                        var dbSynonymFlags = db.SynonymFlags.ToList();

                        //Get the current assigned Compound Synonyms.
                        var dbCompoundSynonymsList = (from cs in db.CompoundSynonyms
                                                      .Include(i => i.CompoundSynonymSynonymFlags)
                                                      where compoundsList.Contains(cs.CompoundId)
                                                      select cs).ToList();

                        //Get a list of all the synonyms to be assigned.
                        var synonymsToAssign = compounds.Where(c => c.Synonyms != null && c.Synonyms.Any())
                                                        .SelectMany(compound => compound.Synonyms)
                                                        .Distinct()
                                                        .ToList();

                        var synonymNamesToAssign = synonymsToAssign.Select(a => a.Name).ToList();

                        //Here we must first retrieve the synonyms without language, then filter by language second or SQL is very slow.
                        var dbExistingSynonymList = (from s in db.Synonyms
                                                                .Include(s => s.SynonymFlags)
                                                     where synonymNamesToAssign.Contains(s.Synonym)
                                                     select s)
                                                     .ToList();

                        //Next remove those that are not in the right language.
                        foreach (var existingSynonym in dbExistingSynonymList
                                                        .Where(existingSynonym => 
                                                            !synonymsToAssign.Any(s => s.Name == existingSynonym.Synonym 
                                                                && s.LanguageId == existingSynonym.LanguageId)))
                        {
                            dbExistingSynonymList.Remove(existingSynonym);
                        }

                        //Iterate each compound that has synonyms to upload.
                        foreach (var compound in compounds.Where(c => c.Synonyms != null && c.Synonyms.Any()))
                        {
                            //The compounds are already registered so retrieve the id.
                            var compoundId = _compoundsDictionary[_inChIKeysDictionary[compound.NonStandardInChI.InChIKey]];

                            foreach (var synonym in compound.Synonyms)
                            {
                                //We can't store anything over 450 in the table so discard.
                                if (synonym.Name.Length <= 450)
                                {
                                    var bExistingSynonym = false;
                                    var bExistingCompoundSynonym = false;

                                    //Does the synonym already exist in the database?
                                    var dbSynonym = dbExistingSynonymList.FirstOrDefault(s => s.Synonym == synonym.Name && s.LanguageId == synonym.LanguageId);

                                    if (dbSynonym == null)
                                    {
                                        //Check to see if we are already adding this synonym.
                                        if (!newSynonyms.Any(s => String.Equals(s.Synonym, synonym.Name, StringComparison.OrdinalIgnoreCase) && s.LanguageId == synonym.LanguageId))
                                        {
                                            nextSynonymId = Guid.NewGuid();

                                            //Create a new Synonym for the Language, Synonym combination.
                                            dbSynonym = new ef_Synonym()
                                            {
                                                Id = nextSynonymId,
                                                Synonym = synonym.Name,
                                                LanguageId = synonym.LanguageId,
                                                DateCreated = synonym.DateCreated == DateTime.MinValue ? DateTime.Now : synonym.DateCreated,
                                                Revisions = new List<ef_Revision>(),
                                            };
                                            newSynonyms.Add(dbSynonym);

                                            //Create the Synonym History for the new Synonym.
                                            nextSynonymHistoryId = Guid.NewGuid();
                                            newSynonymHistory.Add(new ef_SynonymHistory { Id = nextSynonymHistoryId, SynonymId = dbSynonym.Id, CuratorId = user, DateChanged = DateTime.Now });
                                        }
                                    }
                                    else
                                        bExistingSynonym = true;

                                    //Is the synonym already assigned to the compound?
                                    ef_CompoundSynonym dbCompoundSynonym = null;
                                    ef_CompoundSynonymHistory dbCompoundSynonymHistory = null;
                                    var bWriteCompoundSynonymHistory = false;

                                    if (dbSynonym != null)
                                        dbCompoundSynonym = dbCompoundSynonymsList.FirstOrDefault(cs => cs.CompoundId == compoundId && cs.SynonymId == dbSynonym.Id);

                                    if (dbCompoundSynonym == null)
                                    {
                                        nextCompoundSynonymId = Guid.NewGuid();

                                        //Create a new Compound Synonym.
                                        dbCompoundSynonym = new ef_CompoundSynonym
                                        {
                                            Id = nextCompoundSynonymId,
                                            CompoundId = compoundId,
                                            SynonymId = dbSynonym != null ? dbSynonym.Id : nextSynonymId, //Existing or new SynonymId.
                                            IsTitle = synonym.IsTitle,
                                            SynonymState = synonym.State
                                        };
                                        newCompoundSynonyms.Add(dbCompoundSynonym);
                                        bWriteCompoundSynonymHistory = true;
                                    }
                                    else
                                    {
                                        //Need to assign Title to existing Compound Synonym.
                                        if (synonym.IsTitle && !dbCompoundSynonym.IsTitle)
                                        {
                                            //Check to see if another Synonym is assigned the Title and unassign it.
                                            foreach (var compoundSynonymTitle in dbCompoundSynonymsList.Where(cs => cs.IsTitle == true))
                                            {
                                                compoundSynonymTitle.IsTitle = false;

                                                //Create a new CompoundSynonymHistory for all the synonyms that are no longer the title.
                                                nextCompoundSynonymHistoryId = Guid.NewGuid();

                                                dbCompoundSynonymHistory = new ef_CompoundSynonymHistory
                                                {
                                                    Id = nextCompoundSynonymHistoryId,
                                                    CuratorId = user,
                                                    DateChanged = DateTime.Now,
                                                    SynonymState = compoundSynonymTitle.SynonymState,
                                                    IsTitle = compoundSynonymTitle.IsTitle,
                                                    CompoundSynonymId = compoundSynonymTitle.Id
                                                };
                                                newCompoundSynonymHistory.Add(dbCompoundSynonymHistory);
                                            }

                                            dbCompoundSynonym.IsTitle = synonym.IsTitle;
                                            bSaveRequired = true; //We need to save these changes at the end.
                                            bWriteCompoundSynonymHistory = true;
                                        }

                                        //Check to see if this synonym should be approved if currently unconfirmed. //TODO: We could make this more complex later on.
                                        if (synonym.State == CompoundSynonymState.eApproved)
                                        {
                                            if (dbCompoundSynonym.SynonymState == CompoundSynonymState.eUnconfirmed)
                                            {
                                                dbCompoundSynonym.SynonymState = CompoundSynonymState.eApproved; //Set to Approved.
                                                bSaveRequired = true; //We need to save these changes at the end.
                                                bWriteCompoundSynonymHistory = true;
                                            }
                                        }

                                        bExistingCompoundSynonym = true;
                                    }

                                    //Must we write a Compound Synonym History record?
                                    if (bWriteCompoundSynonymHistory)
                                    {
                                        //Create the id here as we need it later on if the flags are changed.
                                        nextCompoundSynonymHistoryId = Guid.NewGuid();
                                    }

                                    //Work out any flags to add if any have been assigned, we must also assign any new flags to existing synonyms.
                                    var bWriteSynonymHistory = false;

                                    if (synonym.Flags != null && synonym.Flags.Any())
                                    {
                                        foreach (var synonymFlag in synonym.Flags)
                                        {
                                            var dbSynonymFlag = dbSynonymFlags.FirstOrDefault(sf => sf.Name == synonymFlag.Name);

                                            if (dbSynonymFlag != null)
                                            {
                                                //Is this a Synonym flag or a Compound Synonym Flag?
                                                if (dbSynonymFlag.Type == SynonymFlagType.SynonymType)
                                                {
                                                    /*
                                                    //Synonym Type flags.
                                                    */
                                                    if (!newSynonymSynonymFlags.Any(ssf => ssf.SynonymFlagId == dbSynonymFlag.Id && ssf.SynonymId == dbSynonym.Id))
                                                    {
                                                        bool bAddFlag = false;

                                                        //Check to see whether the existing synonym already has the flag.
                                                        if (bExistingSynonym)
                                                        {
                                                            if (!dbSynonym.SynonymFlags.Any(sf => sf.SynonymFlagId == dbSynonymFlag.Id))
                                                                bAddFlag = true;
                                                        }
                                                        else
                                                            bAddFlag = true;

                                                        //Add the synonym flag if required.
                                                        if (bAddFlag)
                                                        {
                                                            //Add the SynonymHistory record if an existing synonym and we haven't already created it. 
                                                            if (!bWriteSynonymHistory && bExistingSynonym)
                                                            {
                                                                nextSynonymHistoryId = Guid.NewGuid();
                                                                newSynonymHistory.Add(new ef_SynonymHistory { Id = nextSynonymHistoryId, SynonymId = dbSynonym.Id, CuratorId = user, DateChanged = DateTime.Now });
                                                                bWriteSynonymHistory = true;
                                                            }

                                                            //Check to see if this flag should be unique per language, we can't add it if there is already an instance of it for the given language.
                                                            if (dbSynonymFlag.IsUniquePerLanguage)
                                                            {
                                                                //Deprecate the existing Compound Synonyms if they have been added to the same compound previously using the same flag and language.
                                                                forDeprecationCompoundSynonyms.AddRange((from cs in db.CompoundSynonyms
                                                                                                         join ssf in db.SynonymSynonymFlags on cs.SynonymId equals ssf.SynonymId
                                                                                                         join s in db.Synonyms on ssf.SynonymId equals s.Id
                                                                                                         where cs.CompoundId == compoundId
                                                                                                                 && s.LanguageId == synonym.LanguageId
                                                                                                                 && ssf.SynonymFlagId == dbSynonymFlag.Id
                                                                                                                 && cs.SynonymState != CompoundSynonymState.eDeleted //Exclude deprecated Compound Synonyms.
                                                                                                         select cs).ToList());
                                                            }

                                                            //Write the Flag History.
                                                            nextSynonymSynonymFlagHistoryId = Guid.NewGuid();
                                                            newSynonymSynonymFlagHistory.Add(new ef_SynonymSynonymFlagHistory { Id = nextSynonymSynonymFlagHistoryId, SynonymFlagId = dbSynonymFlag.Id, SynonymHistoryId = nextSynonymHistoryId });

                                                            //Write the SynonymSynonymFlags.
                                                            newSynonymSynonymFlags.Add(new ef_SynonymSynonymFlag { SynonymFlagId = dbSynonymFlag.Id, SynonymId = dbSynonym.Id });
                                                        }
                                                    }
                                                }
                                                else
                                                {
                                                    /*
                                                    //Compound Synonym Type flags.
                                                    */
                                                    if (!newCompoundSynonymSynonymFlags.Any(ssf => ssf.SynonymFlagId == dbSynonymFlag.Id && ssf.CompoundSynonymId == dbCompoundSynonym.Id))
                                                    {
                                                        bool bAddFlag = false;

                                                        //Check to see whether the existing compound synonym already has the flag.
                                                        if (bExistingCompoundSynonym)
                                                        {
                                                            if (!dbCompoundSynonym.CompoundSynonymSynonymFlags.Any(sf => sf.SynonymFlagId == dbSynonymFlag.Id))
                                                                bAddFlag = true;
                                                        }
                                                        else
                                                            bAddFlag = true;

                                                        if (bAddFlag)
                                                        {
                                                            if (dbSynonymFlag.IsUniquePerLanguage)
                                                            {
                                                                //Deprecate the existing Compound Synonyms if they have been added to the same compound previously using the same flag and language.
                                                                forDeprecationCompoundSynonyms.AddRange((from cs in db.CompoundSynonyms
                                                                                                         join cssf in db.CompoundSynonymSynonymFlags on cs.Id equals cssf.CompoundSynonymId
                                                                                                         join s in db.Synonyms on cs.SynonymId equals s.Id
                                                                                                         where cs.CompoundId == compoundId
                                                                                                                 && s.LanguageId == synonym.LanguageId
                                                                                                                 && cssf.SynonymFlagId == dbSynonymFlag.Id
                                                                                                                 && cs.SynonymState != CompoundSynonymState.eDeleted //Exclude deprecated Compound Synonyms.
                                                                                                         select cs).ToList());
                                                            }

                                                            //Add the compound synonym flag.
                                                            nextCompoundSynonymSynonymFlagId = Guid.NewGuid();

                                                            newCompoundSynonymSynonymFlags.Add(new ef_CompoundSynonymSynonymFlag { Id = nextCompoundSynonymSynonymFlagId, CompoundSynonymId = dbCompoundSynonym.Id, SynonymFlagId = dbSynonymFlag.Id });

                                                            //Have we already created the id for Compound Synonyn History? If not create it.
                                                            if (!bWriteCompoundSynonymHistory)
                                                            {
                                                                nextCompoundSynonymHistoryId = Guid.NewGuid();
                                                                bWriteCompoundSynonymHistory = true;
                                                            }

                                                            //Write the flags added to the history.
                                                            nextCompoundSynonymSynonymFlagHistoryId = Guid.NewGuid();
                                                            newCompoundSynonymSynonymFlagHistory.Add(new ef_CompoundSynonymSynonymFlagHistory
                                                            {
                                                                Id = nextCompoundSynonymSynonymFlagHistoryId,
                                                                CompoundSynonymHistoryId = nextCompoundSynonymHistoryId,
                                                                SynonymFlagId = dbSynonymFlag.Id
                                                            });
                                                        }
                                                    }
                                                }
                                            }
                                        }
                                    }

                                    //Must we write a Compound Synonym History record?
                                    if (bWriteCompoundSynonymHistory)
                                    {
                                        //Create a new CompoundSynonymHistory.
                                        dbCompoundSynonymHistory = new ef_CompoundSynonymHistory
                                        {
                                            Id = nextCompoundSynonymHistoryId,
                                            CuratorId = user,
                                            DateChanged = DateTime.Now,
                                            SynonymState = synonym.State,
                                            IsTitle = synonym.IsTitle,
                                            CompoundSynonymId = dbCompoundSynonym.Id
                                        };
                                        newCompoundSynonymHistory.Add(dbCompoundSynonymHistory);

                                        //Add the existing flags to the history, we will add those to add later on.
                                        if (bExistingCompoundSynonym)
                                        {
                                            foreach (var flag in dbCompoundSynonym.CompoundSynonymSynonymFlags)
                                            {
                                                nextCompoundSynonymSynonymFlagHistoryId = Guid.NewGuid();

                                                newCompoundSynonymSynonymFlagHistory.Add(new ef_CompoundSynonymSynonymFlagHistory
                                                {
                                                    Id = nextCompoundSynonymSynonymFlagHistoryId,
                                                    CompoundSynonymHistoryId = nextCompoundSynonymHistoryId,
                                                    SynonymFlagId = flag.SynonymFlagId
                                                });
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        //Deprecate any Compound Synonyms marked for Deprecation.
                        foreach (var cs in forDeprecationCompoundSynonyms)
                        {
                            //Mark as deprecated.
                            cs.SynonymState = CompoundSynonymState.eDeleted;
                            bSaveRequired = true;

                            //Write Compound Synonym History.
                            nextCompoundSynonymHistoryId = Guid.NewGuid();

                            var dbCompoundSynonymHistory = new ef_CompoundSynonymHistory
                            {
                                Id = nextCompoundSynonymHistoryId,
                                CuratorId = user,
                                DateChanged = DateTime.Now,
                                SynonymState = cs.SynonymState,
                                IsTitle = cs.IsTitle,
                                CompoundSynonymId = cs.Id
                            };

                            newCompoundSynonymHistory.Add(dbCompoundSynonymHistory);
                        }

                        //Bulk insert of new Synonyms, Compound Synonyms, Compound Synonym History and both types of Synonym Flags.
                        var bulkInsertOptions = new BulkInsertOptions()
                        {
                            EnableStreaming = true,
                            TimeOut = (int)(db.Database.CommandTimeout ?? -1)
                        };

                        if (newSynonyms.Count > 0)
                            db.BulkInsert(newSynonyms, bulkInsertOptions);

                        if (newCompoundSynonyms.Count > 0)
                            db.BulkInsert(newCompoundSynonyms, bulkInsertOptions);

                        if (newCompoundSynonymHistory.Count > 0)
                            db.BulkInsert(newCompoundSynonymHistory, bulkInsertOptions);

                        if (newCompoundSynonymSynonymFlags.Count > 0)
                            db.BulkInsert(newCompoundSynonymSynonymFlags, bulkInsertOptions);

                        if (newCompoundSynonymSynonymFlagHistory.Count > 0)
                            db.BulkInsert(newCompoundSynonymSynonymFlagHistory, bulkInsertOptions);

                        if (newSynonymHistory.Count > 0)
                            db.BulkInsert(newSynonymHistory, bulkInsertOptions);

                        if (newSynonymSynonymFlags.Count > 0)
                            db.BulkInsert(newSynonymSynonymFlags, bulkInsertOptions);

                        if (newSynonymSynonymFlagHistory.Count > 0)
                            db.BulkInsert(newSynonymSynonymFlagHistory, bulkInsertOptions);

                        //Any changes to existing CompoundSynonyms saved here if required.
                        if (bSaveRequired)
                            db.SaveChanges();

                        //Commit the transaction.
                        transactionScope.Complete();

                        return true;
                    }
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
