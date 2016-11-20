#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Security;
using cdmdotnet.Logging;
using Cqrs.Ninject.Configuration;

namespace Northwind.Domain.Host.Web
{
	public class Global : System.Web.HttpApplication
	{
		protected void Application_Start(object sender, EventArgs e)
		{
			var host = new WebHost();
			host.Configure();
			host.Start();

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
				Action<string, string, Exception> loggerFunction = logger.LogError;
				if (ex is SecurityException)
					loggerFunction = logger.LogWarning;

				loggerFunction("An error occurred.", null, ex);
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