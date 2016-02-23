using Cqrs.Authentication;
using Cqrs.Ninject.Configuration;
using Ninject.Modules;

namespace MyCompany.MyProject.Domain.Host.Configuration
{
	public class DomainConfiguration<THostModule>
			where THostModule : NinjectModule, new()
	{
		public void Start()
		{
			NinjectDependencyResolver.ModulesToLoad.Add(new THostModule());
			NinjectDependencyResolver.ModulesToLoad.Add(new CqrsModule<ISingleSignOnToken, SingleSignOnTokenValueHelper>());

			NinjectDependencyResolver.Start();
		}
	}
}