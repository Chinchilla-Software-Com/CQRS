#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Events;

namespace Cqrs.Azure.DocumentDb.Events
{
	[Obsolete("Use Cqrs.Events.DefaultEventBuilder as a replacement.")]
	public class AzureDocumentDbEventBuilder<TAuthenticationToken> : DefaultEventBuilder<TAuthenticationToken>
	{
	}
}