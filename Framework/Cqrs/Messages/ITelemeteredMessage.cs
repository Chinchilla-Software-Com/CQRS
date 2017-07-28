#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

namespace Cqrs.Messages
{
	/// <summary>
	/// A message that can allows you to manually inform the telemetry system it's name.
	/// </summary>
	public interface ITelemeteredMessage
	{
		/// <summary>
		/// Gets or sets the Name of this message.
		/// </summary>
		string TelemetryName { get; set; }
	}
}