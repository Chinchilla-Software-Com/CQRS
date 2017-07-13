using Chat.MicroServices.Conversations.Commands;
using Cqrs.Authentication;
using Cqrs.Ninject.Azure.WebJobs;

/// <summary>
/// Starts the Webjob
/// </summary>
public class CqrsWebJobProgram : CqrsNinjectJobHost<SingleSignOnToken, DefaultAuthenticationTokenHelper>
{
	public CqrsWebJobProgram()
	{
		HandlerTypes = new[] { typeof(PostCommentCommand) };
	}

	/// <remarks>
	/// Please set the following connection strings in app.config for this WebJob to run:
	/// AzureWebJobsDashboard and AzureWebJobsStorage
	/// Better yet, add them to your Azure portal so they can be changed at runtime without re-deploying or re-compiling.
	/// </remarks>
	public static void Main()
	{
		CoreHost = new CqrsWebJobProgram();
		StartHost();
	}
}