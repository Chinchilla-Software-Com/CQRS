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
	/// The <see cref="IEventStore{TAuthenticationToken}"/> gave more than one event.
	/// </summary>
	/// <typeparam name="TSaga">The <see cref="Type"/> of the <see cref="ISaga{TAuthenticationToken}"/> that wasn't found.</typeparam>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	[Serializable]
	public class DuplicateSagaEventException<TSaga, TAuthenticationToken> : Exception
		where TSaga : ISaga<TAuthenticationToken>
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="DuplicateSagaEventException{TAggregateRoot,TAuthenticationToken}"/> with the provided identifier of the <see cref="ISaga{TAuthenticationToken}"/> that had duplicate <see cref="IEvent{TAuthenticationToken}">events</see>.
		/// and the <paramref name="version"/> that had more than one <see cref="IEvent{TAuthenticationToken}"/> provided.
		/// </summary>
		/// <param name="id">The identifier of the <see cref="ISaga{TAuthenticationToken}"/> that had <see cref="IEvent{TAuthenticationToken}">events</see>.</param>
		/// <param name="version">The version number of the duplicate <see cref="IEvent{TAuthenticationToken}">events</see></param>
		public DuplicateSagaEventException(Guid id, int version)
			: base(string.Format("Eventstore gave more than one event for saga '{0}' of type '{1}' for version {2}", id, typeof(TSaga).FullName, version))
		{
			Id = id;
			SagaType = typeof(TSaga);
		}

		/// <summary>
		/// The identifier of the <see cref="ISaga{TAuthenticationToken}"/> that had duplicate <see cref="IEvent{TAuthenticationToken}">events</see>..
		/// </summary>
		[DataMember]
		public Guid Id { get; set; }

		/// <summary>
		/// The <see cref="Type"/> of the <see cref="ISaga{TAuthenticationToken}"/> that had duplicate <see cref="IEvent{TAuthenticationToken}">events</see>..
		/// </summary>
		[DataMember]
		public Type SagaType { get; set; }
	}
}