#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Runtime.Serialization;
using Cqrs.Entities;

namespace Cqrs.Azure.BlobStorage.Test.Integration
{
	/// <summary>
	/// A Test <see cref="IEntity"/>.
	/// </summary>
	[Serializable]
	[DataContract]
	public class TestEntity : Entity
	{
		/// <summary>
		/// A Name property.
		/// </summary>
		[DataMember]
		public string Name { get; set; }
	}
}