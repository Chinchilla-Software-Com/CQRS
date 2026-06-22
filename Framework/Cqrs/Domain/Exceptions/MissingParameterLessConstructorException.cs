#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;

namespace Cqrs.Domain.Exceptions
{
	/// <summary>
	/// A parameterless constructor is missing.
	/// </summary>
	[Serializable]
	public class MissingParameterLessConstructorException : Exception
	{
		/// <summary>
		/// Instantiate a new instance of <see cref="MissingParameterLessConstructorException"/> with the <see cref="Type"/> of the object that needs to have a parameterless constructor.
		/// </summary>
		/// <param name="type">The <see cref="Type"/> of the object that needs to have a parameterless constructor.</param>
		public MissingParameterLessConstructorException(Type type)
			: base(string.Format("{0} has no constructor without parameters. This can be either public or private", type.FullName))
		{
			Type = type;
		}

		/// <summary>
		/// The <see cref="Type"/> of the object that needs to have a parameterless constructor.
		/// </summary>
		[DataMember]
		public Type Type { get; set; }
	}
}