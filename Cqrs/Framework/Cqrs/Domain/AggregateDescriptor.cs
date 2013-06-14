namespace Cqrs.Domain
{
	internal class AggregateDescriptor
	{
		public IAggregateRoot Aggregate { get; set; }
		public int Version { get; set; }
	}
}