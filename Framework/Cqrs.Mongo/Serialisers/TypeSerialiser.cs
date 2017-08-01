#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Cqrs.Mongo.Serialisers
{
	/// <summary>
	/// A <see cref="IBsonSerializer"/> that stores <see cref="Type"/> information as well.
	/// </summary>
	public class TypeSerialiser : IBsonSerializer
	{
		/// <summary>
		/// Deserialises a <see cref="Type"/> value, first reading the <see cref="Type"/> information from the provide <paramref name="reader"/>.
		/// </summary>
		public object Deserialize(BsonReader reader, Type nominalType, IBsonSerializationOptions options)
		{
			var actualType = nominalType;
			return Deserialize(reader, nominalType, actualType, options);
		}

		/// <summary>
		/// Deserialises a <see cref="Type"/> value, first reading the <see cref="Type"/> information from the provide <paramref name="reader"/>.
		/// </summary>
		public object Deserialize(BsonReader reader, Type nominalType, Type actualType, IBsonSerializationOptions options)
		{
			if (reader.CurrentBsonType == BsonType.Null)
			{
				return null;
			}
			string assemblyQualifiedName = reader.ReadString();
			return Type.GetType(assemblyQualifiedName);
		}

		/// <summary>
		/// Gets the default serialization options for this serializer.
		/// </summary>
		/// <returns>
		/// The default serialization options for this serializer.
		/// </returns>
		public IBsonSerializationOptions GetDefaultSerializationOptions()
		{
			return null;
		}

		/// <summary>
		/// Serialises a <see cref="Type"/> value.
		/// </summary>
		public void Serialize(BsonWriter writer, Type nominalType, object value, IBsonSerializationOptions options)
		{
			if (value == null)
			{
				writer.WriteNull();
			}
			else
			{
				writer.WriteString(((Type)value).AssemblyQualifiedName);
			}
		}
	}
}