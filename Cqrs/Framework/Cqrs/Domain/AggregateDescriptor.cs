namespace Cqrs.Domain
{
	internal class AggregateDescriptor<TPermissionScope>
	{
		public IAggregateRoot<TPermissionScope> Aggregate { get; set; }

		public int Version { get; set; }
	}
}