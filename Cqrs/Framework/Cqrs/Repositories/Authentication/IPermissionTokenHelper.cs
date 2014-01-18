namespace Cqrs.Repositories.Authentication
{
	public interface IPermissionTokenHelper<TPermissionToken>
	{
		TPermissionToken GetPermissionToken();

		TPermissionToken SetPermissionToken(TPermissionToken permissionScope);
	}
}