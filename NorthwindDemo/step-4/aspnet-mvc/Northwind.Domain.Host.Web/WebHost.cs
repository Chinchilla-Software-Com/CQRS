#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Northwind.Domain.Host.Configuration;
using Northwind.Domain.Host.Web.Configuration;

namespace Northwind.Domain.Host.Web
{
	public class WebHost : DomainHost<WebHostModule>
	{
		#region Overrides of DomainHost<WebHostModule>

		protected override DomainConfiguration<WebHostModule> GetDomainConfiguration()
		{
			return new WebDomainConfiguration();
		}

		#endregion

		protected override void Run()
		{
		}
	}
}