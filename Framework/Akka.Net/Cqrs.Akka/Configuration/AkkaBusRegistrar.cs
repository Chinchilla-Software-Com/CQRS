#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Akka.Actor;
using cdmdotnet.Logging;
using Cqrs.Akka.Events;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Configuration;
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.Akka.Configuration
{
	/// <summary>
	/// Triggers the <see cref="BusRegistrar"/> instantiates instances of <see cref="IEventHandler{TAuthenticationToken, TEvent}"/> and <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> classes that inherit the akka.net <see cref="ReceiveActor"/> via the <see cref="IDependencyResolver"/> so their message registration kicks in.
	/// </summary>
	public class AkkaBusRegistrar<TAuthenticationToken> : BusRegistrar
	{
		/// <summary>
		/// Gets or sets the <see cref="IHandlerResolver"/>.
		/// </summary>
		protected IHandlerResolver HandlerResolver { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="AkkaBusRegistrar{TAuthenticationToken}"/>.
		/// </summary>
		public AkkaBusRegistrar(IDependencyResolver dependencyResolver, IHandlerResolver handlerResolver)
			: base(dependencyResolver)
		{
			HandlerResolver = handlerResolver;
		}

		#region Overrides of BusRegistrar

		/// <summary>
		/// Builds a <see cref="HandlerDelegate"/> that will resolve the provided <paramref name="executorType"/> and invoke the Handle method, when executed.
		/// </summary>
		/// <param name="executorType">The type of <see cref="IHandler"/> to resolve.></param>
		/// <param name="resolveMessageHandlerInterface">Not used.</param>
		protected override HandlerDelegate BuildDelegateAction(Type executorType, Func<Type, IEnumerable<Type>> resolveMessageHandlerInterface)
		{
			Type targetedType = executorType;
			Type @interface = resolveMessageHandlerInterface(executorType).FirstOrDefault();
			if (@interface != null && @interface.GenericTypeArguments.Length > 2)
				targetedType = executorType.GenericTypeArguments[1];

			Action<dynamic> handlerDelegate = x =>
			{
				dynamic handler;
				try
				{
					Type messageType = ((object)x).GetType();
					object rsn = messageType.GetProperty("Rsn").GetValue(x, null);
					handler = HandlerResolver.Resolve(executorType, rsn);
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

		/// <summary>
		/// Invokes <paramref name="handlerDelegate"/>, fetching the corresponding "HoldMessageLock" configuration setting 
		/// </summary>
		/// <param name="registerExecutorMethod">The <see cref="MethodInfo"/> to use to invoke <paramref name="handlerDelegate"/>.</param>
		/// <param name="trueForEventsFalseForCommands">Indicates if this is registers <see cref="IEventHandler"/> or <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>.</param>
		/// <param name="handlerDelegate">The actual <see cref="HandlerDelegate"/> that gets executed.</param>
		protected override void InvokeHandlerDelegate(MethodInfo registerExecutorMethod, bool trueForEventsFalseForCommands, HandlerDelegate handlerDelegate)
		{
			base.InvokeHandlerDelegate(registerExecutorMethod, trueForEventsFalseForCommands, handlerDelegate);
			registerExecutorMethod.Invoke(DependencyResolver.Resolve<IAkkaEventPublisher<TAuthenticationToken>>(), new object[] { handlerDelegate.Delegate, handlerDelegate.TargetedType });
		}

		#endregion
	}
}