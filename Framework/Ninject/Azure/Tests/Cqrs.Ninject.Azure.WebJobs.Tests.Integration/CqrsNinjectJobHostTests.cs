#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Configuration;
using System.Net.NetworkInformation;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Ninject.Azure.WebJobs;
using Cqrs.Ninject.Configuration;

#if NET472
#else
using Microsoft.Extensions.Configuration;
#endif

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Cqrs.Azure.ConfigurationManager.Tests.Unit
{
	[TestClass]
	public class CqrsNinjectJobHostTests
	{
		[TestMethod]
		public void Run_RoleNameAndOperationNameSettings_NoErrors()
		{
			// Arrange
			NinjectDependencyResolver.ModulesToLoad.Clear();
			var host = new CqrsNinjectJobHost<Guid, DefaultAuthenticationTokenHelper>();

			// Act
			host.Run();
		}

		static IConfigurationManager ConfigurationManager { get; set; }

#if NET472
#else
		static IConfigurationRoot configuration;
#endif

		[TestInitialize]
		public void Setup()
		{
			Console.WriteLine($"Executing directory is: '{System.IO.Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly()?.Location)}'");
#if NET472
			ConfigurationManager = new CloudConfigurationManager();
#else
			configuration = new ConfigurationBuilder()
//				.SetBasePath(context.FunctionAppDirectory)
				.AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
				.AddEnvironmentVariables()
			.Build();

			CqrsNinjectJobHost<Guid, DefaultAuthenticationTokenHelper>.SetConfigurationManager(configuration);
			ConfigurationManager = new CloudConfigurationManager(configuration);
#endif
		}
	}
}