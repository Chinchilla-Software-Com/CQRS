using System.Text;
using EventStore.ClientAPI;

namespace Cqrs.EventStore.Bus
{
	public static class EventStoreUtilities
	{
		public static string PositionToFormattedString(Position position)
		{
			return position.CommitPosition + "/" + position.PreparePosition;
		}

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

		public static byte[] StringToByteArray(string value)
		{
			byte[] rawEventData = Encoding.UTF8.GetBytes(value);
			return rawEventData;
		}

		public static string ByteArrayToString(byte[] value)
		{
			return Encoding.UTF8.GetString(value);
		}
	}
}