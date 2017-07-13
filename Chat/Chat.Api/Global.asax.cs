namespace Chat.Api
{
	using System;
	using System.Web;
	using System.Web.Http;
	using Code;
	using Cqrs.Authentication;
	using Cqrs.Configuration;
	using Cqrs.Ninject.Configuration;
	using Cqrs.WebApi;
	using MicroServices.Authentication.Entities;
	using MicroServices.Authentication.Helpers;
	using MicroServices.Authentication.Repositories;
	using Newtonsoft.Json;

	public class WebApiApplication : CqrsHttpApplication<string, EventToHubProxy>
	{
		protected override void ConfigureDefaultDependencyResolver()
		{
			DependencyResolver = NinjectDependencyResolver.Current;

			bool createTestData;
			if (DependencyResolver.Resolve<IConfigurationManager>().TryGetSetting("CreateTestData", out createTestData) && createTestData)
			{
				var authenticationHashHelper = DependencyResolver.Resolve<IAuthenticationHashHelper>();
				var credentialRepository = DependencyResolver.Resolve<ICredentialRepository>();
				var userRepository = DependencyResolver.Resolve<IUserRepository>();

				var john = new UserEntity {Rsn = Guid.NewGuid(), FirstName = "John", LastName = "Smith"};
				var sue = new UserEntity { Rsn = Guid.NewGuid(), FirstName = "Sue", LastName = "Wallace" };
				var jane = new UserEntity { Rsn = Guid.NewGuid(), FirstName = "Jane", LastName = "Doe" };
				var bill = new UserEntity { Rsn = Guid.NewGuid(), FirstName = "Bill", LastName = "Hunter" };

				try
				{
					userRepository.DeleteAll();
				}
				catch { /**/ }
				userRepository.Create(john);
				userRepository.Create(sue);
				userRepository.Create(jane);
				userRepository.Create(bill);

				try
				{
					credentialRepository.DeleteAll();
				}
				catch { /**/ }
				credentialRepository.Create(new CredentialEntity { Hash = authenticationHashHelper.GenerateCredentialHash("john@domain.com", "john123"), UserRsn = john.Rsn, Rsn = Guid.NewGuid() });
				credentialRepository.Create(new CredentialEntity { Hash = authenticationHashHelper.GenerateCredentialHash("sue@domain.com", "sue123"), UserRsn = sue.Rsn, Rsn = Guid.NewGuid() });
				credentialRepository.Create(new CredentialEntity { Hash = authenticationHashHelper.GenerateCredentialHash("jane@domain.com", "jane123"), UserRsn = jane.Rsn, Rsn = Guid.NewGuid() });
				credentialRepository.Create(new CredentialEntity { Hash = authenticationHashHelper.GenerateCredentialHash("bill@domain.com", "bill123"), UserRsn = bill.Rsn, Rsn = Guid.NewGuid() });
			}
		}

		protected override void ConfigureMvc()
		{
			GlobalConfiguration.Configure(WebApiConfig.Register);
			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.TypeNameHandling = TypeNameHandling.None;
		}

		protected override void Application_BeginRequest(object sender, EventArgs e)
		{
			base.Application_BeginRequest(sender, e);
			HttpCookie authCookie = Request.Cookies["X-Token"];
			Guid token;
			if (authCookie != null && Guid.TryParse(authCookie.Value, out token))
			{
				// Pass the authentication token to the helper to allow automated authentication handling
				DependencyResolver.Resolve<IAuthenticationTokenHelper<Guid>>().SetAuthenticationToken(token);
			}
		}
	}
}