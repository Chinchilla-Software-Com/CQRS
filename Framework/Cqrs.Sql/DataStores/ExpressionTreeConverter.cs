using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Cqrs.Sql.DataStores
{
	public abstract class ExpressionTreeConverter<TA, TB> : ExpressionVisitor, IExpressionTreeConverter
	{
		private readonly Dictionary<ParameterExpression, ParameterExpression> _parameters = new Dictionary<ParameterExpression, ParameterExpression>();

		private readonly Dictionary<MemberInfo, LambdaExpression> _mappings;

		protected ExpressionTreeConverter(Dictionary<MemberInfo, LambdaExpression> mappings)
		{
			_mappings = mappings;
		}

		protected ExpressionTreeConverter()
		{
			_mappings = GetMappings();
		}

		public abstract Dictionary<MemberInfo, LambdaExpression> GetMappings();

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

		protected override Expression VisitLambda<T>(Expression<T> node)
		{
			return Expression.Lambda(
				Visit(node.Body),
				node.Parameters.Select(Visit).Cast<ParameterExpression>()
				);
		}
	}
}