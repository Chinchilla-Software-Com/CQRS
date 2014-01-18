namespace Cqrs.Domain
{
	internal class AggregateDescriptor<TPermissionToken>
	{
		public IAggregateRoot<TPermissionToken> Aggregate { get; set; }

		public int Version { get; set; }
	}
}