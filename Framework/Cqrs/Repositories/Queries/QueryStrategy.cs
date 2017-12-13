#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cqrs.Repositories.Queries
{
	/// <summary>
	/// A specification for a query to execute.
	/// </summary>
	public abstract class QueryStrategy : IQueryStrategy
	{
		#region Implementation of IQueryStrategy

		/// <summary>
		/// The predicate that will be evaluated.
		/// </summary>
		public IQueryPredicate QueryPredicate { get; protected set; }

		#endregion

		/// <summary>
		/// Filter to all items not logically deleted.
		/// </summary>
		public virtual IQueryPredicate IsNotLogicallyDeleted()
		{
			return BuildQueryPredicate(IsNotLogicallyDeleted);
		}

		/// <summary>
		/// Filter to all items with any permission scope.
		/// </summary>
		public virtual IQueryPredicate WithPermissionScopeAny<TAuthenticationToken>(TAuthenticationToken authenticationToken)
		{
			return BuildQueryPredicate(WithPermissionScopeAny, authenticationToken);
		}

		/// <summary>
		/// Filter to any items the authenticated user can view.
		/// </summary>
		public virtual IQueryPredicate WithPermissionScopeUser<TAuthenticationToken>(TAuthenticationToken authenticationToken)
		{
			return BuildQueryPredicate(WithPermissionScopeUser, authenticationToken);
		}

		/// <summary>
		/// Filter to any items the company the authenticated user can view.
		/// </summary>
		public virtual IQueryPredicate WithPermissionScopeCompany<TAuthenticationToken>(TAuthenticationToken authenticationToken)
		{
			return BuildQueryPredicate(WithPermissionScopeCompany, authenticationToken);
		}

		/// <summary>
		/// Filter to any items the company the authenticated user can view and then filter the results to any items the authenticated user can see.
		/// </summary>
		public virtual IQueryPredicate WithPermissionScopeCompanyAndUser<TAuthenticationToken>(TAuthenticationToken authenticationToken)
		{
			return BuildQueryPredicate(WithPermissionScopeCompanyAndUser, authenticationToken);
		}

		/// <summary>
		/// Builds a <see cref="IQueryPredicate"/> from the provided <paramref name="func"/>.
		/// </summary>
		protected virtual IQueryPredicate BuildQueryPredicate<TData>(Func<TData> func)
		{
			var queryPredicate = new QueryPredicate
			{
				Name = func.Method.Name,
				Parameters = new SortedSet<QueryParameter>()
			};

			return queryPredicate;
		}

		/// <summary>
		/// Builds a <see cref="IQueryPredicate"/> from the provided <paramref name="func"/>
		/// storing the provided <paramref name="parameter1"/>.
		/// </summary>
		protected virtual IQueryPredicate BuildQueryPredicate<TParameter1, TData>(Func<TParameter1, TData> func, TParameter1 parameter1)
		{
			var queryPredicate = new QueryPredicate
			{
				Name = func.Method.Name,
				Parameters = new SortedSet<QueryParameter>()
			};

			foreach (ParameterInfo parameterInfo in func.Method.GetParameters())
			{
				queryPredicate.Parameters.Add(new QueryParameter(parameterInfo.Name, parameter1));
			}

			return queryPredicate;
		}

		/// <summary>
		/// Builds a <see cref="IQueryPredicate"/> from the provided <paramref name="func"/>
		/// storing the provided <paramref name="parameter1"/> and <paramref name="parameter2"/>.
		/// </summary>
		protected virtual IQueryPredicate BuildQueryPredicate<TParameter1, TParameter2, TData>(Func<TParameter1, TParameter2, TData> func, TParameter1 parameter1, TParameter2 parameter2)
		{
			var queryPredicate = new QueryPredicate
			{
				Name = func.Method.Name,
				Parameters = new SortedSet<QueryParameter>()
			};

			int parameterIndex = 1;
			foreach (ParameterInfo parameterInfo in func.Method.GetParameters())
			{
				object parameter;
				switch (parameterIndex)
				{
					case 1:
						parameter = parameter1;
						break;
					case 2:
						parameter = parameter2;
						break;
					default:
						throw new InvalidOperationException();
				}
				queryPredicate.Parameters.Add(new QueryParameter(parameterInfo.Name, parameter));
				parameterIndex++;
			}

			return queryPredicate;
		}

		/// <summary>
		/// Builds a <see cref="IQueryPredicate"/> from the provided <paramref name="func"/>
		/// storing the provided <paramref name="parameter1"/>, <paramref name="parameter2"/> and  <paramref name="parameter3"/>.
		/// </summary>
		protected virtual IQueryPredicate BuildQueryPredicate<TParameter1, TParameter2, TParameter3, TData>(Func<TParameter1, TParameter2, TParameter3, TData> func, TParameter1 parameter1, TParameter2 parameter2, TParameter3 parameter3)
		{
			var queryPredicate = new QueryPredicate
			{
				Name = func.Method.Name,
				Parameters = new SortedSet<QueryParameter>()
			};

			int parameterIndex = 1;
			foreach (ParameterInfo parameterInfo in func.Method.GetParameters())
			{
				object parameter;
				switch (parameterIndex)
				{
					case 1:
						parameter = parameter1;
						break;
					case 2:
						parameter = parameter2;
						break;
					case 3:
						parameter = parameter3;
						break;
					default:
						throw new InvalidOperationException();
				}
				queryPredicate.Parameters.Add(new QueryParameter(parameterInfo.Name, parameter));
				parameterIndex++;
			}

			return queryPredicate;
		}

		/// <summary>
		/// Builds a <see cref="IQueryPredicate"/> from the provided <paramref name="func"/>
		/// storing the provided <paramref name="parameter1"/>, <paramref name="parameter2"/> and  <paramref name="parameter3"/>.
		/// </summary>
		protected virtual IQueryPredicate BuildQueryPredicate<TParameter1, TParameter2, TParameter3, TData>(Func<TParameter1, TParameter2, TParameter3, TData> func, TParameter1 parameter1, TParameter2 parameter2, TParameter3 parameter3)
		{
			var queryPredicate = new QueryPredicate
			{
				Name = func.Method.Name,
				Parameters = new SortedSet<QueryParameter>()
			};

			int parameterIndex = 1;
			foreach (ParameterInfo parameterInfo in func.Method.GetParameters())
			{
				object parameter;
				switch (parameterIndex)
				{
					case 1:
						parameter = parameter1;
						break;
					case 2:
						parameter = parameter2;
						break;
					case 3:
						parameter = parameter3;
						break;
					default:
						throw new InvalidOperationException();
				}
				queryPredicate.Parameters.Add(new QueryParameter(parameterInfo.Name, parameter));
				parameterIndex++;
			}

			return queryPredicate;
		}

		/// <summary>
		/// Builds a <see cref="IQueryPredicate"/> from the provided <paramref name="func"/>
		/// storing the provided <paramref name="parameter1"/>, <paramref name="parameter2"/>, <paramref name="parameter3"/> and <paramref name="parameter4"/>.
		/// </summary>
		protected virtual IQueryPredicate BuildQueryPredicate<TParameter1, TParameter2, TParameter3, TParameter4, TData>(Func<TParameter1, TParameter2, TParameter3, TParameter4, TData> func, TParameter1 parameter1, TParameter2 parameter2, TParameter3 parameter3, TParameter4 parameter4)
		{
			var queryPredicate = new QueryPredicate
			{
				Name = func.Method.Name,
				Parameters = new SortedSet<QueryParameter>()
			};

			int parameterIndex = 1;
			foreach (ParameterInfo parameterInfo in func.Method.GetParameters())
			{
				object parameter;
				switch (parameterIndex)
				{
					case 1:
						parameter = parameter1;
						break;
					case 2:
						parameter = parameter2;
						break;
					case 3:
						parameter = parameter3;
						break;
					case 4:
						parameter = parameter4;
						break;
					default:
						throw new InvalidOperationException();
				}
				queryPredicate.Parameters.Add(new QueryParameter(parameterInfo.Name, parameter));
				parameterIndex++;
			}

			return queryPredicate;
		}

		/// <summary>
		/// Builds a <see cref="IQueryPredicate"/> from the provided <paramref name="func"/>
		/// storing the provided <paramref name="parameter1"/>, <paramref name="parameter2"/>, <paramref name="parameter3"/>, <paramref name="parameter4"/> and <paramref name="parameter5"/>.
		/// </summary>
		protected virtual IQueryPredicate BuildQueryPredicate<TParameter1, TParameter2, TParameter3, TParameter4, TParameter5, TData>(Func<TParameter1, TParameter2, TParameter3, TParameter4, TParameter5, TData> func, TParameter1 parameter1, TParameter2 parameter2, TParameter3 parameter3, TParameter4 parameter4, TParameter5 parameter5)
		{
			var queryPredicate = new QueryPredicate
			{
				Name = func.Method.Name,
				Parameters = new SortedSet<QueryParameter>()
			};

			int parameterIndex = 1;
			foreach (ParameterInfo parameterInfo in func.Method.GetParameters())
			{
				object parameter;
				switch (parameterIndex)
				{
					case 1:
						parameter = parameter1;
						break;
					case 2:
						parameter = parameter2;
						break;
					case 3:
						parameter = parameter3;
						break;
					case 4:
						parameter = parameter4;
						break;
					case 5:
						parameter = parameter5;
						break;
					default:
						throw new InvalidOperationException();
				}
				queryPredicate.Parameters.Add(new QueryParameter(parameterInfo.Name, parameter));
				parameterIndex++;
			}

			return queryPredicate;
		}

		/// <summary>
		/// Builds an <see cref="IOrQueryPredicate"/> between <see cref="QueryPredicate"/>
		/// and the provided <paramref name="queryPredicate"/>
		/// </summary>
		protected virtual IQueryPredicate Or(IQueryPredicate queryPredicate)
		{
			return new OrQueryPredicate
			{
				LeftQueryPredicate = QueryPredicate,
				RightQueryPredicate = queryPredicate
			};
		}
	}
}