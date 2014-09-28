namespace Cqrs.Authentication
{
	public interface IAuthenticationTokenHelper<TAuthenticationToken>
	{
		TAuthenticationToken GetAuthenticationToken();

		TAuthenticationToken SetAuthenticationToken(TAuthenticationToken permissionScope);
	}
}