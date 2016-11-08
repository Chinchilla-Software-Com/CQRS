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
	public class TypeSerialiser : IBsonSerializer<Type>
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
			return ((IBsonSerializer<Type>) this).Deserialize(context, args);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="context">The serialization context.</param><param name="args">The serialization args.</param><param name="value">The value.</param>
		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, Type value)
		{
			if (value == null)
			{
				context.Writer.WriteNull();
			}
			else
			{
				context.Writer.WriteString((value).AssemblyQualifiedName);
			}
		}

		/// <summary>
		/// Deserializes a value.
		/// </summary>
		/// <param name="context">The deserialization context.</param><param name="args">The deserialization args.</param>
		/// <returns>
		/// A deserialized value.
		/// </returns>
		Type IBsonSerializer<Type>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			if (context.Reader.CurrentBsonType == BsonType.Null)
				return null;

			var bsonReader = context.Reader;
			bsonReader.ReadStartDocument();
			string assemblyQualifiedName = context.Reader.ReadString();
			// This moves the pointer forward.
			bsonReader.ReadBsonType();
			bsonReader.ReadEndDocument();
			return Type.GetType(assemblyQualifiedName);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="context">The serialization context.</param><param name="args">The serialization args.</param><param name="value">The value.</param>
		public void Serialize(BsonSerializationContext context, BsonSerializationArgs args, object value)
		{
			Serialize(context, args, (Type)value);
		}

		/// <summary>
		/// Gets the type of the value.
		/// </summary>
		public Type ValueType { get; private set; }

		#endregion
	}
}