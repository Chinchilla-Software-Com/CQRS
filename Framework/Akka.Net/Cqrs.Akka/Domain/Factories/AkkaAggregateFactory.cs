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

		public override TAggregateRoot CreateAggregate<TAggregateRoot>(Guid? rsn = null, bool tryDependencyResolutionFirst = true)
		{
			if (rsn == null)
				rsn = Guid.NewGuid();

			try
			{
				var rawProxy = new AkkaAggregateRootProxy<TAuthenticationToken, TAggregateRoot>
				{
					ActorReference = AggregateResolver.Resolve<TAggregateRoot>(rsn.Value)
				};
				return rawProxy.Aggregate;
			}
			catch (Exception)
			{
				return base.CreateAggregate<TAggregateRoot>();
			}
		}
	}
}