namespace Cqrs.Domain
{
	internal interface IAggregateDescriptor
	{
		int Version { get; set; }
	}

	internal interface IAggregateDescriptor<TAuthenticationToken> : IAggregateDescriptor
	{
		IAggregateRoot<TAuthenticationToken> Aggregate { get; }
	}
}