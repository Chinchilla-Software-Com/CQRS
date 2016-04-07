using System;
using Cqrs.Events;

namespace Cqrs.Azure.DocumentDb.Events
{
	[Obsolete("Use Cqrs.Events.SqlEventBuilder as a replacement.")]
	public class AzureDocumentDbEventBuilder<TAuthenticationToken> : SqlEventBuilder<TAuthenticationToken>
	{
	}
}