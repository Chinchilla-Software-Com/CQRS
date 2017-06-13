#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.Logging;
using Cqrs.Configuration;

namespace Cqrs.Domain
{
	public abstract class SagaEventHandler<TAuthenticationToken, TSaga>
		where TSaga : ISaga<TAuthenticationToken>
	{
		protected ISagaUnitOfWork<TAuthenticationToken> SagaUnitOfWork { get; private set; }

		protected IDependencyResolver DependencyResolver { get; private set; }

		protected ILogger Logger { get; private set; }

		/// <summary>
		/// A constructor for the <see cref="Cqrs.Domain.Factories.IAggregateFactory"/>
		/// </summary>
		protected SagaEventHandler(IDependencyResolver dependencyResolver, ILogger logger)
			: this(dependencyResolver, logger, dependencyResolver.Resolve<ISagaUnitOfWork<TAuthenticationToken>>())
		{
		}

		protected SagaEventHandler(IDependencyResolver dependencyResolver, ILogger logger, ISagaUnitOfWork<TAuthenticationToken> sagaUnitOfWork)
		{
			DependencyResolver = dependencyResolver;
			Logger = logger;
			SagaUnitOfWork = sagaUnitOfWork;
		}

		protected virtual TSaga GetSaga(Guid id)
		{
			return SagaUnitOfWork.Get<TSaga>(id);
		}
	}
}