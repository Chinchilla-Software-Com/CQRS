namespace Chat.Api
{
	using System;
	using System.Collections.Generic;
	using System.Linq;
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
	using MicroServices.Conversations.Entities;
	using MicroServices.Conversations.Repositories;
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
				var userRepository = DependencyResolver.Resolve<IUserRepository>();
				var credentialRepository = DependencyResolver.Resolve<ICredentialRepository>();
				var conversationSummaryRepository = DependencyResolver.Resolve<IConversationSummaryRepository>();
				var messageRepository = DependencyResolver.Resolve<IMessageRepository>();

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

				try
				{
					messageRepository.DeleteAll();
				}
				catch { /**/ }

				var n = new ConversationSummaryEntity { Rsn = Guid.NewGuid(), Name = "Project Nemesis", MessageCount = 3 };
				var s = new ConversationSummaryEntity { Rsn = Guid.NewGuid(), Name = "Sales", MessageCount = 5 };
				var m = new ConversationSummaryEntity { Rsn = Guid.NewGuid(), Name = "Marketing", MessageCount = 7 };

				var nList = new List<MessageEntity>
				{
					new MessageEntity{ Content = "Welcome to the project.\r\nWe'll be meeting next week on Wednesday.", UserName = john.FirstName, UserRsn = john.Rsn, DatePosted = DateTime.Today.AddHours(10).AddMinutes(37), ConversationName = n.Name, ConversationRsn = n.Rsn, Rsn = Guid.NewGuid() },
					new MessageEntity{ Content = "Thanks for including me. When is the project due to be delivered to the customer?", UserName = jane.FirstName, UserRsn = jane.Rsn, DatePosted = DateTime.Today.AddHours(11).AddMinutes(07), ConversationName = n.Name, ConversationRsn = n.Rsn, Rsn = Guid.NewGuid() },
					new MessageEntity{ Content = "We've got 3 weeks of development and then a 2 month time-line to deploy/install it.", UserName = sue.FirstName, UserRsn = sue.Rsn, DatePosted = DateTime.Today.AddHours(11).AddMinutes(10), ConversationName = n.Name, ConversationRsn = n.Rsn, Rsn = Guid.NewGuid() }
				};

				foreach (MessageEntity message in nList)
					messageRepository.Create(message);

				var sList = new List<MessageEntity>
				{
					new MessageEntity{ Content = "How are the sales figures looking Jane.", UserName = sue.FirstName, UserRsn = sue.Rsn, DatePosted = DateTime.Today.AddDays(-3).AddHours(16).AddMinutes(37), ConversationName = s.Name, ConversationRsn = s.Rsn, Rsn = Guid.NewGuid() },
					new MessageEntity{ Content = "I've almost got the report finished. Should be ready tomorrow", UserName = jane.FirstName, UserRsn = jane.Rsn, DatePosted = DateTime.Today.AddDays(-2).AddHours(8).AddMinutes(37), ConversationName = s.Name, ConversationRsn = s.Rsn, Rsn = Guid.NewGuid() },
					new MessageEntity{ Content = "Let's book a meeting to go over the numbers for next week.", UserName = sue.FirstName, UserRsn = sue.Rsn, DatePosted = DateTime.Today.AddDays(-2).AddHours(13).AddMinutes(22), ConversationName = s.Name, ConversationRsn = s.Rsn, Rsn = Guid.NewGuid() },
					new MessageEntity{ Content = "Bill can you make a meeting later today?", UserName = john.FirstName, UserRsn = john.Rsn, DatePosted = DateTime.Today.AddHours(11).AddMinutes(17), ConversationName = s.Name, ConversationRsn = s.Rsn, Rsn = Guid.NewGuid() },
					new MessageEntity{ Content = "Sure.", UserName = bill.FirstName, UserRsn = bill.Rsn, DatePosted = DateTime.Today.AddHours(11).AddMinutes(18), ConversationName = s.Name, ConversationRsn = s.Rsn, Rsn = Guid.NewGuid() }
				};

				foreach (MessageEntity message in sList)
					messageRepository.Create(message);

				var mList = new List<MessageEntity>
				{
					new MessageEntity{ Content = "Do you like the new logo.", UserName = sue.FirstName, UserRsn = sue.Rsn, DatePosted = DateTime.Today.AddDays(-8).AddHours(14).AddMinutes(22), ConversationName = m.Name, ConversationRsn = m.Rsn, Rsn = Guid.NewGuid() },
					new MessageEntity{ Content = "It needs more blue in it", UserName = jane.FirstName, UserRsn = jane.Rsn, DatePosted = DateTime.Today.AddDays(-8).AddHours(14).AddMinutes(37), ConversationName = m.Name, ConversationRsn = m.Rsn, Rsn = Guid.NewGuid() },
					new MessageEntity{ Content = "It should be bigger too.", UserName = sue.FirstName, UserRsn = sue.Rsn, DatePosted = DateTime.Today.AddDays(-8).AddHours(14).AddMinutes(45), ConversationName = m.Name, ConversationRsn = m.Rsn, Rsn = Guid.NewGuid() },
					new MessageEntity{ Content = "I thought it was about right size wise.", UserName = john.FirstName, UserRsn = john.Rsn, DatePosted = DateTime.Today.AddDays(-8).AddHours(15).AddMinutes(17), ConversationName = m.Name, ConversationRsn = m.Rsn, Rsn = Guid.NewGuid() },
					new MessageEntity{ Content = "I've added more blue for you Jane and made it bigger as well Sue.", UserName = bill.FirstName, UserRsn = bill.Rsn, DatePosted = DateTime.Today.AddDays(-3).AddHours(11).AddMinutes(18), ConversationName = m.Name, ConversationRsn = m.Rsn, Rsn = Guid.NewGuid() },
					new MessageEntity{ Content = "Thanks Bill.", UserName = sue.FirstName, UserRsn = sue.Rsn, DatePosted = DateTime.Today.AddDays(-3).AddHours(11).AddMinutes(19), ConversationName = m.Name, ConversationRsn = m.Rsn, Rsn = Guid.NewGuid() },
					new MessageEntity{ Content = "Maybe a lighter shade of blue?", UserName = jane.FirstName, UserRsn = jane.Rsn, DatePosted = DateTime.Today.AddDays(-3).AddHours(11).AddMinutes(22), ConversationName = m.Name, ConversationRsn = m.Rsn, Rsn = Guid.NewGuid() }
				};

				foreach (MessageEntity message in mList)
					messageRepository.Create(message);

				try
				{
					conversationSummaryRepository.DeleteAll();
				}
				catch { /**/ }

				n.MessageCount = nList.Count;
				n.LastUpdatedDate = nList.OrderByDescending(x => x.DatePosted).First().DatePosted;
				s.MessageCount = sList.Count;
				s.LastUpdatedDate = sList.OrderByDescending(x => x.DatePosted).First().DatePosted;
				m.MessageCount = mList.Count;
				m.LastUpdatedDate = mList.OrderByDescending(x => x.DatePosted).First().DatePosted;

				conversationSummaryRepository.Create(n);
				conversationSummaryRepository.Create(s);
				conversationSummaryRepository.Create(m);
			}
		}

		protected override void ConfigureMvc()
		{
			GlobalConfiguration.Configure(WebApiConfig.Register);
			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.TypeNameHandling = TypeNameHandling.None;
			GlobalConfiguration.Configuration.Formatters.JsonFormatter.SerializerSettings.PreserveReferencesHandling = PreserveReferencesHandling.None;
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