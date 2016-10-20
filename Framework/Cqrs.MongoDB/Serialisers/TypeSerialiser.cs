#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Cqrs.MongoDB.Serialisers
{
	public class TypeSerialiser : IBsonSerializer
	{
		#region Implementation of IBsonSerializer

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <param name="context">The deserialization context.</param>
		/// <param name="args">The deserialization args.</param>
		/// <returns>
		/// A deserialized value.
		/// </returns>
		public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			if (context.Reader.CurrentBsonType == BsonType.Null)
			{
				return null;
			}
			string assemblyQualifiedName = context.Reader.ReadString();
			return Type.GetType(assemblyQualifiedName);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="context">The serialization context.</param><param name="args">The serialization args.</param><param name="value">The value.</param>
		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
		{
			if (value == null)
			{
				context.Writer.WriteNull();
			}
			else
			{
				context.Writer.WriteString(((Type)value).AssemblyQualifiedName);
			}
		}

		/// <summary>
		/// Gets the type of the value.
		/// </summary>
		public Type ValueType { get; private set; }

		#endregion
	}
}