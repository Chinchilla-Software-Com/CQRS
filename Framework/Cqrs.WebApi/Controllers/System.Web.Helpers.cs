// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Web.Script.Serialization;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Dynamic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using System.Web.Helpers.Resources;
using Microsoft.CSharp.RuntimeBinder;

// ReSharper disable once CheckNamespace
namespace System.Web.Helpers
{
#pragma warning disable 1591
	public static class Json
	{
		private static readonly JavaScriptSerializer _serializer = CreateSerializer();

		public static string Encode(object value)
		{
			// Serialize our dynamic array type as an array
			DynamicJsonArray jsonArray = value as DynamicJsonArray;
			if (jsonArray != null)
			{
				return _serializer.Serialize((object[])jsonArray);
			}

			return _serializer.Serialize(value);
		}

		public static void Write(object value, TextWriter writer)
		{
			writer.Write(_serializer.Serialize(value));
		}

		public static dynamic Decode(string value)
		{
			return WrapObject(_serializer.DeserializeObject(value));
		}

		public static dynamic Decode(string value, Type targetType)
		{
			return WrapObject(_serializer.Deserialize(value, targetType));
		}

		public static T Decode<T>(string value)
		{
			return _serializer.Deserialize<T>(value);
		}

		private static JavaScriptSerializer CreateSerializer()
		{
			JavaScriptSerializer serializer = new JavaScriptSerializer();
			serializer.RegisterConverters(new[] { new DynamicJavaScriptConverter() });
			return serializer;
		}

		internal static dynamic WrapObject(object value)
		{
			// The JavaScriptSerializer returns IDictionary<string, object> for objects
			// and object[] for arrays, so we wrap those in different dynamic objects
			// so we can access the object graph using dynamic
			var dictionaryValues = value as IDictionary<string, object>;
			if (dictionaryValues != null)
			{
				return new DynamicJsonObject(dictionaryValues);
			}

			var arrayValues = value as object[];
			if (arrayValues != null)
			{
				return new DynamicJsonArray(arrayValues);
			}

			return value;
		}
	}

	// REVIEW: Consider implementing ICustomTypeDescriptor and IDictionary<string, object>
	public class DynamicJsonObject : DynamicObject
	{
		private readonly IDictionary<string, object> _values;

		public DynamicJsonObject(IDictionary<string, object> values)
		{
			Debug.Assert(values != null);
			_values = values.ToDictionary(p => p.Key, p => Json.WrapObject(p.Value),
										  StringComparer.OrdinalIgnoreCase);
		}

		public override bool TryConvert(ConvertBinder binder, out object result)
		{
			result = null;
			if (binder.Type.IsAssignableFrom(_values.GetType()))
			{
				result = _values;
			}
			else
			{
				throw new InvalidOperationException(String.Format(CultureInfo.CurrentCulture, HelpersResources.Json_UnableToConvertType, binder.Type));
			}
			return true;
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			result = GetValue(binder.Name);
			return true;
		}

		public override bool TrySetMember(SetMemberBinder binder, object value)
		{
			_values[binder.Name] = Json.WrapObject(value);
			return true;
		}

		public override bool TrySetIndex(SetIndexBinder binder, object[] indexes, object value)
		{
			string key = GetKey(indexes);
			if (!String.IsNullOrEmpty(key))
			{
				_values[key] = Json.WrapObject(value);
			}
			return true;
		}

		public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
		{
			string key = GetKey(indexes);
			result = null;
			if (!String.IsNullOrEmpty(key))
			{
				result = GetValue(key);
			}
			return true;
		}

		private static string GetKey(object[] indexes)
		{
			if (indexes.Length == 1)
			{
				return (string)indexes[0];
			}
			// REVIEW: Should this throw?
			return null;
		}

		public override IEnumerable<string> GetDynamicMemberNames()
		{
			return _values.Keys;
		}

		private object GetValue(string name)
		{
			object result;
			if (_values.TryGetValue(name, out result))
			{
				return result;
			}
			return null;
		}
	}

	[SuppressMessage("Microsoft.Naming", "CA1710:IdentifiersShouldHaveCorrectSuffix", Justification = "This class isn't meant to be used directly")]
	public class DynamicJsonArray : DynamicObject, IEnumerable<object>
	{
		private readonly object[] _arrayValues;

		public DynamicJsonArray(object[] arrayValues)
		{
			Debug.Assert(arrayValues != null);
			_arrayValues = arrayValues.Select(Json.WrapObject).ToArray();
		}

		public int Length
		{
			get { return _arrayValues.Length; }
		}

		public dynamic this[int index]
		{
			get { return _arrayValues[index]; }
			set { _arrayValues[index] = Json.WrapObject(value); }
		}

		public override bool TryConvert(ConvertBinder binder, out object result)
		{
			if (_arrayValues.GetType().IsAssignableFrom(binder.Type))
			{
				result = _arrayValues;
				return true;
			}
			return base.TryConvert(binder, out result);
		}

		public override bool TryGetMember(GetMemberBinder binder, out object result)
		{
			// Testing for members should never throw. This is important when dealing with
			// services that return different json results. Testing for a member shouldn't throw,
			// it should just return null (or undefined)
			result = null;
			return true;
		}

		public IEnumerator GetEnumerator()
		{
			return _arrayValues.GetEnumerator();
		}

		private IEnumerable<object> GetEnumerable()
		{
			return _arrayValues.AsEnumerable();
		}

