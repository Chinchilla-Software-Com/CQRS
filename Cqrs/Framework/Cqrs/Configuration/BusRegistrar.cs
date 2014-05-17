using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Security;
using Cqrs.Bus;
using Cqrs.Commands;
using Cqrs.Events;

namespace Cqrs.Configuration
{
	public class BusRegistrar
	{
		private IServiceLocator ServiceLocator { get; set; }

		public BusRegistrar(IServiceLocator serviceLocator)
		{
			if(serviceLocator == null)
				throw new ArgumentNullException("serviceLocator");

			ServiceLocator = serviceLocator;
		}

		public void Register(params Type[] typesFromAssemblyContainingMessages)
		{
			var bus = ServiceLocator.GetService<IHandlerRegistrar>();
			
			foreach (var typesFromAssemblyContainingMessage in typesFromAssemblyContainingMessages)
			{
				Assembly executorsAssembly = typesFromAssemblyContainingMessage.Assembly;
				var executorTypes = executorsAssembly
					.GetTypes()
					.Select(type => new { Type = type, Interfaces = ResolveMessageHandlerInterface(type) })
					.Where(type => type.Interfaces != null && type.Interfaces.Any());

				foreach (var executorType in executorTypes)
					foreach (Type @interface in executorType.Interfaces)
					{
						Type safeExecutorType = executorType.Type;
						if (typesFromAssemblyContainingMessage.IsGenericType && typesFromAssemblyContainingMessage.Name == typeof(DtoCommandHandler<,>).Name)
						{
							Type genericType = typesFromAssemblyContainingMessage.GetGenericArguments().Single();
							safeExecutorType = safeExecutorType.MakeGenericType(genericType);
						}
						InvokeHandler(@interface, bus, safeExecutorType);
					}
			}
		}

		/// <summary>
		/// Extract the <see cref="IHandlerRegistrar.RegisterHandler{TMessage}"/> method from the provided <paramref name="bus"/>
		/// Created an <see cref="Action"/> around the provided <paramref name="executorType"/>
		/// Then register the created <see cref="Action"/> using the extracted <see cref="IHandlerRegistrar.RegisterHandler{TMessage}"/> method
		/// </summary>
		private void InvokeHandler(Type @interface, IHandlerRegistrar bus, Type executorType)
		{
			MethodInfo registerExecutorMethod = null;

			MethodInfo originalRegisterExecutorMethod = bus
				.GetType()
				.GetMethods(BindingFlags.Instance | BindingFlags.Public)
				.Where(mi => mi.Name == "RegisterHandler")
				.Where(mi => mi.IsGenericMethod)
				.Where(mi => mi.GetGenericArguments().Count() == 1)
				.Single(mi => mi.GetParameters().Count() == 1);

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

			var del = new Action<dynamic>(x =>
				{
					dynamic handler = ServiceLocator.GetService(executorType);
					handler.Handle(x);
				}
			);
			
			registerExecutorMethod.Invoke(bus, new object[] { del });
		}

		private MethodInfo BuildExecutorMethod(MethodInfo originalRegisterExecutorMethod, Type executorType, Type commandType)
		{
			Type safeCommandType = commandType;
			if (safeCommandType.IsGenericType && safeCommandType.Name == typeof(DtoCommand<,>).Name && executorType.IsGenericType && executorType.Name == typeof(DtoCommandHandler<,>).Name)
			{
				Type genericType = executorType.GetGenericArguments().Single();
				safeCommandType = typeof(DtoCommand<,>).MakeGenericType(genericType);
			}

			return originalRegisterExecutorMethod.MakeGenericMethod(safeCommandType);
		}

		private IEnumerable<Type> ResolveMessageHandlerInterface(Type type)
		{
			return type
				.GetInterfaces()
				.Where
				(
					@interface =>
						@interface.IsGenericType &&
						(
							@interface.GetGenericTypeDefinition() == typeof(ICommandHandler<,>)
								||
							@interface.GetGenericTypeDefinition() == typeof(IEventHandler<,>)
						)
				);
		}

	}
}
