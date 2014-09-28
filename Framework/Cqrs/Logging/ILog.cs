using System;

namespace Cqrs.Logging
{
	public interface ILog
	{
		void LogInfo(string message, string container = null, Exception exception = null);

		void LogDebug(string message, string container = null, Exception exception = null);

		void LogWarning(string message, string container = null, Exception exception = null);

		void LogError(string message, string container = null, Exception exception = null);

		void LogFatalError(string message, string container = null, Exception exception = null);
	}
}