#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cqrs.DependencyInjection.Modules
{
    /// <summary>
    /// The <see cref="Module"/> for use with the Cqrs package.
    /// </summary>
    /// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
    public class MemoryCacheEventStoreModule<TAuthenticationToken> : ResolvableModule
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
        }

        /// <summary>
        /// Register the <see cref="IEventStore{TAuthenticationToken}"/>
        /// </summary>
        public virtual void RegisterEventStore(IServiceCollection services)
        {
            services.AddSingleton<IEventStore<TAuthenticationToken>, MemoryCacheEventStore<TAuthenticationToken>>();
        }
    }
}