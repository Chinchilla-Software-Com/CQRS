using System;

namespace Cqrs.Logging
{
	public class ConsoleLog : ILog
	{
		protected ICorrolationIdHelper CorrolationIdHelper { get; private set; }

		public ConsoleLog(ICorrolationIdHelper corrolationIdHelper)
		{
			CorrolationIdHelper = corrolationIdHelper;
		}

		#region Implementation of ILog

		public void LogInfo(string message, string container = null, Exception exception = null)
		{
			Log("Info", ConsoleColor.Gray, message, container, exception);
		}

		public void LogDebug(string message, string container = null, Exception exception = null)
		{
			Log("Debug", ConsoleColor.Blue, message, container, exception);
		}

		public void LogWarning(string message, string container = null, Exception exception = null)
		{
			Log("Warning", ConsoleColor.DarkYellow, message, container, exception);
		}

		public void LogError(string message, string container = null, Exception exception = null)
		{
			Log("Error", ConsoleColor.DarkRed, message, container, exception);
		}

		public void LogFatalError(string message, string container = null, Exception exception = null)
		{
			Log("Fatal", ConsoleColor.Red, message, container, exception);
		}

		#endregion

		protected virtual void Log(string level, ConsoleColor foregroundColor, string message, string container = null, Exception exception = null)
		{
			ConsoleColor originalColour = Console.ForegroundColor;
			Console.ForegroundColor = foregroundColor;
			string corrolationId = CorrolationIdHelper.GetCorrolationId();

			string pattern = "[{0}] {1:r}:";
			if (!string.IsNullOrWhiteSpace(corrolationId))
				pattern = "[{0}] [{7}] {1:r}:";
			if (!string.IsNullOrWhiteSpace(container))
				pattern = pattern + " {3}::";
			pattern = pattern + " {2}";
			if (exception != null)
				pattern = pattern + "\r\n{5}\r\n{6}\r\n{4}";
			string messageToLog = string.Format(pattern, 
				level, // 0
				DateTime.Now, // 1
				message, // 2
				container, // 3
				exception, // 4
				exception == null ? null : exception.Message, // 5
				exception == null ? null : exception.StackTrace, // 6
				corrolationId // 7
			);
			Console.WriteLine(messageToLog);

			Console.ForegroundColor = originalColour;
		}
	}
}