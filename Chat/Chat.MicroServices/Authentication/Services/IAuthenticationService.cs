namespace Chat.MicroServices.Authentication.Services
{
	using System;
	using System.ServiceModel;
	using Cqrs.Services;

	/// <summary>
	/// A WCF contract for accessing and modifying credentials.
	/// </summary>
	[ServiceContract(Namespace = "https://getcqrs.net/Authentication/1001/")]
	public interface IAuthenticationService : IEventService<Guid>
	{
		/// <summary>
		/// Validate the provided <paramref name="serviceRequest">credentials</paramref> are valid.
		/// </summary>
		/// <param name="serviceRequest">The user credentials to validate.</param>
		/// <returns>The users identifier.</returns>
		[OperationContract]
		IServiceResponseWithResultData<Guid?> Login(IServiceRequestWithData<Guid, AuthenticationService.LoginParameters> serviceRequest);
	}
}