#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Linq.Expressions;

namespace Cqrs.Infrastructure
{
	/// <summary>
	/// Adjusts <see cref="Expression"/> using <see cref="Expression.Convert(System.Linq.Expressions.Expression,System.Type)"/>
	/// </summary>
	public static class DelegateAdjuster
	{
		/// <summary>
		/// If <typeparamref name="TDerived"/> equals <typeparamref name="TBase"/> then <paramref name="source"/> is compiled using <see cref="Expression{TDelegate}.Compile()"/>
		/// Otherwise <paramref name="source"/> is converted to type <typeparamref name="TDerived"/> from <typeparamref name="TBase"/>
		/// </summary>
		/// <typeparam name="TBase">The source <see cref="Type"/>.</typeparam>
		/// <typeparam name="TDerived">The target <see cref="Type"/>.</typeparam>
		/// <param name="source">The delegate to adjust.</param>
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

