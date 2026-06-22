#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using MongoDB.Bson;
using MongoDB.Bson.IO;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;

namespace Cqrs.MongoDB.Serialisers
{
	/// <summary>
	/// A <see cref="StructSerializerBase{TValue}"/> with reasonable level support for structs.
	/// </summary>
	/// <typeparam name="TStruct">The <see cref="Type"/> of struct.</typeparam>
	public class BasicStructSerialiser<TStruct> : StructSerializerBase<TStruct>
		where TStruct : struct
	{
		/// <summary>
		/// Serializes the provide <paramref name="value"/> as a set of key/value pairs.
		/// </summary>
		/// <param name="context">The serialisation context.</param>
		/// <param name="args">The serialisation arguments.</param>
		/// <param name="value">The value.</param>
		public override void Serialize(BsonSerializationContext context, BsonSerializationArgs args, TStruct value)
		{
			Type nominalType = args.NominalType;
			FieldInfo[] fields = nominalType.GetFields(BindingFlags.Instance | BindingFlags.Public);
			PropertyInfo[] propsAll = nominalType.GetProperties(BindingFlags.Instance | BindingFlags.Public);

			IEnumerable<PropertyInfo> props = propsAll.Where(prop => prop.CanWrite).ToList();

			IBsonWriter bsonWriter = context.Writer;

			bsonWriter.WriteStartDocument();

			foreach (FieldInfo field in fields)
			{
				bsonWriter.WriteName(field.Name);
				BsonSerializer.Serialize(bsonWriter, field.FieldType, field.GetValue(value));
			}
			foreach (PropertyInfo prop in props)
			{
				bsonWriter.WriteName(prop.Name);
				BsonSerializer.Serialize(bsonWriter, prop.PropertyType, prop.GetValue(value, null));
			}

			bsonWriter.WriteEndDocument();
		}

		/// <summary>
		/// Deserialises a value from key/value pairs.
		/// </summary>
		/// <param name="context">The deserialisation context.</param>
		/// <param name="args">The deserialisation arguments.</param>
		/// <returns>
		/// A deserialised value.
		/// </returns>
		public override TStruct Deserialize(BsonDeserializationContext context, BsonDeserializationArgs args)
		{
			//boxing is required for SetValue to work
			var obj = (object)(new TStruct());
			Type actualType = args.NominalType;
			IBsonReader bsonReader = context.Reader;

			bsonReader.ReadStartDocument();

			while (bsonReader.ReadBsonType() != BsonType.EndOfDocument)
			{
				var name = bsonReader.ReadName();

				var field = actualType.GetField(name);
				if (field != null)
				{
					var value = BsonSerializer.Deserialize(bsonReader, field.FieldType);
					field.SetValue(obj, value);
				}

				var prop = actualType.GetProperty(name);
				if (prop != null)
				{
					var value = BsonSerializer.Deserialize(bsonReader, prop.PropertyType);
					prop.SetValue(obj, value, null);
				}
			}

			bsonReader.ReadEndDocument();

			return (TStruct)obj;
		}
	}
}