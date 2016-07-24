using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.VisualStudio.TestTools.UnitTesting;

using RSC.Compounds.DataExport;
using RSC.Properties;

namespace RSC.Compounds.DataExport_Tests
{
    [TestClass]
    public class PropertyTurtle : DataExportTestBase
    {
        [TestMethod]
        public void PropertyTurtle_GetTurtleProps()
        {
            // try it without any properties
            var result = PropertiesDataExportFile.CompoundPropertyLines("29292", new List<Property>(), new List<PropertyDefinition>());
            Assert.AreEqual(4, result.Count);
            // the properties and propertyDefs come in from DataExportTestBase
            var r2 = PropertiesDataExportFile.CompoundPropertyLines("29300", properties, propertyDefs);
            foreach (string l in r2) Console.WriteLine(l);
            Assert.AreEqual(29, r2.Count);
            Assert.AreEqual(1, r2.Count(l => l.Contains("OBI_0000293")), "should be 1 experimental input");
            Assert.AreEqual(4, r2.Count(l => l.Contains("OBI_0000299")), "should be 4 experimental outputs");
            Assert.AreEqual(2, r2.Count(l => l.Contains("DegreeCelsius")), "should be two lines mentioning DegreeCelsius");
            Assert.AreEqual(1, r2.Count(l => l.Contains("CHEMINF_000363")), "should be one line specifying K_OC");
            Assert.AreEqual(1, r2.Count(l => l.Contains("(pH 5.5)")), "should be one line mentioning pH 5.5");
            Assert.AreEqual(1, r2.Count(l => l.Contains("(pH 7.4)")), "should be one line mentioning pH 7.4");
        }

        [TestMethod]
        public void PropertyTurtle_PropertyProvenance()
        {
            var result = PropertiesDataExportFile.PropertyProvenanceLines("29292");
            Assert.AreEqual(4, result.Count);
            Assert.AreEqual(":29292ct rdf:type cheminf:CHEMINF_000055 .", result.First());
            Assert.AreEqual(":29292ct obo:IAO_0000136 ops:OPS29292 .", result.ElementAt(1));
            Assert.AreEqual(":29292execution rdf:type cheminf:CHEMINF_000354 .", result.ElementAt(2));
            Assert.AreEqual(":29292execution obo:OBI_0000293 :29292ct .", result.ElementAt(3));
        }

        [TestMethod]
        public void PropertyTurtle_PropertyLinesWithError()
        {
            Property p = new Property() { Name = PropertyName.LOG_P, Value = "7", Error = 1 };
            var result = p.ToTurtle("29292", 1, UnitStyle.QUDT, propertyDefs);
            Assert.AreEqual(7, result.Count);
            Assert.AreEqual(":29292execution obo:OBI_0000299 :29292prop1 .", result.First());
            Assert.AreEqual(":29292prop1 obo:IAO_0000136 ops:29292 .", result.ElementAt(2));
            var subjects = result.Select(l => l.Split(new char[] { ' ' })[0]);
            // check that all of the subjects (apart from the first one) are correct
            foreach (var s in subjects.Skip(1)) // and these are the properties
            {
                Assert.AreEqual(":29292prop1", s);
            }
            var predicates = result.Select(l => l.Split(new char[] { ' ' })[1]);
            Assert.IsTrue(predicates.Contains("qudt:numericValue"));
            Assert.IsTrue(predicates.Contains("qudt:unit"));
            Assert.IsTrue(predicates.Contains("qudt:standardUncertainty"));
            foreach (var l in result) Console.WriteLine(l);
        }

        [TestMethod]
        public void PropertyTurtle_PropertyLinesWithoutError()
        {
            Property p = new Property() { Name = PropertyName.BOILING_POINT, Value = "373" };
            var result = p.ToTurtle("29292", 1, UnitStyle.QUDT, propertyDefs);
            foreach (var l in result) Console.WriteLine(l);
            Assert.AreEqual(6, result.Count);
            var subjects = result.Select(l => l.Split(new char[] { ' ' })[0]);
            foreach (var s in subjects.Skip(1)) // and these are the properties
            {
                Assert.AreEqual(":29292prop1", s);
            }
            var predicates = result.Select(l => l.Split(new char[] { ' ' })[1]);
            Assert.IsTrue(predicates.Contains("qudt:numericValue"));
            Assert.IsTrue(predicates.Contains("qudt:unit"));
            Assert.IsFalse(predicates.Contains("qudt:standardUncertainty"));
        }
    }
}
