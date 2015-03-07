namespace Cqrs.Messages
{
	public interface IMessageWithAuthenticationToken<TAuthenticationToken> : IMessage
	{
		TAuthenticationToken AuthenticationToken { get; set; }
	}
}