#region Copyright
// -----------------------------------------------------------------------
// <copyright company="Chinchilla Software Limited">
//     Copyright Chinchilla Software Limited. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#endregion

using Cqrs.Authentication;

namespace Cqrs.Services
{
	/// <summary>
	/// A <see cref="BasicServiceParameterResolver{TServiceParameter,TAuthenticationToken}"/>.
	/// </summary>
	public abstract class ServiceParameterResolver<TServiceParameter, TSingleSignOnTokenResolver> : BasicServiceParameterResolver<TServiceParameter, TSingleSignOnTokenResolver>
		where TSingleSignOnTokenResolver : ISingleSignOnToken
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="ServiceParameterResolver{TServiceParameter,TSingleSignOnTokenResolver}"/>
		/// </summary>
		protected ServiceParameterResolver(ISingleSignOnTokenResolver tokenResolver, IEventDataResolver eventDataResolver)
			: base(tokenResolver, eventDataResolver)
		{
		}
	}
}