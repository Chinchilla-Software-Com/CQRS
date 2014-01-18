using System.Runtime.Remoting.Messaging;

namespace Cqrs.Authentication
{
	public class SingleSignOnTokenValueHelper : IAuthenticationTokenHelper<ISingleSignOnToken>
	{
		private const string CallContextPermissoinScopeValueKey = "SingleSignOnTokenValue";

		#region Implementation of IAuthenticationTokenHelper<out ISingleSignOnToken>

		public ISingleSignOnToken GetAuthenticationToken()
		{
			return (ISingleSignOnToken)CallContext.GetData(CallContextPermissoinScopeValueKey);
		}

		public ISingleSignOnToken SetAuthenticationToken(ISingleSignOnToken value)
		{
			CallContext.SetData(CallContextPermissoinScopeValueKey, value);
			return value;
		}

		#endregion
	}
}