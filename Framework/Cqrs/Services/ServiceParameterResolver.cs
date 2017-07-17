#region Copyright
// -----------------------------------------------------------------------
// <copyright company="Chinchilla Software Limited">
//     Copyright Chinchilla Software Limited. All rights reserved.
// </copyright>
// -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using Cqrs.Authentication;

namespace Cqrs.Services
{
	/// <summary>
	/// A <see cref="DataContractResolver"/> for use via WCF
	/// </summary>
	public abstract class ServiceParameterResolver<TServiceParameter, TSingleSignOnTokenResolver> : BasicServiceParameterResolver<TServiceParameter, TSingleSignOnTokenResolver>
		where TSingleSignOnTokenResolver : ISingleSignOnToken
	{
		protected ServiceParameterResolver(ISingleSignOnTokenResolver tokenResolver, IEventDataResolver eventDataResolver)
			: base(tokenResolver, eventDataResolver)
		{
		}

		[Obsolete("Use TokenResolver.")]
		protected ISingleSignOnTokenResolver SingleSignOnTokenResolver
		{
			get { return (ISingleSignOnTokenResolver)TokenResolver; }
		}
	}
}