#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using cdmdotnet.Logging;
using cdmdotnet.Logging.Configuration;
using Ninject.Modules;

namespace $ext_safeprojectname$.Configuration
{
	public class BaseModule : NinjectModule
	{
		#region Overrides of NinjectModule

		/// <summary>
		/// Loads the module into the kernel.
		/// </summary>
		public override void Load()
		{
		}

		#endregion
	}
}