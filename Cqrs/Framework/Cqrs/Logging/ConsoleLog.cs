using System;

namespace Cqrs.Logging
{
	public class ConsoleLog : ILog
	{
		#region Implementation of ILog

		public void LogInfo(string message, string container = null, Exception exception = null)
		{
			Console.ForegroundColor = ConsoleColor.Gray;
			Log("Info", ConsoleColor.Gray, message, container, exception);
		}

		public void LogDebug(string message, string container = null, Exception exception = null)
		{
			Console.ForegroundColor = ConsoleColor.Blue;
			Log("Debug", ConsoleColor.Gray, message, container, exception);
		}

		public void LogWarning(string message, string container = null, Exception exception = null)
		{
			Console.ForegroundColor = ConsoleColor.DarkYellow;
			Log("Warning", ConsoleColor.Gray, message, container, exception);
		}

		public void LogError(string message, string container = null, Exception exception = null)
		{
			Console.ForegroundColor = ConsoleColor.DarkRed;
			Log("Error", ConsoleColor.Gray, message, container, exception);
		}

		public void LogFatalError(string message, string container = null, Exception exception = null)
		{
			Console.ForegroundColor = ConsoleColor.Red;
			Log("Fatal", ConsoleColor.Gray, message, container, exception);
		}

		#endregion

		protected virtual void Log(string level, ConsoleColor foregroundColor, string message, string container = null, Exception exception = null)
		{
			ConsoleColor originalColour = Console.ForegroundColor;
			Console.ForegroundColor = foregroundColor;

			string pattern = "[{0}] {1:r}: {2}";
			if (!string.IsNullOrWhiteSpace(container))
				pattern = "[{0}] {1}: {3}:: {2}";
			if (exception != null)
				pattern = pattern + "\r\n{4}";
			string messageToLog = string.Format(pattern, level, DateTime.Now, message, container, exception, exception == null ? null : exception.Message, exception == null ? null : exception.StackTrace);
			Console.WriteLine(messageToLog);

			Console.ForegroundColor = originalColour;
		}
	}
}