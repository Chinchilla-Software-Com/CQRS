using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using Chat.MicroServices.Authentication.Entities;
using Chat.MicroServices.Authentication.Helpers;
using Chat.MicroServices.Authentication.Repositories;
using Chat.MicroServices.Configuration;
using Chat.MicroServices.Conversations;
using Chat.MicroServices.Conversations.Commands;
using Chat.MicroServices.Conversations.Events;
using Chat.MicroServices.Conversations.Repositories;
using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Events;
using Microsoft.Azure;
using Ninject.Modules;

/// <summary>
/// Starts the Webjob
/// </summary>
public partial class CqrsWebJobProgram
{
	partial void GetCommandOrEventType(ref Type commandOrEventType)
	{
		commandOrEventType = typeof(StartConversation);
	}

	#region Overrides of CqrsNinjectJobHost<Guid,DefaultAuthenticationTokenHelper>

	protected override IEnumerable<INinjectModule> GetSupplementaryModules()
	{
		IList<INinjectModule> results = base.GetSupplementaryModules().ToList();

		results.Add(new QueriesModule());

		return results;
	}

	#endregion

	#region Overrides of CoreHost<Guid>

	protected override void Start()
	{
		base.Start();

		string absolute = System.IO.Path.GetFullPath(DependencyResolver.Current.Resolve<IConfigurationManager>().GetSetting("Cqrs.Azure.WebJobs.DataDirectory"));
		AppDomain.CurrentDomain.SetData("DataDirectory", absolute);

		bool createTestData;
		if (DependencyResolver.Current.Resolve<IConfigurationManager>().TryGetSetting("CreateTestData", out createTestData) && createTestData)
		{
			using (var connection = new SqlConnection(System.Configuration.ConfigurationManager.ConnectionStrings[CloudConfigurationManager.GetSetting("Cqrs.SqlEventStore.ConnectionStringName", false)].ConnectionString))
			{
				using (SqlCommand command = connection.CreateCommand())
				{
					command.CommandText = "TRUNCATE TABLE EventStore";
					connection.Open();
					command.ExecuteNonQuery();
				}
			}

			var authenticationHashHelper = DependencyResolver.Current.Resolve<IAuthenticationHashHelper>();

			var userRepository = DependencyResolver.Current.Resolve<IUserRepository>();
			var credentialRepository = DependencyResolver.Current.Resolve<ICredentialRepository>();
			var conversationSummaryRepository = DependencyResolver.Current.Resolve<IConversationSummaryRepository>();
			var messageRepository = DependencyResolver.Current.Resolve<IMessageRepository>();

			var john = new UserEntity { Rsn = Guid.NewGuid(), FirstName = "John", LastName = "Smith" };
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
				conversationSummaryRepository.DeleteAll();
			}
			catch { /**/ }
			var n = new ConversationStarted(Guid.NewGuid(), "Project Nemesis"){Version = 1};
			PushEvent<Conversation>(n, n.Rsn);
			var s = new ConversationStarted(Guid.NewGuid(), "Sales") { Version = 1 };
			PushEvent<Conversation>(s, s.Rsn);
			var m = new ConversationStarted(Guid.NewGuid(), "Marketing") { Version = 1 };
			PushEvent<Conversation>(m, m.Rsn);

			try
			{
				messageRepository.DeleteAll();
			}
			catch { /**/ }
			var nList = new List<CommentPosted>
			{
				new CommentPosted(n.Rsn, Guid.NewGuid(), n.Name, john.Rsn, john.FirstName, "Welcome to the project.\r\nWe'll be meeting next week on Wednesday.", DateTime.Today.AddHours(10).AddMinutes(37), 1 ){Version = 2},
				new CommentPosted(n.Rsn, Guid.NewGuid(), n.Name, jane.Rsn, jane.FirstName, "Thanks for including me. When is the project due to be delivered to the customer?", DateTime.Today.AddHours(11).AddMinutes(07), 2 ){Version = 3},
				new CommentPosted(n.Rsn, Guid.NewGuid(), n.Name, sue.Rsn, sue.FirstName, "We've got 3 weeks of development and then a 2 month time-line to deploy/install it.", DateTime.Today.AddHours(11).AddMinutes(10), 3 ){Version = 4}
			};

			foreach (CommentPosted @event in nList)
				PushEvent<Conversation>(@event, @event.UserRsn);

			var sList = new List<CommentPosted>
			{
				new CommentPosted(s.Rsn, Guid.NewGuid(), s.Name, sue.Rsn, sue.FirstName, "How are the sales figures looking Jane.", DateTime.Today.AddDays(-3).AddHours(16).AddMinutes(37), 1 ){Version = 2},
				new CommentPosted(s.Rsn, Guid.NewGuid(), s.Name, jane.Rsn, jane.FirstName, "I've almost got the report finished. Should be ready tomorrow", DateTime.Today.AddDays(-2).AddHours(8).AddMinutes(37), 2 ){Version = 3},
				new CommentPosted(s.Rsn, Guid.NewGuid(), s.Name, sue.Rsn, sue.FirstName, "Let's book a meeting to go over the numbers for next week.", DateTime.Today.AddDays(-2).AddHours(13).AddMinutes(22), 3 ){Version = 4},
				new CommentPosted(s.Rsn, Guid.NewGuid(), s.Name, john.Rsn, john.FirstName, "Bill can you make a meeting later today?", DateTime.Today.AddHours(11).AddMinutes(17), 4 ){Version = 5},
				new CommentPosted(s.Rsn, Guid.NewGuid(), s.Name, bill.Rsn, bill.FirstName, "Sure.", DateTime.Today.AddHours(11).AddMinutes(18), 5 ){Version = 6}
			};

			foreach (CommentPosted @event in sList)
				PushEvent<Conversation>(@event, @event.UserRsn);

			var mList = new List<CommentPosted>
			{
				new CommentPosted(m.Rsn, Guid.NewGuid(), m.Name, sue.Rsn, sue.FirstName, "Do you like the new logo.", DateTime.Today.AddDays(-8).AddHours(14).AddMinutes(22), 1 ){Version = 2},
				new CommentPosted(m.Rsn, Guid.NewGuid(), m.Name, jane.Rsn, jane.FirstName, "It needs more blue in it", DateTime.Today.AddDays(-8).AddHours(14).AddMinutes(37), 2 ){Version = 3},
				new CommentPosted(m.Rsn, Guid.NewGuid(), m.Name, sue.Rsn, sue.FirstName, "It should be bigger too.", DateTime.Today.AddDays(-8).AddHours(14).AddMinutes(45), 3 ){Version = 4},
				new CommentPosted(m.Rsn, Guid.NewGuid(), m.Name, john.Rsn, john.FirstName, "I thought it was about right size wise.", DateTime.Today.AddDays(-8).AddHours(15).AddMinutes(17), 4 ){Version = 5},
				new CommentPosted(m.Rsn, Guid.NewGuid(), m.Name, bill.Rsn, bill.FirstName, "I've added more blue for you Jane and made it bigger as well Sue.", DateTime.Today.AddDays(-3).AddHours(11).AddMinutes(18), 5 ){Version = 6},
				new CommentPosted(m.Rsn, Guid.NewGuid(), m.Name, sue.Rsn, sue.FirstName, "Thanks Bill.", DateTime.Today.AddDays(-3).AddHours(11).AddMinutes(19), 6 ){Version = 7},
				new CommentPosted(m.Rsn, Guid.NewGuid(), m.Name, jane.Rsn, jane.FirstName, "Maybe a lighter shade of blue?", DateTime.Today.AddDays(-3).AddHours(11).AddMinutes(22), 7 ){Version = 8}
			};

			foreach (CommentPosted @event in mList)
				PushEvent<Conversation>(@event, @event.UserRsn);
		}
	}

	#endregion

	private void PushEvent<TAggregate>(IEvent<Guid> @event, Guid userRsn)
	{
		DependencyResolver.Current.Resolve<IAuthenticationTokenHelper<Guid>>().SetAuthenticationToken(userRsn);
		var eventStore = DependencyResolver.Current.Resolve<IEventStore<Guid>>();
		var eventPublisher = DependencyResolver.Current.Resolve<IEventPublisher<Guid>>();
		eventStore.Save<TAggregate>(@event);
		eventPublisher.Publish(@event);
	}
}