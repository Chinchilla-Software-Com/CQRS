using System;

namespace Cqrs.Messages
{
	/// <summary>
	/// Encrypts values to base64 and decrypts from base64
	/// </summary>
	public interface IEncryptionService
	{
		/// <summary>
		/// Encrypts the provided <paramref name="value"/> to a base64 string.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of <paramref name="value"/>.</typeparam>
		/// <param name="encryptionKey">The encryption key to use.</param>
		/// <param name="value">The value to encrypt.</param>
		/// <returns>A base64 encoded string.</returns>
		string Encrypt<T>(string encryptionKey, T value);

		/// <summary>
		/// Encrypts the provided <paramref name="value"/> to a base64 string.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> of <paramref name="value"/>.</typeparam>
		/// <param name="encryptionKey">The encryption key to use.</param>
		/// <param name="value">The value to encrypt.</param>
		/// <returns>A base64 encoded string.</returns>
		string Encrypt<T>(byte[] encryptionKey, T value);

		/// <summary>
		/// Encrypts a value to a base64 string.
		/// </summary>
		/// <param name="encryptionKey">The encryption key to use.</param>
		/// <param name="type">The <see cref="Type"/> of <paramref name="value"/>.</param>
		/// <param name="value">The value to encrypt.</param>
		/// <returns>A base64 encoded string.</returns>
		string Encrypt(string encryptionKey, Type type, object value);

		/// <summary>
		/// Encrypts a value to a base64 string.
		/// </summary>
		/// <param name="encryptionKey">The encryption key to use.</param>
		/// <param name="type">The <see cref="Type"/> of <paramref name="value"/>.</param>
		/// <param name="value">The value to encrypt.</param>
		/// <returns>A base64 encoded string.</returns>
		string Encrypt(byte[] encryptionKey, Type type, object value);

		/// <summary>
		/// Decrypts the provided <paramref name="value"/> from base64 back to a typed value.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to decrypt to.</typeparam>
		/// <param name="encryptionKey">The encryption key to use.</param>
		/// <param name="value">The value to decrypt.</param>
		/// <returns>The typed decrypted value.</returns>
		object Decrypt<T>(string encryptionKey, string value);

		/// <summary>
		/// Decrypts the provided <paramref name="value"/> from base64 back to a typed value.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to decrypt to.</typeparam>
		/// <param name="encryptionKey">The encryption key to use.</param>
		/// <param name="value">The value to decrypt.</param>
		/// <returns>The typed decrypted value.</returns>
		object Decrypt<T>(byte[] encryptionKey, string value);

		/// <summary>
		/// Decrypts the provided <paramref name="value"/> from base64 back to a typed value.
		/// </summary>
		/// <param name="encryptionKey">The encryption key to use.</param>
		/// <param name="type">The <see cref="Type"/> of <paramref name="value"/>.</param>
		/// <param name="value">The value to decrypt.</param>
		/// <returns>The typed decrypted value.</returns>
		object Decrypt(string encryptionKey, Type type, string value);

		/// <summary>
		/// Decrypts the provided <paramref name="value"/> from base64 back to a typed value.
		/// </summary>
		/// <param name="encryptionKey">The encryption key to use.</param>
		/// <param name="type">The <see cref="Type"/> of <paramref name="value"/>.</param>
		/// <param name="value">The value to decrypt.</param>
		/// <returns>The typed decrypted value.</returns>
		object Decrypt(byte[] encryptionKey, Type type, string value);
	}
}