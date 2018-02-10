#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.Logging;
using Cqrs.Commands;
using Cqrs.Events;

namespace Cqrs.Configuration
{
	/// <summary>
	/// A <see cref="SampleRuntime{TAuthenticationToken,TCommandHanderOrEventHandler}"/> that uses SQL Server.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	/// <typeparam name="TCommandHanderOrEventHandler">The <see cref="Type"/> of any <see cref="ICommandHandle"/> or <see cref="IEventHandler"/>.</typeparam>
	public class SqlSampleRuntime<TAuthenticationToken, TCommandHanderOrEventHandler>
		: SampleRuntime<TAuthenticationToken, TCommandHanderOrEventHandler>
	{
		#region Overrides of SampleRuntime<TAuthenticationToken,TCommandHanderOrEventHandler>

		/// <summary>
		/// Sets the <see cref="SampleRuntime{TAuthenticationToken,TCommandHanderOrEventHandler}.EventStoreCreator"/> to use <see cref="InProcessEventStore{TAuthenticationToken}"/>
		/// </summary>
		protected override void SetEventStoreCreator()
		{
			EventStoreCreator = dependencyResolver => new SqlEventStore<TAuthenticationToken>(dependencyResolver.Resolve<IEventBuilder<TAuthenticationToken>>(), dependencyResolver.Resolve<IEventDeserialiser<TAuthenticationToken>>(), dependencyResolver.Resolve<ILogger>(), dependencyResolver.Resolve<IConfigurationManager>());
		}

		#endregion
	}
}