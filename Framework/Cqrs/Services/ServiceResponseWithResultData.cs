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
	/// A <see cref="IServiceResponse">response message envelope</see> that holds request state, correlation information as well as the response data returned from making a public API request.
	/// </summary>
	/// <typeparam name="TResultData">The <see cref="Type">type</see> of data returned from making a public API request.</typeparam>
	[Serializable]
	[DataContract]
	public class ServiceResponseWithResultData<TResultData> : ServiceResponse, IServiceResponseWithResultData<TResultData>
	{
		/// <summary>
		/// The data returned from making a public API request.
		/// </summary>
		[DataMember]
		public TResultData ResultData { get; set; }

		/// <summary>
		/// Instantiate a new instance of the <see cref="ServiceResponseWithResultData{TResultData}"/> class.
		/// </summary>
		public ServiceResponseWithResultData()
		{
		}

		/// <summary>
		/// Instantiate a new instance of the <see cref="ServiceResponseWithResultData{TResultData}"/> class providing <paramref name="resultData"/>.
		/// </summary>
		public ServiceResponseWithResultData(TResultData resultData)
		{
			State = ServiceResponseStateType.Succeeded;
			ResultData = resultData;
		}
	}
}