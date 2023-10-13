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

using Cqrs.Configuration;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace Cqrs.Messages
{
	/// <summary>
	/// A <see cref="IContractResolver"/> that can handle encryption as specified by <see cref="EncryptedAttribute"/> .
	/// </summary>
	public partial class EncryptedContractResolver
		 : DefaultContractResolver
	{
		IList<Type> supportedTypes = new List<Type>
		{
			typeof(string),
			typeof(short),
			typeof(int),
			typeof(long),
			typeof(ushort),
			typeof(uint),
			typeof(ulong),
			typeof(decimal),
			typeof(float),
			typeof(double),
			typeof(Guid),
			typeof(Enum)
		};

		/// <summary>
		/// Instantiates a new instance of the <see cref="EncryptedContractResolver"/>
		/// </summary>
		/// <param name="secretFactory">The <see cref="ISecretFactory"/> to use to obtain secrets</param>
		public EncryptedContractResolver(ISecretFactory secretFactory)
		{
			SecretFactory = secretFactory;
		}

		/// <summary>
		/// The <see cref="ISecretFactory"/> used to obtain secrets
		/// </summary>
		protected ISecretFactory SecretFactory { get; set; }

		/// <summary>
		/// Creates properties for the given <see cref="JsonContract"/>. If a property has the <see cref="EncryptedAttribute"/> applied,
		/// then the <see cref="JsonProperty.ValueProvider"/> is set to a custom provider to handle encryption
		/// and the <see cref="JsonProperty.PropertyType"/> is set to <see cref="string"/>
		/// </summary>
		/// <param name="type">The type to create properties for.</param>
		/// <param name="memberSerialization">The member serialization mode for the type.</param>
		/// <returns>Properties for the given <see cref="JsonContract"/>.</returns>
		protected override IList<JsonProperty> CreateProperties(Type type, MemberSerialization memberSerialization)
		{
			IList<JsonProperty> props = base.CreateProperties(type, memberSerialization);

			// Find all string properties that have a [JsonEncrypt] attribute applied
			// and attach an EncryptedStringValueProvider instance to them
			foreach (JsonProperty prop in props.Where(p => supportedTypes.Any(st => p.PropertyType.IsAssignableFrom(st))))
			{
				PropertyInfo pi = type.GetProperty(prop.UnderlyingName);
				if (pi != null)
				{
					EncryptedAttribute encryptedAttribute =
#if NET40_OR_GREATER
						pi.GetCustomAttributes(true).Where(ca => ca is EncryptedAttribute).FirstOrDefault()
#else
						pi.GetCustomAttribute(typeof(EncryptedAttribute), true)
#endif
						as EncryptedAttribute;
					if (encryptedAttribute != null)
					{
						string secret = SecretFactory.GetSecret(encryptedAttribute.KeyName);

						prop.ValueProvider = new EncryptedStringValueProvider(new EncryptionService(), pi, secret);
						prop.PropertyType = typeof(string);
					}
				}
			}

			return props;
		}
	}
}