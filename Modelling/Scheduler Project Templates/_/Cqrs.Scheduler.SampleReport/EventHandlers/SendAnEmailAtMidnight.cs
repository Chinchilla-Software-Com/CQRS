namespace $safeprojectname$.EventHandlers
{
	using System;
	using System.Net.Mail;
	using Cqrs.Configuration;
	using Cqrs.Events;
	using Events;

	/// <summary>
	/// Sends an email advising when each <see cref="TimeZoneInfo">time-zone</see> reaches mid-night.
	/// </summary>
	public class SendAnEmailAtMidnight : IEventHandler<Guid, ItsMidnightInEvent>
	{
		/// <summary>
		/// Instantiates a new instance of <see cref="SendAnEmailAtMidnight"/>.
		/// </summary>
		/// <param name="configurationManager">The <see cref="IConfigurationManager"/> to use to get email settings.</param>
		public SendAnEmailAtMidnight(IConfigurationManager configurationManager)
		{
			ReportEmailAddress = configurationManager.GetSetting("ReportEmailAddress");
		}

		/// <summary>
		/// The email address of the account to receive the email report.
		/// </summary>
		protected string ReportEmailAddress { get; private set; }

		#region Implementation of IMessageHandler<in ItsMidnightInEvent>

		/// <summary>
		/// Responds to the provided <paramref name="message"/>.
		/// </summary>
		/// <param name="message">The <see cref="ItsMidnightInEvent"/> to respond to or "handle"</param>
		public void Handle(ItsMidnightInEvent message)
		{
			var emailMessage = string.Format("At {0} it was midnight in {1}", message.ProcessDate, message.TimezoneId);
			var smtpClient = new SmtpClient();
			smtpClient.Send(ReportEmailAddress, ReportEmailAddress, string.Format("It's midnight in {0}", message.TimezoneId), emailMessage);
		}

		#endregion
	}
}