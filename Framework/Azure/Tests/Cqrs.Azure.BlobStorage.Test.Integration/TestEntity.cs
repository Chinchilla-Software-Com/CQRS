using System.Runtime.Serialization;
using Cqrs.Entities;

namespace Cqrs.Azure.BlobStorage.Test.Integration
{
	public class TestEntity : Entity
	{
		[DataMember]
		public string Name { get; set; }
	}
}