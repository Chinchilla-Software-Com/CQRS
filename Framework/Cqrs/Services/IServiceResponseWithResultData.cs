using System;
using System.Runtime.Serialization;

namespace Cqrs.Services
{
	/// <summary>
	/// A <see cref="IServiceResponse">response message envelope</see> that holds request state, correlation information as well as the response data returned from making a public API request.
	/// </summary>
	/// <typeparam name="TResultData">The <see cref="Type">type</see> of data returned from making a public API request.</typeparam>
	public interface IServiceResponseWithResultData<TResultData> : IServiceResponse
	{
		/// <summary>
		/// The data returned from making a public API request.
		/// </summary>
		[DataMember]
		TResultData ResultData { get; set; }
	}
}