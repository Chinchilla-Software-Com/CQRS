using System;
using System.Linq.Expressions;

namespace Cqrs.Infrastructure
{
	public static class DelegateAdjuster
	{
		public static Action<TBase> CastArgument<TBase, TDerived>(Expression<Action<TDerived>> source)
			where TDerived : TBase
		{
			if (typeof(TDerived) == typeof(TBase))
			{
				return (Action<TBase>)((Delegate)source.Compile());
			}

			ParameterExpression sourceParameter = Expression.Parameter(typeof(TBase), "source");
			Expression<Action<TBase>> result = Expression.Lambda<Action<TBase>>
			(
				Expression.Invoke(source, Expression.Convert(sourceParameter, typeof(TDerived))),
				sourceParameter
			);
			return result.Compile();
		}
	}
}

