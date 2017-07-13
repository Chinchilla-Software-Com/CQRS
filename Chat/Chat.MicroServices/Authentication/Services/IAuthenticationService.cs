namespace Chat.MicroServices.Authentication.Services
{
	using Cqrs.Services;
	using Entities;
	using System;

	public interface IAuthenticationService
	{
		/// <summary>
		/// Validate the provided <paramref name="emailAddress"/> and <paramref name="password"/> match a valid set of credentials
		/// </summary>
		/// <returns>The <see cref="UserEntity.Rsn"/></returns>
		IServiceResponseWithResultData<Guid?> ValidateCredentials(string emailAddress, string password);
	}
}