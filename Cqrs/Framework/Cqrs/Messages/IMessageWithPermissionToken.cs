namespace Cqrs.Messages
{
	public interface IMessageWithPermissionToken<TPermissionToken> : IMessage
	{
		TPermissionToken PermissionToken { get; set; }
	}
}