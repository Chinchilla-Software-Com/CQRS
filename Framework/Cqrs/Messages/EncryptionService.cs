#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.IO;
using System;
using System.Security.Cryptography;
using System.Text;
using System.Net.Sockets;

namespace Cqrs.Messages
{
	/// <summary>
	/// Encrypts values to base64 and decrypts from base64
	/// </summary>
	public class EncryptionService
		: IEncryptionService
	{
		/// <summary>
		/// Encrypts the provided <paramref name="value"/> to a base64 string.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of <paramref name="value"/>.</typeparam>
		/// <param name="encryptionKey">The encryption key to use.</param>
		/// <param name="value">The value to encrypt.</param>
		/// <returns>A base64 encoded string.</returns>
		public virtual string Encrypt<T>(string encryptionKey, T value)
		{
			byte[] encryptionKeyBytes;
			using (SHA256Managed sha = new SHA256Managed())
				encryptionKeyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));

			return Encrypt<T>(encryptionKeyBytes, value);
		}

		/// <summary>
		/// Encrypts the provided <paramref name="value"/> to a base64 string.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of <paramref name="value"/>.</typeparam>
		/// <param name="encryptionKey">The encryption key to use.</param>
		/// <param name="value">The value to encrypt.</param>
		/// <returns>A base64 encoded string.</returns>
		public virtual string Encrypt<T>(byte[] encryptionKey, T value)
		{
			return Encrypt(encryptionKey, typeof(T), value);
		}

		/// <summary>
		/// Encrypts a value to a base64 string.
		/// </summary>
		/// <param name="encryptionKey">The encryption key to use.</param>
		/// <param name="type">The <see cref="Type"/> of <paramref name="value"/>.</param>
		/// <param name="value">The value to encrypt.</param>
		/// <returns>A base64 encoded string.</returns>
		public virtual string Encrypt(string encryptionKey, Type type, object value)
		{
			byte[] encryptionKeyBytes;
			using (SHA256Managed sha = new SHA256Managed())
				encryptionKeyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));

			return Encrypt(encryptionKeyBytes, type, value);
		}

		/// <summary>
		/// Encrypts a value to a base64 string.
		/// </summary>
		/// <param name="encryptionKey">The encryption key to use.</param>
		/// <param name="type">The <see cref="Type"/> of <paramref name="value"/>.</param>
		/// <param name="value">The value to encrypt.</param>
		/// <returns>A base64 encoded string.</returns>
		public virtual string Encrypt(byte[] encryptionKey, Type type, object value)
		{
			string strungValue;
			if (typeof(string) == type)
				strungValue = (string)value;
			else if (typeof(short) == type)
				strungValue = ((short)value).ToString();
			else if (typeof(int) == type)
				strungValue = ((int)value).ToString();
			else if (typeof(long) == type)
				strungValue = ((long)value).ToString();
			else if (typeof(ushort) == type)
				strungValue = ((ushort)value).ToString();
			else if (typeof(uint) == type)
				strungValue = ((uint)value).ToString();
			else if (typeof(ulong) == type)
				strungValue = ((ulong)value).ToString();
			else if (typeof(decimal) == type)
				strungValue = ((decimal)value).ToString();
			else if (typeof(float) == type)
				strungValue = ((float)value).ToString();
			else if (typeof(double) == type)
				strungValue = ((double)value).ToString();
			else if (typeof(Guid) == type)
				strungValue = ((Guid)value).ToString();
			else if (typeof(Enum) == type)
				strungValue = ((Enum)value).ToString();
			else
				throw new InvalidOperationException($"Data type {type.FullName} not supported.");
			byte[] buffer = Encoding.UTF8.GetBytes(strungValue);

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

		/// <summary>
		/// Decrypts the provided <paramref name="value"/> from base64 back to a typed value.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to decrypt to.</typeparam>
		/// <param name="encryptionKey">The encryption key to use.</param>
		/// <param name="value">The value to decrypt.</param>
		/// <returns>The typed decrypted value.</returns>
		public virtual object Decrypt<T>(string encryptionKey, string value)
		{
			byte[] encryptionKeyBytes;
			using (SHA256Managed sha = new SHA256Managed())
				encryptionKeyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));

			return Decrypt<T>(encryptionKeyBytes, value);
		}

		/// <summary>
		/// Decrypts the provided <paramref name="value"/> from base64 back to a typed value.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to decrypt to.</typeparam>
		/// <param name="encryptionKey">The encryption key to use.</param>
		/// <param name="value">The value to decrypt.</param>
		/// <returns>The typed decrypted value.</returns>
		public virtual object Decrypt<T>(byte[] encryptionKey, string value)
		{
			return (T)Decrypt(encryptionKey, typeof(T), value);
		}

		/// <summary>
		/// Decrypts the provided <paramref name="value"/> from base64 back to a typed value.
		/// </summary>
		/// <param name="encryptionKey">The encryption key to use.</param>
		/// <param name="type">The <see cref="Type"/> of <paramref name="value"/>.</param>
		/// <param name="value">The value to decrypt.</param>
		/// <returns>The typed decrypted value.</returns>
		public virtual object Decrypt(string encryptionKey, Type type, string value)
		{
			byte[] encryptionKeyBytes;
			using (SHA256Managed sha = new SHA256Managed())
				encryptionKeyBytes = sha.ComputeHash(Encoding.UTF8.GetBytes(encryptionKey));

			return Decrypt(encryptionKey, type, value);
		}

		/// <summary>
		/// Decrypts the provided <paramref name="value"/> from base64 back to a typed value.
		/// </summary>
		/// <param name="encryptionKey">The encryption key to use.</param>
		/// <param name="type">The <see cref="Type"/> of <paramref name="value"/>.</param>
		/// <param name="value">The value to decrypt.</param>
		/// <returns>The typed decrypted value.</returns>
		public virtual object Decrypt(byte[] encryptionKey, Type type, string value)
		{
			byte[] buffer = Convert.FromBase64String(value);

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
							throw new CryptographicException("IV is missing or invalid.");

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
			if (typeof(string) == type)
				finalValue = decryptedValue;
			else if (typeof(short) == type)
				finalValue = short.Parse(decryptedValue);
			else if (typeof(int) == type)
				finalValue = int.Parse(decryptedValue);
			else if (typeof(long) == type)
				finalValue = long.Parse(decryptedValue);
			else if (typeof(ushort) == type)
				finalValue = ushort.Parse(decryptedValue);
			else if (typeof(uint) == type)
				finalValue = uint.Parse(decryptedValue);
			else if (typeof(ulong) == type)
				finalValue = ulong.Parse(decryptedValue);
			else if (typeof(decimal) == type)
				finalValue = decimal.Parse(decryptedValue);
			else if (typeof(float) == type)
				finalValue = float.Parse(decryptedValue);
			else if (typeof(double) == type)
				finalValue = double.Parse(decryptedValue);
			else if (typeof(Guid) == type)
				finalValue = new Guid(decryptedValue);
			else if (typeof(Enum) == type)
				finalValue = Enum.Parse(type, decryptedValue);
			else
				throw new InvalidOperationException($"Data type {type.FullName} not supported.");

			return finalValue;
		}
	}
}