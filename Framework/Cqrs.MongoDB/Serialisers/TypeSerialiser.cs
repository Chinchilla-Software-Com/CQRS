#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;

namespace Cqrs.MongoDB.Serialisers
{
	/// <summary>
	/// A <see cref="IBsonSerializer"/> that stores <see cref="Type"/> information as well.
	/// </summary>
	public class TypeSerialiser : IBsonSerializer<Type>
	{
		#region Implementation of IBsonSerializer

		/// <summary>
		/// Deserialises a value, first reading the <see cref="Type"/> information from the provide <paramref name="context"/>.
		/// </summary>
		/// <param name="context">The deserialisation context.</param>
		/// <param name="args">The deserialisation arguments.</param>
		/// <returns>
		/// A deserialised value.
		/// </returns>
		public object Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			return ((IBsonSerializer<Type>) this).Deserialize(context, args);
		}

		/// <summary>
		/// Serializes a value.
		/// </summary>
		/// <param name="context">The serialisation context.</param>
		/// <param name="args">The serialisation arguments.</param>
		/// <param name="value">The value.</param>
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
		/// Deserialises a value.
		/// </summary>
		/// <param name="context">The deserialisation context.</param>
		/// <param name="args">The deserialisation arguments.</param>
		/// <returns>
		/// A deserialised value.
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
		/// <param name="context">The serialization context.</param>
		/// <param name="args">The serialization arguments.</param>
		/// <param name="value">The value.</param>
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