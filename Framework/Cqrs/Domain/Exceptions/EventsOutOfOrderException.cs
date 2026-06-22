#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using Cqrs.Events;

namespace Cqrs.Domain.Exceptions
{
	/// <summary>
	/// The <see cref="IEventStore{TAuthenticationToken}"/> gave <see cref="IEvent{TAuthenticationToken}">events</see> out of order
	/// or an expected <see cref="IEvent{TAuthenticationToken}"/> with a specific version number wasn't provided.
	/// </summary>
	[Serializable]
	public class EventsOutOfOrderException : Exception
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="EventsOutOfOrderException"/> with the provided identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> that had out of order <see cref="IEvent{TAuthenticationToken}" />.
		/// and the <paramref name="currentVersion"/> the <see cref="IAggregateRoot{TAuthenticationToken}"/> was at and the <paramref name="providedEventVersion">the event version that was provided</paramref>.
		/// </summary>
		/// <param name="id">The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> that had <see cref="IEvent{TAuthenticationToken}">events</see>.</param>
		/// <param name="aggregateType">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> that the <see cref="IEvent{TAuthenticationToken}"/> was trying to be saved on.</param>
		/// <param name="currentVersion">The version number the <see cref="IAggregateRoot{TAuthenticationToken}"/> was at when it received an out of order <see cref="IEvent{TAuthenticationToken}"/>.</param>
		/// <param name="providedEventVersion">The version number the <see cref="IEvent{TAuthenticationToken}"/> that was provided, that was out of order.</param>
		public EventsOutOfOrderException(Guid id, Type aggregateType, int currentVersion, int providedEventVersion)
			: base(string.Format("Eventstore gave event for aggregate '{0}' of type '{1}' out of order at version {2} by providing {3}", id, aggregateType.FullName, currentVersion, providedEventVersion))
		{
			Id = id;
			AggregateType = aggregateType;
			CurrentVersion = currentVersion;
			ProvidedEventVersion = providedEventVersion;
		}

		/// <summary>
		/// The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> that had out of order <see cref="IEvent{TAuthenticationToken}" />.
		/// </summary>
		[DataMember]
		public Guid Id { get; set; }

		/// <summary>
		/// The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> that had out of order <see cref="IEvent{TAuthenticationToken}" />.
		/// </summary>
		[DataMember]
		public Type AggregateType { get; set; }

		/// <summary>
		/// The version number the <see cref="IAggregateRoot{TAuthenticationToken}"/> was at when it received an out of order <see cref="IEvent{TAuthenticationToken}"/>.
		/// </summary>
		[DataMember]
		public int CurrentVersion { get; set; }

		/// <summary>
		/// The version number the <see cref="IEvent{TAuthenticationToken}"/> that was provided, that was out of order.
		/// </summary>
		[DataMember]
		public int ProvidedEventVersion { get; set; }
	}
}