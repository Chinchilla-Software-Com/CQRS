#region Copyright
// // -----------------------------------------------------------------------
// // <copyright company="cdmdotnet Limited">
// // 	Copyright cdmdotnet Limited. All rights reserved.
// // </copyright>
// // -----------------------------------------------------------------------
#endregion

using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using Cqrs.Entities;

namespace Cqrs.Repositories
{
	public class InMemoryDatabase
	{
		private static IDictionary<Type, object> Database { get; set; }

		static InMemoryDatabase()
		{
			Database = new ConcurrentDictionary<Type, object>();
		}

		public IDictionary<Guid, TEntity> Get<TEntity>()
			where TEntity : Entity
		{
			IDictionary<Guid, TEntity> result;
			if (!Database.ContainsKey(typeof(TEntity)))
			{
				result = new Dictionary<Guid, TEntity>();
				Database.Add(typeof(TEntity), result);
			}
			else
			{
				object rawResult = Database[typeof(TEntity)];
				result = (IDictionary<Guid, TEntity>)rawResult;
			}
			return result;
		}

		public IList<TEntity> GetAll<TEntity>()
			where TEntity : Entity
		{
			IDictionary<Guid, TEntity> result = Get<TEntity>();

			return new CollectionWrapper<TEntity>(result);
		}

		class CollectionWrapper<TEntity> : IList<TEntity>
			where TEntity : Entity
		{
			IDictionary<Guid, TEntity> Source { get; set; }

			public CollectionWrapper(IDictionary<Guid, TEntity> source)
			{
				Source = source;
			}

			#region Implementation of IEnumerable

			/// <summary>
			/// Returns an enumerator that iterates through the collection.
			/// </summary>
			/// <returns>
			/// A <see cref="T:System.Collections.Generic.IEnumerator`1"/> that can be used to iterate through the collection.
			/// </returns>
			public IEnumerator<TEntity> GetEnumerator()
			{
				return Source.Values.GetEnumerator();
			}

			/// <summary>
			/// Returns an enumerator that iterates through a collection.
			/// </summary>
			/// <returns>
			/// An <see cref="T:System.Collections.IEnumerator"/> object that can be used to iterate through the collection.
			/// </returns>
			IEnumerator IEnumerable.GetEnumerator()
			{
				return GetEnumerator();
			}

			#endregion

			#region Implementation of ICollection<T>

			/// <summary>
			/// Adds an item to the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <param name="item">The object to add to the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
			public void Add(TEntity item)
			{
				Source.Add(item.Rsn, item);
			}

			/// <summary>
			/// Removes all items from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only. </exception>
			public void Clear()
			{
				Source.Clear();
			}

			/// <summary>
			/// Determines whether the <see cref="T:System.Collections.Generic.ICollection`1"/> contains a specific value.
			/// </summary>
			/// <returns>
			/// true if <paramref name="item"/> is found in the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false.
			/// </returns>
			/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param>
			public bool Contains(TEntity item)
			{
				return Source.Values.Contains(item);
			}

			/// <summary>
			/// Copies the elements of the <see cref="T:System.Collections.Generic.ICollection`1"/> to an <see cref="T:System.Array"/>, starting at a particular <see cref="T:System.Array"/> index.
			/// </summary>
			/// <param name="array">The one-dimensional <see cref="T:System.Array"/> that is the destination of the elements copied from <see cref="T:System.Collections.Generic.ICollection`1"/>. The <see cref="T:System.Array"/> must have zero-based indexing.</param><param name="arrayIndex">The zero-based index in <paramref name="array"/> at which copying begins.</param><exception cref="T:System.ArgumentNullException"><paramref name="array"/> is null.</exception><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="arrayIndex"/> is less than 0.</exception><exception cref="T:System.ArgumentException"><paramref name="array"/> is multidimensional.-or-The number of elements in the source <see cref="T:System.Collections.Generic.ICollection`1"/> is greater than the available space from <paramref name="arrayIndex"/> to the end of the destination <paramref name="array"/>.-or-Type <paramref name="TEntity"/> cannot be cast automatically to the type of the destination <paramref name="array"/>.</exception>
			public void CopyTo(TEntity[] array, int arrayIndex)
			{
				Source.Values.CopyTo(array, arrayIndex);
			}

			/// <summary>
			/// Removes the first occurrence of a specific object from the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <returns>
			/// true if <paramref name="item"/> was successfully removed from the <see cref="T:System.Collections.Generic.ICollection`1"/>; otherwise, false. This method also returns false if <paramref name="item"/> is not found in the original <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </returns>
			/// <param name="item">The object to remove from the <see cref="T:System.Collections.Generic.ICollection`1"/>.</param><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.</exception>
			public bool Remove(TEntity item)
			{
				return Source.Remove(item.Rsn);
			}

			/// <summary>
			/// Gets the number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </summary>
			/// <returns>
			/// The number of elements contained in the <see cref="T:System.Collections.Generic.ICollection`1"/>.
			/// </returns>
			public int Count
			{
				get { return Source.Count; }
			}

			/// <summary>
			/// Gets a value indicating whether the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only.
			/// </summary>
			/// <returns>
			/// true if the <see cref="T:System.Collections.Generic.ICollection`1"/> is read-only; otherwise, false.
			/// </returns>
			public bool IsReadOnly
			{
				get { return Source.IsReadOnly; }
			}

			#endregion

			#region Implementation of IList<T>

			/// <summary>
			/// Determines the index of a specific item in the <see cref="T:System.Collections.Generic.IList`1"/>.
			/// </summary>
			/// <returns>
			/// The index of <paramref name="item"/> if found in the list; otherwise, -1.
			/// </returns>
			/// <param name="item">The object to locate in the <see cref="T:System.Collections.Generic.IList`1"/>.</param>
			public int IndexOf(TEntity item)
			{
				return Source.Values.ToList().IndexOf(item);
			}

			/// <summary>
			/// Inserts an item to the <see cref="T:System.Collections.Generic.IList`1"/> at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index at which <paramref name="item"/> should be inserted.</param><param name="item">The object to insert into the <see cref="T:System.Collections.Generic.IList`1"/>.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
			public void Insert(int index, TEntity item)
			{
				Add(item);
			}

			/// <summary>
			/// Removes the <see cref="T:System.Collections.Generic.IList`1"/> item at the specified index.
			/// </summary>
			/// <param name="index">The zero-based index of the item to remove.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
			public void RemoveAt(int index)
			{
				Remove(Source.Values.ToList()[index]);
			}

			/// <summary>
			/// Gets or sets the element at the specified index.
			/// </summary>
			/// <returns>
			/// The element at the specified index.
			/// </returns>
			/// <param name="index">The zero-based index of the element to get or set.</param><exception cref="T:System.ArgumentOutOfRangeException"><paramref name="index"/> is not a valid index in the <see cref="T:System.Collections.Generic.IList`1"/>.</exception><exception cref="T:System.NotSupportedException">The property is set and the <see cref="T:System.Collections.Generic.IList`1"/> is read-only.</exception>
			public TEntity this[int index]
			{
				get { return Source.Values.ToList()[index]; }
				set { Source[this[index].Rsn] = value; }
			}

			#endregion
		}
	}
}