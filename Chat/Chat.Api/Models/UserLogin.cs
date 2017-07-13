namespace Chat.Api.Models
{
	/// <summary>
	/// User login credentials
	/// </summary>
	public class UserLogin
	{
		/// <summary>
		/// The user's email address
		/// </summary>
		public string EmailAddress { get; set; }

		/// <summary>
		/// The user's password
		/// </summary>
		public string Password { get; set; }
	}
}