using System;
using System.Collections;
using System.Linq;
using System.Reflection;

using Omu.ValueInjecter;

namespace Cqrs.Entities
{
	/// <summary>
	/// A helper class to aid auto-mapping
	/// </summary>
	public class AutomapHelper : IAutomapHelper
	{
		/// <summary>
		/// Creates and returns a new <typeparamref name="TTarget"/> with all the easily auto-mapped properties from <paramref name="source"/> set.
		/// </summary>
		/// <param name="source">The <typeparamref name="TSource"/> to convert.</param>
		public virtual TTarget Automap<TSource, TTarget>(TSource source)
			where TTarget : new()
		{
			if (source == null)
			{
				return default(TTarget);
			}

			return Automap(source, new TTarget());
		}

		/// <summary>
		/// Creates and returns a new <typeparamref name="TTarget"/> with all the easily auto-mapped properties from <paramref name="source"/> set.
		/// </summary>
		public virtual TTarget Automap<TSource, TTarget>(TSource source, TTarget target)
		{
			target.InjectFrom(source);

			// The following tries to compensate for collections
			return AutomapCollections(typeof(TSource), typeof(TTarget), source, target);
		}

		/// <summary>
		/// Convert the provided <paramref name="source"/> into <typeparamref name="TTarget"/> where both items are <see cref="IEnumerable"/>
		/// </summary>
		protected virtual TTarget AutomapCollections<TSource, TTarget>(Type sourceType, Type targetType, TSource source, TTarget target)
		{
			if (sourceType == null)
			{
				throw new ArgumentNullException("sourceType");
			}

			if (targetType == null)
			{
				throw new ArgumentNullException("targetType");
			}

			Type actualTargetType = target.GetType();

			if (!sourceType.IsGenericType || !(targetType.IsGenericType || actualTargetType.IsGenericType))
			{
				if (!targetType.IsPrimitive)
				{
					AutomapNonPrimitives(sourceType, targetType, source, target);
				}

				return target;
			}

			Type[] sourceChildTypeCollection = sourceType.GetGenericArguments();
			if (sourceChildTypeCollection.Length == 1)
			{
				/*
				Type sourceChildType = sourceChildTypeCollection.Single();
				if (typeof(IQueryView).IsAssignableFrom(sourceChildType))
				{
					Type genericPagedQueryViewType = typeof(PagedQueryView<>).MakeGenericType(sourceChildTypeCollection.Single());
					if (genericPagedQueryViewType.IsAssignableFrom(sourceType))
					{
						AutomapNonPrimitives(sourceType, targetType, source, target);
					}
				}
				*/
			}

			var sourceCollection = source as IEnumerable;
			if (sourceCollection == null)
			{
				return target;
			}

			var targetCollection = target as IList;
/*
			if (targetCollection == null)
			{
				var otargetLimitedList = target as ILimitedSizeClientView;
				if (otargetLimitedList != null)
				{
					targetCollection = otargetLimitedList.GetResults() as IList;
					if (targetCollection == null)
					{
						return target;
					}
				}
			}

			if (targetCollection == null)
			{
				var targetLimitedList = target as IClientViewList;
				if (targetLimitedList == null)
				{
					return target;
				}

				targetCollection = targetLimitedList.GetResults() as IList;
*/
				if (targetCollection == null)
				{
					return target;
				}
/*
			}
*/

			Type[] targetChildTypeCollection = targetType.GetGenericArguments();
			if (targetChildTypeCollection.Length == 0)
			{
				targetChildTypeCollection = actualTargetType.GetGenericArguments();
			}

			Type targetChildType = targetChildTypeCollection.Single();
			foreach (object sourceItem in sourceCollection)
			{
				object targetItem = Activator.CreateInstance(targetChildType);
				targetItem.InjectFrom(sourceItem);
				targetCollection.Add(AutomapCollections(sourceItem.GetType(), targetItem.GetType(), sourceItem, targetItem));
			}

			return target;
		}

		protected virtual void AutomapNonPrimitives(Type sourceType, Type targetType, object source, object target)
		{
			if (source == null)
			{
				return;
			}

			if (target == null)
			{
				return;
			}

			var sourcePropertyList = source.GetType()
										   .GetProperties(BindingFlags.Instance | BindingFlags.Public)
										   .Where(p => !p.GetIndexParameters().Any())
										   .Where(p => p.CanRead)
										   .ToList();
			var targetPropertyList = target.GetType()
										   .GetProperties(BindingFlags.Instance | BindingFlags.Public)
										   .Where(p => !p.GetIndexParameters().Any())
										   .Where(p => p.CanRead && p.CanWrite)
										   .ToList();
			foreach (PropertyInfo sourceProperty in sourcePropertyList)
			{
				object sourceValue = sourceProperty.GetValue(source, null);
				if (sourceValue != null)
				{
					PropertyInfo targetProperty = targetPropertyList.SingleOrDefault(property => property.Name == sourceProperty.Name);
					if (targetProperty == null)
					{
						continue;
					}

					object targetValue = targetProperty.GetValue(target, null);
					if (targetValue == null)
					{
						try
						{
							object targetItem;
							if (targetProperty.PropertyType.IsPrimitive || targetProperty.PropertyType == typeof(string))
							{
								targetProperty.SetValue(target, sourceValue, null);
								continue;
							}

							if (!targetProperty.PropertyType.IsInterface)
							{
								targetItem = Activator.CreateInstance(targetProperty.PropertyType);
							}
							else
							{
								if (!typeof(IEnumerable).IsAssignableFrom(targetProperty.PropertyType))
								{
									continue;
								}

								Type genericEnumerableType = typeof(System.Collections.Generic.List<>).MakeGenericType(targetProperty.PropertyType.GetGenericArguments().Single());
								targetItem = Activator.CreateInstance(genericEnumerableType);
							}

							AutomapCollections(sourceProperty.PropertyType, targetProperty.PropertyType, sourceValue, targetItem);
							targetProperty.SetValue(target, targetItem, null);
						}
						catch (Exception)
						{
						}
					}
					else if (targetProperty.PropertyType.IsPrimitive)
					{
						targetProperty.SetValue(target, sourceValue, null);
					}
				}
			}
		}
	}
}