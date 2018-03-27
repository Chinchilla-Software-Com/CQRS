#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Events;
using Cqrs.Ninject.Configuration;

namespace Cqrs.Tests.Integrations.Configuration
{
	public class TestCqrsModule
		: ResolvableModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
			Bind<ISnapshotBuilder>()
				.To<DefaultSnapshotBuilder>()
				.InSingletonScope();
		}

		#endregion
	}
}