#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Security;
using System.Web;
using System.Web.SessionState;
using cdmdotnet.Logging;
using Cqrs.Authentication;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;

namespace Cqrs.Hosts
{
	/// <summary>
	/// An <see cref="HttpApplication"/> that prepares and configures CQRS with use in IIS or other web server.
	/// </summary>
	public abstract class CqrsHttpApplication
		: HttpApplication
	{
		/// <summary>
		/// Each <see cref="Type"/> will be traced back to it's assembly, and that assembly will be scanned for other handlers to auto register.
		/// </summary>
		protected Type[] HandlerTypes { get; set; }

		/// <summary>
		/// The <see cref="IEventReceiver"/> that will be configured to receive <see cref="IEvent{TAuthenticationToken}">events</see>.
		/// </summary>
		protected IEventReceiver EventBus { get; set; }

		/// <summary>
		/// The <see cref="ICommandReceiver"/> that will be configured to receive <see cref="ICommand{TAuthenticationToken}">commands</see>.
		/// </summary>
		protected ICommandReceiver CommandBus { get; set; }

		/// <summary>
		/// Instantiate a new instance of a <see cref="CqrsHttpApplication"/>
		/// </summary>
		protected CqrsHttpApplication()
		{
			HandlerTypes = new Type[]{};
		}

		/// <summary>
		/// Calls <see cref="SetBuses"/>, <see cref="RegisterCommandAndEventHandlers"/> and finally <see cref="LogApplicationStarted"/>.
		/// Gets executed once during the life cycle of the application when the first request for any resource in the application is made. A resource can be a page or an image in the application. 
		/// If the server where the application is hosted is restarted then this is fired once again upon the first request for any resource in the application.
		/// </summary>
		protected virtual void Application_Start(object sender, EventArgs e)
		{
			SetBuses();

			RegisterCommandAndEventHandlers();

			StartBuses();

			LogApplicationStarted();
		}

		/// <summary>
		/// Set <see cref="EventBus"/> and <see cref="CommandBus"/>.
		/// </summary>
		protected abstract void SetBuses();

		/// <summary>
		/// Start the <see cref="BusRegistrar"/> by calling <see cref="BusRegistrar.Register(System.Type[])"/> passing <see cref="HandlerTypes"/>
		/// </summary>
		protected virtual BusRegistrar RegisterCommandAndEventHandlers()
		{
			var registrar = new BusRegistrar(DependencyResolver.Current);
			registrar.Register(HandlerTypes);

			return registrar;
		}

		/// <summary>
		/// Starts the command bus and event bus receivers depending on configuration settings
		/// </summary>
		protected virtual void StartBuses()
		{

			var configurationManager = DependencyResolver.Current.Resolve<IConfigurationManager>();
			bool setting;
			if (!configurationManager.TryGetSetting("Cqrs.Hosts.EnableEventReceiving", out setting))
				setting = true;
			if (setting)
				EventBus.Start();
			if (!configurationManager.TryGetSetting("Cqrs.Hosts.EnableCommandReceiving", out setting))
				setting = true;
			if (setting)
				CommandBus.Start();
		}

		/// <summary>
		/// Log that the application has started
		/// </summary>
		protected virtual void LogApplicationStarted()
		{
			try
			{
				ILogger logger = DependencyResolver.Current.Resolve<ILogger>();

				if (logger != null)
				{
					DependencyResolver.Current.Resolve<ICorrelationIdHelper>().SetCorrelationId(Guid.Empty);
					logger.LogInfo("Application started.");
				}
			}
			catch { /**/ }
		}

		/// <summary>
		/// Gets executed once during the life cycle of the application when it is unloaded.
		/// This is normally fired when the application is taken off-line or when the server is stopped.
		/// </summary>
		protected virtual void Application_End(object sender, EventArgs e)
		{
			try
			{
				ILogger logger = DependencyResolver.Current.Resolve<ILogger>();

				if (logger != null)
				{
					DependencyResolver.Current.Resolve<ICorrelationIdHelper>().SetCorrelationId(Guid.Empty);
					logger.LogInfo("Application stopped.");
				}
			}
			catch { /**/ }
		}

		/// <summary>
		/// Logs all error via <see cref="ILogger.LogError"/> unless the execution is <see cref="SecurityException"/>, in which case <see cref="ILogger.LogWarning"/> is used.
		/// Gets executed when any un-handled <see cref="Exception"/>/error occurs anywhere in the application. Any un-handled <see cref="Exception"/> here means exception which are not caught using try catch block. Also if you have custom errors enabled in your application i.e. in web.config file then the configuration in web.config takes precedence and all errors will be directed to the file mentioned in the tag.
		/// </summary>
		protected virtual void Application_Error(object sender, EventArgs e)
		{
			try
			{
				Exception ex = Server.GetLastError();

				ILogger logger = DependencyResolver.Current.Resolve<ILogger>();
				Action<string, string, Exception, IDictionary<string, object>, IDictionary<string, object>> loggerFunction = logger.LogError;
				if (ex is SecurityException)
					loggerFunction = logger.LogWarning;

				loggerFunction("An error occurred.", null, ex, null, null);
			}
			catch { /**/ }
		}

		/// <summary>
		/// Gets executed for each and every request which comes to the application to generate a new CorrelationId and then sets the generated CorrelationId via <see cref="ICorrelationIdHelper.SetCorrelationId"/>.
		/// The generated CorrelationId is also set in the <see cref="HttpResponse.Headers"/>.
		/// </summary>
		protected virtual void Application_BeginRequest(object sender, EventArgs e)
		{
			try
			{
				Guid correlationId = Guid.NewGuid();
				DependencyResolver.Current.Resolve<ICorrelationIdHelper>().SetCorrelationId(correlationId);
				Response.AddHeader("CorrelationId", correlationId.ToString("N"));
			}
			catch (NullReferenceException) { }
		}

		/// <summary>
		/// Gets executed after <see cref="Application_BeginRequest"/> and before the stream starts getting sent to the client.
		/// </summary>
		protected virtual void Application_EndRequest(object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Gets executed after <see cref="Application_BeginRequest"/>.
		/// Override this method to extract any authentication token from the request and then call <see cref="IAuthenticationTokenHelper{TAuthenticationToken}.SetAuthenticationToken"/>.
		/// </summary>
		protected virtual void Application_AuthenticateRequest(object sender, EventArgs e)
		{
		}

		/// <summary>
		/// Makes a call to retrieve the <see cref="HttpSessionState.SessionID"/>. This is done so the session is generated at the beginning of the request.
		/// If this isn't called the session (when using WCF) is generated late in the pipeline, which can cause issues when trying to work with WCF.
		/// Gets executed after <see cref="Application_AuthenticateRequest"/> when a new session for a "user" starts such as a first request or after a session has expired.
		/// This event doesn't get triggered if you are not using sessions which can be disabled in the web.config.
		/// </summary>
		protected virtual void Session_Start(object sender, EventArgs e)
		{
			// This is required otherwise the first call per new session will fail due to a WCF issue. This forces the SessionID to be created now, not after the response has been flushed on the pipeline.
			// ReSharper disable UnusedVariable
			string sessionId = Session.SessionID;
			// ReSharper restore UnusedVariable
		}

		/// <summary>
		/// Whenever a user's session in the application expires this gets executed. The session is no longer available or accessible. 
		/// The session expiration time can be set in web.config file. By default session time out is set to 20 mins.
		/// </summary>
		protected virtual void Session_End(object sender, EventArgs e)
		{
		}
	}

	/// <summary>
	/// An <see cref="HttpApplication"/> that prepares and configures CQRS with use in IIS or other web server with knowledge of a basic type authentication token being sent in <see cref="HttpRequest.Headers"/>, <see cref="HttpRequest.Cookies"/>, <see cref="HttpRequest.Form"/> or <see cref="HttpRequest.QueryString"/>.
	/// A basic type authentication token is <see cref="Guid"/>, <see cref="string"/> or <see cref="int"/>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public abstract class CqrsHttpApplication<TAuthenticationToken>
		: CqrsHttpApplication
	{
		#region Overrides of CqrsHttpApplication

		/// <summary>
		/// Gets executed after <see cref="CqrsHttpApplication.Application_BeginRequest"/>.
		/// Extracts the authentication token looking for a <see cref="KeyValuePair{TKey,TValue}"/> where the key as defined by the <see cref="System.Configuration.ConfigurationManager.AppSettings"/> "Cqrs.Web.AuthenticationTokenName",
		/// from the <see cref="HttpRequest.Headers"/>, if one isn't found we then try the following in order 
		/// <see cref="HttpRequest.Cookies"/>, <see cref="HttpRequest.Form"/> or <see cref="HttpRequest.QueryString"/>; then
		/// calls <see cref="IAuthenticationTokenHelper{TAuthenticationToken}.SetAuthenticationToken"/> to make it accessible to others parts of the system if one is found.
		/// </summary>
		protected override void Application_AuthenticateRequest(object sender, EventArgs e)
		{
			if (typeof (TAuthenticationToken) != typeof (Guid) && typeof (TAuthenticationToken) != typeof (string) && typeof (TAuthenticationToken) != typeof (int))
				return;

			string authenticationTokenName = DependencyResolver.Current.Resolve<IConfigurationManager>().GetSetting("Cqrs.Web.AuthenticationTokenName") ?? "X-Token";

			string xToken = Request.Headers[authenticationTokenName];
			if (string.IsNullOrWhiteSpace(xToken))
			{
				HttpCookie authCookie = Request.Cookies[authenticationTokenName];
				if (authCookie != null)
					xToken = authCookie.Value;
			}
			if (string.IsNullOrWhiteSpace(xToken))
				xToken = Request.Form[authenticationTokenName];
			if (string.IsNullOrWhiteSpace(xToken))
				xToken = Request.QueryString[authenticationTokenName];
			if (typeof(TAuthenticationToken) != typeof(Guid))
			{
				Guid token;
				if (Guid.TryParse(xToken, out token))
				{
					// Pass the authentication token to the helper to allow automated authentication handling
					DependencyResolver.Current.Resolve<IAuthenticationTokenHelper<TAuthenticationToken>>().SetAuthenticationToken((TAuthenticationToken)(object)token);
				}
			}
			else if (typeof(TAuthenticationToken) != typeof(int))
			{
				int token;
				if (int.TryParse(xToken, out token))
				{
					// Pass the authentication token to the helper to allow automated authentication handling
					DependencyResolver.Current.Resolve<IAuthenticationTokenHelper<TAuthenticationToken>>().SetAuthenticationToken((TAuthenticationToken)(object)token);
				}
			}
			else
				DependencyResolver.Current.Resolve<IAuthenticationTokenHelper<TAuthenticationToken>>().SetAuthenticationToken((TAuthenticationToken)(object)xToken);
		}

		/// <summary>
		/// Set <see cref="CqrsHttpApplication.EventBus"/> and <see cref="CqrsHttpApplication.CommandBus"/>.
		/// </summary>
		protected override void SetBuses()
		{
			EventBus = DependencyResolver.Current.Resolve<IEventReceiver<TAuthenticationToken>>();
			CommandBus = DependencyResolver.Current.Resolve<ICommandReceiver<TAuthenticationToken>>();
		}

		#endregion
	}
}