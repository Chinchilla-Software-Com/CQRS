#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Security;
using System.Web.Http;
using System.Web.Mvc;
using System.Web.Routing;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Cqrs.Ninject.Configuration;
using $safeprojectname$.SignalR;

namespace $safeprojectname$
{
	public class Global : System.Web.HttpApplication
	{
		protected void Application_Start(object sender, EventArgs e)
		{
			// https://alexandrebrisebois.wordpress.com/2013/03/24/why-are-webrequests-throttled-i-want-more-throughput/
			ServicePointManager.UseNagleAlgorithm = false;
			ServicePointManager.DefaultConnectionLimit = 1000;
			// http://stackoverflow.com/questions/12915585/azure-queue-performance
			ServicePointManager.Expect100Continue = false;

			// Register the default hubs route: ~/signalr/hubs
			RouteTable.Routes.MapOwinPath("/signalr");

			GlobalConfiguration.Configure(WebApiConfig.Register);

			// Add this code, if not present.
			AreaRegistration.RegisterAllAreas();

			var registrar = new BusRegistrar(NinjectDependencyResolver.Current);
			registrar.Register(typeof(EventToHubProxy));

			try
			{
				ILogger logger = NinjectDependencyResolver.Current.Resolve<ILogger>();

				if (logger != null)
				{
					NinjectDependencyResolver.Current.Resolve<ICorrelationIdHelper>().SetCorrelationId(Guid.Empty);
					logger.LogInfo("Application started.");
				}
			}
			catch { }
		}

		protected void Application_End(object sender, EventArgs e)
		{
			try
			{
				ILogger logger = NinjectDependencyResolver.Current.Resolve<ILogger>();

				if (logger != null)
				{
					NinjectDependencyResolver.Current.Resolve<ICorrelationIdHelper>().SetCorrelationId(Guid.Empty);
					logger.LogInfo("Application stopped.");
				}
			}
			catch { }
		}

		protected void Application_Error(object sender, EventArgs e)
		{
			try
			{
				Exception ex = Server.GetLastError();

				ILogger logger = NinjectDependencyResolver.Current.Resolve<ILogger>();
				Action<string, string, Exception, IDictionary<string, object>, IDictionary<string, object>> loggerFunction = logger.LogError;
				if (ex is SecurityException)
					loggerFunction = logger.LogWarning;

				loggerFunction("An error occurred.", null, ex, null, null);
			}
			catch { }
		}

		protected void Application_BeginRequest(object sender, EventArgs e)
		{
			try
			{
				ICorrelationIdHelper correlationIdHelper = NinjectDependencyResolver.Current.Resolve<ICorrelationIdHelper>();
				correlationIdHelper.SetCorrelationId(Guid.NewGuid());
			}
			catch (NullReferenceException) { }
		}

		protected void Application_AuthenticateRequest(object sender, EventArgs e)
		{
		}

		protected void Session_Start(object sender, EventArgs e)
		{
			// This is required otherwise the first call per new session will fail due to a WCF issue. This forces the SessionID to be created now, not after the response has been flushed on the pipeline.
			string sessionId = Session.SessionID;
		}

		protected void Session_End(object sender, EventArgs e)
		{
		}
	}
}