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
	/// An <see cref="IEvent{TAuthenticationToken}"/> was processed out of order or an expected <see cref="IEvent{TAuthenticationToken}"/> was not found.
	/// </summary>
	[Serializable]
	public class ConcurrencyException : Exception
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="ConcurrencyException"/> with the provided identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> that had a concurrency issue.
		/// </summary>
		/// <param name="id">The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> that wasn't found.</param>
		public ConcurrencyException(Guid id)
			: base(string.Format("A different version than expected was found in aggregate {0}", id))
		{
			Id = id;
		}

		/// <summary>
		/// The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> that had a concurrency issue.
		/// </summary>
		[DataMember]
		public Guid Id { get; set; }
	}
}