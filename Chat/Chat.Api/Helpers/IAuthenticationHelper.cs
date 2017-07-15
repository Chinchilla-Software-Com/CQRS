namespace Chat.Api.Helpers
{
	using System;

	public interface IAuthenticationHelper
	{
		string GetCurrentUsersName();

		Guid GetCurrentUser();
	}
}