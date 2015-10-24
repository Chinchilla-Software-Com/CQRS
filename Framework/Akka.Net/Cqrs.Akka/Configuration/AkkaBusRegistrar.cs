#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;
using Akka.Actor;
using cdmdotnet.Logging;
using Cqrs.Akka.Events;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;

namespace Cqrs.Akka.Configuration
{
	/// <summary>
	/// Triggers the <see cref="BusRegistrar"/> instantiates instances of <see cref="IEventHandler{TAuthenticationToken, TEvent}"/> and <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> classes that inherit the akka.net <see cref="ReceiveActor"/> via the <see cref="IDependencyResolver"/> so their message registration kicks in.
	/// </summary>
	public class AkkaBusRegistrar<TAuthenticationToken> : BusRegistrar
	{
		protected IHandlerResolver HandlerResolver { get; private set; }

		public AkkaBusRegistrar(IDependencyResolver dependencyResolver, IHandlerResolver handlerResolver)
			: base(dependencyResolver)
		{
			HandlerResolver = handlerResolver;
		}

		#region Overrides of BusRegistrar

		protected override HandlerDelegate BuildDelegateAction(Type executorType)
		{
			Type targetedType = executorType;
			if (executorType.GenericTypeArguments.Length > 2)
				targetedType = executorType.GenericTypeArguments[1];

			Action<dynamic> handlerDelegate = x =>
			{
				dynamic handler;
				try
				{
					Type messageType = ((object)x).GetType();
					object rsn = messageType.GetProperty("Rsn").GetValue(x, null);
					handler = HandlerResolver.Resolve(executorType, executorType, rsn);
				}
				catch (Exception)
				{
					handler = DependencyResolver.Resolve(executorType);
				}
				IActorRef actorReference = handler as IActorRef;
				try
				{
					if (actorReference != null)
						actorReference.Tell((object)x);
					else
						handler.Handle(x);
				}
				catch (NotImplementedException exception)
				{
					var logger = DependencyResolver.Resolve<ILogger>();
					logger.LogInfo(string.Format("An event message arrived of the type '{0}' went to a handler of type '{1}' but was not implemented.", x.GetType().FullName, handler.GetType().FullName), exception: exception);
				}
			};

			// Instantiate an instance so it triggers the constructor with it's registrations
			DependencyResolver.Resolve(executorType);

			return new HandlerDelegate { Delegate = handlerDelegate, TargetedType = targetedType };
		}

		protected override void InvokeHandlerDelegate(MethodInfo registerExecutorMethod, IHandlerRegistrar bus, HandlerDelegate handlerDelegate)
		{
			base.InvokeHandlerDelegate(registerExecutorMethod, bus, handlerDelegate);
			registerExecutorMethod.Invoke(DependencyResolver.Resolve<IAkkaEventBus<TAuthenticationToken>>(), new object[] { handlerDelegate.Delegate, handlerDelegate.TargetedType });
		}

		#endregion
	}
}