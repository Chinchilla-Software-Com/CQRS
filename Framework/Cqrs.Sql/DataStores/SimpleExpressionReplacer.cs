using System.Linq.Expressions;

namespace Cqrs.Sql.DataStores
{
	public class SimpleExpressionReplacer : ExpressionVisitor
	{
		private readonly Expression _replacement;

		private readonly Expression _toFind;

		public override Expression Visit(Expression node)
		{
			return node == _toFind ? _replacement : base.Visit(node);
		}

		public SimpleExpressionReplacer(Expression toFind, Expression replacement)
		{
			_toFind = toFind;
			_replacement = replacement;
		}
	}
}