#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Azure.Messaging.ServiceBus;
using System;
using System.IO;
using System.Runtime.Serialization;
using System.Text;
using System.Xml;

namespace Cqrs.Azure.Functions.ServiceBus
{
	internal static class MessageExtensions
	{
		public static void AddUserProperty(this ServiceBusReceivedMessage message, string key, object value)
		{
			throw new NotImplementedException();
		}

		public static bool TryGetUserPropertyValue(this ServiceBusReceivedMessage message, string key, out object value)
		{
			return message.ApplicationProperties.TryGetValue(key, out value);
		}

		private static DataContractBinarySerializer Serialiser = new DataContractBinarySerializer(typeof(string));

		public static string GetBodyAsString(this ServiceBusReceivedMessage message)
		{
			byte[] rawStream = message.Body.ToArray();
			using (var stream = new MemoryStream(rawStream.Length))
			{
				stream.Write(rawStream, 0, rawStream.Length);
				stream.Flush();
				stream.Position = 0;
				try
				{
					return (string)Serialiser.ReadObject(stream);
				}
				catch (SerializationException ex)
				{
					stream.Position = 0;
					using (StreamReader reader = new StreamReader(stream, Encoding.UTF8))
						return reader.ReadToEnd();
				}
			}
		}
	}

	internal sealed class DataContractBinarySerializer : XmlObjectSerializer
	{
		private readonly DataContractSerializer dataContractSerializer;

		public DataContractBinarySerializer(Type type)
		{
			this.dataContractSerializer = new DataContractSerializer(type);
		}

		public override object ReadObject(Stream stream)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			return ReadObject(XmlDictionaryReader.CreateBinaryReader(stream, XmlDictionaryReaderQuotas.Max));
		}

		public override void WriteObject(Stream stream, object graph)
		{
			if (stream == null)
				throw new ArgumentNullException(nameof(stream));
			XmlDictionaryWriter binaryWriter = XmlDictionaryWriter.CreateBinaryWriter(stream, (IXmlDictionary)null, (XmlBinaryWriterSession)null, false);
			WriteObject(binaryWriter, graph);
			binaryWriter.Flush();
		}

		public override void WriteObject(XmlDictionaryWriter writer, object graph)
		{
			if (writer == null)
				throw new ArgumentNullException(nameof(writer));
			dataContractSerializer.WriteObject(writer, graph);
		}

		public override bool IsStartObject(XmlDictionaryReader reader)
		{
			return dataContractSerializer.IsStartObject(reader);
		}

		public override object ReadObject(XmlDictionaryReader reader, bool verifyObjectName)
		{
			return dataContractSerializer.ReadObject(reader, verifyObjectName);
		}

		public override void WriteEndObject(XmlDictionaryWriter writer)
		{
			dataContractSerializer.WriteEndObject(writer);
		}

		public override void WriteObjectContent(XmlDictionaryWriter writer, object graph)
		{
			dataContractSerializer.WriteObjectContent(writer, graph);
		}

		public override void WriteStartObject(XmlDictionaryWriter writer, object graph)
		{
			dataContractSerializer.WriteStartObject(writer, graph);
		}
	}
}