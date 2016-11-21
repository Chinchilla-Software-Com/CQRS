using System;
using Cqrs.Authentication;

namespace Cqrs.Azure.ServiceBus.Tests.Integration
{
	public class GuidSingleSignOnTokenValueHelper : IAuthenticationTokenHelper<Guid>
	{
		#region IAuthenticationTokenHelper<Guid> Members

		public Guid GetAuthenticationToken()
		{
			return Guid.Empty;
		}

		public Guid SetAuthenticationToken(Guid permissionScope)
		{
			return Guid.Empty;
		}

		#endregion
	}
}