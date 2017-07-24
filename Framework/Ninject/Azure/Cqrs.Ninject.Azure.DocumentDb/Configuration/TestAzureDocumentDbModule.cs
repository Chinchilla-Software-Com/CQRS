#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Azure.DocumentDb.Factories;
using Cqrs.Ninject.Azure.DocumentDb.Factories;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.DocumentDb.Configuration
{
	/// <summary>
	/// A <see cref="INinjectModule"/> that wires up <see cref="TestAzureDocumentDbDataStoreConnectionStringFactory"/> as the
	/// <see cref="IAzureDocumentDbDataStoreConnectionStringFactory"/>.
	/// </summary>
	public class TestAzureDocumentDbModule : AzureDocumentDbModule
	{
		/// <summary>
		/// Register the all factories
		/// </summary>
		public override void RegisterFactories()
		{
			Bind<IAzureDocumentDbDataStoreConnectionStringFactory>()
				.To<TestAzureDocumentDbDataStoreConnectionStringFactory>()
				.InSingletonScope();
		}
	}
}