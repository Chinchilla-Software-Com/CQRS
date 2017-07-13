#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Authentication;
using Cqrs.Events;

namespace Cqrs.WebApi.SignalR.Hubs
{
	public interface ISingleSignOnTokenNotificationHub
	{
		/// <summary>
		/// Send out an event to specific user RSNs
		/// </summary>
		void SendUsersEvent<TSingleSignOnToken>(IEvent<TSingleSignOnToken> eventData, params Guid[] userRsnCollection)
			where TSingleSignOnToken : ISingleSignOnToken;

		/// <summary>
		/// Send out an event to specific user token
		/// </summary>
		void SendUserEvent<TSingleSignOnToken>(IEvent<TSingleSignOnToken> eventData, string userToken)
			where TSingleSignOnToken : ISingleSignOnToken;

		/// <summary>
		/// Send out an event to all users
		/// </summary>
		void SendAllUsersEvent<TSingleSignOnToken>(IEvent<TSingleSignOnToken> eventData)
			where TSingleSignOnToken : ISingleSignOnToken;

		/// <summary>
		/// Send out an event to all users except the specific user token
		/// </summary>
		void SendExceptThisUserEvent<TSingleSignOnToken>(IEvent<TSingleSignOnToken> eventData, string userToken)
			where TSingleSignOnToken : ISingleSignOnToken;
	}
}