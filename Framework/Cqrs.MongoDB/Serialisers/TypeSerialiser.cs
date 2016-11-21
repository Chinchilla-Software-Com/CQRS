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
		/// <param name="context">The deserialization context.</param>
		/// <param name="args">The deserialization args.</param>
		/// <returns>
		/// A deserialized value.
		/// </returns>
		Type IBsonSerializer<Type>.Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			var bsonReader = context.Reader;
			BsonType currentBsonType = bsonReader.CurrentBsonType;
			if (currentBsonType == BsonType.Null)
				return null;

			string assemblyQualifiedName;
			switch (currentBsonType)
			{
				case BsonType.Document:
					bsonReader.ReadStartDocument();
					assemblyQualifiedName = context.Reader.ReadString();
					// This moves the pointer forward.
					bsonReader.ReadBsonType();
					bsonReader.ReadEndDocument();
					break;
				default:
					assemblyQualifiedName = bsonReader.ReadString();
					break;
			}
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
		public Type ValueType { get; protected set; }

		#endregion
	}
}