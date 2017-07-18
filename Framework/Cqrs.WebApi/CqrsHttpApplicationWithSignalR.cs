#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Web.Http;
using System.Web.Routing;
using Cqrs.Bus;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Hosts;
using Cqrs.WebApi.Configuration;
using Cqrs.WebApi.Events.Handlers;

namespace Cqrs.WebApi
{
	/// <summary>
	/// A <see cref="CqrsHttpApplication"/> that allows you to specify how <see cref="IEvent{TAuthenticationToken}">events</see> are sent to SignalR.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	/// <typeparam name="TEventToHubProxy">The <see cref="Type"/> of the proxy class that specifies how <see cref="IEvent{TAuthenticationToken}">events</see> are sent to SignalR.</typeparam>
	public abstract class CqrsHttpApplicationWithSignalR<TAuthenticationToken, TEventToHubProxy>
		: CqrsHttpApplication<TAuthenticationToken>
		where TEventToHubProxy : EventToHubProxy<TAuthenticationToken>
	{
		protected override void Application_Start(object sender, EventArgs e)
		{
			SetBuses();

			RegisterDefaultRoutes();
			RegisterServiceParameterResolver();

			ConfigureMvcOrWebApi();

			BusRegistrar registrar = RegisterCommandAndEventHandlers();
			RegisterSignalR(registrar);

			StartBuses();

			LogApplicationStarted();
		}

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
		/// Override to configure MVC or WebAPI components such as AreaRegistration.RegisterAllAreas();
		/// </summary>
		protected virtual void ConfigureMvcOrWebApi()
		{
		}
	}

	/// <summary>
	/// A <see cref="CqrsHttpApplication"/> that uses the <see cref="GlobalEventToHubProxy{TAuthenticationToken}"/> to automatically proxy all <see cref="IEvent{TAuthenticationToken}">events</see> to SignalR
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public abstract class CqrsHttpApplicationWithSignalR<TAuthenticationToken>
		: CqrsHttpApplicationWithSignalR<TAuthenticationToken, GlobalEventToHubProxy<TAuthenticationToken>>
	{
		#region Overrides of CqrsHttpApplicationWithSignalR<TAuthenticationToken, TEventToHubProxy>

		/// <summary>
		/// Register SignalR and auto wire-up <see cref="GlobalEventToHubProxy{TAuthenticationToken}"/> to automatically proxy all <see cref="IEvent{TAuthenticationToken}">events</see> to SignalR.
		/// </summary>
		protected override void RegisterSignalR(BusRegistrar registrar)
		{
			base.RegisterSignalR(registrar);

			var eventHandlerRegistrar = DependencyResolver.Current.Resolve<IEventHandlerRegistrar>();
			var proxy = DependencyResolver.Current.Resolve<GlobalEventToHubProxy<TAuthenticationToken>>();
			eventHandlerRegistrar.RegisterGlobalEventHandler<IEvent<TAuthenticationToken>>(proxy.Handle, false);
		}

		#endregion
	}
}