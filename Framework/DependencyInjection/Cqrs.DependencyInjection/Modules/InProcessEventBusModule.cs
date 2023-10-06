#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq;
using Cqrs.Bus;
using Cqrs.Events;
using Microsoft.Extensions.DependencyInjection;

namespace Cqrs.DependencyInjection.Modules
{
    /// <summary>
    /// A <see cref="Module"/> that configures the <see cref="InProcessBus{TAuthenticationToken}"/> as a <see cref="IEventPublisher{TAuthenticationToken}"/> and <see cref="IEventReceiver"/>.
    /// </summary>
    /// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
    public class InProcessEventBusModule<TAuthenticationToken> : ResolvableModule
    {
        #region Overrides of NinjectModule

        /// <summary>
        /// Loads the module into the kernel.
        /// </summary>
        public override void Load(IServiceCollection services)
        {
            RegisterFactories(services);
            RegisterServices(services);
            RegisterCqrsRequirements(services);
        }

        #endregion

        /// <summary>
        /// Register the all factories
        /// </summary>
        public virtual void RegisterFactories(IServiceCollection services)
        {
        }

        /// <summary>
        /// Register the all services
        /// </summary>
        public virtual void RegisterServices(IServiceCollection services)
        {
        }

        /// <summary>
        /// Register the <see cref="IEventPublisher{TAuthenticationToken}"/>, <see cref="IEventReceiver{TAuthenticationToken}"/> and <see cref="IEventHandlerRegistrar"/>.
        /// </summary>
        public virtual void RegisterCqrsRequirements(IServiceCollection services)
        {
            bool isInProcessBusBound = IsRegistered<InProcessBus<TAuthenticationToken>>(services);
            InProcessBus<TAuthenticationToken> inProcessBus;
            if (!isInProcessBusBound)
            {
                inProcessBus = Resolve<InProcessBus<TAuthenticationToken>>(services);
                services.AddSingleton(inProcessBus);
            }
            else
                inProcessBus = Resolve<InProcessBus<TAuthenticationToken>>(services);

            services.AddSingleton<IEventPublisher<TAuthenticationToken>>(inProcessBus);

            services.AddSingleton<IEventReceiver<TAuthenticationToken>>(inProcessBus);

            bool isHandlerRegistrationBound = IsRegistered<IEventHandlerRegistrar>(services);
            if (!isHandlerRegistrationBound)
            {
                services.AddSingleton<IEventHandlerRegistrar>(inProcessBus);
            }
        }
    }
}
