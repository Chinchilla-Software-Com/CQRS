using System;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Ninject.Configuration;
using Ninject;
using Ninject.Web.Common;

[assembly: WebActivatorEx.PreApplicationStartMethod(typeof(Cqrs.Ninject.WebApi.Configuration.SimplifiedNinjectWebApi), "Start", Order = 50)]
[assembly: WebActivatorEx.ApplicationShutdownMethodAttribute(typeof(Cqrs.Ninject.WebApi.Configuration.SimplifiedNinjectWebApi), "Stop", Order = 50)]

namespace Cqrs.Ninject.WebApi.Configuration
{
	using Microsoft.Web.Infrastructure.DynamicModuleHelper;

	public static class SimplifiedNinjectWebApi
	{
		private static readonly Bootstrapper Bootstrapper = new Bootstrapper();

		/// <summary>
		/// Starts the application
		/// </summary>
		public static void Start()
		{
			DynamicModuleUtility.RegisterModule(typeof(OnePerRequestHttpModule));
			DynamicModuleUtility.RegisterModule(typeof(NinjectHttpModule));
			Bootstrapper.Initialize(CreateKernel);
		}

		/// <summary>
		/// Stops the application.
		/// </summary>
		public static void Stop()
		{
			Bootstrapper.ShutDown();
		}

		/// <summary>
		/// Creates the kernel that will manage your application.
		/// </summary>
		/// <returns>The created kernel.</returns>
		private static IKernel CreateKernel()
		{
			NinjectDependencyResolver.ModulesToLoad.Insert(0, new WebApiModule());

			string authenticationType;
			if (!new ConfigurationManager().TryGetSetting("AuthenticationTokenType", out authenticationType))
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

			NinjectDependencyResolver.ModulesToLoad.Insert(2, new SimplifiedSqlModule<SingleSignOnToken>());

			// NinjectDependencyResolver.Start();
			var kernel = new StandardKernel();
			// This is only done so the follow Wcf safe method can be called. Otherwise use the commented out line above.
			NinjectDependencyResolver.Start(kernel, true);

			return kernel;
		}
	}
}
