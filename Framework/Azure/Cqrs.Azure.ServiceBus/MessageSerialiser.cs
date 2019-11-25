#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
#if NET452
using System.Reflection;
#endif
using Cqrs.Commands;
using Cqrs.Events;
using Newtonsoft.Json;

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// Serialises <see cref="IEvent{TAuthenticationToken}">events</see> and <see cref="ICommand{TAuthenticationToken}">commands</see>.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
	public class MessageSerialiser<TAuthenticationToken> : IMessageSerialiser<TAuthenticationToken>
	{
		/// <summary>
		/// The default <see cref="JsonSerializerSettings"/> to use.
		/// </summary>
		public static JsonSerializerSettings DefaultSettings { get; private set; }

		static MessageSerialiser()
		{
#if NET452
			RedirectAssembly("System.Private.CoreLib", "mscorlib");
#endif
			DefaultSettings = DefaultJsonSerializerSettings.DefaultSettings;
		}

#if NET452
		/// <summary>
		/// Redirect an assembly resolution, used heavily for polumorphic serialisation and deserialisation such as between .NET Core and the .NET Framework
		/// </summary>
		/// <param name="fromAssemblyShortName">The name of the assembly to redirect.</param>
		/// <param name="replacmentAssemblyShortName">The name of the replacement assembly.</param>
		public static void RedirectAssembly(string fromAssemblyShortName, string replacmentAssemblyShortName)
		{
			Console.WriteLine($"Adding custom resolver redirect rule form:{fromAssemblyShortName}, to:{replacmentAssemblyShortName}");
			ResolveEventHandler handler = null;
			handler = (sender, args) =>
			{
				// Use latest strong name & version when trying to load SDK assemblies
				var requestedAssembly = new AssemblyName(args.Name);
				Console.WriteLine($"RedirectAssembly >  requesting:{requestedAssembly}; replacment from:{fromAssemblyShortName}, to:{replacmentAssemblyShortName}");
				if (requestedAssembly.Name != fromAssemblyShortName)
					return null;

				try
				{
					Console.WriteLine($"Redirecting Assembly {fromAssemblyShortName} to: {replacmentAssemblyShortName}");
					var replacmentAssembly = Assembly.Load(replacmentAssemblyShortName);
					return replacmentAssembly;
				}
				catch (Exception e)
				{
					Console.WriteLine($"ERROR while trying to provide replacement Assembly {fromAssemblyShortName} to: {replacmentAssemblyShortName}");
					Console.WriteLine(e);
					return null;
				}
			};

			AppDomain.CurrentDomain.AssemblyResolve += handler;
		}
#endif


		/// <summary>
		/// Serialise the provided <paramref name="event"/>.
		/// </summary>
		/// <typeparam name="TEvent">The <see cref="Type"/> of the <see cref="IEvent{TAuthenticationToken}"/> being serialised.</typeparam>
		/// <param name="event">The <see cref="IEvent{TAuthenticationToken}"/> being serialised.</param>
		/// <returns>A <see cref="string"/> representation of the provided <paramref name="event"/>.</returns>
		public virtual string SerialiseEvent<TEvent>(TEvent @event)
			where TEvent : IEvent<TAuthenticationToken>
		{
			return JsonConvert.SerializeObject(@event, GetSerialisationSettings());
		}

		/// <summary>
		/// Serialise the provided <paramref name="command"/>.
		/// </summary>
		/// <typeparam name="TCommand">The <see cref="Type"/> of the <see cref="ICommand{TAuthenticationToken}"/> being serialised.</typeparam>
		/// <param name="command">The <see cref="ICommand{TAuthenticationToken}"/> being serialised.</param>
		/// <returns>A <see cref="string"/> representation of the provided <paramref name="command"/>.</returns>
		public virtual string SerialiseCommand<TCommand>(TCommand command) where TCommand : ICommand<TAuthenticationToken>
		{
			return JsonConvert.SerializeObject(command, GetSerialisationSettings());
		}

		/// <summary>
		/// Deserialise the provided <paramref name="event"/> from its <see cref="string"/> representation.
		/// </summary>
		/// <param name="event">A <see cref="string"/> representation of an <see cref="IEvent{TAuthenticationToken}"/> to deserialise.</param>
		public virtual IEvent<TAuthenticationToken> DeserialiseEvent(string @event)
		{
			return JsonConvert.DeserializeObject<IEvent<TAuthenticationToken>>(@event, GetSerialisationSettings());
		}

		/// <summary>
		/// Deserialise the provided <paramref name="command"/> from its <see cref="string"/> representation.
		/// </summary>
		/// <param name="command">A <see cref="string"/> representation of an <see cref="ICommand{TAuthenticationToken}"/> to deserialise.</param>
		public virtual ICommand<TAuthenticationToken> DeserialiseCommand(string command)
		{
			return JsonConvert.DeserializeObject<ICommand<TAuthenticationToken>>(command, GetSerialisationSettings());
		}

		/// <summary>
		/// Returns <see cref="DefaultSettings"/>
		/// </summary>
		/// <returns><see cref="DefaultSettings"/></returns>
		protected virtual JsonSerializerSettings GetSerialisationSettings()
		{
			return DefaultSettings;
		}
	}
}