#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.DependencyInjection.Modules;
using Microsoft.Extensions.DependencyInjection;

namespace Cqrs.DependencyInjection
{
    /// <summary>
    /// A start-up class.
    /// </summary>
    /// <typeparam name="THostModule">The base <see cref="Module"/> that is loaded first.</typeparam>
    public class SimplifiedStartUp<THostModule>
		where THostModule : Module, new()
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="SimplifiedStartUp{THostModule}"/>
		/// </summary>
		public SimplifiedStartUp(IConfigurationManager configurationManager)
		{
			ConfigurationManager = configurationManager;
		}

		/// <summary>
		/// The <see cref="IConfigurationManager"/> that will be used to resolve configuration settings in <see cref="SetupModulesToLoad"/>.
		/// It is not used elsewhere.
		/// </summary>
		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Prepares the modules needed for simplified startup
		/// </summary>
		public virtual void SetupModulesToLoad(IServiceCollection services)
		{
			DependencyResolver.ModulesToLoad.Insert(0, new THostModule());

			string authenticationType;
			if (!ConfigurationManager.TryGetSetting("Cqrs.AuthenticationTokenType", out authenticationType) || string.IsNullOrWhiteSpace(authenticationType))
				authenticationType = "Guid";

			if (authenticationType.ToLowerInvariant() == "int" || authenticationType.ToLowerInvariant() == "integer")
				DependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<int, DefaultAuthenticationTokenHelper>(true, false));
			else if (authenticationType.ToLowerInvariant() == "guid")
				DependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<Guid, DefaultAuthenticationTokenHelper>(true, false));
			else if (authenticationType.ToLowerInvariant() == "string" || authenticationType.ToLowerInvariant() == "text")
				DependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<string, DefaultAuthenticationTokenHelper>(true, false));

			else if (authenticationType == "SingleSignOnToken")
				DependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<SingleSignOnToken, DefaultAuthenticationTokenHelper>(true, false));
			else if (authenticationType == "SingleSignOnTokenWithUserRsn")
				DependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<SingleSignOnTokenWithUserRsn, DefaultAuthenticationTokenHelper>(true, false));
			else if (authenticationType == "SingleSignOnTokenWithCompanyRsn")
				DependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<SingleSignOnTokenWithCompanyRsn, DefaultAuthenticationTokenHelper>(true, false));
			else if (authenticationType == "SingleSignOnTokenWithUserRsnAndCompanyRsn")
				DependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<SingleSignOnTokenWithUserRsnAndCompanyRsn, DefaultAuthenticationTokenHelper>(true, false));

			else if (authenticationType == "ISingleSignOnToken")
				DependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<ISingleSignOnToken, AuthenticationTokenHelper>(true, false));
			else if (authenticationType == "ISingleSignOnTokenWithUserRsn")
				DependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<ISingleSignOnTokenWithUserRsn, AuthenticationTokenHelper>(true, false));
			else if (authenticationType == "ISingleSignOnTokenWithCompanyRsn")
				DependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<ISingleSignOnTokenWithCompanyRsn, AuthenticationTokenHelper>(true, false));
			else if (authenticationType == "ISingleSignOnTokenWithUserRsnAndCompanyRsn")
				DependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<ISingleSignOnTokenWithUserRsnAndCompanyRsn, AuthenticationTokenHelper>(true, false));

			AddSupplementryModules(services);
		}

		/// <summary>
		/// When overridden allows for the addition of any additional <see cref="Module">modules</see> required.
		/// </summary>
		protected virtual void AddSupplementryModules(IServiceCollection services)
		{
		}

		/// <summary>
		/// Calls <see cref="DependencyResolver.Start"/>
		/// </summary>
		protected virtual void StartResolver(IServiceCollection kernel)
		{
			DependencyResolver.Start(kernel, true);
		}
	}
}