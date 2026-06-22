#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Cqrs.Authentication;
using Cqrs.Configuration;
using Cqrs.Ninject.Configuration;

namespace Cqrs.Ninject.Azure.Wcf.Configuration
{

	/// <summary>
	/// Referenced internally.
	/// </summary>
	class WcfStartUp : SimplifiedNinjectStartUp<WebHostModule>
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="WcfStartUp"/>
		/// </summary>
		public WcfStartUp(IConfigurationManager configurationManager)
			: base(configurationManager)
		{
		}

		#region Overrides of SimplifiedNinjectStartUp<WebHostModule>

#if NET472
		/// <summary>
		/// Adds the <see cref="SimplifiedSqlModule{TAuthenticationToken}"/> into <see cref="NinjectDependencyResolver.ModulesToLoad"/>.
		/// </summary>
		protected override void AddSupplementryModules()
		{
			NinjectDependencyResolver.ModulesToLoad.Insert(2, new SimplifiedSqlModule<SingleSignOnToken>());
		}
#endif

		#endregion
	}
}