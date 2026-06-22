#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Cqrs.Sql.DataStores
{
	/// <summary>
	/// Converts <see cref="Expression"/> trees.
	/// </summary>
	public abstract class ExpressionTreeConverter<TA, TB> : ExpressionVisitor, IExpressionTreeConverter
	{
		private readonly Dictionary<ParameterExpression, ParameterExpression> _parameters = new Dictionary<ParameterExpression, ParameterExpression>();

		private readonly Dictionary<MemberInfo, LambdaExpression> _mappings;

		/// <summary>
		/// Instantiates a new instance of the <see cref="ExpressionTreeConverter{TA,TB}"/> class
		/// </summary>
		protected ExpressionTreeConverter(Dictionary<MemberInfo, LambdaExpression> mappings)
		{
			_mappings = mappings;
		}

		/// <summary>
		/// Instantiates a new instance of the <see cref="ExpressionTreeConverter{TA,TB}"/> class
		/// </summary>
		protected ExpressionTreeConverter()
		{
			_mappings = GetMappings();
		}

		/// <summary>
		/// A collection of <see cref="LambdaExpression"/> grouped by <see cref="MemberInfo"/>.
		/// </summary>
		public abstract Dictionary<MemberInfo, LambdaExpression> GetMappings();

		/// <summary>
		/// If the <paramref name="node"/> if of <see cref="Type"/> <typeparamref name="TA"/>
		/// A <see cref="ParameterExpression"/> of <see cref="Type"/> <typeparamref name="TB"/> will to created with the same name.
		/// </summary>
		protected override Expression VisitParameter(ParameterExpression node)
		{
			if (node.Type == typeof(TA))
			{
				ParameterExpression parameter;
				if (!_parameters.TryGetValue(node, out parameter))
				{
					_parameters.Add(node, parameter = Expression.Parameter(typeof(TB), node.Name));
				}
				return parameter;
			}
			return node;
		}

		/// <summary>
		/// If the <see cref="MemberExpression.Member"/> of the provided <paramref name="node"/> matches a mapping in 
		/// <see cref="GetMappings"/>, then that mapping is executed.
		/// </summary>
		protected override Expression VisitMember(MemberExpression node)
		{
			if (node.Expression == null || node.Expression.Type != typeof(TA))
				return base.VisitMember(node);

			Expression expression = Visit(node.Expression);
			if (expression.Type != typeof(TB))
				throw new Exception("Whoops");

			LambdaExpression lambdaExpression;
			if (_mappings.TryGetValue(node.Member, out lambdaExpression))
				return new SimpleExpressionReplacer(lambdaExpression.Parameters.Single(), expression).Visit(lambdaExpression.Body);

			return Expression.Property(expression, node.Member.Name);
		}

		/// <summary>
		/// Visits the children of the <paramref name="node"/>.
		/// </summary>
		protected override Expression VisitLambda<T>(Expression<T> node)
		{
			return Expression.Lambda(
				Visit(node.Body),
				node.Parameters.Select(Visit).Cast<ParameterExpression>()
				);
		}
	}
}