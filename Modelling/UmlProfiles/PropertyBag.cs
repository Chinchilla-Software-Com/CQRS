using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

namespace Cqrs.Modelling.UmlProfiles
{
	/// <summary>
	/// IPropertyBag but then with parsable HResult
	/// </summary>
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown), ComImport]
	[Guid("55272A00-42CB-11CE-8135-00AA004BB851")]
	interface ICOMPropertyBag
	{
		[PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int Read(string pszPropName, out object pVar, IErrorLog pErrorLog, uint VARTYPE, object pUnkObj);

		[PreserveSig, MethodImpl(MethodImplOptions.InternalCall, MethodCodeType = MethodCodeType.Runtime)]
		int Write(string pszPropName, ref object pVar);
	}

	public interface IPropertyMap : IDisposable
	{
		void SetValue(string key, string value);
		void SetRawValue(string key, string value);

		bool TryGetValue(string key, out string value);

		void SetQuoted(string key, string value);
		bool TryGetQuoted(string key, out string value);

		void Flush();
	}
	sealed class PropertyBag : IPropertyMap
	{
		readonly ICOMPropertyBag _bag;
		readonly SortedList<string, string> _toWrite = new SortedList<string, string>(StringComparer.OrdinalIgnoreCase);

		internal PropertyBag(ICOMPropertyBag bag)
		{
			if (bag == null)
				throw new ArgumentNullException("bag");
			_bag = bag;
		}

		public PropertyBag(IPropertyBag bag)
			: this((ICOMPropertyBag)bag)
		{
		}

		/// <summary>
		/// Tries to get the value.
		/// </summary>
		/// <param name="propName">Name of the prop.</param>
		/// <param name="value">The value.</param>
		/// <returns></returns>
		public bool TryGetValue(string propName, out string value)
		{
			if (string.IsNullOrEmpty(propName))
				throw new ArgumentNullException("propName");

			object var;
			if (ErrorHandler.Succeeded(_bag.Read(propName, out var, null, 0, null)))
			{
				value = var as string;

				return value != null;
			}
			value = null;
			return false;
		}

		/// <summary>
		/// Sets the value.
		/// </summary>
		/// <param name="propName">Name of the prop.</param>
		/// <param name="value">The value.</param>
		public void SetValue(string propName, string value)
		{
			if (string.IsNullOrEmpty(propName))
				throw new ArgumentNullException("propName");
			if (value == null)
				throw new ArgumentNullException("value");

			_toWrite[propName] = value;
		}

		public void SetQuoted(string propName, string value)
		{
			if (value == null)
				throw new ArgumentNullException("value");

			SetValue(propName, Quote(value));
		}

		public bool TryGetQuoted(string propName, out string value)
		{
			if (!TryGetValue(propName, out value))
				return false;

			value = Unquote(value);
			return true;
		}

		public void SetRawValue(string propName, string value)
		{
			object var = value;
			_bag.Write(propName, ref var);
		}

		public void Flush()
		{
			foreach (KeyValuePair<string, string> kv in _toWrite)
			{
				SetRawValue(kv.Key, kv.Value);
			}
			_toWrite.Clear();
		}

		public void Dispose()
		{
			Flush();
		}

		static string Quote(string value)
		{
			return '\"' + value.Replace("\"", "\"\"") + '\"';
		}

		static string Unquote(string value)
		{
			if (string.IsNullOrEmpty(value))
				return null;

			value = value.Trim();

			if (string.IsNullOrEmpty(value))
				return "";

			if (value.Length >= 2 && value[0] == '\"' && value[value.Length - 1] == '\"')
			{
				value = value.Substring(1, value.Length - 2).Replace("\"\"", "\"");
			}

			return value;
		}
	}
}