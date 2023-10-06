#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Events;
using Cqrs.Snapshots;
using Microsoft.Extensions.DependencyInjection;

namespace Cqrs.DependencyInjection.Modules
{
    /// <summary>
    /// The <see cref="Module"/> to wireup <see cref="IEvent{TAuthenticationToken}"/> to <see cref="SqlEventStore{TAuthenticationToken}"/>.
    /// </summary>
    /// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
    public class SimplifiedSqlModule<TAuthenticationToken> : ResolvableModule
    {
        #region Overrides of NinjectModule

        /// <summary>
        /// Loads the module into the kernel.
        /// </summary>
        public override void Load(IServiceCollection services)
        {
            RegisterEventSerialisationConfiguration(services);
            RegisterEventStore(services);
        }

        #endregion

        /// <summary>
        /// Register the all event serialisation configurations
        /// </summary>
        public virtual void RegisterEventSerialisationConfiguration(IServiceCollection services)
        {
            services.AddSingleton<IEventBuilder<TAuthenticationToken>, DefaultEventBuilder<TAuthenticationToken>>();
            services.AddSingleton<IEventDeserialiser<TAuthenticationToken>, EventDeserialiser<TAuthenticationToken>>();
            services.AddSingleton<ISnapshotDeserialiser, SnapshotDeserialiser>();
        }

        /// <summary>
        /// Register the <see cref="IEventStore{TAuthenticationToken}"/>
        /// </summary>
        public virtual void RegisterEventStore(IServiceCollection services)
        {
            services.AddSingleton<IEventStore<TAuthenticationToken>, SqlEventStore<TAuthenticationToken>>();
            services.AddSingleton<ISnapshotStore, SqlSnapshotStore>();
        }
    }
}