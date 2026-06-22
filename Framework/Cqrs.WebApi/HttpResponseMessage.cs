#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Net.Http;

namespace Cqrs.WebApi
{
	/// <summary>
	/// A <see cref="HttpResponseMessage"/> that supports auto self-documenting generation.
	/// </summary>
	/// <typeparam name="TData">The <see cref="Type"/> of data returned.</typeparam>
	public class HttpResponseMessage<TData> : HttpResponseMessage
	{
	}
}