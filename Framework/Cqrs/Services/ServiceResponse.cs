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
	[Serializable]
	[DataContract]
	public class ServiceResponse : IVersionedServiceResponse
	{
		/// <summary>
		/// Instantiate a new instance of the <see cref="ServiceResponse"/> class.
		/// </summary>
		public ServiceResponse()
		{
			State = ServiceResponseStateType.Succeeded;
		}

		#region Implementation of IServiceResponse

		/// <summary>
		/// The state of the request.
		/// </summary>
		[DataMember]
		public ServiceResponseStateType State { get; set; }

		/// <summary>
		/// The correlation id used to group together events and notifications.
		/// </summary>
		[DataMember]
		public Guid CorrelationId { get; set; }

		#endregion

		#region Implementation of IVersionedServiceResponse

		/// <summary>
		/// The version of the data being returned
		/// </summary>
		[DataMember]
		public double Version { get; set; }

		#endregion
	}
}