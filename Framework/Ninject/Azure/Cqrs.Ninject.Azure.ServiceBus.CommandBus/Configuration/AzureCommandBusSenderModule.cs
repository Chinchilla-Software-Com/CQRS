#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.ServiceBus.CommandBus.Configuration
{
	/// <summary>
	/// The <see cref="INinjectModule"/> for use with the Cqrs package.
	/// </summary>
	[Obsolete("Use AzureCommandBusPublisherModule")]
	public class AzureCommandBusSenderModule<TAuthenticationToken> : AzureCommandBusPublisherModule<TAuthenticationToken>
	{
	}
}