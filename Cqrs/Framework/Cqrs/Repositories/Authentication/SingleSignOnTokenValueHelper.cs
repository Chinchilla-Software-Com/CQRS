using System.Runtime.Remoting.Messaging;

namespace Cqrs.Repositories.Authentication
{
	public class SingleSignOnTokenValueHelper : IPermissionTokenHelper<ISingleSignOnToken>
	{
		private const string CallContextPermissoinScopeValueKey = "SingleSignOnTokenValue";

		#region Implementation of IPermissionTokenHelper<out ISingleSignOnToken>

		public ISingleSignOnToken GetPermissionToken()
		{
			return (ISingleSignOnToken)CallContext.GetData(CallContextPermissoinScopeValueKey);
		}

		public ISingleSignOnToken SetPermissionToken(ISingleSignOnToken value)
		{
			CallContext.SetData(CallContextPermissoinScopeValueKey, value);
			return value;
		}

		#endregion
	}
}