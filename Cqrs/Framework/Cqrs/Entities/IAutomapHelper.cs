namespace Cqrs.Entities
{
	/// <summary>
	/// A helper class to aid auto-mapping
	/// </summary>
	public interface IAutomapHelper
	{
		/// <summary>
		/// Creates and returns a new <typeparamref name="TTarget"/> with all the easily auto-mapped properties from <paramref name="source"/> set.
		/// </summary>
		/// <param name="source">The <typeparamref name="TSource"/> to convert.</param>
		TTarget Automap<TSource, TTarget>(TSource source)
			where TTarget : new();

		/// <summary>
		/// Creates and returns a new <typeparamref name="TTarget"/> with all the easily auto-mapped properties from <paramref name="source"/> set.
		/// </summary>
		TTarget Automap<TSource, TTarget>(TSource source, TTarget target);
	}
}