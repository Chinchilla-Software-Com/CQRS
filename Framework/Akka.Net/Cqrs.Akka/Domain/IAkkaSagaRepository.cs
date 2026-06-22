#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using Cqrs.Domain;
using Cqrs.Domain.Exceptions;
using Cqrs.Events;

namespace Cqrs.Akka.Domain
{
	/// <summary>
	/// A <see cref="SagaRepository{TAuthenticationToken}"/> that is safe to use within Akka.NET
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	public interface IAkkaSagaRepository<TAuthenticationToken> : ISagaRepository<TAuthenticationToken>
	{
		/// <summary>
		/// If <paramref name="events"/> is null, loads the events from an <see cref="IEventStore{TAuthenticationToken}"/>, checks for duplicates and then
		/// rehydrates the <paramref name="saga"/> with the events.
		/// </summary>
		/// <typeparam name="TSaga">The <see cref="Type"/> of <see cref="ISaga{TAuthenticationToken}"/>.</typeparam>
		/// <param name="saga">The <typeparamref name="TSaga"/> to rehydrate.</param>
		/// <param name="events">
		/// A collection of <see cref="IEvent{TAuthenticationToken}"/> to replay on the retrieved <see cref="ISaga{TAuthenticationToken}"/>.
		/// If null, the <see cref="IEventStore{TAuthenticationToken}"/> will be used to retrieve a list of <see cref="IEvent{TAuthenticationToken}"/> for you.
		/// </param>
		/// <param name="throwExceptionOnNoEvents">If true will throw an instance of <see cref="SagaNotFoundException{TSaga,TAuthenticationToken}"/> if no aggregate events or provided or found in the <see cref="IEventStore{TAuthenticationToken}"/>.</param>
		void LoadSagaHistory<TSaga>(TSaga saga, IList<ISagaEvent<TAuthenticationToken>> events = null, bool throwExceptionOnNoEvents = true)
			where TSaga : ISaga<TAuthenticationToken>;
	}
}