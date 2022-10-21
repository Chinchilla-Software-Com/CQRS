#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Newtonsoft.Json.Serialization;
using System.Net.Sockets;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Cqrs.Messages
{
	internal class EncryptedStringValueProvider : IValueProvider
	{
		public PropertyInfo TargetProperty { get; protected set; }

		public byte[] EncryptionKey { get; protected set; }

		public EncryptionService EncryptionService { get; protected set; }

		public EncryptedStringValueProvider(EncryptionService encryptionService, PropertyInfo targetProperty, byte[] encryptionKey)
		{
			EncryptionService = encryptionService;
			TargetProperty = targetProperty;
			EncryptionKey = encryptionKey;
		}

		public EncryptedStringValueProvider(EncryptionService encryptionService, PropertyInfo targetProperty, string encryptionKey)
		{
			EncryptionService = encryptionService;
			TargetProperty = targetProperty;

			byte[] encryptionKeyBytes;
			using (SHA256Managed sha = new SHA256Managed())
				encryptionKeyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));

			EncryptionKey = encryptionKeyBytes;
		}

		protected virtual object GetValue(PropertyInfo targetProperty, object target)
		{
#if NET40
			return targetProperty.GetValue(target, null);
#else
			return targetProperty.GetValue(target);
#endif
		}

		// GetValue is called by Json.Net during serialization.
		// The target parameter has the object from which to read the unencrypted string;
		// the return value is an encrypted string that gets written to the JSON
		public virtual object GetValue(object target)
		{
			return EncryptionService.Encrypt(EncryptionKey, TargetProperty.PropertyType, GetValue(TargetProperty, target));
		}

		protected virtual void SetValue(PropertyInfo targetProperty, object target, object value)
		{
#if NET40
			targetProperty.SetValue(target, value, null);
#else
			targetProperty.SetValue(target, value);
#endif
		}

		// SetValue gets called by Json.Net during deserialization.
		// The value parameter has the encrypted value read from the JSON;
		// target is the object on which to set the decrypted value.
		public virtual void SetValue(object target, object value)
		{
			object finalValue = EncryptionService.Decrypt(EncryptionKey, TargetProperty.PropertyType, (string)value);

			SetValue(TargetProperty, target, finalValue);
		}
	}
}