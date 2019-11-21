#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
#if NET452
using Microsoft.ServiceBus.Messaging;
#endif
#if NETCOREAPP3_0
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.EventHubs.Processor;
#endif

namespace Cqrs.Azure.ServiceBus
{
	/// <summary>
	/// Represents the factory for the default event processor.
	/// </summary>
	/// <typeparam name="TEventProcessor">The type of the event.</typeparam>
	internal class DefaultEventProcessorFactory<TEventProcessor>
		: IEventProcessorFactory
		where TEventProcessor : IEventProcessor
	{
		protected TEventProcessor Instance { get; set; }

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Microsoft.ServiceBus.Messaging.DefaultEventProcessorFactory`1"/> class.
		/// </summary>
		public DefaultEventProcessorFactory()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Microsoft.ServiceBus.Messaging.DefaultEventProcessorFactory`1"/> class using the specified instance.
		/// </summary>
		/// <param name="instance">The instance.</param>
		public DefaultEventProcessorFactory(TEventProcessor instance)
		{
			Instance = instance;
		}

		/// <summary>
		/// Creates an event processor.
		/// </summary>
		/// <param name="context">The partition context.</param>
		/// <returns>
		/// The created event processor.
		/// </returns>
		public IEventProcessor CreateEventProcessor(PartitionContext context)
		{
			if (Instance == null)
				return Activator.CreateInstance<TEventProcessor>();
			return Instance;
		}
	}
}