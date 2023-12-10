#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Threading.Tasks;

using Cqrs.Azure.ConfigurationManager;
using Cqrs.Configuration;
using NUnit.Framework;

using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

#if NET472_OR_GREATER
#else
using Microsoft.Extensions.Configuration;
#endif

namespace Cqrs.Azure.KeyVault.Tests.Integration
{
	[TestClass]
	public class KeyVaultSecretFactoryTests
	{
		[TestMethod]
		public void GetSecret_KnownTestSecret_KnownValue()
		{
			// Arrange
			IConfigurationManager configurationManager;
#if NET472_OR_GREATER
			configurationManager = new CloudConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("cqrs.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			configurationManager = new CloudConfigurationManager(config);
			Configuration.ConfigurationManager.BaseConfiguration = config;
			DependencyResolver.ConfigurationManager = configurationManager;
#endif
			var factory = new KeyVaultSecretFactory(configurationManager);

			// Act
			string val = factory.GetSecret("TestKey");

			// Assert
			Assert.AreEqual("Ab3rP&4o4@FE9t9T9prn45@#pKEPX3eLKKmjR4T!", val);
		}

		[TestMethod]
		public async Task GetSecretAsync_KnownTestSecret_KnownValue()
		{
			// Arrange
			IConfigurationManager configurationManager;
#if NET472_OR_GREATER
			configurationManager = new CloudConfigurationManager();
#else
			IConfigurationRoot config = new ConfigurationBuilder()
				.AddJsonFile("cqrs.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
				.Build();

			configurationManager = new CloudConfigurationManager(config);
			Configuration.ConfigurationManager.BaseConfiguration = config;
			DependencyResolver.ConfigurationManager = configurationManager;
#endif
			var factory = new KeyVaultSecretFactory(configurationManager);

			// Act
			string val = await factory.GetSecretAsync("TestKey");

			// Assert
			Assert.AreEqual("Ab3rP&4o4@FE9t9T9prn45@#pKEPX3eLKKmjR4T!", val);
		}
	}
}