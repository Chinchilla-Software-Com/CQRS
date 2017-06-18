using System;
using System.Collections.Generic;
using System.Security;
using System.Web.Http;
using System.Web.Routing;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Cqrs.WebApi.Configuration;
using Cqrs.WebApi.Events.Handlers;

namespace Cqrs.WebApi
{
	public abstract class CqrsHttpApplication<TAuthenticationToken, TEventToHubProxy>
		: System.Web.HttpApplication
		where TEventToHubProxy : EventToHubProxy<TAuthenticationToken>
	{
		protected static IDependencyResolver DependencyResolver { get; set; }

		protected virtual void Application_Start(object sender, EventArgs e)
		{
			ConfigureDefaultDependencyResolver();
			RegisterDefaultRoutes();

			ConfigureMvc();

			BusRegistrar registrar = RegisterCommandAndEventHandlers();
			RegisterSignalR(registrar);

			LogApplicationStarted();
		}

		protected abstract void ConfigureDefaultDependencyResolver();

		/// <summary>
		/// Register SignalR to the path /signalr
		/// </summary>
		protected virtual void RegisterSignalR(BusRegistrar registrar)
		{
			RouteTable.Routes.MapOwinPath("/signalr");
			registrar.Register(typeof(TEventToHubProxy));
		}

		/// <summary>
		/// Register default offered routes and controllers such as the Java-script Client
		/// </summary>
		protected virtual void RegisterDefaultRoutes()
		{
			GlobalConfiguration.Configure(WebApiConfig.Register);
		}

		/// <summary>
		/// Override to configure MVC components such as AreaRegistration.RegisterAllAreas();
		/// </summary>
		protected virtual void ConfigureMvc()
		{
		}

		protected virtual BusRegistrar RegisterCommandAndEventHandlers()
		{
			var registrar = new BusRegistrar(DependencyResolver);
			return registrar;
		}

		/// <summary>
		/// Log that the application has started
		/// </summary>
		protected virtual void LogApplicationStarted()
		{
			try
			{
				ILogger logger = DependencyResolver.Resolve<ILogger>();

				if (logger != null)
				{
					DependencyResolver.Resolve<ICorrelationIdHelper>().SetCorrelationId(Guid.Empty);
					logger.LogInfo("Application started.");
				}
			}
			catch { /**/ }
		}

		protected virtual void Application_End(object sender, EventArgs e)
		{
			try
			{
				ILogger logger = DependencyResolver.Resolve<ILogger>();

				if (logger != null)
				{
					DependencyResolver.Resolve<ICorrelationIdHelper>().SetCorrelationId(Guid.Empty);
					logger.LogInfo("Application stopped.");
				}
			}
			catch { /**/ }
		}

		protected virtual void Application_Error(object sender, EventArgs e)
		{
			try
			{
				Exception ex = Server.GetLastError();

				ILogger logger = DependencyResolver.Resolve<ILogger>();
				Action<string, string, Exception, IDictionary<string, object>, IDictionary<string, object>> loggerFunction = logger.LogError;
				if (ex is SecurityException)
					loggerFunction = logger.LogWarning;

				loggerFunction("An error occurred.", null, ex, null, null);
			}
			catch { /**/ }
		}

		protected virtual void Application_BeginRequest(object sender, EventArgs e)
		{
			try
			{
				Guid correlationId = Guid.NewGuid();
				DependencyResolver.Resolve<ICorrelationIdHelper>().SetCorrelationId(correlationId);
				Response.AddHeader("CorrelationId", correlationId.ToString("N"));
			}
			catch (NullReferenceException) { }
		}

		protected virtual void Application_AuthenticateRequest(object sender, EventArgs e)
		{
		}

		protected virtual void Session_Start(object sender, EventArgs e)
		{
			// This is required otherwise the first call per new session will fail due to a WCF issue. This forces the SessionID to be created now, not after the response has been flushed on the pipeline.
			string sessionId = Session.SessionID;
		}

		protected virtual void Session_End(object sender, EventArgs e)
		{
		}
	}
}