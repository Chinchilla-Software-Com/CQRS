#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Configuration;

namespace Cqrs.Repositories.Queries
{
	/// <summary>
	/// A factory to create new instances of <see cref="ResultQuery{TQueryStrategy,TData}"/>.
	/// </summary>
	public class QueryFactory : IQueryFactory
	{
		/// <summary>
		/// Gets or sets the <see cref="IDependencyResolver"/>.
		/// </summary>
		protected IDependencyResolver DependencyResolver { get; private set; }

		/// <summary>
		/// Instantiates a new instance of <see cref="QueryFactory"/>
		/// </summary>
		public QueryFactory(IDependencyResolver dependencyResolver)
		{
			DependencyResolver = dependencyResolver;
		}

		/// <summary>
		/// Creates a new <see cref="ISingleResultQuery{TQueryStrategy,TData}"/>
		/// using <see cref="DependencyResolver"/> to create a new <typeparamref name="TUserQueryStrategy"/>.
		/// </summary>
		/// <typeparam name="TUserQueryStrategy">The <see cref="Type"/> of <see cref="IQueryStrategy"/> it will use.</typeparam>
		/// <typeparam name="TData">The <see cref="Type"/> of data the <see cref="ISingleResultQuery{TQueryStrategy,TData}"/> will operate on.</typeparam>
		public ISingleResultQuery<TUserQueryStrategy, TData> CreateNewSingleResultQuery<TUserQueryStrategy, TData>()
			where TUserQueryStrategy : IQueryStrategy
		{
			var queryStrategy = DependencyResolver.Resolve<TUserQueryStrategy>();
			return new SingleResultQuery<TUserQueryStrategy, TData>
			{
				QueryStrategy = queryStrategy
			};
		}

		/// <summary>
		/// Creates a new <see cref="ICollectionResultQuery{TQueryStrategy,TData}"/>
		/// using <see cref="DependencyResolver"/> to create a new <typeparamref name="TUserQueryStrategy"/>.
		/// </summary>
		/// <typeparam name="TUserQueryStrategy">The <see cref="Type"/> of <see cref="IQueryStrategy"/> it will use.</typeparam>
		/// <typeparam name="TData">The <see cref="Type"/> of data the <see cref="ISingleResultQuery{TQueryStrategy,TData}"/> will operate on.</typeparam>
		public ICollectionResultQuery<TUserQueryStrategy, TData> CreateNewCollectionResultQuery<TUserQueryStrategy, TData>()
			where TUserQueryStrategy : IQueryStrategy
		{
			var queryStrategy = DependencyResolver.Resolve<TUserQueryStrategy>();
			return new CollectionResultQuery<TUserQueryStrategy, TData>
			{
				QueryStrategy = queryStrategy
			};
		}
	}
}