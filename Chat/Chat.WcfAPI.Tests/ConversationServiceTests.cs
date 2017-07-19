using Chat.MicroServices.Conversations.Entities;
using Chat.MicroServices.Conversations.Services;
using Chat.MicroServices.Conversations.Services.ServiceChannelFactories;
using Cqrs.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Chat.WcfAPI.Tests
{
	/// <summary>
	/// A series of tests on <see cref="IConversationService"/> using WCF for communication.
	/// </summary>
	[TestClass]
	public class ConversationServiceTests
	{
		/// <summary>
		/// Tests that a call to <see cref="IConversationService.Get"/> returns a non-empty result.
		/// </summary>
		[TestMethod]
		public void Get_NoParameters_MoreThanNoItems()
		{
			// Arrange
			HttpConversationServiceChannelFactory factory = new HttpConversationServiceChannelFactory();
			IConversationService service = factory.CreateChannel();

			// Act
			IServiceResponseWithResultData<IEnumerable<ConversationSummaryEntity>> actual = service.Get(new ServiceRequest<Guid>());

			// Assert
			Assert.IsTrue(actual.ResultData.Any());
		}

		/// <summary>
		/// Calls <see cref="IConversationService.Get"/> to get a collection of <see cref="ConversationSummaryEntity"/> then
		/// tests that a call to <see cref="IConversationService.GetMessages"/> returns a non-empty result for each returned <see cref="ConversationSummaryEntity"/>.
		/// </summary>
		[TestMethod]
		public void GetMessages_EachConversationsFromGet_MoreThanNoItems()
		{
			// Arrange
			HttpConversationServiceChannelFactory factory = new HttpConversationServiceChannelFactory();
			IConversationService service = factory.CreateChannel();
			IServiceResponseWithResultData<IEnumerable<ConversationSummaryEntity>> conversations = service.Get(new ServiceRequest<Guid>());

			foreach (ConversationSummaryEntity conversationSummary in conversations.ResultData)
			{
				// Act
				var actual = service.GetMessages(new ServiceRequestWithData<Guid, ConversationService.ConversationParameters> { Data = new ConversationService.ConversationParameters { ConversationRsn = conversationSummary.Rsn } });

				// Assert
				Assert.IsTrue(actual.ResultData.Any());
			}
		}

		/// <summary>
		/// Tests that a call to <see cref="IConversationService.StartConversation"/> returns <see cref="ServiceResponseStateType.Succeeded"/>.
		/// </summary>
		[TestMethod]
		public void StartConversation_TestConversationName_Succeeded()
		{
			// Arrange
			HttpConversationServiceChannelFactory factory = new HttpConversationServiceChannelFactory();
			IConversationService service = factory.CreateChannel();
			var random = new Random();

			// Act
			IServiceResponse actual = service.StartConversation(new ServiceRequestWithData<Guid, ConversationService.StartConversationParameters> { Data = new ConversationService.StartConversationParameters { Name = string.Format("WCF Test Conversation {0:N0}", random.Next()) } });

			// Assert
			Assert.AreEqual(ServiceResponseStateType.Succeeded, actual.State);
		}

		/// <summary>
		/// Tests that a call to <see cref="IConversationService.StartConversation"/> returns <see cref="ServiceResponseStateType.FailedValidation"/>.
		/// </summary>
		[TestMethod]
		public void StartConversation_NoConversationName_FailedValidation()
		{
			// Arrange
			HttpConversationServiceChannelFactory factory = new HttpConversationServiceChannelFactory();
			IConversationService service = factory.CreateChannel();

			// Act
			IServiceResponse actual = service.StartConversation(new ServiceRequestWithData<Guid, ConversationService.StartConversationParameters> { Data = new ConversationService.StartConversationParameters { Name = null } });

			// Assert
			Assert.AreEqual(ServiceResponseStateType.FailedValidation, actual.State);
		}

		/// <summary>
		/// Tests that a call to <see cref="IConversationService.UpdateConversation"/> returns <see cref="ServiceResponseStateType.Succeeded"/>.
		/// </summary>
		[TestMethod]
		public void UpdateConversation_TestConversationName_Succeeded()
		{
			// Arrange
			HttpConversationServiceChannelFactory factory = new HttpConversationServiceChannelFactory();
			IConversationService service = factory.CreateChannel();
			var random = new Random();

			// Act
			IServiceResponse actual = service.UpdateConversation(new ServiceRequestWithData<Guid, ConversationService.UpdateConversationParameters> { Data = new ConversationService.UpdateConversationParameters { Name = string.Format("WCF Test Conversation {0:N0}", random.Next()) } });

			// Assert
			Assert.AreEqual(ServiceResponseStateType.Succeeded, actual.State);
		}

		/// <summary>
		/// Tests that a call to <see cref="IConversationService.UpdateConversation"/> returns <see cref="ServiceResponseStateType.FailedValidation"/>.
		/// </summary>
		[TestMethod]
		public void UpdateConversation_NoConversationName_FailedValidation()
		{
			// Arrange
			HttpConversationServiceChannelFactory factory = new HttpConversationServiceChannelFactory();
			IConversationService service = factory.CreateChannel();

			// Act
			IServiceResponse actual = service.UpdateConversation(new ServiceRequestWithData<Guid, ConversationService.UpdateConversationParameters> { Data = new ConversationService.UpdateConversationParameters { Name = null } });

			// Assert
			Assert.AreEqual(ServiceResponseStateType.FailedValidation, actual.State);
		}

		/// <summary>
		/// Tests that a call to <see cref="IConversationService.DeleteConversation"/> returns <see cref="ServiceResponseStateType.Succeeded"/>.
		/// </summary>
		[TestMethod]
		public void DeleteConversation_TestConversationName_Succeeded()
		{
			// Arrange
			HttpConversationServiceChannelFactory factory = new HttpConversationServiceChannelFactory();
			IConversationService service = factory.CreateChannel();

			// Act
			IServiceResponse actual = service.DeleteConversation(new ServiceRequestWithData<Guid, ConversationService.ConversationParameters>{Data = new ConversationService.ConversationParameters()});

			// Assert
			Assert.AreEqual(ServiceResponseStateType.Succeeded, actual.State);
		}
	}
}