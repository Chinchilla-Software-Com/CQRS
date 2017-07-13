namespace Chat.MicroServices.Authentication.Helpers
{
	using System.Security.Cryptography;
	using System.Text;

	public class AuthenticationHashHelper : IAuthenticationHashHelper
	{
		private const string Salt1 = "a6f723b251304867bdada865f4d93694";
		private const string Salt2 = "b71a8bc00f1d41fb804921febac180d2";

		public virtual string GenerateCredentialHash(string emailAddress, string password)
		{
			SHA512 sha512 = SHA512.Create();
			byte[] bytes = Encoding.UTF8.GetBytes(string.Format("{0}::{1}::{2}::{3}", Salt1, emailAddress.ToLowerInvariant(), Salt2, password));
			byte[] hash = sha512.ComputeHash(bytes);
			return System.Convert.ToBase64String(hash);
		}
	}
}