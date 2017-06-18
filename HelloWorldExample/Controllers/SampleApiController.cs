using System.Web.Http;
using Cqrs.Commands;
using Cqrs.Ninject.Configuration;
using HelloWorldExample.Controllers.Commands;

namespace HelloWorldExample.Controllers
{
	[RoutePrefix("Sample")]
	public class SampleApiController : ApiController
	{
		public SampleApiController(ICommandPublisher<string> commandPublisher)
		{
			CommandPublisher = commandPublisher;
		}

		public SampleApiController()
			: this(NinjectDependencyResolver.Current.Resolve<ICommandPublisher<string>>())
		{
		}

		private ICommandPublisher<string> CommandPublisher { get; set; }

		[Route("SayHelloWorld")]
		[HttpPost]
		public IHttpActionResult SayHelloWorld()
		{
			CommandPublisher.Publish(new SayHelloCommand());
			return Ok();
		}
	}
}