using Microsoft.Extensions.Configuration;
using Cosmos.DataTransfer.JsonExtension.UnitTests;
using Cosmos.DataTransfer.Interfaces;

namespace Cosmos.DataTransfer.SqlServerExtension.UnitTests
{
    [TestClass]
    public class SqlServerExtensionTests
    {
        private Dictionary<string,string> GetBaseConfig() {
            return new Dictionary<string, string>() {
                { "ConnectionString", "Server" }
            };
        }

        [TestMethod]
        public void TestSourceSettings_ValidationOverload() 
        {
            var settings = TestHelpers.CreateConfig(new Dictionary<string, string>() {
                { "", "" }
            }).Get<SqlServerSourceSettings>();

            Assert.ThrowsException<AggregateException>(() => settings.Validate());
        }

        [TestMethod]
        public void TestSourceSettings_NeitherQueryFails() {
            // Minimum requirement
            var config = GetBaseConfig();
            var settings = TestHelpers.CreateConfig(config).Get<SqlServerSourceSettings>();
            var e = Assert.ThrowsException<AggregateException>(() => settings.Validate());
            CollectionAssert.Contains(e.InnerExceptions.Select(x => x.Message).ToList(), 
               "Either `QueryText` or `FilePath` are required!");
        }

        [TestMethod]
        public void TestSourceSettings_ValidateWithQueryText() {
            // Minimum requirement
            var config = GetBaseConfig();
            config["QueryText"] = "SELECT * FROM foobar;";
            var settings = TestHelpers.CreateConfig(config).Get<SqlServerSourceSettings>();
            settings.Validate();
        }

        [TestMethod]
        public void TestSourceSettings_ValidateWithFilePath() {
            // Minimum requirement
            var config = GetBaseConfig();
            config["FilePath"] = "file";
            var settings = TestHelpers.CreateConfig(config).Get<SqlServerSourceSettings>();
            settings.Validate();
        }

        [TestMethod]
        public void TestSourceSettingsValidation_BothTextOrPathFails() 
        {
            var config = GetBaseConfig();
            config["QueryText"] = "SELECT * FROM foobar;";
            config["FilePath"] = "file";
            var settings = TestHelpers.CreateConfig(config).Get<SqlServerSourceSettings>();
            var e = Assert.ThrowsException<AggregateException>(() => settings.Validate());
            CollectionAssert.Contains(e.InnerExceptions.Select(x => x.Message).ToList(), 
                "Both `QueryText` and `FilePath` is not allowed.");
        }
    }
}