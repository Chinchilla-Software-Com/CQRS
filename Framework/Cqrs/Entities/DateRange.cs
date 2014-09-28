using System;

namespace Cqrs.Entities
{
	/// <summary>
	/// A <see cref="Range{TPrimitive}"/> object for collecting a <see cref="DateTime"/> lower and upper limit.
	/// </summary>
	public class DateRange : Range<DateTime>
	{
		/// <summary>
		/// Instantiates and Initialises a new instance of the <see cref="DateRange"/> class.
		/// </summary>
		public DateRange()
		{
			IsFromInclusive = true;
			IsToInclusive = true;
		}
	}
}