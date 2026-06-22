using System;
using Cqrs.Authentication;

namespace Cqrs.Azure.ServiceBus.Tests.Integration
{
	/// <summary>
	/// A test <see cref="IAuthenticationTokenHelper{TAuthenticationToken}"/> that always 
	/// returns <see cref="Guid.Empty"/>
	/// </summary>
	public class GuidAuthenticationTokenHelper : IAuthenticationTokenHelper<Guid>
	{
		#region IAuthenticationTokenHelper<Guid> Members

		/// <summary>
		/// Gets <see cref="Guid.Empty"/>
		/// </summary>
		/// <returns><see cref="Guid.Empty"/></returns>
		public Guid GetAuthenticationToken()
		{
			return Guid.Empty;
		}

		/// <summary>
		/// Does nothing
		/// </summary>
		/// <returns><see cref="Guid.Empty"/></returns>
		public Guid SetAuthenticationToken(Guid permissionScope)
		{
			return Guid.Empty;
		}

		#endregion
	}
}