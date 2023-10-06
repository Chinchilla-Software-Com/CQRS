#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Bus;
using Cqrs.Commands;
using Microsoft.Extensions.DependencyInjection;

namespace Cqrs.DependencyInjection.Modules
{
    /// <summary>
    /// A <see cref="Module"/> that configures the <see cref="InProcessBus{TAuthenticationToken}"/> as a <see cref="ICommandPublisher{TAuthenticationToken}"/> and <see cref="ICommandReceiver"/>.
    /// </summary>
    /// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of the authentication token.</typeparam>
    public class InProcessCommandBusModule<TAuthenticationToken> : ResolvableModule
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

#pragma warning disable 618
        /// <summary>
        /// Register the <see cref="ICommandPublisher{TAuthenticationToken}"/>, <see cref="IPublishAndWaitCommandPublisher{TAuthenticationToken}"/>, <see cref="ICommandReceiver{TAuthenticationToken}"/> and <see cref="ICommandHandlerRegistrar"/>
        /// Register (for backwards compatibility) <see cref="ICommandPublisher{TAuthenticationToken}"/>
        /// </summary>
#pragma warning restore 618
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

            services.AddSingleton<ICommandPublisher<TAuthenticationToken>>(inProcessBus);

            services.AddSingleton<IPublishAndWaitCommandPublisher<TAuthenticationToken>>(inProcessBus);

            services.AddSingleton<ICommandReceiver<TAuthenticationToken>>(inProcessBus);

            bool isHandlerRegistrationBound = IsRegistered<ICommandHandlerRegistrar>(services);
            if (!isHandlerRegistrationBound)
            {
                services.AddSingleton<ICommandHandlerRegistrar>(inProcessBus);
            }
        }
    }
}
