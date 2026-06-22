#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Linq.Expressions;

namespace Cqrs.Sql.DataStores
{
	/// <summary>
	/// Replaces or rewriter expression trees.
	/// </summary>
	public class SimpleExpressionReplacer : ExpressionVisitor
	{
		private readonly Expression _replacement;

		private readonly Expression _toFind;

		/// <summary>
		/// If the <see cref="Expression"/> to find (in the constructor) returns true, then the replacement <see cref="Expression"/> is returned.
		/// </summary>
		public override Expression Visit(Expression node)
		{
			return node == _toFind ? _replacement : base.Visit(node);
		}

		/// <summary>
		/// Instantiates a new instance of the <see cref="SimpleExpressionReplacer"/> class
		/// </summary>
		public SimpleExpressionReplacer(Expression toFind, Expression replacement)
		{
			_toFind = toFind;
			_replacement = replacement;
		}
	}
}