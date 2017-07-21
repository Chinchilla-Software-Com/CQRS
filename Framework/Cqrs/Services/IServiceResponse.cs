#region Copyright
// -----------------------------------------------------------------------
// <copyright company="Chinchilla Software Limited">
//     Copyright Chinchilla Software Limited. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;

namespace Cqrs.Services
{
	/// <summary>
	/// A response message envelope that holds request state and correlation information in response to using public API requests.
	/// </summary>
	public interface IServiceResponse
	{
		/// <summary>
		/// The state of the request.
		/// </summary>
		[DataMember]
		ServiceResponseStateType State { get; set; }

		/// <summary>
		/// The correlation id used to group together events and notifications.
		/// </summary>
		[DataMember]
		Guid CorrelationId { get; set; }
	}
}