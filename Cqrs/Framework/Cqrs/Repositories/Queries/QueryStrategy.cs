using System;
using System.Collections.Generic;
using System.Reflection;

namespace Cqrs.Repositories.Queries
{
	public abstract class QueryStrategy : IQueryStrategy
	{
		#region Implementation of IQueryStrategy

		public IQueryPredicate QueryPredicate { get; protected set; }

		#endregion

		public virtual IQueryPredicate IsNotLogicallyDeleted()
		{
			return BuildQueryPredicate(IsNotLogicallyDeleted);
		}

		public virtual IQueryPredicate WithPermissionScopeAny<TAuthenticationToken>(TAuthenticationToken authenticationToken)
		{
			return BuildQueryPredicate(WithPermissionScopeAny, authenticationToken);
		}

		public virtual IQueryPredicate WithPermissionScopeUser<TAuthenticationToken>(TAuthenticationToken authenticationToken)
		{
			return BuildQueryPredicate(WithPermissionScopeUser, authenticationToken);
		}

		public virtual IQueryPredicate WithPermissionScopeCompany<TAuthenticationToken>(TAuthenticationToken authenticationToken)
		{
			return BuildQueryPredicate(WithPermissionScopeCompany, authenticationToken);
		}

		public virtual IQueryPredicate WithPermissionScopeCompanyAndUser<TAuthenticationToken>(TAuthenticationToken authenticationToken)
		{
			return BuildQueryPredicate(WithPermissionScopeCompanyAndUser, authenticationToken);
		}

		protected virtual IQueryPredicate BuildQueryPredicate<TData>(Func<TData> func)
		{
			var queryPredicate = new QueryPredicate
			{
				Name = func.Method.Name,
				Parameters = new SortedSet<QueryParameter>()
			};

			return queryPredicate;
		}

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

		protected virtual IQueryPredicate And(IQueryPredicate queryPredicate)
		{
			return new AndQueryPredicate
			{
				LeftQueryPredicate = QueryPredicate,
				RightQueryPredicate = queryPredicate
			};
		}

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