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
using Ninject;
using Ninject.Modules;

namespace Cqrs.Ninject.Configuration
{
	/// <summary>
	/// A start-up class.
	/// </summary>
	/// <typeparam name="THostModule">The base <see cref="INinjectModule"/> that is loaded first.</typeparam>
	public class SimplifiedNinjectStartUp<THostModule>
		where THostModule : INinjectModule, new()
	{
		public SimplifiedNinjectStartUp(IConfigurationManager configurationManager)
		{
			ConfigurationManager = configurationManager;
		}

		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Creates the kernel that will manage your application.
		/// </summary>
		/// <returns>The created kernel.</returns>
		public virtual IKernel CreateKernel()
		{
			NinjectDependencyResolver.ModulesToLoad.Insert(0, new THostModule());

			string authenticationType;
			if (!ConfigurationManager.TryGetSetting("Cqrs.AuthenticationTokenType", out authenticationType))
				authenticationType = "Guid";

			if (authenticationType.ToLowerInvariant() == "int" || authenticationType.ToLowerInvariant() == "integer")
				NinjectDependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<int, DefaultAuthenticationTokenHelper>(true, false));
			else if (authenticationType.ToLowerInvariant() == "guid")
				NinjectDependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<Guid, DefaultAuthenticationTokenHelper>(true, false));
			else if (authenticationType.ToLowerInvariant() == "string" || authenticationType.ToLowerInvariant() == "text")
				NinjectDependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<string, DefaultAuthenticationTokenHelper>(true, false));

			else if (authenticationType == "SingleSignOnToken")
				NinjectDependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<SingleSignOnToken, DefaultAuthenticationTokenHelper>(true, false));
			else if (authenticationType == "SingleSignOnTokenWithUserRsn")
				NinjectDependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<SingleSignOnTokenWithUserRsn, DefaultAuthenticationTokenHelper>(true, false));
			else if (authenticationType == "SingleSignOnTokenWithCompanyRsn")
				NinjectDependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<SingleSignOnTokenWithCompanyRsn, DefaultAuthenticationTokenHelper>(true, false));
			else if (authenticationType == "SingleSignOnTokenWithUserRsnAndCompanyRsn")
				NinjectDependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<SingleSignOnTokenWithUserRsnAndCompanyRsn, DefaultAuthenticationTokenHelper>(true, false));

			else if (authenticationType == "ISingleSignOnToken")
				NinjectDependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<ISingleSignOnToken, AuthenticationTokenHelper>(true, false));
			else if (authenticationType == "ISingleSignOnTokenWithUserRsn")
				NinjectDependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<ISingleSignOnTokenWithUserRsn, AuthenticationTokenHelper>(true, false));
			else if (authenticationType == "ISingleSignOnTokenWithCompanyRsn")
				NinjectDependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<ISingleSignOnTokenWithCompanyRsn, AuthenticationTokenHelper>(true, false));
			else if (authenticationType == "ISingleSignOnTokenWithUserRsnAndCompanyRsn")
				NinjectDependencyResolver.ModulesToLoad.Insert(1, new CqrsModule<ISingleSignOnTokenWithUserRsnAndCompanyRsn, AuthenticationTokenHelper>(true, false));

			AddSupplementryModules();

			StandardKernel kernel = CreateNinjectKernel();
			StartResolver(kernel);

			return kernel;
		}

		/// <summary>
		/// When overridden allows for the addition of any additional <see cref="INinjectModule">modules</see> required.
		/// </summary>
		protected virtual void AddSupplementryModules()
		{
		}

		/// <summary>
		/// Create a new <see cref="StandardKernel"/>
		/// </summary>
		protected virtual StandardKernel CreateNinjectKernel()
		{
			return new StandardKernel();
		}

		/// <summary>
		/// Calls <see cref="NinjectDependencyResolver.Start"/>
		/// </summary>
		protected virtual void StartResolver(IKernel kernel)
		{
			NinjectDependencyResolver.Start(kernel, true);
		}
	}
}