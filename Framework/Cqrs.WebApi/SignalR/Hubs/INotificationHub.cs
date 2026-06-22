#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Events;

namespace Cqrs.WebApi.SignalR.Hubs
{
	/// <summary>
	/// Sends <see cref="IEvent{TAuthenticationToken}">events</see> to different groups of users.
	/// </summary>
	public interface INotificationHub
	{
		/// <summary>
		/// Send out an event to specific user RSNs
		/// </summary>
		void SendUsersEvent<TAuthenticationToken>(IEvent<TAuthenticationToken> eventData, params Guid[] userRsnCollection);

		/// <summary>
		/// Send out an event to specific user token
		/// </summary>
		void SendUserEvent<TAuthenticationToken>(IEvent<TAuthenticationToken> eventData, string userToken);

		/// <summary>
		/// Send out an event to all users
		/// </summary>
		void SendAllUsersEvent<TAuthenticationToken>(IEvent<TAuthenticationToken> eventData);

		/// <summary>
		/// Send out an event to all users except the specific user token
		/// </summary>
		void SendExceptThisUserEvent<TAuthenticationToken>(IEvent<TAuthenticationToken> eventData, string userToken);
	}
}