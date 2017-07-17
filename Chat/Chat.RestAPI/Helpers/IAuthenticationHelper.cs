namespace Chat.RestAPI.Helpers
{
	using System;

	public interface IAuthenticationHelper
	{
		string GetCurrentUsersName();

		Guid GetCurrentUser();
	}
}