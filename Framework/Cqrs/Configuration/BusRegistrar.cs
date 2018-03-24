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
using System.Security;
using cdmdotnet.Logging;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Events;
using Cqrs.Messages;

namespace Cqrs.Configuration
{
	/// <summary>
	/// Triggers the <see cref="IEventHandlerRegistrar"/> and <see cref="ICommandHandlerRegistrar"/> if they are registered in the <see cref="IDependencyResolver"/>.
	/// </summary>
	public class BusRegistrar
	{
		/// <summary>
		/// Gets or set the <see cref="IDependencyResolver"/>.
		/// </summary>
		protected IDependencyResolver DependencyResolver { get; private set; }

		/// <summary>
		/// A <see cref="Func{Type, Type, THandlerRegistrar}"/> to use in-place of <see cref="IEventHandlerRegistrar"/>
		/// </summary>
		public static Func<Type, Type, IHandlerRegistrar> GetEventHandlerRegistrar { get; set; }

		/// <summary>
		/// A <see cref="Func{Type, Type, THandlerRegistrar}"/> to use in-place of <see cref="ICommandHandlerRegistrar"/>
		/// </summary>
		public static Func<Type, Type, IHandlerRegistrar> GetCommandHandlerRegistrar { get; set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="BusRegistrar"/>.
		/// </summary>
		public BusRegistrar(IDependencyResolver dependencyResolver)
		{
			if(dependencyResolver == null)
				throw new ArgumentNullException("dependencyResolver");

			DependencyResolver = dependencyResolver;
			if (GetEventHandlerRegistrar == null)
				GetEventHandlerRegistrar = (messageType, handlerDelegateTargetedType) => DependencyResolver.Resolve<IEventHandlerRegistrar>();
			if (GetCommandHandlerRegistrar == null)
				GetCommandHandlerRegistrar = (messageType, handlerDelegateTargetedType) => DependencyResolver.Resolve<ICommandHandlerRegistrar>();
		}

		/// <summary>
		/// Registers all <see cref="IEventHandler"/> and <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> instances found in the <see cref="Assembly"/> for each <see cref="Type"/> in <paramref name="typesFromAssemblyContainingMessages"/>.
		/// </summary>
		/// <param name="typesFromAssemblyContainingMessages">A collection of <see cref="Type"/> to track back to their containing <see cref="Assembly"/> and scan.</param>
		public virtual void Register(params Type[] typesFromAssemblyContainingMessages)
		{
			var eventHandlerRegistrar = DependencyResolver.Resolve<IEventHandlerRegistrar>();
			if (eventHandlerRegistrar != null)
				Register(true, ResolveEventHandlerInterface, true, typesFromAssemblyContainingMessages);

			var commandHandlerRegistrar = DependencyResolver.Resolve<ICommandHandlerRegistrar>();
			if (commandHandlerRegistrar != null)
				Register(false, ResolveCommandHandlerInterface, false, typesFromAssemblyContainingMessages);
		}

		/// <summary>
		/// Registers all <see cref="IHandler"/> instances found in the <see cref="Assembly"/> for each <see cref="Type"/> in <paramref name="typesFromAssemblyContainingMessages"/>.
		/// </summary>
		/// <param name="trueForEventsFalseForCommands">Indicates if this is registers <see cref="IEventHandler"/> or <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>.</param>
		/// <param name="resolveMessageHandlerInterface"><see cref="ResolveEventHandlerInterface"/> or <see cref="ResolveCommandHandlerInterface"/></param>
		/// <param name="skipCommandHandlers">Indicates if registering of <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> is enabled.</param>
		/// <param name="typesFromAssemblyContainingMessages">A collection of <see cref="Type"/> to track back to their containing <see cref="Assembly"/> and scan.</param>
		public virtual void Register(bool trueForEventsFalseForCommands, Func<Type, IEnumerable<Type>> resolveMessageHandlerInterface, bool skipCommandHandlers, params Type[] typesFromAssemblyContainingMessages)
		{
			foreach (Type typesFromAssemblyContainingMessage in typesFromAssemblyContainingMessages)
			{
				Assembly executorsAssembly = typesFromAssemblyContainingMessage.Assembly;
				HandlerTypeInformation[] executorTypes = executorsAssembly
					.GetTypes()
					.Select(type => new HandlerTypeInformation { Type = type, Interfaces = resolveMessageHandlerInterface(type) })
					.Where(type => type.Interfaces != null && type.Interfaces.Any())
					.ToArray();

				Register(trueForEventsFalseForCommands, resolveMessageHandlerInterface, skipCommandHandlers, executorTypes);
			}
		}

		/// <summary>
		/// Information explaining the registration to make
		/// </summary>
		public class HandlerTypeInformation
		{
			/// <summary>
			/// The <see cref="Type"/> of the hanlder to register
			/// </summary>
			public Type Type { get; set; }

			/// <summary>
			/// The Handler type to resolve to, so either <see cref="IEventHandler{TAuthenticationToken,TEvent}"/> or <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>
			/// </summary>
			public IEnumerable<Type> Interfaces { get; set; }
		}

		/// <summary>
		/// Registers all <see cref="IHandler"/> instances in the provided <paramref name="executorTypes"/>.
		/// </summary>
		/// <param name="trueForEventsFalseForCommands">Indicates if this is registers <see cref="IEventHandler"/> or <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>.</param>
		/// <param name="resolveMessageHandlerInterface"><see cref="ResolveEventHandlerInterface"/> or <see cref="ResolveCommandHandlerInterface"/></param>
		/// <param name="skipCommandHandlers">Indicates if registering of <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> is enabled.</param>
		/// <param name="executorTypes">A collection of <see cref="Type"/> to register.</param>
		public virtual void Register(bool trueForEventsFalseForCommands, Func<Type, IEnumerable<Type>> resolveMessageHandlerInterface, bool skipCommandHandlers, params HandlerTypeInformation[] executorTypes)
		{
			foreach (var executorType in executorTypes)
			{
				foreach (Type @interface in executorType.Interfaces)
				{
					Type safeExecutorType = executorType.Type;
					if (safeExecutorType.IsGenericType && safeExecutorType.Name == typeof(DtoCommandHandler<,>).Name)
					{
						if (skipCommandHandlers)
							continue;
						Type[] genericArguments = safeExecutorType.GetGenericArguments().Take(2).ToArray();
						safeExecutorType = safeExecutorType.MakeGenericType(genericArguments.Take(2).ToArray());
					}
					InvokeHandler(@interface, trueForEventsFalseForCommands, resolveMessageHandlerInterface, safeExecutorType);
				}
			}
		}

		/// <summary>
		/// Extract the <see cref="IHandlerRegistrar.RegisterHandler{TMessage}(System.Action{TMessage},System.Type,bool)"/> method of <see cref="GetEventHandlerRegistrar"/> or <see cref="GetCommandHandlerRegistrar"/>.
		/// Create an <see cref="Action"/> around the provided <paramref name="executorType"/>
		/// Then register the created <see cref="Action"/> using the extracted <see cref="IHandlerRegistrar.RegisterHandler{TMessage}(System.Action{TMessage},System.Type,bool)"/> method
		/// </summary>
		/// <param name="interface">The <see cref="Type"/> of <see cref="IHandler"/></param>
		/// <param name="trueForEventsFalseForCommands">Indicates if this is registers <see cref="IEventHandler"/> or <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>.</param>
		/// <param name="resolveMessageHandlerInterface"><see cref="ResolveEventHandlerInterface"/> or <see cref="ResolveCommandHandlerInterface"/></param>
		/// <param name="executorType">The <see cref="Type"/> of the event handler that will do the handling</param>
		protected virtual void InvokeHandler(Type @interface, bool trueForEventsFalseForCommands, Func<Type, IEnumerable<Type>> resolveMessageHandlerInterface, Type executorType)
		{
			MethodInfo registerExecutorMethod = null;

			MethodInfo originalRegisterExecutorMethod = (trueForEventsFalseForCommands ? GetEventHandlerRegistrar(null, executorType) : GetCommandHandlerRegistrar(null, executorType))
				.GetType()
				.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Where(mi => mi.Name == "RegisterHandler")
				.Where(mi => mi.IsGenericMethod)
				.Where(mi => mi.GetGenericArguments().Count() == 1)
				.Single(mi => mi.GetParameters().Count() == 3);

			IList<Type> interfaceGenericArguments = @interface.GetGenericArguments().ToList();
			if (interfaceGenericArguments.Count == 2)
			{
				Type commandType = interfaceGenericArguments[1];
				Type[] genericArguments = commandType.GetGenericArguments();
				if (genericArguments.Length == 1)
					if (typeof (IEvent<>).MakeGenericType(genericArguments.First()) == commandType)
						return;

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

			InvokeHandlerDelegate(registerExecutorMethod, trueForEventsFalseForCommands, handlerDelegate);
		}

		/// <summary>
		/// Invokes <paramref name="handlerDelegate"/>, fetching the corresponding "HoldMessageLock" configuration setting 
		/// </summary>
		/// <param name="registerExecutorMethod">The <see cref="MethodInfo"/> to use to invoke <paramref name="handlerDelegate"/>.</param>
		/// <param name="trueForEventsFalseForCommands">Indicates if this is registers <see cref="IEventHandler"/> or <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/>.</param>
		/// <param name="handlerDelegate">The actual <see cref="HandlerDelegate"/> that gets executed.</param>
		protected virtual void InvokeHandlerDelegate(MethodInfo registerExecutorMethod, bool trueForEventsFalseForCommands, HandlerDelegate handlerDelegate)
		{
			Type messageType = null;
			bool holdMessageLock;
			try
			{
				messageType = registerExecutorMethod.GetParameters()[0].ParameterType.GetGenericArguments()[0];
				string messageTypeName = messageType.FullName;
				string configuration = string.Format("{0}<{1}>.HoldMessageLock", handlerDelegate.TargetedType.FullName, messageTypeName);
				// If this cannot be parsed then assume the safe route that this must be required to hold the lock.
				if (!bool.TryParse(DependencyResolver.Resolve<IConfigurationManager>().GetSetting(configuration), out holdMessageLock))
					holdMessageLock = true;
			}
			catch
			{
				holdMessageLock = true;
			}
			registerExecutorMethod.Invoke(trueForEventsFalseForCommands ? GetEventHandlerRegistrar(messageType, handlerDelegate.TargetedType) : GetCommandHandlerRegistrar(messageType, handlerDelegate.TargetedType), new object[] { handlerDelegate.Delegate, handlerDelegate.TargetedType, holdMessageLock });
		}

		/// <summary>
		/// Builds a <see cref="HandlerDelegate"/> that will resolve the provided <paramref name="executorType"/> and invoke the Handle method, when executed.
		/// </summary>
		/// <param name="executorType">The type of <see cref="IHandler"/> to resolve.></param>
		/// <param name="resolveMessageHandlerInterface">Not used.</param>
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

		/// <summary>
		/// Builds a method replacing the generic type with <paramref name="commandType"/>.
		/// </summary>
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

		/// <summary>
		/// Finds all <see cref="Type"/> that implement <see cref="IEventHandler{TAuthenticationToken,TEvent}"/> that are implemented by <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> to find all <see cref="IEventHandler{TAuthenticationToken,TEvent}"/> of.</param>
		public virtual IEnumerable<Type> ResolveEventHandlerInterface(Type type)
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

		/// <summary>
		/// Finds all <see cref="Type"/> that implement <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> that are implemented by <paramref name="type"/>.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> to find all <see cref="ICommandHandler{TAuthenticationToken,TCommand}"/> of.</param>
		public virtual IEnumerable<Type> ResolveCommandHandlerInterface(Type type)
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