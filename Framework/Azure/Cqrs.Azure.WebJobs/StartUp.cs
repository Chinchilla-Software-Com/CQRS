#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.Logging;
using Cqrs.Configuration;

namespace Cqrs.Azure.WebJobs
{
	public class StartUp
	{
		protected Action DependencyResolverConfigurationFunction { get; set; }

		public StartUp(Action dependencyResolverConfigurationFunction)
		{
			DependencyResolverConfigurationFunction = dependencyResolverConfigurationFunction;
		}

		public virtual void Initialise()
		{
			DependencyResolverConfigurationFunction();

			var correlationIdHelper = DependencyResolver.Current.Resolve<ICorrelationIdHelper>();
			correlationIdHelper.SetCorrelationId(Guid.NewGuid());

			var logger = DependencyResolver.Current.Resolve<ILogger>();

			logger.LogInfo("Application Initialised.");
		}
	}
}