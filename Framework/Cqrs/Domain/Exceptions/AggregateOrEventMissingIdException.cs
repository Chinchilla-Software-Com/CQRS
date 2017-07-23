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
	/// The <see cref="IEvent{TAuthenticationToken}"/> had not <see cref="IEvent{TAuthenticationToken}.Id"/> set.
	/// </summary>
	[Serializable]
	public class AggregateOrEventMissingIdException : Exception
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="AggregateOrEventMissingIdException"/> with the <see cref="Type"/> of the <see cref="IEvent{TAuthenticationToken}"/> and <see cref="IAggregateRoot{TAuthenticationToken}"/>.
		/// The <paramref name="eventType"/> that was trying to be saved on an <paramref name="aggregateType"/>.
		/// </summary>
		/// <param name="aggregateType">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> that the <see cref="IEvent{TAuthenticationToken}"/> was trying to be saved on.</param>
		/// <param name="eventType">The <see cref="Type"/> of the <see cref="IEvent{TAuthenticationToken}"/> that was trying to be saved.</param>
		public AggregateOrEventMissingIdException(Type aggregateType, Type eventType)
			: base(string.Format("An event of type {0} tried to be saved from {1} but no id was set on either", eventType.FullName, aggregateType.FullName))
		{
			AggregateType = aggregateType;
			EventType = eventType;
		}

		/// <summary>
		/// The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> that the <see cref="IEvent{TAuthenticationToken}"/> was trying to be saved on.
		/// </summary>
		[DataMember]
		public Type AggregateType { get; set; }

		/// <summary>
		/// The <see cref="Type"/> of the <see cref="IEvent{TAuthenticationToken}"/> that was trying to be saved.
		/// </summary>
		[DataMember]
		public Type EventType { get; set; }
	}
}