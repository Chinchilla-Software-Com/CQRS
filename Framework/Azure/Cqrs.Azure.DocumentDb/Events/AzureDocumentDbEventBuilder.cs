using System;
using Cqrs.Events;

namespace Cqrs.Azure.DocumentDb.Events
{
	[Obsolete("Use Cqrs.Events.DefaultEventBuilder as a replacement.")]
	public class AzureDocumentDbEventBuilder<TAuthenticationToken> : DefaultEventBuilder<TAuthenticationToken>
	{
	}
}