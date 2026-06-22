#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using Cqrs.Domain;

namespace Cqrs.Events
{
	/// <summary>
	/// An <see cref="IEvent{TAuthenticationToken}"/> used specifically by a <see cref="ISaga{TAuthenticationToken}"/>
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public interface ISagaEvent<TAuthenticationToken>
		: IEvent<TAuthenticationToken>
	{
		/// <summary>
		/// The identity of the <see cref="ISaga{TAuthenticationToken}" /> being targeted.
		/// </summary>
		[DataMember]
		Guid Rsn { get; set; }

		/// <summary>
		/// The <see cref="IEvent{TAuthenticationToken}"/> this <see cref="ISagaEvent{TAuthenticationToken}"/> encases.
		/// </summary>
		[DataMember]
		IEvent<TAuthenticationToken> Event { get; set; }
	}
}