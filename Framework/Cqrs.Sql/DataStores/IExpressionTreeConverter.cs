#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Cqrs.Sql.DataStores
{
	/// <summary>
	/// Converts <see cref="Expression"/> trees.
	/// </summary>
	public interface IExpressionTreeConverter
	{
		/// <summary>
		/// A collection of <see cref="LambdaExpression"/> grouped by <see cref="MemberInfo"/>.
		/// </summary>
		Dictionary<MemberInfo, LambdaExpression> GetMappings();

		/// <summary>
		/// Dispatches the <see cref="Expression"/> to one of the more specialized visit methods in this class.
		/// </summary>
		/// <param name="node">The <see cref="Expression"/> to visit.</param>
		/// <returns>The modified <see cref="Expression"/>, if it or any subexpression was modified; otherwise, returns the original <see cref="Expression"/>.</returns>
		Expression Visit(Expression node);

		/// <summary>
		/// Dispatches the list of expressions to one of the more specialized visit methods in this class.
		/// </summary>
		/// <param name="nodes">The expressions to visit.</param>
		/// <returns>The modified <see cref="Expression"/> list, if any one of the elements were modified; otherwise, returns the <see cref="Expression"/> expression list.</returns>
		ReadOnlyCollection<Expression> Visit(ReadOnlyCollection<Expression> nodes);
	}
}