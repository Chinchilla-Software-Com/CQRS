﻿/*
// -----------------------------------------------------------------------------------------
// <copyright file="EntityPropertyConverter.cs" company="Microsoft">
//    Copyright 2013 Microsoft Corporation
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.
// </copyright>
// -----------------------------------------------------------------------------------------

using System.Collections;
using System.Diagnostics;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace Microsoft.WindowsAzure.Storage.Table
{
	using System;
	using System.Collections.Generic;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Reflection;
	using System.Runtime.CompilerServices;
	using System.Runtime.Serialization;

	/// <summary>
	/// EntityPropertyConverter class.
	/// </summary>
	public static class EntityPropertyConverter
	{
		/// <summary>
		/// The default <see cref="JsonSerializerSettings"/> to use.
		/// </summary>
		public static JsonSerializerSettings DefaultSettings { get; private set; }

		static EntityPropertyConverter()
		{
			DefaultSettings = new JsonSerializerSettings
			{
				Formatting = Formatting.None,
				MissingMemberHandling = MissingMemberHandling.Ignore,
				DateParseHandling = DateParseHandling.DateTimeOffset,
				DateTimeZoneHandling = DateTimeZoneHandling.RoundtripKind,
				Converters = new List<JsonConverter> { new StringEnumConverter() },
				DateFormatHandling = DateFormatHandling.IsoDateFormat,
				DefaultValueHandling = DefaultValueHandling.IgnoreAndPopulate,
				FloatFormatHandling = FloatFormatHandling.DefaultValue,
				NullValueHandling = NullValueHandling.Include,
				PreserveReferencesHandling = PreserveReferencesHandling.All,
				ReferenceLoopHandling = ReferenceLoopHandling.Error,
				StringEscapeHandling = StringEscapeHandling.EscapeNonAscii,
				TypeNameHandling = TypeNameHandling.All
			};
		}

		/// <summary>
		/// The property delimiter.
		/// </summary>
		public const string DefaultPropertyNameDelimiter = "_";

		/// <summary>
		/// Traverses object graph, flattens and converts all nested (and not nested) properties to EntityProperties, stores them in the property dictionary.
		/// The keys are constructed by appending the names of the properties visited during pre-order depth first traversal from root to each end node property delimited by '_'.
		/// Allows complex objects to be stored in persistent storage systems or passed between web services in a generic way.
		/// </summary>
		/// <param name="root">The object to flatten and convert.</param>
		/// <param name="operationContext">An <see cref="OperationContext"/> object that represents the context for the current operation.</param>
		/// <returns>The result containing <see cref="IDictionary{TKey,TValue}"/> of <see cref="EntityProperty"/> objects for all properties of the flattened root object.</returns>
		public static Dictionary<string, EntityProperty> Flatten(object root, OperationContext operationContext)
		{
			return Flatten(root, new EntityPropertyConverterOptions { PropertyNameDelimiter = DefaultPropertyNameDelimiter }, operationContext);
		}

		/// <summary>
		/// Traverses object graph, flattens and converts all nested (and not nested) properties to EntityProperties, stores them in the property dictionary.
		/// The keys are constructed by appending the names of the properties visited during pre-order depth first traversal from root to each end node property delimited by '_'.
		/// Allows complex objects to be stored in persistent storage systems or passed between web services in a generic way.
		/// </summary>
		/// <param name="root">The object to flatten and convert.</param>
		/// <param name="entityPropertyConverterOptions">A <see cref="EntityPropertyConverterOptions"/> object that specifies options for the entity property conversion.</param>
		/// <param name="operationContext">An <see cref="OperationContext"/> object that represents the context for the current operation.</param>
		/// <returns>The result containing <see cref="IDictionary{TKey,TValue}"/> of <see cref="EntityProperty"/> objects for all properties of the flattened root object.</returns>
		public static Dictionary<string, EntityProperty> Flatten(object root, EntityPropertyConverterOptions entityPropertyConverterOptions, OperationContext operationContext)
		{
			if (root == null)
			{
				return null;
			}

			Dictionary<string, EntityProperty> propertyDictionary = new Dictionary<string, EntityProperty>();
			HashSet<object> antecedents = new HashSet<object>(new ObjectReferenceEqualityComparer());

			return Flatten(propertyDictionary, root, string.Empty, antecedents, entityPropertyConverterOptions, operationContext) ? propertyDictionary : null;
		}

		/// <summary>
		/// Reconstructs the complete object graph of type T using the flattened entity property dictionary and returns reconstructed object.
		/// The property dictionary may contain only basic properties, only nested properties or a mix of both types.
		/// </summary>
		/// <typeparam name="T">The type of the object to populate</typeparam>
		/// <param name="flattenedEntityProperties">The flattened entity property dictionary.</param>
		/// <param name="operationContext">An <see cref="OperationContext"/> object that represents the context for the current operation.</param>
		/// <returns>The result containing the reconstructed object with its full object hierarchy.</returns>
		public static T ConvertBack<T>(IDictionary<string, EntityProperty> flattenedEntityProperties, OperationContext operationContext)
		{
			return ConvertBack<T>(flattenedEntityProperties, new EntityPropertyConverterOptions { PropertyNameDelimiter = DefaultPropertyNameDelimiter }, operationContext);
		}

		/// <summary>
		/// Reconstructs the complete object graph of type T using the flattened entity property dictionary and returns reconstructed object.
		/// The property dictionary may contain only basic properties, only nested properties or a mix of both types.
		/// </summary>
		/// <typeparam name="T">The type of the object to populate</typeparam>
		/// <param name="flattenedEntityProperties">The flattened entity property dictionary.</param>
		/// <param name="entityPropertyConverterOptions">A <see cref="EntityPropertyConverterOptions"/> object that specifies options for the entity property conversion.</param>
		/// <param name="operationContext">An <see cref="OperationContext"/> object that represents the context for the current operation.</param>
		/// <returns>The result containing the reconstructed object with its full object hierarchy.</returns>
		public static T ConvertBack<T>(
			IDictionary<string, EntityProperty> flattenedEntityProperties,
			EntityPropertyConverterOptions entityPropertyConverterOptions,
			OperationContext operationContext)
		{
			if (flattenedEntityProperties == null)
			{
				return default(T);
			}

			T root = (T)Activator.CreateInstance(typeof(T));

			return flattenedEntityProperties.Aggregate(root, (current, kvp) => (T)SetProperty(current, kvp.Key, kvp.Value.PropertyAsObject, entityPropertyConverterOptions, operationContext));
		}

		/// <summary>
		/// Traverses object graph, flattens and converts all nested (and not nested) properties to EntityProperties, stores them in the property dictionary.
		/// The keys are constructed by appending the names of the properties visited during pre-order depth first traversal from root to each end node property delimited by '.'.
		/// Allows complex objects to be stored in persistent storage systems or passed between web services in a generic way.
		/// </summary>
		/// <param name="propertyDictionary">The property dictionary.</param>
		/// <param name="current">The current object.</param>
		/// <param name="objectPath">The object path.</param>
		/// <param name="antecedents">The antecedents of current object, used to detect circular references in object graph.</param>
		/// <param name="entityPropertyConverterOptions">A <see cref="EntityPropertyConverterOptions"/> object that specifies options for the entity property conversion.</param>
		/// <param name="operationContext">An <see cref="OperationContext"/> object that represents the context for the current operation.</param>
		/// <returns>The <see cref="bool"/> to indicate success of conversion to flattened EntityPropertyDictionary.</returns>
		private static bool Flatten(
			Dictionary<string, EntityProperty> propertyDictionary,
			object current,
			string objectPath,
			HashSet<object> antecedents,
			EntityPropertyConverterOptions entityPropertyConverterOptions,
			OperationContext operationContext)
		{
			if (current == null)
			{
				return true;
			}

			Type type;
			do
			{
				type = current.GetType();
				EntityProperty entityProperty = CreateEntityPropertyWithType(current, type);

				if (entityProperty != null)
				{
					propertyDictionary.Add(objectPath, entityProperty);
					return true;
				}
				// Better support for IEnumerable
				if (current is IEnumerable && !(current is string))
					current = string.Format("_$¿={0}", JsonConvert.SerializeObject(current, GetSerialisationSettings()));
				else
					break;
			} while (true);

			IEnumerable<PropertyInfo> propertyInfos = type.GetProperties();

			if (!propertyInfos.Any())
			{
				throw new SerializationException(string.Format(CultureInfo.InvariantCulture, SR.UnsupportedPropertyTypeForEntityPropertyConversion, type, objectPath));
			}

			bool isAntecedent = false;

			if (!type.IsValueType)
			{
				if (antecedents.Contains(current))
				{
					throw new SerializationException(string.Format(CultureInfo.InvariantCulture, SR.RecursiveReferencedObject, objectPath, type));
				}

				antecedents.Add(current);
				isAntecedent = true;
			}

			string propertyNameDelimiter = entityPropertyConverterOptions != null ? entityPropertyConverterOptions.PropertyNameDelimiter : DefaultPropertyNameDelimiter;

			bool success = propertyInfos
				.Where(propertyInfo => !ShouldSkip(propertyInfo, objectPath, operationContext))
				.All(propertyInfo =>
				{
					if (propertyInfo.Name.Contains(propertyNameDelimiter))
					{
						throw new SerializationException(string.Format(CultureInfo.InvariantCulture, SR.PropertyDelimiterExistsInPropertyName, propertyNameDelimiter, propertyInfo.Name, objectPath));
					}

					object value;
					try
					{
						value = propertyInfo.GetValue(current, index: null);
					}
					catch (Exception)
					{
						// Support for unsupported data types
						value = string.Format("_$¿={0}", JsonConvert.SerializeObject(current, GetSerialisationSettings()));
					}

					return Flatten(
						propertyDictionary,
						value,
						string.IsNullOrWhiteSpace(objectPath) ? propertyInfo.Name : objectPath + propertyNameDelimiter + propertyInfo.Name,
						antecedents,
						entityPropertyConverterOptions,
						operationContext);
				});

			if (isAntecedent)
			{
				antecedents.Remove(current);
			}

			return success;
		}

		/// <summary>
		/// Returns <see cref="DefaultSettings"/>
		/// </summary>
		/// <returns><see cref="DefaultSettings"/></returns>
		static JsonSerializerSettings GetSerialisationSettings()
		{
			return DefaultSettings;
		}

		/// <summary>Creates entity property with given type.</summary>
		/// <param name="value">The value.</param>
		/// <param name="type">The type.</param>
		/// <returns>The <see cref="EntityProperty"/>.</returns>
		private static EntityProperty CreateEntityPropertyWithType(object value, Type type)
		{
			if (type == typeof(string))
			{
				return new EntityProperty((string)value);
			}
			else if (type == typeof(byte[]))
			{
				return new EntityProperty((byte[])value);
			}
			else if (type == typeof(bool))
			{
				return new EntityProperty((bool)value);
			}
			else if (type == typeof(bool?))
			{
				return new EntityProperty((bool?)value);
			}
			else if (type == typeof(DateTime))
			{
				return new EntityProperty((DateTime)value);
			}
			else if (type == typeof(DateTime?))
			{
				return new EntityProperty((DateTime?)value);
			}
			else if (type == typeof(DateTimeOffset))
			{
				return new EntityProperty((DateTimeOffset)value);
			}
			else if (type == typeof(DateTimeOffset?))
			{
				return new EntityProperty((DateTimeOffset?)value);
			}
			else if (type == typeof(double))
			{
				return new EntityProperty((double)value);
			}
			else if (type == typeof(double?))
			{
				return new EntityProperty((double?)value);
			}
			else if (type == typeof(Guid?))
			{
				return new EntityProperty((Guid?)value);
			}
			else if (type == typeof(Guid))
			{
				return new EntityProperty((Guid)value);
			}
			else if (type == typeof(int))
			{
				return new EntityProperty((int)value);
			}
			else if (type == typeof(int?))
			{
				return new EntityProperty((int?)value);
			}
			else if (type == typeof(uint))
			{
				return new EntityProperty(unchecked((int)Convert.ToUInt32(value, CultureInfo.InvariantCulture)));
			}
			else if (type == typeof(uint?))
			{
				return new EntityProperty(unchecked((int?)Convert.ToUInt32(value, CultureInfo.InvariantCulture)));
			}
			else if (type == typeof(long))
			{
				return new EntityProperty((long)value);
			}
			else if (type == typeof(long?))
			{
				return new EntityProperty((long?)value);
			}
			else if (type == typeof(ulong))
			{
				return new EntityProperty(unchecked((long)Convert.ToUInt64(value, CultureInfo.InvariantCulture)));
			}
			else if (type == typeof(ulong?))
			{
				return new EntityProperty(unchecked((long?)Convert.ToUInt64(value, CultureInfo.InvariantCulture)));
			}
			else if (type.IsEnum)
			{
				return new EntityProperty(value.ToString());
			}
			else if (type == typeof(TimeSpan))
			{
				return new EntityProperty(value.ToString());
			}
			else if (type == typeof(TimeSpan?))
			{
				return new EntityProperty(value != null ? value.ToString() : null);
			}
			else
			{
				return null;
			}
		}

		/// <summary>Sets the property given with the property path on the passed in object.</summary>
		/// <param name="root">The root object.</param>
		/// <param name="propertyPath">The full property path formed by the name of properties from root object to the target property(included), appended by '.'.</param>
		/// <param name="propertyValue">The property value.</param>
		/// <param name="entityPropertyConverterOptions">A <see cref="EntityPropertyConverterOptions"/> object that specifies options for the entity property conversion.</param>
		/// <param name="operationContext">An <see cref="OperationContext"/> object that represents the context for the current operation.</param>
		/// <returns>The updated <see cref="object"/>.</returns>
		private static object SetProperty(
			object root,
			string propertyPath,
			object propertyValue,
			EntityPropertyConverterOptions entityPropertyConverterOptions,
			OperationContext operationContext)
		{
			if (root == null)
			{
				throw new ArgumentNullException("root");
			}

			if (propertyPath == null)
			{
				throw new ArgumentNullException("propertyPath");
			}

			try
			{
				string propertyNameDelimiter = entityPropertyConverterOptions != null ? entityPropertyConverterOptions.PropertyNameDelimiter : DefaultPropertyNameDelimiter;

				Stack<Tuple<object, object, PropertyInfo>> valueTypePropertyHierarchy = new Stack<Tuple<object, object, PropertyInfo>>();
				string[] properties = propertyPath.Split(new[] { propertyNameDelimiter }, StringSplitOptions.RemoveEmptyEntries);

				object parentProperty = root;
				bool valueTypeDetected = false;

				for (int i = 0; i < properties.Length - 1; i++)
				{
					PropertyInfo propertyToGet = parentProperty.GetType().GetProperty(properties[i]);
					object temp = propertyToGet.GetValue(parentProperty, null);
					Type type = propertyToGet.PropertyType;

					if (temp == null)
					{
						temp = Activator.CreateInstance(type);
						propertyToGet.SetValue(parentProperty, ChangeType(temp, propertyToGet.PropertyType), index: null);
					}

					if (valueTypeDetected || type.IsValueType)
					{
						valueTypeDetected = true;
						valueTypePropertyHierarchy.Push(new Tuple<object, object, PropertyInfo>(temp, parentProperty, propertyToGet));
					}

					parentProperty = temp;
				}

				PropertyInfo propertyToSet = parentProperty.GetType().GetProperty(properties.Last());

				string stringValue = propertyValue as string;
				// Support for unsupported data types, but only if the target type isn't a string... if the target type is a string, then this isn't unsupported data-type-ing
				if (stringValue != null && propertyToSet.PropertyType != typeof(string) && stringValue.StartsWith("_$¿="))
					propertyToSet.SetValue(parentProperty, Deserialise(stringValue.Substring(4), propertyToSet.PropertyType), index: null);
				else
					propertyToSet.SetValue(parentProperty, ChangeType(propertyValue, propertyToSet.PropertyType), index: null);

				object termValue = parentProperty;
				while (valueTypePropertyHierarchy.Count != 0)
				{
					Tuple<object, object, PropertyInfo> propertyTuple = valueTypePropertyHierarchy.Pop();
					propertyTuple.Item3.SetValue(propertyTuple.Item2, ChangeType(termValue, propertyTuple.Item3.PropertyType), index: null);
					termValue = propertyTuple.Item2;
				}

				return root;
			}
			catch (Exception ex)
			{
				Trace.TraceError(SR.TraceSetPropertyError, propertyPath, propertyValue, ex.Message);
				throw;
			}
		}

		static object Deserialise(string json, Type type)
		{
			using (var stringReader = new StringReader(json))
			using (var jsonTextReader = new JsonTextReader(stringReader))
				return GetSerialiser().Deserialize(jsonTextReader, type);
		}

		/// <summary>
		/// Creates a new <see cref="JsonSerializer"/> using the settings from <see cref="GetSerialisationSettings"/>.
		/// </summary>
		/// <returns>A new instance of <see cref="JsonSerializer"/>.</returns>
		static JsonSerializer GetSerialiser()
		{
			JsonSerializerSettings settings = GetSerialisationSettings();
			return JsonSerializer.Create(settings);
		}

		/// <summary>Creates an object of specified propertyType from propertyValue.</summary>
		/// <param name="propertyValue">The property value.</param>
		/// <param name="propertyType">The property type.</param>
		/// <returns>The <see cref="object"/>.</returns>
		private static object ChangeType(object propertyValue, Type propertyType)
		{
			Type underlyingType = Nullable.GetUnderlyingType(propertyType);
			Type type = underlyingType ?? propertyType;

			if (type.IsEnum)
			{
				return Enum.Parse(type, propertyValue.ToString());
			}

			if (type == typeof(DateTimeOffset))
			{
				return new DateTimeOffset((DateTime)propertyValue);
			}

			if (type == typeof(TimeSpan))
			{
				return TimeSpan.Parse(propertyValue.ToString(), CultureInfo.InvariantCulture);
			}

			if (type == typeof(uint))
			{
				return unchecked((uint)(int)propertyValue);
			}

			if (type == typeof(ulong))
			{
				return unchecked((ulong)(long)propertyValue);
			}

			return Convert.ChangeType(propertyValue, type, CultureInfo.InvariantCulture);
		}

		/// <summary>
		/// Indicates whether the object member should be skipped from being flattened
		/// </summary>
		/// <param name="propertyInfo">The property info.</param>
		/// <param name="objectPath">The object path.</param>
		/// <param name="operationContext">An <see cref="OperationContext"/> object that represents the context for the current operation.</param>
		/// <returns>The <see cref="bool"/> to indicate whether the object member should be skipped from being flattened.</returns>
		private static bool ShouldSkip(PropertyInfo propertyInfo, string objectPath, OperationContext operationContext)
		{
			if (!propertyInfo.CanWrite)
			{
				Trace.TraceInformation(SR.TraceNonExistingSetter, propertyInfo.Name, objectPath);
				return true;
			}

			if (!propertyInfo.CanRead)
			{
				Trace.TraceInformation(SR.TraceNonExistingGetter, propertyInfo.Name, objectPath);
				return true;
			}

			return Attribute.IsDefined(propertyInfo, typeof(IgnorePropertyAttribute));
		}

		/// <summary>
		/// The object reference equality comparer.
		/// </summary>
		private class ObjectReferenceEqualityComparer : IEqualityComparer<object>
		{
			/// <summary>
			/// Determines whether the specified object instances are considered equal.
			/// </summary>
			/// <param name="x">The first object to compare.</param>
			/// <param name="y">The second object to compare.</param>
			/// <returns>true if the objects are considered equal; otherwise, false. If both objA and objB are null, the method returns true.</returns>
			public new bool Equals(object x, object y)
			{
				return ReferenceEquals(x, y);
			}

			/// <summary>
			/// Serves as a hash function for a particular type.
			/// </summary>
			/// <returns>A hash code for the current <see cref="object"/>.</returns>
			public int GetHashCode(object obj)
			{
				return RuntimeHelpers.GetHashCode(obj);
			}
		}
	}

	internal class SR
	{
		public const string AbsoluteAddressNotPermitted = "Address '{0}' is an absolute address. Only relative addresses are permitted.";
		public const string AccountNameMismatch = "Account names do not match.  First account name is {0}, second is {1}.";
		public const string ArgumentEmptyError = "The argument must not be empty string.";
		public const string ArgumentOutOfRangeError = "The argument is out of range. Value passed: {0}";
		public const string ArgumentTooLargeError = "The argument '{0}' is larger than maximum of '{1}'";
		public const string ArgumentTooSmallError = "The argument '{0}' is smaller than minimum of '{1}'";
		public const string AttemptedEdmTypeForTheProperty = "Attempting to deserialize '{0}' as type '{1}'";
		public const string BatchWithRetreiveContainsOtherOperations = "A batch transaction with a retrieve operation cannot contain any other operations.";
		public const string BatchExceededMaximumNumberOfOperations = "The maximum number of operations allowed in one batch has been exceeded.";
		public const string BatchOperationRequiresPartitionKeyRowKey = "A batch non-retrieve operation requires a non-null partition key and row key.";
		public const string BatchErrorInOperation = "Element {0} in the batch returned an unexpected response code.";
		public const string BinaryMessageShouldUseBase64Encoding = "EncodeMessage should be true for binary message.";
		public const string Blob = "blob";
		public const string BlobDataCorrupted = "Blob data corrupted (integrity check failed), Expected value is '{0}', retrieved '{1}'";
		public const string BlobEndPointNotConfigured = "No blob endpoint configured.";
		public const string BlobInvalidSequenceNumber = "The sequence number may not be specified for an increment operation.";
		public const string BlobOverMaxBlockLimit = "The total blocks required for this upload exceeds the maximum block limit. Please increase the block size if applicable and ensure the Blob size is not greater than the maximum Blob size limit.";
		public const string BlobStreamAlreadyCommitted = "Blob stream has already been committed once.";
		public const string BlobStreamFlushPending = "Blob stream has a pending flush operation. Please call EndFlush first.";
		public const string BlobStreamReadPending = "Blob stream has a pending read operation. Please call EndRead first.";
		public const string BlobTypeMismatch = "Blob type of the blob reference doesn't match blob type of the blob.";
		public const string BufferTooSmall = "The provided buffer is too small to fit in the blob data given the offset.";
		public const string BufferManagerProvidedIncorrectLengthBuffer = "The IBufferManager provided an incorrect length buffer to the stream, Expected {0}, received {1}. Buffer length should equal the value returned by IBufferManager.GetDefaultBufferSize().";
		public const string CannotCreateSASSignatureForGivenCred = "Cannot create Shared Access Signature as the credentials does not have account name information. Please check that the credentials used support creating Shared Access Signature.";
		public const string CannotCreateSASWithoutAccountKey = "Cannot create Shared Access Signature unless Account Key credentials are used.";
		public const string CannotModifySnapshot = "Cannot perform this operation on a blob representing a snapshot.";
		public const string CannotModifyShareSnapshot = "Cannot perform this operation on a share representing a snapshot.";
		public const string CannotTransformNonHttpsUriWithHttpsOnlyCredentials = "Cannot transform a Uri object using a StorageCredentials object that is marked HTTPS only.";
		public const string CannotUpdateKeyWithoutAccountKeyCreds = "Cannot update key unless Account Key credentials are used.";
		public const string CannotUpdateSasWithoutSasCreds = "Cannot update Shared Access Signature unless Sas credentials are used.";
		public const string ConcurrentOperationsNotSupported = "Could not acquire exclusive use of the TableServiceContext, Concurrent operations are not supported.";
		public const string Container = "container";
		public const string ContentMD5NotCalculated = "The operation requires a response body but no data was copied to the destination buffer.";
		public const string CopyAborted = "The copy operation has been aborted by the user.";
		public const string CopyFailed = "The copy operation failed with the following error message: {0}";
		public const string CryptoError = "Cryptographic error occurred. Please check the inner exception for more details.";
		public const string CryptoFunctionFailed = "Crypto function failed with error code '{0}'";
		public const string DecryptionLogicError = "Decryption logic threw error. Please check the inner exception for more details.";
		public const string DeleteSnapshotsNotValidError = "The option '{0}' must be 'None' to delete a specific snapshot specified by '{1}'";
		public const string Directory = "directory";
		public const string EmptyBatchOperation = "Cannot execute an empty batch operation";
		public const string EncryptedMessageTooLarge = "Encrypted Messages cannot be larger than {0} bytes. Please note that encrypting queue messages can increase their size.";
		public const string EncryptionDataNotPresentError = "Encryption data does not exist. If you do not want to decrypt the data, please do not set the RequireEncryption flag on request options.";
		public const string EncryptionLogicError = "Encryption logic threw error. Please check the inner exception for more details.";
		public const string EncryptedMessageDeserializingError = "Error while de-serializing the encrypted queue message string from the wire. Please check inner exception for more details.";
		public const string EncryptionNotSupportedForOperation = "Encryption is not supported for the current operation. Please ensure that EncryptionPolicy is not set on RequestOptions.";
		public const string EncryptingNullPropertiesNotAllowed = "Null properties cannot be encrypted. Please assign a default value to the property if you wish to encrypt it.";
		public const string EncryptionMetadataError = "Error while de-serializing the encryption metadata string from the wire.";
		public const string EncryptionNotSupportedForExistingBlobs = "Encryption is not supported for a blob that already exists. Please do not specify an encryption policy.";
		public const string EncryptionNotSupportedForFiles = "Encryption is not supported for files.";
		public const string EncryptionNotSupportedForPageBlobsOnPhone = "Encryption is not supported for PageBlobs on Windows Phone.";
		public const string EncryptionPolicyMissingInStrictMode = "Encryption Policy is mandatory when RequireEncryption is set to true. If you do not want to encrypt/decrypt data, please set RequireEncryption to false in request options.";
		public const string EncryptionProtocolVersionInvalid = "Invalid Encryption Agent. This version of the client library does not understand the Encryption Agent set on the blob.";
		public const string ETagMissingForDelete = "Delete requires an ETag (which may be the '*' wildcard).";
		public const string ETagMissingForMerge = "Merge requires an ETag (which may be the '*' wildcard).";
		public const string ETagMissingForReplace = "Replace requires an ETag (which may be the '*' wildcard).";
		public const string ExceptionOccurred = "An exception has occurred. For more information please deserialize this message via RequestResult.TranslateFromExceptionMessage.";
		public const string ExtendedErrorUnavailable = "An unknown error has occurred, extended error information not available.";
		public const string File = "file";
		public const string FileDataCorrupted = "File data corrupted (integrity check failed), Expected value is '{0}', retrieved '{1}'";
		public const string FileEndPointNotConfigured = "No file endpoint configured.";
		public const string FileStreamAlreadyCommitted = "File stream has already been committed once.";
		public const string FileStreamFlushPending = "File stream has a pending flush operation. Please call EndFlush first.";
		public const string FileStreamReadPending = "File stream has a pending read operation. Please call EndRead first.";
		public const string FailParseProperty = "Failed to parse property '{0}' with value '{1}' as type '{2}'";
		public const string GetServiceStatsInvalidOperation = "GetServiceStats cannot be run with a 'PrimaryOnly' location mode.";
		public const string IncorrectNumberOfBytes = "Incorrect number of bytes received. Expected '{0}', received '{1}'";
		public const string InternalStorageError = "Unexpected internal storage client error.";
		public const string InvalidAclType = "Invalid acl public access type returned '{0}'. Expected blob or container.";
		public const string InvalidBlobListItem = "Invalid blob list item returned";
		public const string InvalidCorsRule = "A CORS rule must contain at least one allowed origin and allowed method, and MaxAgeInSeconds cannot have a value less than zero.";
		public const string InvalidDelimiter = "\\ is an invalid delimiter.";
		public const string InvalidFileAclType = "Invalid acl public access type returned '{0}'. Expected file or share.";
		public const string InvalidEncryptionAlgorithm = "Invalid Encryption Algorithm found on the resource. This version of the client library does not support the specified encryption algorithm.";
		public const string InvalidEncryptionMode = "Invalid BlobEncryptionMode set on the policy. Please set it to FullBlob when the policy is used with UploadFromStream.";
		public const string InvalidFileListItem = "Invalid file list item returned";
		public const string InvalidGeoReplicationStatus = "Invalid geo-replication status in response: '{0}'";
		public const string InvalidHeaders = "Headers are not supported in the 2012-02-12 version.";
		public const string InvalidIPAddress = "Error when parsing IP address: IP address is invalid.";
		public const string InvalidLeaseStatus = "Invalid lease status in response: '{0}'";
		public const string InvalidLeaseState = "Invalid lease state in response: '{0}'";
		public const string InvalidLeaseDuration = "Invalid lease duration in response: '{0}'";
		public const string InvalidListingDetails = "Invalid blob listing details specified.";
		public const string InvalidLoggingLevel = "Invalid logging operations specified.";
		public const string InvalidMetricsLevel = "Invalid metrics level specified.";
		public const string InvalidBlockSize = "Append block data should not exceed the maximum blob size condition value.";
		public const string InvalidPageSize = "Page data must be a multiple of 512 bytes.";
		public const string InvalidProtocolsInSAS = "Invalid value {0} for the SharedAccessProtocol parameter when creating a SharedAccessSignature.  Use 'null' if you do not wish to include a SharedAccessProtocol.";
		public const string InvalidResourceName = "Invalid {0} name. Check MSDN for more information about valid {0} naming.";
		public const string InvalidResourceNameLength = "Invalid {0} name length. The {0} name must be between {1} and {2} characters long.";
		public const string InvalidResourceReservedName = "Invalid {0} name. This {0} name is reserved.";
		public const string InvalidSASVersion = "SAS Version invalid. Valid versions include 2012-02-12 and 2013-08-15.";
		public const string InvalidStorageService = "Invalid storage service specified.";
		public const string IPAddressOrRangeMustBeNullInOldVersion = "IPAddressOrRange must be null when creating a SAS token with an older service version parameter.";
		public const string IPMustBeIPV4InSAS = "When specifying an IP Address in a SAS token, it must be an IPv4 address. Input address was {0}.";
		public const string IQueryableExtensionObjectMustBeTableQuery = "Query must be a TableQuery<T>";
		public const string JsonNotSupportedOnRT = "JSON payloads are not supported in Windows Runtime.";
		public const string JsonReaderNotInCompletedState = "The JSON reader has not yet reached the completed state.";
		public const string KeyAndResolverMissingError = "Key and Resolver are not initialized. Decryption requires either of them to be initialized.";
		public const string KeyMismatch = "Key mismatch. The key id stored on the service does not match the specified key.";
		public const string KeyMissingError = "Key is not initialized. Encryption requires it to be initialized.";
		public const string KeyResolverCannotResolveExistingKey = "KeyResolver is not able to resolve the existing encryption key used to encrypt this blob.";
		public const string KeyRotationInvalidAccessCondition = @"Cannot supply an AccessCondition with an ETag or Modified-Since condition for key rotation.  An If-Match condition will be automatically applied.";
		public const string KeyRotationNoEncryptionKey = "Must supply a new encryption key as the Encryption Policy's \"Key\" parameter when rotating encryption keys.";
		public const string KeyRotationNoEncryptionKeyResolver = "Must supply a key resolver on the encryption policy when rotating encryption keys.";
		public const string KeyRotationNoEncryptionMetadata = "Cannot rotate the encryption key when encryption metadata is not available on the blob.  Either the blob is not encrypted, or you need to call FetchAttributes before calling this method.";
		public const string KeyRotationNoEncryptionPolicy = "Encryption Policy on the Request Options cannot be null for an encryption key rotation call.";
		public const string KeyRotationNoEtag = "Cannot rotate the encryption key when the ETag is not available on the blob.  You need to call FetchAttributes before calling this method.";
		public const string KeyRotationNoKeyID = "Cannot rotate encryption key when the encryption metadata does not contain a KeyID.";
		public const string KeyRotationPreconditionFailed = "Precondition failed.  If this is due to an incorrect ETag value, call FetchAttributes on the local blob object and retry rotating the encryption key.";
		public const string LeaseConditionOnSource = "A lease condition cannot be specified on the source of a copy.";
		public const string LeaseTimeNotReceived = "Valid lease time expected but not received from the service.";
		public const string LengthNotInRange = "The length provided is out of range. The range must be between 0 and the length of the byte array.";
		public const string ListSnapshotsWithDelimiterError = "Listing snapshots is only supported in flat mode (no delimiter). Consider setting the useFlatBlobListing parameter to true.";
		public const string LogStreamEndError = "Error parsing log record: unexpected end of stream at position '{0}'.";
		public const string LogStreamDelimiterError = "Error parsing log record: expected the delimiter '{0}', but read '{1}' at position '{2}'.";
		public const string LogStreamParseError = "Error parsing log record: could not parse '{0}' using format: {1}";
		public const string LogStreamQuoteError = "Error parsing log record: unexpected quote character found. String so far: '{0}'. Character read: '{1}'";
		public const string LogVersionUnsupported = "A storage log version of {0} is unsupported.";
		public const string LoggingVersionNull = "The logging version is null or empty.";
		public const string MD5MismatchError = "Calculated MD5 does not match existing property";
		public const string MD5NotPossible = "MD5 cannot be calculated for an existing blob because it would require reading the existing data. Please disable StoreBlobContentMD5.";
		public const string MD5NotPresentError = "MD5 does not exist. If you do not want to force validation, please disable UseTransactionalMD5.";
		public const string MessageTooLarge = "Messages cannot be larger than {0} bytes.";
		public const string MetricVersionNull = "The metrics version is null or empty.";
		public const string MissingAccountInformationInUri = "Cannot find account information inside Uri '{0}'";
		public const string MissingContainerInformation = "Invalid blob address '{0}', missing container information";
		public const string MissingCredentials = "No credentials provided.";
		public const string MissingLeaseIDChanging = "A lease ID must be specified when changing a lease.";
		public const string MissingLeaseIDReleasing = "A lease ID must be specified when releasing a lease.";
		public const string MissingLeaseIDRenewing = "A lease ID must be specified when renewing a lease.";
		public const string MissingMandatoryParametersForSAS = "Missing mandatory parameters for valid Shared Access Signature";
		public const string MissingShareInformation = "Invalid file address '{0}', missing share information";
		public const string MissingWrappingIV = "A key wrapping IV must be present in the encryption metadata while decrypting.";
		public const string StorageUriMustMatch = "Primary and secondary location URIs in a StorageUri must point to the same resource.";
		public const string MultipleCredentialsProvided = "Cannot provide credentials as part of the address and as constructor parameter. Either pass in the address or use a different constructor.";
		public const string MultipleSnapshotTimesProvided = "Multiple different snapshot times provided as part of query '{0}' and as constructor parameter '{1}'.";
		public const string NegativeBytesRequestedInCopy = "Internal Error - negative copyLength requested when attempting to copy a stream.  CopyLength = {0}, totalBytes = {1}, total bytes recorded so far = {2}.";
		public const string NoPropertyResolverAvailable = "No property resolver available. Deserializing the entity properties as strings.";
		public const string OffsetNotInRange = "The offset provided is out of range. The range must be between 0 and the length of the byte array.";
		public const string ODataReaderNotInCompletedState = "OData Reader state expected to be Completed state. Actual state: {0}.";
		public const string OperationCanceled = "Operation was canceled by user.";
		public const string ParseError = "Error parsing value";
		public const string PartitionKey = "All entities in a given batch must have the same partition key.";
		public const string PathStyleUriMissingAccountNameInformation = "Missing account name information inside path style uri. Path style uris should be of the form http://<IPAddressPlusPort>/<accountName>";
		public const string PayloadFormat = "Setting payload format for the request to '{0}'.";
		public const string PreconditionFailed = "The condition specified using HTTP conditional header(s) is not met.";
		public const string PreconditionFailureIgnored = "Pre-condition failure on a retry is being ignored since the request should have succeeded in the first attempt.";
		public const string PrimaryOnlyCommand = "This operation can only be executed against the primary storage location.";
		public const string PropertyDelimiterExistsInPropertyName = "Property delimiter: {0} exists in property name: {1}. Object Path: {2}";
		public const string PropertyResolverCacheDisabled = "Property resolver cache is disabled.";
		public const string PropertyResolverThrewError = "The custom property resolver delegate threw an exception. Check the inner exception for more details.";
		public const string ProtocolsMustBeNullInOldVersion = "SharedAccessProtocol must be null when creating a SAS token with an older service version parameter.";
		public const string PutBlobNeedsStoreBlobContentMD5 = "When uploading a blob in a single request, StoreBlobContentMD5 must be set to true if UseTransactionalMD5 is true, because the MD5 calculated for the transaction will be stored in the blob.";
		public const string QueryBuilderKeyNotFound = "'{0}' key not found in the query builder.";
		public const string Queue = "queue";
		public const string QueueEndPointNotConfigured = "No queue endpoint configured.";
		public const string RangeDownloadNotPermittedOnPhone = "Windows Phone does not support downloading closed ranges from an encrypted blob. Please download the full blob or an open range (by specifying length as null)";
		public const string RecursiveReferencedObject = "Recursive reference detected. Object Path: {0} Property Type: {1}.";
		public const string RelativeAddressNotPermitted = "Address '{0}' is a relative address. Only absolute addresses are permitted.";
		public const string ResourceConsumed = "Resource consumed";
		public const string ResourceNameEmpty = "Invalid {0} name. The {0} name may not be null, empty, or whitespace only.";
		public const string RetrieveWithContinuationToken = "Retrieved '{0}' results with continuation token '{1}'.";
		public const string SecondaryOnlyCommand = "This operation can only be executed against the secondary storage location.";
		public const string SetServicePropertiesRequiresNonNullSettings = "At least one service property needs to be non-null for SetServiceProperties API.";
		public const string Share = "share";
		public const string StartTimeExceedsEndTime = "StartTime invalid. The start time '{0}' occurs after the end time '{1}'.";
		public const string StorageUriMissingLocation = "The Uri for the target storage location is not specified. Please consider changing the request's location mode.";
		public const string StreamLengthError = "The length of the stream exceeds the permitted length.";
		public const string StreamLengthMismatch = "Cannot specify both copyLength and maxLength.";
		public const string StreamLengthShortError = "The requested number of bytes exceeds the length of the stream remaining from the specified position.";
		public const string Table = "table";
		public const string TableEndPointNotConfigured = "No table endpoint configured.";
		public const string TableQueryDynamicPropertyAccess = "Accessing property dictionary of DynamicTableEntity requires a string constant for property name.";
		public const string TableQueryEntityPropertyInQueryNotSupported = "Referencing {0} on EntityProperty only supported with properties dictionary exposed via DynamicTableEntity.";
		public const string TableQueryFluentMethodNotAllowed = "Fluent methods may not be invoked on a Query created via CloudTable.CreateQuery<T>()";
		public const string TableQueryMustHaveQueryProvider = "Unknown Table. The TableQuery does not have an associated CloudTable Reference. Please execute the query via the CloudTable ExecuteQuery APIs.";
		public const string TableQueryTypeMustImplementITableEnitty = "TableQuery Generic Type must implement the ITableEntity Interface";
		public const string TableQueryTypeMustHaveDefaultParameterlessCtor = "TableQuery Generic Type must provide a default parameterless constructor.";
		public const string TakeCountNotPositive = "Take count must be positive and greater than 0.";
		public const string TimeoutExceptionMessage = "The client could not finish the operation within specified timeout.";
		public const string TooManyPolicyIdentifiers = "Too many '{0}' shared access policy identifiers provided. Server does not support setting more than '{1}' on a single container, queue, table, or share.";
		public const string TooManyPathSegments = "The count of URL path segments (strings between '/' characters) as part of the blob name cannot exceed 254.";
		public const string TraceAbort = "Aborting pending request due to timeout.";
		public const string TraceAbortError = "Could not abort pending request because of {0}.";
		public const string TraceAbortRetry = "Aborting pending retry due to user request.";
		public const string TraceDispose = "Disposing action invoked.";
		public const string TraceDisposeError = "Disposing action threw an exception : {0}.";
		public const string TraceDownload = "Downloading response body.";
		public const string TraceDownloadError = "Downloading error response body.";
		public const string TraceRetryInfo = "The extended retry policy set the next location to {0} and updated the location mode to {1}.";
		public const string TraceGenericError = "Exception thrown during the operation: {0}.";
		public const string TraceGetResponse = "Waiting for response.";
		public const string TraceGetResponseError = "Exception thrown while waiting for response: {0}.";
		public const string TraceIgnoreAttribute = "Omitting property '{0}' from serialization/de-serialization because IgnoreAttribute has been set on that property.";
		public const string TraceInitLocation = "Starting operation with location {0} per location mode {1}.";
		public const string TraceInitRequestError = "Exception thrown while initializing request: {0}.";
		public const string TraceMissingDictionaryEntry = "Omitting property '{0}' from de-serialization because there is no corresponding entry in the dictionary provided.";
		public const string TraceNextLocation = "The next location has been set to {0}, based on the location mode.";
		public const string TraceNonPublicGetSet = "Omitting property '{0}' from serialization/de-serialization because the property's getter/setter are not public.";
		public const string TraceNonExistingGetter = "Omitting property: {0} from serialization/de-serialization because the property does not have a getter. Object path: {1}";
		public const string TraceNonExistingSetter = "Omitting property: {0} from serialization/de-serialization because the property does not have a setter."
			+ " The property needs to have at least a private setter. Object Path: {1}";
		public const string TracePrepareUpload = "Preparing to write request data.";
		public const string TracePrepareUploadError = "Exception thrown while preparing to write request data: {0}.";
		public const string TracePreProcessDone = "Response headers were processed successfully, proceeding with the rest of the operation.";
		public const string TracePreProcessError = "Exception thrown while processing response: {0}.";
		public const string TracePostProcess = "Processing response body.";
		public const string TracePostProcessError = "Exception thrown while ending operation: {0}.";
		public const string TraceResponse = "Response received. Status code = {0}, Request ID = {1}, Content-MD5 = {2}, ETag = {3}.";
		public const string TraceRetry = "Retrying failed operation.";
		public const string TraceRetryCheck = "Checking if the operation should be retried. Retry count = {0}, HTTP status code = {1}, Retryable exception = {2}, Exception = {3}.";
		public const string TraceRetryDecisionPolicy = "Retry policy did not allow for a retry. Failing with {0}.";
		public const string TraceRetryDecisionTimeout = "Operation cannot be retried because the maximum execution time has been reached. Failing with {0}.";
		public const string TraceRetryDelay = "Operation will be retried after {0}ms.";
		public const string TraceRetryError = "Exception thrown while retrying operation: {0}.";
		public const string TraceSetPropertyError = "Exception thrown while trying to set property value. Property Path: {0} Property Value: {1}. Exception Message: {2}";
		public const string TraceStartRequestAsync = "Starting asynchronous request to {0}.";
		public const string TraceStartRequestSync = "Starting synchronous request to {0}.";
		public const string TraceStringToSign = "StringToSign = {0}.";
		public const string TraceSuccess = "Operation completed successfully.";
		public const string TraceUpload = "Writing request data.";
		public const string TraceUploadError = "Exception thrown while writing request data: {0}.";
		public const string UndefinedBlobType = "The blob type cannot be undefined.";
		public const string UnexpectedElement = "Unexpected Element '{0}'";
		public const string UnexpectedEmptyElement = "Unexpected Empty Element '{0}'";
		public const string UnexpectedParameterInSAS = "The parameter `api-version` should not be included in the SAS token. Please allow the library to set the  `api-version` parameter.";
		public const string UnexpectedContinuationType = "Unexpected Continuation Type";
		public const string UnexpectedLocation = "Unexpected Location '{0}'";
		public const string UnexpectedResponseCode = "Unexpected response code, Expected:{0}, Received:{1}";
		public const string UnsupportedPropertyTypeForEncryption = "Unsupported type : {0} encountered during encryption. Only string properties can be encrypted on the client side.";
		public const string UnsupportedPropertyTypeForEntityPropertyConversion = "Unsupported type : {0} encountered during conversion to EntityProperty. Object Path: {1}";
		public const string UpdateMessageVisibilityRequired = "Calls to UpdateMessage must include the Visibility flag.";
		public const string UsingDefaultPropertyResolver = "Using the default property resolver to deserialize the entity.";
		public const string UsingUserProvidedPropertyResolver = "Using the property resolver provided via TableRequestOptions to deserialize the entity.";

#if WINDOWS_PHONE && WINDOWS_DESKTOP
		public const string WindowsPhoneDoesNotSupportMD5 = "MD5 is not supported on Windows Phone";
#endif
		// Table IQueryable Exception messages
		public const string ALinqCouldNotConvert = "Could not convert constant {0} expression to string.";
		public const string ALinqMethodNotSupported = "The method '{0}' is not supported.";
		public const string ALinqUnaryNotSupported = "The unary operator '{0}' is not supported.";
		public const string ALinqBinaryNotSupported = "The binary operator '{0}' is not supported.";
		public const string ALinqConstantNotSupported = "The constant for '{0}' is not supported.";
		public const string ALinqTypeBinaryNotSupported = "An operation between an expression and a type is not supported.";
		public const string ALinqConditionalNotSupported = "The conditional expression is not supported.";
		public const string ALinqParameterNotSupported = "The parameter expression is not supported.";
		public const string ALinqMemberAccessNotSupported = "The member access of '{0}' is not supported.";
		public const string ALinqLambdaNotSupported = "Lambda Expressions not supported.";
		public const string ALinqNewNotSupported = "New Expressions not supported.";
		public const string ALinqMemberInitNotSupported = "Member Init Expressions not supported.";
		public const string ALinqListInitNotSupported = "List Init Expressions not supported.";
		public const string ALinqNewArrayNotSupported = "New Array Expressions not supported.";
		public const string ALinqInvocationNotSupported = "Invocation Expressions not supported.";
		public const string ALinqUnsupportedExpression = "The expression type {0} is not supported.";
		public const string ALinqCanOnlyProjectTheLeaf = "Can only project the last entity type in the query being translated.";
		public const string ALinqCantCastToUnsupportedPrimitive = "Can't cast to unsupported type '{0}'";
		public const string ALinqCantTranslateExpression = "The expression {0} is not supported.";
		public const string ALinqCantNavigateWithoutKeyPredicate = "Navigation properties can only be selected from a single resource. Specify a key predicate to restrict the entity set to a single instance.";
		public const string ALinqCantReferToPublicField = "Referencing public field '{0}' not supported in query option expression.  Use public property instead.";
		public const string ALinqCannotConstructKnownEntityTypes = "Construction of entity type instances must use object initializer with default constructor.";
		public const string ALinqCannotCreateConstantEntity = "Referencing of local entity type instances not supported when projecting results.";
		public const string ALinqExpressionNotSupportedInProjectionToEntity = "Initializing instances of the entity type {0} with the expression {1} is not supported.";
		public const string ALinqExpressionNotSupportedInProjection = "Constructing or initializing instances of the type {0} with the expression {1} is not supported.";
		public const string ALinqProjectionMemberAssignmentMismatch = "Cannot initialize an instance of entity type '{0}' because '{1}' and '{2}' do not refer to the same source entity.";
		public const string ALinqPropertyNamesMustMatchInProjections = "Cannot assign the value from the {0} property to the {1} property.  When projecting results into a entity type, the property names of the source type and the target type must match for the properties being projected.";
		public const string ALinqQueryOptionOutOfOrder = "The {0} query option cannot be specified after the {1} query option.";
		public const string ALinqQueryOptionsOnlyAllowedOnLeafNodes = "Can only specify query options (orderby, where, take, skip) after last navigation.";
	}
}
*/