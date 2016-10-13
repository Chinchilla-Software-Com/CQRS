using System;
using Cqrs.Configuration;

namespace Cqrs.Bus
{
	public class BusHelper : IBusHelper
	{
		public BusHelper(IConfigurationManager configurationManager)
		{
			ConfigurationManager = configurationManager;
		}

		protected IConfigurationManager ConfigurationManager { get; private set; }

		/// <summary>
		/// Checks if a white-list or black-list approach is taken, then checks the <see cref="IConfigurationManager"/> to see if a key exists defining if the event is required or not.
		/// If the event is required and it cannot be resolved, an error will be raised.
		/// Otherwise the event will be marked as processed.
		/// </summary>
		/// <param name="messageType">The <see cref="Type"/> of the message being processed.</param>
		public virtual bool IsEventRequired(Type messageType)
		{
			return IsEventRequired(string.Format("{0}.IsRequired", messageType.FullName));
		}

		/// <summary>
		/// Checks if a white-list or black-list approach is taken, then checks the <see cref="IConfigurationManager"/> to see if a key exists defining if the event is required or not.
		/// If the event is required and it cannot be resolved, an error will be raised.
		/// Otherwise the event will be marked as processed.
		/// </summary>
		/// <param name="configurationKey">The configuration key to check.</param>
		public virtual bool IsEventRequired(string configurationKey)
		{
			bool isblackListRequired;
			if (!ConfigurationManager.TryGetSetting("Cqrs.MessageBus.BlackListProcessing", out isblackListRequired))
				isblackListRequired = true;

			bool isRequired;
			if (!ConfigurationManager.TryGetSetting(configurationKey, out isRequired))
				isRequired = isblackListRequired;

			return isRequired;
		}
	}
}