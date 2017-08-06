using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.Scheduler.Events
{
	/// <summary>
	/// A <see cref="TimeZoneInfo"/> event
	/// </summary>
	[DataContract]
	public class TimeZoneEvent : IEvent<Guid>, ITelemeteredMessage
	{
		private string _timezoneId;

		private short _localHour;

		#region Implementation of IMessageWithAuthenticationToken<Guid>

		/// <summary>
		/// The authentication token of the entity that triggered the event to be raised.
		/// </summary>
		[DataMember]
		Guid IMessageWithAuthenticationToken<Guid>.AuthenticationToken { get; set; }

		#endregion

		#region Implementation of IEvent<Guid>

		/// <summary>
		/// The identifier of the event itself.
		/// </summary>
		[DataMember]
		Guid IEvent<Guid>.Id { get; set; }

		/// <summary>
		/// The new version number.
		/// </summary>
		[DataMember]
		int IEvent<Guid>.Version { get; set; }

		/// <summary>
		/// The date and time the event was raised or published.
		/// </summary>
		[DataMember]
		DateTimeOffset IEvent<Guid>.TimeStamp { get; set; }


		#endregion

		#region Implementation of IMessage

		/// <summary>
		/// An identifier used to group together several <see cref="IMessage"/>. Any <see cref="IMessage"/> with the same <see cref="IMessage.CorrelationId"/> were triggered by the same initiating request.
		/// </summary>
		[DataMember]
		Guid IMessage.CorrelationId { get; set; }

		/// <summary>
		/// The originating framework this message was sent from.
		/// </summary>
		[DataMember]
		string IMessage.OriginatingFramework { get; set; }

		/// <summary>
		/// The frameworks this <see cref="IMessage"/> has been delivered to/sent via already.
		/// </summary>
		[DataMember]
		IEnumerable<string> IMessage.Frameworks { get; set; }

		#endregion

		/// <summary>
		/// The <see cref="TimeZoneInfo.Id"/> of the <see cref="TimeZoneInfo"/> this event is for.
		/// </summary>
		[DataMember]
		public string TimezoneId
		{
			get { return _timezoneId; }
			set
			{
				_timezoneId = value;
				SetTelemetryName();
			}
		}

		/// <summary>
		/// The <see cref="DateTime.Hour"/> at the <see cref="TimeZoneInfo"/> this event is for.
		/// </summary>
		[DataMember]
		public short LocalHour
		{
			get { return _localHour; }
			set
			{
				_localHour = value;
				SetTelemetryName();
			}
		}

		/// <summary>
		/// The original <see cref="DateTimeOffset"/> used to find the <see cref="TimeZoneInfo"/> this event is for.
		/// </summary>
		[DataMember]
		public DateTimeOffset ProcessDate { get; set; }

		/// <summary>
		/// Instantiate a new instance of <see cref="TimeZoneEvent"/>
		/// </summary>
		public TimeZoneEvent()
		{
			var @event = (IEvent<Guid>)this;
			@event.Id = Guid.NewGuid();
			@event.TimeStamp = DateTimeOffset.UtcNow;
		}

		#region Implementation of ITelemeteredMessage

		/// <summary>
		/// Gets or sets the Name of this message.
		/// </summary>
		string ITelemeteredMessage.TelemetryName { get; set; }

		#endregion

		/// <summary>
		/// Set the <see cref="ITelemeteredMessage.TelemetryName"/>.
		/// </summary>
		protected virtual void SetTelemetryName()
		{
			((ITelemeteredMessage)this).TelemetryName = string.Format("{0}/{1}/{2}", GetType().Name, TimezoneId, LocalHour);
		}
	}
}