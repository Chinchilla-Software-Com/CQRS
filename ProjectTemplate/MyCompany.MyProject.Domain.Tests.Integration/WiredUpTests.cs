#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Ninject.Configuration;
using Cqrs.Ninject.InProcess.CommandBus.Configuration;
using Cqrs.Ninject.InProcess.EventBus.Configuration;
using Cqrs.Ninject.InProcess.EventStore.Configuration;
using MyCompany.MyProject.Domain.Configuration;
using NUnit.Framework;
using TestClass = NUnit.Framework.TestFixtureAttribute;
using TestMethod = NUnit.Framework.TestAttribute;
using TestInitialize = NUnit.Framework.SetUpAttribute;
using TestCleanup = NUnit.Framework.TearDownAttribute;
using TestContext = System.Object;

namespace MyCompany.MyProject.Domain.Tests.Integration
{
	public abstract class WiredUpTests
	{
		[TestInitialize]
		public void TestInitialize()
		{
			NinjectDependencyResolver.ModulesToLoad.Clear();

			// Core Module
			NinjectDependencyResolver.ModulesToLoad.Add(new CqrsModule<ISingleSignOnToken, SingleSignOnTokenValueHelper>());
			// Event Store Module
			NinjectDependencyResolver.ModulesToLoad.Add(new InProcessEventStoreModule<ISingleSignOnToken>());
			// Command Bus Module
			NinjectDependencyResolver.ModulesToLoad.Add(new InProcessCommandBusModule<ISingleSignOnToken>());
			// Event Bus Module
			NinjectDependencyResolver.ModulesToLoad.Add(new InProcessEventBusModule<ISingleSignOnToken>());
			// Domain Core Module
			NinjectDependencyResolver.ModulesToLoad.Add(new DomainCoreModule());
			NinjectDependencyResolver.Start();

			var corrolationIdHelper = NinjectDependencyResolver.Current.Resolve<ICorrolationIdHelper>();
			corrolationIdHelper.SetCorrolationId(Guid.NewGuid());

			var authenticationTokenHelper = NinjectDependencyResolver.Current.Resolve<IAuthenticationTokenHelper<ISingleSignOnToken>>();
			var singleSignOnTokenFactory = NinjectDependencyResolver.Current.Resolve<ISingleSignOnTokenFactory>();
			authenticationTokenHelper.SetAuthenticationToken(singleSignOnTokenFactory.CreateNew());
		}
	}
}