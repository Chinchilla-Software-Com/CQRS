#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Azure.DocumentDb.Events;
using Cqrs.Events;
using Cqrs.Ninject.Azure.DocumentDb.Events;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.DocumentDb.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="AzureDocumentDbEventStoreConnectionStringFactory"/> as the
	/// <see cref="IAzureDocumentDbEventStoreConnectionStringFactory"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class TestAzureDocumentDbEventStoreModule<TAuthenticationToken> : AzureDocumentDbEventStoreModule<TAuthenticationToken>
	{
		/// <summary>
		/// Register the <see cref="IAzureDocumentDbEventStoreConnectionStringFactory"/> and <see cref="IEventStore{TAuthenticationToken}"/>
		/// </summary>
		public override void RegisterEventStore()
		{
			Bind<IAzureDocumentDbEventStoreConnectionStringFactory>()
				.To<TestAzureDocumentDbEventStoreConnectionStringFactory>()
				.InSingletonScope();

			Bind<IEventStore<TAuthenticationToken>>()
				.To<AzureDocumentDbEventStore<TAuthenticationToken>>()
				.InSingletonScope();
		}
	}
}