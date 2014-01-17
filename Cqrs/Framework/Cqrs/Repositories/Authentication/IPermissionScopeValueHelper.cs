namespace Cqrs.Repositories.Authentication
{
	public interface IPermissionScopeValueHelper<TPermissionScope>
	{
		TPermissionScope GetPermissionScope();

		TPermissionScope SetPermissionScope(TPermissionScope permissionScope);
	}
}