#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using cdmdotnet.AutoMapper;

namespace Cqrs.Sql.DataStores
{
	/// <summary>
	/// Converts and clones object data.
	/// </summary>
	public static class Converters
	{
		/// <summary>
		/// Convert/Clone data from the provided <paramref name="value"/> into a new instance of type <typeparamref name="T"/>.
		/// </summary>
		/// <typeparam name="T">The <see cref="Type"/> to convert to.</typeparam>
		/// <param name="value">The <see cref="object"/> to convert/clone data from</param>
		public static T ConvertTo<T>(object value)
			where T : new()
		{
			var results = new AutomapHelper().Automap<object, T>(value);
			return results;
		}
	}
}