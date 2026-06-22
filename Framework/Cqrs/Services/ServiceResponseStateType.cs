#region Copyright
// -----------------------------------------------------------------------
// <copyright company="Chinchilla Software Limited">
//     Copyright Chinchilla Software Limited. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using Cqrs.Domain;
using Cqrs.Entities;

namespace Cqrs.Services
{
	/// <summary>
	/// The response state of a given request.
	/// </summary>
	[Serializable]
	[DataContract]
	public enum ServiceResponseStateType
	{
		/// <summary>
		/// The state is unknown.
		/// </summary>
		[EnumMember]
		Unknown = 0,

		/// <summary>
		/// The request succeeded in being placed.
		/// </summary>
		[EnumMember(Value = "Succeeded")]
		Succeeded = 1,

		/// <summary>
		/// The request failed with a fatal <see cref="Exception"/>.
		/// </summary>
		[EnumMember]
		FailedWithAFatalException = 2,

		/// <summary>
		/// The request failed with an unknown, non-fatal <see cref="Exception"/>.
		/// </summary>
		[EnumMember]
		FailedWithAnUnexpectedException = 3,

		/// <summary>
		/// The request failed to be authenticated.
		/// </summary>
		[EnumMember]
		FailedAuthentication = 11,

		/// <summary>
		/// The request failed to be authorised.
		/// </summary>
		[EnumMember]
		FailedAuthorisation = 12,

		/// <summary>
		/// The request failed validation.
		/// </summary>
		[EnumMember]
		FailedValidation = 21,

		/// <summary>
		/// The request failed as the <see cref="IAggregateRoot{TAuthenticationToken}"/> was not found.
		/// </summary>
		[EnumMember]
		AggregateNotFound = 31,

		/// <summary>
		/// The request failed as the <see cref="IEntity"/> was not found.
		/// </summary>
		[EnumMember]
		EntityNotFound = 32,

		/// <summary>
		/// The request failed as the <see cref="Saga{TAuthenticationToken}"/> was not found.
		/// </summary>
		[EnumMember]
		SagaNotFound = 33
	}
}