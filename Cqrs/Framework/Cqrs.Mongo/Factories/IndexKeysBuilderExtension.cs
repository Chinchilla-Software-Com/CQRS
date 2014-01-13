using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using MongoDB.Driver.Builders;

namespace Cqrs.Mongo.Factories
{
	public static class IndexKeysBuilderExtension
	{
		public static IndexKeysBuilder Ascending<T>(this IndexKeysBuilder mongoIndexKeys, params Expression<Func<T, object>>[] selectors)
		{
			var properties = new List<string>();
			foreach (Expression<Func<T, object>> selector in selectors)
			{
				var selectorUnaryExpression = selector.Body as UnaryExpression;
				MemberExpression selectorMemberExpression;
				if (selectorUnaryExpression != null)
				{
					selectorMemberExpression = (MemberExpression)selectorUnaryExpression.Operand;
				}
				else
				{
					selectorMemberExpression = (MemberExpression) selector.Body;
				}
				string memberName = CheckForChildProperty(selectorMemberExpression);
				properties.Add(memberName);
			}
			return mongoIndexKeys.Ascending(properties.ToArray());
		}

		public static string CheckForChildProperty(MemberExpression selectorMemberExpression)
		{
			string memberName = selectorMemberExpression.Member.Name;
			var selectorMethodCallExpression = selectorMemberExpression.Expression as MethodCallExpression;
			if (selectorMethodCallExpression != null)
			{
				var selectorMethodCallExpressionArgument = selectorMethodCallExpression.Arguments.FirstOrDefault() as MemberExpression;
				if (selectorMethodCallExpressionArgument != null)
					memberName = string.Format("{1}.{0}", memberName, selectorMethodCallExpressionArgument.Member.Name);
			}
			else
			{
				if (selectorMemberExpression.Expression.GetType().Name == "PropertyExpression")
				{
					dynamic propertyExpression = selectorMemberExpression.Expression;
					if (propertyExpression.Expression.NodeType == ExpressionType.MemberAccess)
						memberName = string.Format("{2}.{1}.{0}", memberName, propertyExpression.Member.Name, propertyExpression.Expression.Member.Name);
					else
						memberName = string.Format("{1}.{0}", memberName, propertyExpression.Member.Name);
				}
			}
			return memberName;
		}
	}
}