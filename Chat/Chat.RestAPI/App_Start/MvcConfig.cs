namespace Chat.RestAPI
{
	using Cqrs.WebApi;
	using System;
	using System.Linq;
	using System.Web.Mvc;

	public static class MvcConfig
	{
		public static void Register()
		{
			HelpPageConfig<Guid>.GenerateAssemblyXmlFileNames();
			string webAssemblyName = HelpPageConfig<Guid>.AssemblyXmlFileNames.First();
			// Clear the defaults.
			HelpPageConfig<Guid>.AssemblyXmlFileNames.Clear();
			// Re-add this assembly.
			HelpPageConfig<Guid>.AssemblyXmlFileNames.Add(webAssemblyName);
			// Add our micro-services assembly.
			HelpPageConfig<Guid>.AssemblyXmlFileNames.Add(webAssemblyName.Replace("RestAPI", "MicroServices"));

			AreaRegistration.RegisterAllAreas();
		}
	}
}