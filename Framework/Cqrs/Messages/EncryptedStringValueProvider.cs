#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using Newtonsoft.Json.Serialization;
using System;
using System.IO;
using System.Reflection;
using System.Security.Cryptography;
using System.Text;

namespace Cqrs.Messages
{
	class EncryptedStringValueProvider : IValueProvider
	{
		PropertyInfo targetProperty;
		private byte[] encryptionKey;

		public EncryptedStringValueProvider(PropertyInfo targetProperty, byte[] encryptionKey)
		{
			this.targetProperty = targetProperty;
			this.encryptionKey = encryptionKey;
		}

		private object GetValue(PropertyInfo targetProperty, object target)
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
		public object GetValue(object target)
		{
			byte[] buffer = null;
			if (typeof(string) == targetProperty.PropertyType)
			{
				string value = (string)GetValue(targetProperty, target);
				buffer = Encoding.UTF8.GetBytes(value);
			}
			else if (typeof(short) == targetProperty.PropertyType)
			{
				string value = ((short)GetValue(targetProperty, target)).ToString();
				buffer = Encoding.UTF8.GetBytes(value);
			}
			else if (typeof(int) == targetProperty.PropertyType)
			{
				string value = ((int)GetValue(targetProperty, target)).ToString();
				buffer = Encoding.UTF8.GetBytes(value);
			}
			else if (typeof(long) == targetProperty.PropertyType)
			{
				string value = ((long)GetValue(targetProperty, target)).ToString();
				buffer = Encoding.UTF8.GetBytes(value);
			}
			else if (typeof(ushort) == targetProperty.PropertyType)
			{
				string value = ((ushort)GetValue(targetProperty, target)).ToString();
				buffer = Encoding.UTF8.GetBytes(value);
			}
			else if (typeof(uint) == targetProperty.PropertyType)
			{
				string value = ((uint)GetValue(targetProperty, target)).ToString();
				buffer = Encoding.UTF8.GetBytes(value);
			}
			else if (typeof(ulong) == targetProperty.PropertyType)
			{
				string value = ((ulong)GetValue(targetProperty, target)).ToString();
				buffer = Encoding.UTF8.GetBytes(value);
			}
			else if (typeof(decimal) == targetProperty.PropertyType)
			{
				string value = ((decimal)GetValue(targetProperty, target)).ToString();
				buffer = Encoding.UTF8.GetBytes(value);
			}
			else if (typeof(float) == targetProperty.PropertyType)
			{
				string value = ((float)GetValue(targetProperty, target)).ToString();
				buffer = Encoding.UTF8.GetBytes(value);
			}
			else if (typeof(double) == targetProperty.PropertyType)
			{
				string value = ((double)GetValue(targetProperty, target)).ToString();
				buffer = Encoding.UTF8.GetBytes(value);
			}
			else if (typeof(Guid) == targetProperty.PropertyType)
			{
				string value = ((Guid)GetValue(targetProperty, target)).ToString();
				buffer = Encoding.UTF8.GetBytes(value);
			}
			else if (typeof(Enum) == targetProperty.PropertyType)
			{
				string value = ((Enum)GetValue(targetProperty, target)).ToString();
				buffer = Encoding.UTF8.GetBytes(value);
			}

			using (MemoryStream inputStream = new MemoryStream(buffer, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					using (AesManaged aes = new AesManaged { Key = encryptionKey })
					{
						byte[] iv = aes.IV;  // first access generates a new IV
						outputStream.Write(iv, 0, iv.Length);
						outputStream.Flush();

						ICryptoTransform encryptor = aes.CreateEncryptor(encryptionKey, iv);
						using (CryptoStream cryptoStream = new CryptoStream(outputStream, encryptor, CryptoStreamMode.Write))
						{
							inputStream.CopyTo(cryptoStream);
						}

						return Convert.ToBase64String(outputStream.ToArray());
					}
				}
			}
		}

		private void SetValue(PropertyInfo targetProperty, object target, object value)
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
		public void SetValue(object target, object value)
		{
			byte[] buffer = Convert.FromBase64String((string)value);

			string decryptedValue;
			using (MemoryStream inputStream = new MemoryStream(buffer, false))
			{
				using (MemoryStream outputStream = new MemoryStream())
				{
					using (AesManaged aes = new AesManaged { Key = encryptionKey })
					{
						byte[] iv = new byte[16];
						int bytesRead = inputStream.Read(iv, 0, 16);
						if (bytesRead < 16)
						{
							throw new CryptographicException("IV is missing or invalid.");
						}

						ICryptoTransform decryptor = aes.CreateDecryptor(encryptionKey, iv);
						using (CryptoStream cryptoStream = new CryptoStream(inputStream, decryptor, CryptoStreamMode.Read))
						{
							cryptoStream.CopyTo(outputStream);
						}

						decryptedValue = Encoding.UTF8.GetString(outputStream.ToArray());
					}
				}
			}

			object finalValue = null;
			if (typeof(string) == targetProperty.PropertyType)
				finalValue = decryptedValue;
			else if (typeof(short) == targetProperty.PropertyType)
				finalValue = short.Parse(decryptedValue);
			else if (typeof(int) == targetProperty.PropertyType)
				finalValue = int.Parse(decryptedValue);
			else if (typeof(long) == targetProperty.PropertyType)
				finalValue = long.Parse(decryptedValue);
			else if (typeof(ushort) == targetProperty.PropertyType)
				finalValue = ushort.Parse(decryptedValue);
			else if (typeof(uint) == targetProperty.PropertyType)
				finalValue = uint.Parse(decryptedValue);
			else if (typeof(ulong) == targetProperty.PropertyType)
				finalValue = ulong.Parse(decryptedValue);
			else if (typeof(decimal) == targetProperty.PropertyType)
				finalValue = decimal.Parse(decryptedValue);
			else if (typeof(float) == targetProperty.PropertyType)
				finalValue = float.Parse(decryptedValue);
			else if (typeof(double) == targetProperty.PropertyType)
				finalValue = double.Parse(decryptedValue);
			else if (typeof(Guid) == targetProperty.PropertyType)
				finalValue = new Guid(decryptedValue);
			else if (typeof(Enum) == targetProperty.PropertyType)
				finalValue = Enum.Parse(targetProperty.PropertyType, decryptedValue);

			SetValue(targetProperty, target, finalValue);
		}
	}
}