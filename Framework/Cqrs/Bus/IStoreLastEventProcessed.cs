using System.ServiceModel;

namespace Cqrs.Bus
{
	[ServiceContract(Namespace = "http://cqrs.co.nz/Bus/StoreLastEventProcessed")]
	public interface IStoreLastEventProcessed
	{
		string EventLocation { get; set; }
	}
}