using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.IO;
using FluentAssertions;

namespace ASettings.Tests
{
    [TestClass()]
    public class SettingsTests
    {
        [TestMethod()]
        public void SettingsTest()
        {
            var errorLine = "john = fred = 1";
            var contents = new List<string>
            {
                "[alpha]",
                "fred=jim",
                "fred=harry",

                " [beta]",
                "harry = sally",
                "# testing",
                "mary = we#ndy",
                "mary = 'we#ndy'",
                errorLine
            };

            var filename = Path.GetTempFileName();
            File.WriteAllLines(filename, contents);
            var s = new Settings(filename);
            var sections = s.Sections;
            sections.Should().BeEquivalentTo("alpha", "beta");

            var s1 = s.Setting("beta", "mary");
            s1.Should().HaveCount(2).And.BeEquivalentTo("we", "'we#ndy'");
            var s2 = s.Setting("beta", "joseph");
            s2.Should().HaveCount(0);
            var s3 = s.Setting("Raymond", "Smith");
            s3.Should().HaveCount(0);
            s.Errors.Should().HaveCount(1);

            var errorLineNumber = contents.IndexOf(errorLine);
            s.Errors[0].linenumber.Should().Equals(errorLineNumber);
            s.Errors[0].line.Should().Equals(errorLine);

        }

        [TestMethod()]
        public void SettingTest()
        {
            Assert.Fail();
        }
    }
}