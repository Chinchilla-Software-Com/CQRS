using System;
using System.Runtime.Serialization;

namespace Cqrs.Services
{
	/// <summary>
	/// A <see cref="IServiceRequest{TAuthenticationToken}">request message envelope</see> that holds authentication, correlation information as well as request data used when making public API requests.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type">type</see> of authentication token used.</typeparam>
	/// <typeparam name="TData">The <see cref="Type">type</see> of data sent along with the request to act upon/with.</typeparam>
	public interface IServiceRequestWithData<TAuthenticationToken, TData> : IServiceRequest<TAuthenticationToken>
	{
		/// <summary>
		/// The data sent along with the request to act upon/with.
		/// </summary>
		[DataMember]
		TData Data { get; set; }
	}
}