		IEnumerator<object> IEnumerable<object>.GetEnumerator()
		{
			return GetEnumerable().GetEnumerator();
		}

		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "This class isn't meant to be used directly")]
		public static implicit operator object[](DynamicJsonArray obj)
		{
			return obj._arrayValues;
		}

		[SuppressMessage("Microsoft.Usage", "CA2225:OperatorOverloadsHaveNamedAlternates", Justification = "This class isn't meant to be used directly")]
		public static implicit operator Array(DynamicJsonArray obj)
		{
			return obj._arrayValues;
		}
	}

	/// <summary>
	/// Converter that knows how to get the member values from a dynamic object.
	/// </summary>
	internal class DynamicJavaScriptConverter : JavaScriptConverter
	{
		public override IEnumerable<Type> SupportedTypes
		{
			get
			{
				// REVIEW: For some reason the converters don't pick up interfaces
				yield return typeof(IDynamicMetaObjectProvider);
				yield return typeof(DynamicObject);
			}
		}

		public override object Deserialize(IDictionary<string, object> dictionary, Type type, JavaScriptSerializer serializer)
		{
			throw new NotSupportedException();
		}

		public override IDictionary<string, object> Serialize(object obj, JavaScriptSerializer serializer)
		{
			var values = new Dictionary<string, object>(StringComparer.OrdinalIgnoreCase);
			var memberNames = DynamicHelper.GetMemberNames(obj);

			// This should never happen
			Debug.Assert(memberNames != null);

			// Get the value for each member in the dynamic object
			foreach (string memberName in memberNames)
			{
				values[memberName] = DynamicHelper.GetMemberValue(obj, memberName);
			}

			return values;
		}
	}

	/// <summary>
	/// Helper to evaluate different method on dynamic objects
	/// </summary>
	internal static class DynamicHelper
	{
		// We must pass in "object" instead of "dynamic" for the target dynamic object because if we use dynamic, the compiler will
		// convert the call to this helper into a dynamic expression, even though we don't need it to be.  Since this class is internal,
		// it cannot be accessed from a dynamic expression and thus we get errors.

		// Dev10 Bug 914027 - Changed the first parameter from dynamic to object, see comment at top for details
		public static bool TryGetMemberValue(object obj, string memberName, out object result)
		{
			try
			{
				result = GetMemberValue(obj, memberName);
				return true;
			}
			catch (RuntimeBinderException)
			{
			}
			catch (RuntimeBinderInternalCompilerException)
			{
			}

			// We catch the C# specific runtime binder exceptions since we're using the C# binder in this case
			result = null;
			return false;
		}

		// Dev10 Bug 914027 - Changed the first parameter from dynamic to object, see comment at top for details
		[SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes", Justification = "We want to swallow exceptions that happen during runtime binding")]
		public static bool TryGetMemberValue(object obj, GetMemberBinder binder, out object result)
		{
			try
			{
				// VB us an instance of GetBinderAdapter that does not implement FallbackGetMemeber. This causes lookup of property expressions on dynamic objects to fail.
				// Since all types are private to the assembly, we assume that as long as they belong to CSharp runtime, it is the right one. 
				if (typeof(Binder).Assembly.Equals(binder.GetType().Assembly))
				{
					// Only use the binder if its a C# binder.
					result = GetMemberValue(obj, binder);
				}
				else
				{
					result = GetMemberValue(obj, binder.Name);
				}
				return true;
			}
			catch
			{
				result = null;
				return false;
			}
		}

		// Dev10 Bug 914027 - Changed the first parameter from dynamic to object, see comment at top for details
		public static object GetMemberValue(object obj, string memberName)
		{
			var callSite = GetMemberAccessCallSite(memberName);
			return callSite.Target(callSite, obj);
		}

		// Dev10 Bug 914027 - Changed the first parameter from dynamic to object, see comment at top for details
		public static object GetMemberValue(object obj, GetMemberBinder binder)
		{
			var callSite = GetMemberAccessCallSite(binder);
			return callSite.Target(callSite, obj);
		}

		// dynamic d = new object();
		// object s = d.Name;
		// The following code gets generated for this expression:
		// callSite = CallSite<Func<CallSite, object, object>>.Create(Binder.GetMember(CSharpBinderFlags.None, "Name", typeof(Program), new CSharpArgumentInfo[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) }));
		// callSite.Target(callSite, d);
		// typeof(Program) is the containing type of the dynamic operation.
		// Dev10 Bug 914027 - Changed the callsite's target parameter from dynamic to object, see comment at top for details
		public static CallSite<Func<CallSite, object, object>> GetMemberAccessCallSite(string memberName)
		{
			var binder = Binder.GetMember(CSharpBinderFlags.None, memberName, typeof(DynamicHelper), new[] { CSharpArgumentInfo.Create(CSharpArgumentInfoFlags.None, null) });
			return GetMemberAccessCallSite(binder);
		}

		// Dev10 Bug 914027 - Changed the callsite's target parameter from dynamic to object, see comment at top for details
		public static CallSite<Func<CallSite, object, object>> GetMemberAccessCallSite(CallSiteBinder binder)
		{
			return CallSite<Func<CallSite, object, object>>.Create(binder);
		}

		// Dev10 Bug 914027 - Changed the first parameter from dynamic to object, see comment at top for details
		public static IEnumerable<string> GetMemberNames(object obj)
		{
			var provider = obj as IDynamicMetaObjectProvider;
			Debug.Assert(provider != null, "obj doesn't implement IDynamicMetaObjectProvider");

			Expression parameter = Expression.Parameter(typeof(object));
			return provider.GetMetaObject(parameter).GetDynamicMemberNames();
		}
	}
#pragma warning restore 1591
}