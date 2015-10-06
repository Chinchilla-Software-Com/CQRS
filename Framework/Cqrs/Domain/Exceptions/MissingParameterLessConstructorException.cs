#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;

namespace Cqrs.Domain.Exceptions
{
	[Serializable]
	public class MissingParameterLessConstructorException : Exception
	{
		public MissingParameterLessConstructorException(Type type)
			: base(string.Format("{0} has no constructor without parameters. This can be either public or private", type.FullName))
		{
		}
	}
}