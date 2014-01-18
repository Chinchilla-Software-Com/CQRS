namespace Cqrs.Repositories.Authentication
{
	public interface IAuthenticationTokenHelper<TAuthenticationToken>
	{
		TAuthenticationToken GetAuthenticationToken();

		TAuthenticationToken SetAuthenticationToken(TAuthenticationToken permissionScope);
	}
}