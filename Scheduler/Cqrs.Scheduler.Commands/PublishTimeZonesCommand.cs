#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Cqrs.Commands;
using Cqrs.Messages;
using Newtonsoft.Json;

namespace Cqrs.Scheduler.Commands
{
	[DataContract]
	public class PublishTimeZonesCommand : ICommand<Guid>, ITelemeteredMessage
	{
		public PublishTimeZonesCommand()
		{
			((ICommand<Guid>)this).Id = Guid.NewGuid();
			SetTelemetryName();
		}

		#region Implementation of IMessageWithAuthenticationToken<Guid>

		/// <summary>
		/// The authentication token of the entity that triggered the event to be raised.
		/// </summary>
		[DataMember]
		[JsonProperty]
		Guid IMessageWithAuthenticationToken<Guid>.AuthenticationToken { get; set; }

		#endregion

		#region Implementation of ICommand<Guid>

		/// <summary>
		/// The identifier of the command itself.
		/// </summary>
		[DataMember]
		[JsonProperty]
		Guid ICommand<Guid>.Id { get; set; }

		/// <summary>
		/// The new version number.
		/// </summary>
		[DataMember]
		[JsonProperty]
		int ICommand<Guid>.ExpectedVersion { get; set; }


		#endregion

		#region Implementation of IMessage

		/// <summary>
		/// An identifier used to group together several <see cref="IMessage"/>. Any <see cref="IMessage"/> with the same <see cref="IMessage.CorrelationId"/> were triggered by the same initiating request.
		/// </summary>
		[DataMember]
		[JsonProperty]
		Guid IMessage.CorrelationId { get; set; }

		/// <summary>
		/// The originating framework this message was sent from.
		/// </summary>
		[DataMember]
		[JsonProperty]
		string IMessage.OriginatingFramework { get; set; }

		/// <summary>
		/// The frameworks this <see cref="IMessage"/> has been delivered to/sent via already.
		/// </summary>
		[DataMember]
		[JsonProperty]
		IEnumerable<string> IMessage.Frameworks { get; set; }

		#endregion

		#region Implementation of ITelemeteredMessage

		/// <summary>
		/// Gets or sets the Name of this message.
		/// </summary>
		[JsonProperty]
		string ITelemeteredMessage.TelemetryName { get; set; }

		#endregion

		/// <summary>
		/// Set the <see cref="ITelemeteredMessage.TelemetryName"/>.
		/// </summary>
		protected virtual void SetTelemetryName()
		{
			((ITelemeteredMessage)this).TelemetryName = string.Format("{0}", GetType().Name);
		}
	}
}