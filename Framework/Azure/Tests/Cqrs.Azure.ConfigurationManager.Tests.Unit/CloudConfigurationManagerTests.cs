using Cqrs.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cqrs.Azure.ConfigurationManager.Tests.Unit
{
	[TestClass]
	public class CloudConfigurationManagerTests
	{
		[TestMethod]
		public void GetSetting_TestKey_ReturnsExpectedValue()
		{
			// Arrange
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("test-settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();
			IConfigurationManager configurationManager = new CloudConfigurationManager(config);

			// Act
			string value = configurationManager.GetSetting("Cqrs.Azure.CommandBus.ConnectionString");

			// Assert
			Assert.AreEqual("bob", value);
		}

		[TestMethod]
		public void GetSetting_TestBooleanKeys_ReturnsExpectedValue()
		{
			// Arrange
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("test-settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();
			IConfigurationManager configurationManager = new CloudConfigurationManager(config);

			// Act
			bool value1 = bool.Parse(configurationManager.GetSetting("Cqrs.BooleanTestTrue"));
			bool value2 = bool.Parse(configurationManager.GetSetting("Cqrs.BooleanStringTestTrue"));
			bool value3 = bool.Parse(configurationManager.GetSetting("Cqrs.BooleanTestFalse"));
			bool value4 = bool.Parse(configurationManager.GetSetting("Cqrs.BooleanStringTestFalse"));

			// Assert
			Assert.IsTrue(value1);
			Assert.IsTrue(value2);
			Assert.IsFalse(value3);
			Assert.IsFalse(value4);
		}

		[TestMethod]
		public void GetSetting_MissingTestKey_ReturnsNull()
		{
			// Arrange
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("test-settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();
			IConfigurationManager configurationManager = new CloudConfigurationManager(config);

			// Act
			string value = configurationManager.GetSetting("Cqrs.Key.Not.Set");

			// Assert
			Assert.IsNull(value);
		}

		[TestMethod]
		public void GetSetting_RandomTest_ReturnsExpectedValue()
		{
			// Arrange
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("test-settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			// Act
			string value1 = config.GetValue<string>("Chinchilla.Logging.ModuleName".Replace(".", ":"));
			bool value2 = bool.Parse(config.GetValue<string>("Chinchilla.Logging.DoAsyncWork.EnableLogProgress".Replace(".", ":")));

			// Assert
			Assert.AreEqual("MyCompany", value1);
			Assert.IsTrue(value2);
		}

		[TestMethod]
		public void GetConnectionString_AppInSightsKey_ReturnsExpectedFullyQualifiedValue()
		{
			// Arrange
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("test-settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();
			IConfigurationManager configurationManager = new CloudConfigurationManager(config);

			// Act
			string value = configurationManager.GetConnectionString("Cqrs.Hosts.ApplicationInsights.ConnectionString");

			// Assert
			Assert.AreEqual("Application Insights Connection string from fully-qualified connection strings", value);
		}
	}
}