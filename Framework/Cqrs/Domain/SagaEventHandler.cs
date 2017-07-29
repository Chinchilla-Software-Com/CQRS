#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.Logging;
using Cqrs.Configuration;
using Cqrs.Events;

namespace Cqrs.Domain
{
	/// <summary>
	/// A process manager that you can implement <see cref="IEventHandler"/> instances on top of.
	/// </summary>
	/// <typeparam name="TAuthenticationToken">The <see cref="Type"/> of authentication token.</typeparam>
	/// <typeparam name="TSaga">The <see cref="Type"/> of <see cref="ISaga{TAuthenticationToken}"/>.</typeparam>
	public abstract class SagaEventHandler<TAuthenticationToken, TSaga>
		where TSaga : ISaga<TAuthenticationToken>
	{
		/// <summary>
		/// Gets or set the <see cref="ISagaUnitOfWork{TAuthenticationToken}"/>.
		/// </summary>
		protected ISagaUnitOfWork<TAuthenticationToken> SagaUnitOfWork { get; private set; }

		/// <summary>
		/// Gets or set the <see cref="IDependencyResolver"/>.
		/// </summary>
		protected IDependencyResolver DependencyResolver { get; private set; }

		/// <summary>
		/// Gets or set the <see cref="ILogger"/>.
		/// </summary>
		protected ILogger Logger { get; private set; }

		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/>
		/// </summary>
		protected SagaEventHandler(IDependencyResolver dependencyResolver, ILogger logger)
			: this(dependencyResolver, logger, dependencyResolver.Resolve<ISagaUnitOfWork<TAuthenticationToken>>())
		{
		}

		/// <summary>
		/// Instantiate a new instance of <see cref="SagaEventHandler{TAuthenticationToken,TSaga}"/>
		/// </summary>
		protected SagaEventHandler(IDependencyResolver dependencyResolver, ILogger logger, ISagaUnitOfWork<TAuthenticationToken> sagaUnitOfWork)
		{
			DependencyResolver = dependencyResolver;
			Logger = logger;
			SagaUnitOfWork = sagaUnitOfWork;
		}

		/// <summary>
		/// Gets the <typeparamref name="TSaga"/> from the <see cref="SagaUnitOfWork"/>.
		/// </summary>
		/// <param name="id">The identifier of the <typeparamref name="TSaga"/> to get.</param>
		protected virtual TSaga GetSaga(Guid id)
		{
			return SagaUnitOfWork.Get<TSaga>(id);
		}
	}
}