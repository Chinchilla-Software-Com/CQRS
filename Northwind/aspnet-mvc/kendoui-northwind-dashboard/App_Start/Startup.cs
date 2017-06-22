[assembly: Microsoft.Owin.OwinStartup(typeof(KendoUI.Northwind.Dashboard.Startup))]

namespace KendoUI.Northwind.Dashboard
{
	using Owin;

	public partial class Startup
	{
		public void Configuration(IAppBuilder app)
		{
		}
	}
}