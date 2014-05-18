using System;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;

namespace Cqrs.Mongo.Serialisers
{
	public class TypeSerialiser : IBsonSerializer
	{
		public object Deserialize(BsonReader reader, Type nominalType, IBsonSerializationOptions options)
		{
			var actualType = nominalType;
			return Deserialize(reader, nominalType, actualType, options);
		}

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