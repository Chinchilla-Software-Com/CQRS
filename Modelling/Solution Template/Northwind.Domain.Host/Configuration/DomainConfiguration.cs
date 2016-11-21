#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Authentication;
using Cqrs.Ninject.Configuration;
using Cqrs.Ninject.InProcess.CommandBus.Configuration;
using Cqrs.Ninject.InProcess.EventBus.Configuration;
using Ninject.Modules;

namespace Northwind.Domain.Host.Configuration
{
	public class DomainConfiguration<THostModule>
			where THostModule : NinjectModule, new()
	{
		public virtual void Start()
		{
			NinjectDependencyResolver.ModulesToLoad.Add(new THostModule());
			NinjectDependencyResolver.ModulesToLoad.Add(new CqrsModule<ISingleSignOnToken, SingleSignOnTokenValueHelper>());
			NinjectDependencyResolver.ModulesToLoad.Add(new SimplifiedSqlModule<ISingleSignOnToken>());
			NinjectDependencyResolver.ModulesToLoad.Add(new InProcessEventBusModule<ISingleSignOnToken>());
			NinjectDependencyResolver.ModulesToLoad.Add(new InProcessCommandBusModule<ISingleSignOnToken>());
            // Uncomment once you generate your model.
            // NinjectDependencyResolver.ModulesToLoad.Add(new DomainNinjectModule());

			StartResolver();
		}

		protected virtual void StartResolver()
		{
			NinjectDependencyResolver.Start();
		}
	}
}