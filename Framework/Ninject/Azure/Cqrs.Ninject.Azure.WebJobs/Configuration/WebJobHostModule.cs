#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Chinchilla.StateManagement;
using Chinchilla.StateManagement.Threaded;
using Cqrs.Ninject.Azure.Wcf.Configuration;
using Ninject.Modules;

namespace Cqrs.Ninject.Azure.WebJobs.Configuration
{
	/// <summary>
	/// The core <see cref="INinjectModule"/> for use defining base level requirements.
	/// </summary>
	public class WebJobHostModule : WebHostModule
	{
		/// <summary>
		/// Registers the <see cref="IContextItemCollectionFactory"/> required.
		/// </summary>
		protected override void RegisterContextItemCollectionFactory()
		{
			// We use console state as, even though a webjob runs in an azure website, it's technically loaded via something call the 'WindowsScriptHost', which is not web and IIS based so it's threading model is very different and more console based.
			bool isContextItemCollectionFactoryBound = Kernel.GetBindings(typeof(IContextItemCollectionFactory)).Any();
			if (isContextItemCollectionFactoryBound)
				Unbind<IContextItemCollectionFactory>();
			Bind<IContextItemCollectionFactory>()
				.To<ContextItemCollectionFactory>()
				.InSingletonScope();
		}
	}
}