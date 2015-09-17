namespace Cqrs.Domain
{
	internal class AggregateDescriptor<TAggregateRoot, TAuthenticationToken> : IAggregateDescriptor<TAuthenticationToken>
		where TAggregateRoot : IAggregateRoot<TAuthenticationToken>
	{
		IAggregateRoot<TAuthenticationToken> IAggregateDescriptor<TAuthenticationToken>.Aggregate
		{
			get { return Aggregate; }
		}

		public TAggregateRoot Aggregate { get; set; }

		public int Version { get; set; }
	}
}