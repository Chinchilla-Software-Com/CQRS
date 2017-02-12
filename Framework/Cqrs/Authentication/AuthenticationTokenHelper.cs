using cdmdotnet.StateManagement;

namespace Cqrs.Authentication
{
	public class AuthenticationTokenHelper<TToken1>
		: IAuthenticationTokenHelper<TToken1>
	{
		protected string CacheKey = string.Format("{0}AuthenticationToken", typeof(TToken1).FullName);

		protected IContextItemCollection Cache { get; private set; }

		public AuthenticationTokenHelper(IContextItemCollectionFactory factory)
		{
			Cache = factory.GetCurrentContext();
		}

		#region Implementation of IAuthenticationTokenHelper<out TToken1>

		public TToken1 SetAuthenticationToken(TToken1 token)
		{
			Cache.SetData(CacheKey, token);
			return token;
		}

		public TToken1 GetAuthenticationToken()
		{
			return Cache.GetData<TToken1>(CacheKey);
		}

		#endregion
	}
}