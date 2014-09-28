namespace Cqrs.Domain
{
	internal class AggregateDescriptor<TAuthenticationToken>
	{
		public IAggregateRoot<TAuthenticationToken> Aggregate { get; set; }

		public int Version { get; set; }
	}
}