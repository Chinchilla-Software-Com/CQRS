using System.Runtime.Remoting.Messaging;

namespace Cqrs.Repositories.Authentication
{
	public class SingleSignOnTokenValueHelper : IPermissionScopeValueHelper<ISingleSignOnToken>
	{
		private const string CallContextPermissoinScopeValueKey = "SingleSignOnTokenValue";

		#region Implementation of IPermissionScopeValueHelper<out ISingleSignOnToken>

		public ISingleSignOnToken GetPermissionScope()
		{
			return (ISingleSignOnToken)CallContext.GetData(CallContextPermissoinScopeValueKey);
		}

		public ISingleSignOnToken SetPermissionScope(ISingleSignOnToken value)
		{
			CallContext.SetData(CallContextPermissoinScopeValueKey, value);
			return value;
		}

		#endregion
	}
}