#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using Cqrs.Events;

namespace Cqrs.Azure.DocumentDb.Events
{
	[Obsolete("Use Cqrs.Events.EventDeserialiser as a replacement.")]
	public class AzureDocumentDbEventDeserialiser<TAuthenticationToken> : EventDeserialiser<TAuthenticationToken>
	{
	}
}