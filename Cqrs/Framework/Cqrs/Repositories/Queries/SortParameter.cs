namespace Cqrs.Repositories.Queries
{
	public class SortParameter<TSortBy>
	{
		public enum SortParameterDirectionType
		{
			Ascending = 0,

			Descending = 2
		}

		public TSortBy SortBy { get; set; }

		public SortParameterDirectionType Direction { get; set; }
	}
}