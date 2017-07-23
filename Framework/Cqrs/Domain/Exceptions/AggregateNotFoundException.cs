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
	/// The <see cref="IAggregateRoot{TAuthenticationToken}"/> requested was not found.
	/// </summary>
	/// <typeparam name="TAggregateRoot">The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> that wasn't found.</typeparam>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	[Serializable]
	public class AggregateNotFoundException<TAggregateRoot, TAuthenticationToken> : Exception
		where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="AggregateNotFoundException{TAggregateRoot,TAuthenticationToken}"/> with the provided identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> that wasn't found.
		/// </summary>
		/// <param name="id">The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> that wasn't found.</param>
		public AggregateNotFoundException(Guid id)
			: base(string.Format("Aggregate '{0}' of type '{1}' was not found", id, typeof(TAggregateRoot).FullName))
		{
			Id = id;
			AggregateType = typeof(TAggregateRoot);
		}

		/// <summary>
		/// The identifier of the <see cref="IAggregateRoot{TAuthenticationToken}"/> that wasn't found.
		/// </summary>
		[DataMember]
		public Guid Id { get; set; }

		/// <summary>
		/// The <see cref="Type"/> of the <see cref="IAggregateRoot{TAuthenticationToken}"/> that wasn't found.
		/// </summary>
		[DataMember]
		public Type AggregateType { get; set; }
	}
}