using System;

namespace Cqrs.Domain.Exception
{
	[Serializable]
	public class MissingParameterLessConstructorException : System.Exception
	{
		public MissingParameterLessConstructorException(Type type)
			: base(string.Format("{0} has no constructor without parameters. This can be either public or private", type.FullName))
		{
		}
	}
}