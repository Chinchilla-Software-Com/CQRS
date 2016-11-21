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
		}

		protected void Application_End(object sender, EventArgs e)
		{
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

            Context.Response.AddHeader("Access-Control-Allow-Origin", "*");

            if (Context.Request.HttpMethod == "OPTIONS")
            {
                //These headers are handling the "pre-flight" OPTIONS call sent by the browser
                Context.Response.AddHeader("Access-Control-Allow-Methods", "GET, POST, PUT, DELETE");
                Context.Response.AddHeader("Access-Control-Allow-Headers", "Content-Type, Accept");
                Context.Response.AddHeader("Access-Control-Max-Age", "1728000");
                Context.Response.End();
            }
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