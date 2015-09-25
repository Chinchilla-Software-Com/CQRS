#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Akka.Actor;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;

namespace Cqrs.Akka.Configuration
{
	/// <summary>
	/// Triggers the <see cref="BusRegistrar"/> instantiates instances of <see cref="IEventHandler{TAuthenticationToken,TEvent}"/> and <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> classes that inherit the akka.net <see cref="ReceiveActor"/> via the <see cref="IDependencyResolver"/> so their message registration kicks in.
	/// </summary>
	public class AkkaBusRegistrar : BusRegistrar
	{
		public AkkaBusRegistrar(IDependencyResolver dependencyResolver)
			: base(dependencyResolver)
		{
		}

		#region Overrides of BusRegistrar

		protected override Action<dynamic> BuildDelegateAction(Type executorType)
		{
			Action<dynamic> del = base.BuildDelegateAction(executorType);

			// Instantiate and instance so it trigger the constructor with it's registrations
			DependencyResolver.Resolve(executorType);

			return del;
		}

		#endregion
	}
}