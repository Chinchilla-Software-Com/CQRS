#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Text;
using EventStore.ClientAPI;

namespace Cqrs.EventStore.Bus
{
	/// <summary>
	/// A collection of utility functions.
	/// </summary>
	public static class EventStoreUtilities
	{
		/// <summary>
		/// Converts the provide <paramref name="position"/> into a string representation of itself.
		/// </summary>
		public static string PositionToFormattedString(Position position)
		{
			return position.CommitPosition + "/" + position.PreparePosition;
		}

		/// <summary>
		/// Converts the provided <paramref name="formattedPosition">string representation</paramref> of a <see cref="Position"/> back into a <see cref="Position"/>.
		/// </summary>
		public static Position FormattedStringToPosition(string formattedPosition)
		{
			Position position = Position.End;

			if (!string.IsNullOrEmpty(formattedPosition))
			{
				string[] substrings = formattedPosition.Split('/');
				long commitPosition, preparePosition;
				if (long.TryParse(substrings[0], out commitPosition) &&
					long.TryParse(substrings[1], out preparePosition))
				{
					position = new Position(commitPosition, preparePosition);
				}
				else
				{
					throw new InvalidLastEventProcessedException(formattedPosition);
				}
			}

			return position;
		}

		/// <summary>
		/// Converts the provided <paramref name="value"/> into a <see cref="byte"/> <see cref="Array"/>.
		/// </summary>
		public static byte[] StringToByteArray(string value)
		{
			byte[] rawEventData = Encoding.UTF8.GetBytes(value);
			return rawEventData;
		}

		/// <summary>
		/// Converts the provided <paramref name="value">byte array</paramref> into a string.
		/// </summary>
		public static string ByteArrayToString(byte[] value)
		{
			return Encoding.UTF8.GetString(value);
		}
	}
}