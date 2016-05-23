#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using cdmdotnet.Logging;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Events;

namespace Cqrs.Configuration
{
	/// <summary>
	/// Triggers the <see cref="IEventHandlerRegistrar"/> and <see cref="ICommandHandlerRegistrar"/> if they are registered in the <see cref="IDependencyResolver"/>.
	/// </summary>
	public class BusRegistrar
	{
		protected IDependencyResolver DependencyResolver { get; private set; }

		public BusRegistrar(IDependencyResolver dependencyResolver)
		{
			if(dependencyResolver == null)
				throw new ArgumentNullException("dependencyResolver");

			DependencyResolver = dependencyResolver;
		}

		public virtual void Register(params Type[] typesFromAssemblyContainingMessages)
		{
			var eventHandlerRegistrar = DependencyResolver.Resolve<IEventHandlerRegistrar>();
			if (eventHandlerRegistrar != null)
				Register(eventHandlerRegistrar, ResolveEventHandlerInterface, true, typesFromAssemblyContainingMessages);

			var commandHandlerRegistrar = DependencyResolver.Resolve<ICommandHandlerRegistrar>();
			if (commandHandlerRegistrar != null)
				Register(commandHandlerRegistrar, ResolveCommandHandlerInterface, false, typesFromAssemblyContainingMessages);
		}

		public virtual void Register(IHandlerRegistrar handlerRegistrar, Func<Type, IEnumerable<Type>> resolveMessageHandlerInterface, bool skipCommandHandlers, params Type[] typesFromAssemblyContainingMessages)
		{
			foreach (Type typesFromAssemblyContainingMessage in typesFromAssemblyContainingMessages)
			{
				Assembly executorsAssembly = typesFromAssemblyContainingMessage.Assembly;
				var executorTypes = executorsAssembly
					.GetTypes()
					.Select(type => new { Type = type, Interfaces = resolveMessageHandlerInterface(type) })
					.Where(type => type.Interfaces != null && type.Interfaces.Any());

				foreach (var executorType in executorTypes)
				{
					foreach (Type @interface in executorType.Interfaces)
					{
						Type safeExecutorType = executorType.Type;
						if (typesFromAssemblyContainingMessage.IsGenericType && typesFromAssemblyContainingMessage.Name == typeof(DtoCommandHandler<,>).Name)
						{
							if (skipCommandHandlers)
								continue;
							Type[] genericArguments = typesFromAssemblyContainingMessage.GetGenericArguments().Take(2).ToArray();
							safeExecutorType = safeExecutorType.MakeGenericType(genericArguments.Take(2).ToArray());
						}
						InvokeHandler(@interface, handlerRegistrar, resolveMessageHandlerInterface, safeExecutorType);
					}
				}
			}
		}

		/// <summary>
		/// Extract the <see cref="IHandlerRegistrar.RegisterHandler{TMessage}"/> method from the provided <paramref name="bus"/>
		/// Create an <see cref="Action"/> around the provided <paramref name="executorType"/>
		/// Then register the created <see cref="Action"/> using the extracted <see cref="IHandlerRegistrar.RegisterHandler{TMessage}"/> method
		/// </summary>
		/// <param name="executorType">The <see cref="Type"/> of the event handler that will do the handling</param>
		protected virtual void InvokeHandler(Type @interface, IHandlerRegistrar bus, Func<Type, IEnumerable<Type>> resolveMessageHandlerInterface, Type executorType)
		{
			MethodInfo registerExecutorMethod = null;

			MethodInfo originalRegisterExecutorMethod = bus
				.GetType()
				.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Where(mi => mi.Name == "RegisterHandler")
				.Where(mi => mi.IsGenericMethod)
				.Where(mi => mi.GetGenericArguments().Count() == 1)
				.Single(mi => mi.GetParameters().Count() == 2);

			IList<Type> interfaceGenericArguments = @interface.GetGenericArguments().ToList();
			if (interfaceGenericArguments.Count == 2)
			{
				Type commandType = interfaceGenericArguments[1];
				registerExecutorMethod = BuildExecutorMethod(originalRegisterExecutorMethod, executorType, commandType);
			}
			else
			{
				foreach (Type commandType in interfaceGenericArguments)
				{
					try
					{
						registerExecutorMethod = BuildExecutorMethod(originalRegisterExecutorMethod, executorType, commandType);
						break;
					}
					catch (VerificationException)
					{
					}
					catch (ArgumentException)
					{
					}
				}
			}

			if (registerExecutorMethod == null)
				throw new InvalidOperationException("No executor method could be compiled for " + @interface.FullName);

			HandlerDelegate handlerDelegate = BuildDelegateAction(executorType, resolveMessageHandlerInterface);
			
			InvokeHandlerDelegate(registerExecutorMethod, bus, handlerDelegate);
		}

		protected virtual void InvokeHandlerDelegate(MethodInfo registerExecutorMethod, IHandlerRegistrar bus, HandlerDelegate handlerDelegate)
		{
			registerExecutorMethod.Invoke(bus, new object[] { handlerDelegate.Delegate, handlerDelegate.TargetedType });
		}

		protected virtual HandlerDelegate BuildDelegateAction(Type executorType, Func<Type, IEnumerable<Type>> resolveMessageHandlerInterface)
		{
			Action<dynamic> handlerDelegate = x =>
			{
				dynamic handler = DependencyResolver.Resolve(executorType);
				try
				{
					handler.Handle(x);
				}
				catch (NotImplementedException exception)
				{
					var logger = DependencyResolver.Resolve<ILogger>();
					logger.LogInfo(string.Format("An event message arrived of the type '{0}' went to a handler of type '{1}' but was not implemented.", x.GetType().FullName, handler.GetType().FullName), exception: exception);
				}
			};

			return new HandlerDelegate { Delegate = handlerDelegate, TargetedType = executorType };
		}

		protected virtual MethodInfo BuildExecutorMethod(MethodInfo originalRegisterExecutorMethod, Type executorType, Type commandType)
		{
			Type safeCommandType = commandType;
			if (safeCommandType.IsGenericType && safeCommandType.Name == typeof(DtoCommand<,>).Name && executorType.IsGenericType && executorType.Name == typeof(DtoCommandHandler<,>).Name)
			{
				Type[] genericArguments = executorType.GetGenericArguments().Take(2).ToArray();
				safeCommandType = typeof(DtoCommand<,>).MakeGenericType(genericArguments);
			}

			return originalRegisterExecutorMethod.MakeGenericMethod(safeCommandType);
		}

		protected virtual IEnumerable<Type> ResolveEventHandlerInterface(Type type)
		{
			return type
				.GetInterfaces()
				.Where
				(
					@interface =>
						@interface.IsGenericType &&
						(
							@interface.GetGenericTypeDefinition() == typeof(IEventHandler<,>)
						)
				);
		}

		protected virtual IEnumerable<Type> ResolveCommandHandlerInterface(Type type)
		{
			return type
				.GetInterfaces()
				.Where
				(
					@interface =>
						@interface.IsGenericType &&
						(
							@interface.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)
						)
				);
		}
	}
}