using Microsoft.VisualStudio.TestTools.UnitTesting;
using RSC.Compounds.EntityFramework;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace RSC.Compounds.Tests
{
	[TestClass]
	public class AddInChIs
	{
/*
		[TestMethod]
		public void AddInChITest()
		{
			string inchi = "InChI=1/C30H26BrN7O2/c1-37-13-23(35-15-37)28(39)30-36-27-26-25-18(7-9-32-21(25)8-10-33-26)24(29(27)40-30)20(14-38(2,3)4)19-12-34-22-11-16(31)5-6-17(19)22/h5-13,15,20,34H,14H2,1-4H3/p+2/t20?/fC30H28BrN7O2/h32-33H/q+2";
			string inchikey = "DBDKFLCLXBAJEZ-GIJIHOCANA-P";

			using (CompoundsContext context = new CompoundsContext())
			{
				//context.Configuration.AutoDetectChangesEnabled = false;
				context.Configuration.ValidateOnSaveEnabled = false;

				context.InChIs.Add(new RSC.Compounds.EntityFramework.ef_InChI()
				{
					InChI = inchi,
					InChIKey = inchikey
				});

				context.SaveChanges();
			}

			using (CompoundsContext context = new CompoundsContext())
			{
				var entity = context.InChIs.Where(i => i.InChIKey == inchikey).FirstOrDefault();

				entity.InChI = "InChI=1S/C18H32O4/c1-5-8-9-13-11-17(21,6-2)16(12(13)4)18(7-3)14(19)10-15(20)22-18/h12-14,16,19,21H,5-11H2,1-4H3";
				entity.InChIKey = "UBWSIOKTVLUARI-UHFFFAOYSA-N";

				context.SaveChanges();
			}

			//InChI=1S/C18H32O4/c1-5-8-9-13-11-17(21,6-2)16(12(13)4)18(7-3)14(19)10-15(20)22-18/h12-14,16,19,21H,5-11H2,1-4H3
			//UBWSIOKTVLUARI-UHFFFAOYSA-N
		}

		[TestMethod]
		public void AddBulkInChITest()
		{
			IEnumerable<Compounds.InChI> inchis = null;

			var ds = new DataContractSerializer(typeof(IEnumerable<Compounds.InChI>));

			using (XmlReader reader = XmlReader.Create(new StringReader(TestResources.InChIs)))
			{
				inchis = (IEnumerable<Compounds.InChI>)ds.ReadObject(reader);
			}

			using (CompoundsContext context = new CompoundsContext())
			{
				var count = context.Compounds.Count();
				context.InChIs.RemoveRange(context.InChIs);
				context.SaveChanges();
			}

			var watch = Stopwatch.StartNew();

			using (var context = new CompoundsContext())
			{
				//context.Configuration.AutoDetectChangesEnabled = false;
				//context.Configuration.ValidateOnSaveEnabled = false;

				using (var dbContextTransaction = context.Database.BeginTransaction())
				{
					try
					{
						//	remove empty InChIs and duplicates...
						inchis = inchis.Where(i => !string.IsNullOrEmpty(i.Inchi)).Distinct(new InChIComparer());

						var allKeys = inchis.Select(i => i.InChIKey).Distinct();

						//	get the list of InChIKeys that already registered in the system...
						var existingInChIKeys = (from i in context.InChIs.AsNoTracking()
												 where allKeys.Contains(i.InChIKey)
												 select i.InChIKey).ToList();

						//	get list of InChIs that should be created...
						var inchisToCreate = inchis.Where(i => !existingInChIKeys.Any(k => k == i.InChIKey)).ToList();

						//	create new InChIs...
						if (inchisToCreate.Any())
						{
							context.InChIs.AddRange(inchisToCreate.Select(i => new RSC.Compounds.EntityFramework.ef_InChI() { InChI = i.Inchi, InChIKey = i.InChIKey }).ToList());

							context.SaveChanges();
						}

						dbContextTransaction.Commit();
					}
					catch (Exception)
					{
						dbContextTransaction.Rollback();
					}
				}
			}

			watch.Stop();
			var elapsedMs = watch.ElapsedMilliseconds;
		}
*/
	}
}
