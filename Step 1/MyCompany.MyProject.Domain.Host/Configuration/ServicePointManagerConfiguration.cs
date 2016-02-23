using System.Net;

namespace MyCompany.MyProject.Domain.Host.Configuration
{
	public class ServicePointManagerConfiguration
	{
		public void Start()
		{
			// https://alexandrebrisebois.wordpress.com/2013/03/24/why-are-webrequests-throttled-i-want-more-throughput/
			ServicePointManager.UseNagleAlgorithm = false;
			ServicePointManager.DefaultConnectionLimit = 1000;
		}
	}
}