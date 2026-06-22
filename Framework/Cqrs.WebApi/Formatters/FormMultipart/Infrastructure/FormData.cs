#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="Chinchilla Software Limited">
// // 	Copyright Chinchilla Software Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Cqrs.WebApi.Formatters.FormMultipart.Infrastructure
{
	/// <summary>
	/// Provides access to all fields and files in a multi-part form-data request.
	/// </summary>
	public class FormData
	{
		private List<ValueFile> _files;

		private List<ValueString> _fields;

		/// <summary>
		/// All <see cref="ValueFile">files</see> in the request.
		/// </summary>
		public List<ValueFile> Files
		{
			get
			{
				if(_files == null)
					_files = new List<ValueFile>();
				return _files;
			}
			set
			{
				_files = value;
			}
		}

		/// <summary>
		/// All <see cref="ValueString">fields</see> in the request.
		/// </summary>
		public List<ValueString> Fields
		{
			get
			{
				if(_fields == null)
					_fields = new List<ValueString>();
				return _fields;
			}
			set
			{
				_fields = value;
			}
		}

		/// <summary>
		/// Get's all keys from <see cref="Fields"/> and <see cref="Files"/>.
		/// </summary>
		public List<string> GetAllKeys()
		{
			return Fields.Select(m => m.Name).Concat(Files.Select(m => m.Name)).ToList();
		}

		/// <summary>
		/// Adds a new <see cref="ValueString"/> to <see cref="Fields"/>.
		/// </summary>
		/// <param name="name">The <see cref="ValueString.Name"/>.</param>
		/// <param name="value">The <see cref="ValueString.Value"/>.</param>
		public void Add(string name, string value)
		{
			Fields.Add(new ValueString { Name = name, Value = value});
		}

		/// <summary>
		/// Adds a new <see cref="ValueFile"/> to <see cref="Files"/>.
		/// </summary>
		/// <param name="name">The <see cref="ValueFile.Name"/>.</param>
		/// <param name="value">The <see cref="ValueFile.Value"/>.</param>
		public void Add(string name, HttpFile value)
		{
			Files.Add(new ValueFile { Name = name, Value = value });
		}

		/// <summary>
		/// Get's the first <see cref="ValueString.Value"/> from <see cref="Fields"/> with a <see cref="ValueString.Name"/> equal to <paramref name="name"/>.
		/// </summary>
		public bool TryGetValue(string name, CultureInfo culture, out string value)
		{
			var field = Fields.FirstOrDefault(m => culture.CompareInfo.Compare(m.Name, name, CompareOptions.IgnoreCase) == 0);
			if (field != null)
			{
				value = field.Value;
				return true;
			}
			value = null;
			return false;
		}

		/// <summary>
		/// Get's the first <see cref="ValueFile.Value"/> from <see cref="Files"/> with a <see cref="ValueFile.Name"/> equal to <paramref name="name"/>.
		/// </summary>
		public bool TryGetValue(string name, CultureInfo culture, out HttpFile value)
		{
			var file = Files.FirstOrDefault(m => culture.CompareInfo.Compare(m.Name, name, CompareOptions.IgnoreCase) == 0);
			if (file != null)
			{
				value = file.Value;
				return true;
			}
			value = null;
			return false;
		}

		/// <summary>
		/// Get's all <see cref="ValueFile.Value"/> from <see cref="Files"/> with a <see cref="ValueFile.Name"/> equal to <paramref name="name"/>.
		/// </summary>
		public List<string> GetValues(string name, CultureInfo culture)
		{
			return Fields
				.Where(m => culture.CompareInfo.Compare(m.Name, name, CompareOptions.IgnoreCase) == 0)
				.Select(m => m.Value)
				.ToList();
		}

		/// <summary>
		/// Get's all <see cref="ValueString.Value"/> from <see cref="Fields"/> with a <see cref="ValueString.Name"/> equal to <paramref name="name"/>.
		/// </summary>
		public List<HttpFile> GetFiles(string name, CultureInfo culture)
		{
			return Files
				.Where(m => culture.CompareInfo.Compare(m.Name, name, CompareOptions.IgnoreCase) == 0)
				.Select(m => m.Value)
				.ToList();
		}

		/// <summary>
		/// Determines whether <see cref="Files"/> or <see cref="Fields"/> contains an item with the specific name.
		/// </summary>
		/// <param name="name">The name of the item to locate.</param>
		/// <param name="culture">The <see cref="CultureInfo"/> to use in key evaluation.</param>
		/// <returns>true if an item is found; otherwise, false.</returns>
		public bool Contains(string name, CultureInfo culture)
		{
			string val;
			HttpFile file;

			return TryGetValue(name, culture, out val) || TryGetValue(name, culture, out file);
		}

		/// <summary>
		/// A string value.
		/// </summary>
		public class ValueString
		{
			/// <summary>
			/// A name of the value.
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// The value itself.
			/// </summary>
			public string Value { get; set; }
		}

		/// <summary>
		/// A file value.
		/// </summary>
		public class ValueFile
		{
			/// <summary>
			/// A name of the value.
			/// </summary>
			public string Name { get; set; }

			/// <summary>
			/// The file itself.
			/// </summary>
			public HttpFile Value { get; set; }
		}
	}
}