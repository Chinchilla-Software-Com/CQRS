#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Chinchilla.Logging;
using Cqrs.Configuration;

namespace Cqrs.Hosts
{
	/// <summary>
	/// A startUp action for the <see cref="CoreHost{TAuthenticationToken}"/>
	/// </summary>
	public class StartUp
	{
		/// <summary>
		/// The <see cref="Action"/> that will configure the <see cref="DependencyResolver"/>.
		/// </summary>
		protected Action DependencyResolverConfigurationFunction { get; set; }

		/// <summary>
		/// Instantiate a new instance of a <see cref="StartUp"/>
		/// </summary>
		public StartUp(Action dependencyResolverConfigurationFunction)
		{
			DependencyResolverConfigurationFunction = dependencyResolverConfigurationFunction;
		}

		/// <summary>
		/// Initialise by calling the <see cref="DependencyResolverConfigurationFunction"/>.
		/// </summary>
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