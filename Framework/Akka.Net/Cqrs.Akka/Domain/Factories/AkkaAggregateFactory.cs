#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Configuration;
using Cqrs.Domain.Factories;

namespace Cqrs.Akka.Domain.Factories
{
	public class AkkaAggregateFactory<TAuthenticationToken> : AggregateFactory
	{
		protected IAkkaAggregateResolver AggregateResolver { get; private set; }

		public AkkaAggregateFactory(IDependencyResolver dependencyResolver, IAkkaAggregateResolver aggregateResolver)
			: base(dependencyResolver)
		{
			AggregateResolver = aggregateResolver;
		}

		public override TAggregateRoot CreateAggregate<TAggregateRoot>(Guid? rsn = null)
		{
			if (rsn == null)
				throw new ArgumentNullException("rsn");

			try
			{
				var akkaAggregateRootProxy = DependencyResolver.Resolve<IAkkaAggregateRootProxy<TAggregateRoot>>();
				var rawProxy = akkaAggregateRootProxy as AkkaAggregateRootProxy<TAuthenticationToken, TAggregateRoot>;
				if (rawProxy != null)
					rawProxy.ActorReference = AggregateResolver.Resolve<TAggregateRoot>(rsn.Value);
				return akkaAggregateRootProxy.Aggregate;
			}
			catch (Exception)
			{
				return base.CreateAggregate<TAggregateRoot>();
			}
		}
	}
}