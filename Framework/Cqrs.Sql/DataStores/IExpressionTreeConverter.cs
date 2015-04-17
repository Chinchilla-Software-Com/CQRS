using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Cqrs.Sql.DataStores
{
	public interface IExpressionTreeConverter
	{
		Dictionary<MemberInfo, LambdaExpression> GetMappings();

		Expression Visit(Expression node);

		ReadOnlyCollection<Expression> Visit(ReadOnlyCollection<Expression> nodes);
	}
}