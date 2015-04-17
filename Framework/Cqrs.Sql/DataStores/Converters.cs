using cdmdotnet.AutoMapper;

namespace Cqrs.Sql.DataStores
{
	public static class Converters
	{
		public static T ConvertTo<T>(object value)
			where T : new()
		{
			var results = new AutomapHelper().Automap<object, T>(value);
			return results;
		}
	}
}