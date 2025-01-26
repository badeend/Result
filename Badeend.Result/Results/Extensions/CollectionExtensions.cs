using System.Collections.Concurrent;
using System.Runtime.CompilerServices;

namespace Badeend.Results.Extensions;

/// <summary>
/// Fallible overloads for commonly used collection operations.
/// </summary>
public static class CollectionExtensions
{
	private static readonly Error KeyNotFoundError = new("Key not found in dictionary.");
	private static readonly Error ElementNotFoundInListError = new("Element not found in list.");
	private static readonly Error NoMatchingElementFoundError = new("No matching element found in sequence.");
	private static readonly Error ListEmptyError = new("List is empty.");
	private static readonly Error SequenceEmptyError = new("Sequence is empty.");
	private static readonly Error NoNonNullValues = new("Sequence does not contain any values or only contains null values.");
	private static readonly Error IndexNotFound = new("Sequence does not contain the provided index.");

	/// <summary>
	/// Get the value associated with the specified <paramref name="key"/>.
	/// Returns an error if the key was not found.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is null.</exception>
	public static Result<TValue> TryGetValue<TKey, TValue>(this IReadOnlyDictionary<TKey, TValue> dictionary, TKey key)
	{
		if (dictionary is null)
		{
			throw new ArgumentNullException(nameof(dictionary));
		}

		return dictionary.TryGetValue(key, out var value) ? value : KeyNotFoundError;
	}

	/// <summary>
	/// Remove the value with the specified <paramref name="key"/>
	/// from the <paramref name="dictionary"/>.
	/// Returns the removed value or an error if the key was not found.
	/// </summary>
	/// <exception cref="ArgumentNullException"><paramref name="dictionary"/> is null.</exception>
	public static Result<TValue> TryRemove<TKey, TValue>(this IDictionary<TKey, TValue> dictionary, TKey key)
	{
		if (dictionary is null)
		{
			throw new ArgumentNullException(nameof(dictionary));
		}

#pragma warning disable CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.
#if NETSTANDARD2_1_OR_GREATER || NETCOREAPP2_0_OR_GREATER
		if (dictionary is Dictionary<TKey, TValue> systemDictionary)
		{
			return systemDictionary.Remove(key, out var value) ? value : KeyNotFoundError;
		}
#endif

		if (dictionary is ConcurrentDictionary<TKey, TValue> concurrentDictionary)
		{
			return concurrentDictionary.TryRemove(key, out var value) ? value : KeyNotFoundError;
		}
#pragma warning restore CS8714 // The type cannot be used as type parameter in the generic type or method. Nullability of type argument doesn't match 'notnull' constraint.

		{
			if (dictionary.TryGetValue(key, out var value) && dictionary.Remove(key))
			{
				return value;
			}

			return KeyNotFoundError;
		}
	}

	/// <summary>
	/// Find the index of the first occurrence of <paramref name="item"/> in
	/// the list. Returns an error if not found.
	/// </summary>
	/// <remarks>
	/// Similar to <see cref="List{T}.IndexOf(T)"/>, but with the added guarantee
	/// that the returned integer is always a valid index (i.e. not negative).
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
	public static Result<int> TryIndexOf<T>(this IReadOnlyList<T> list, T item)
	{
		if (list is null)
		{
			throw new ArgumentNullException(nameof(list));
		}

		if (list is IList<T> ilist)
		{
			var index = ilist.IndexOf(item);
			if (index >= 0)
			{
				return index;
			}

			return ElementNotFoundInListError;
		}

		var count = list.Count;
		for (int i = 0; i < count; i++)
		{
			if (EqualityComparer<T>.Default.Equals(list[i], item))
			{
				return i;
			}
		}

		return ElementNotFoundInListError;
	}

	/// <summary>
	/// Find the index of the last occurrence of <paramref name="item"/> in
	/// the list. Returns an error if not found.
	/// </summary>
	/// <remarks>
	/// Similar to <see cref="List{T}.LastIndexOf(T)"/>, but with the added guarantee
	/// that the returned integer is always a valid index (i.e. not negative).
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="list"/> is null.</exception>
	public static Result<int> TryLastIndexOf<T>(this IReadOnlyList<T> list, T item)
	{
		if (list is null)
		{
			throw new ArgumentNullException(nameof(list));
		}

		if (list is List<T> systemList)
		{
			var index = systemList.LastIndexOf(item);
			if (index >= 0)
			{
				return index;
			}

			return ElementNotFoundInListError;
		}
		else if (list is T[] systemArray)
		{
			var index = Array.LastIndexOf(systemArray, item);
			if (index >= 0)
			{
				return index;
			}

			return ElementNotFoundInListError;
		}

		var count = list.Count;
		for (int i = count - 1; i >= 0; i--)
		{
			if (EqualityComparer<T>.Default.Equals(list[i], item))
			{
				return i;
			}
		}

		return ElementNotFoundInListError;
	}

	/// <summary>
	/// Get the first element in the collection.
	/// Returns an error if the collection is empty.
	/// </summary>
	/// <remarks>
	/// This method is similar to:
	/// <list type="bullet">
	///   <item>
	///     <see cref="Enumerable.First{TSource}(IEnumerable{TSource})"/>,
	///     except that this method doesn't throw for empty collections.
	///   </item>
	///   <item>
	///     <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource})"/>,
	///     except that this method retains the distinction between: an empty
	///     collection, and: a non-empty collection whose first element happens
	///     to be the <c>default</c> value.
	///   </item>
	/// </list>
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static Result<T> TryFirst<T>(this IEnumerable<T> source)
	{
		if (source is null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		if (source is IList<T> list)
		{
			if (list.Count == 0)
			{
				return ListEmptyError;
			}

			return list[0];
		}

		foreach (var item in source)
		{
			return item;
		}

		return SequenceEmptyError;
	}

	/// <summary>
	/// Get the first element in the collection that satisfies a specified
	/// condition. Returns an error if no element in the collection satisfies
	/// the condition in <paramref name="predicate"/>.
	/// </summary>
	/// <remarks>
	/// This method is similar to:
	/// <list type="bullet">
	///   <item>
	///     <see cref="Enumerable.First{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>,
	///     except that this method doesn't throw if there were no matches.
	///   </item>
	///   <item>
	///     <see cref="Enumerable.FirstOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>,
	///     except that this method retains the distinction between: there were
	///     no matches, and: there was a match but the element happens to be the
	///     <c>default</c> value.
	///   </item>
	/// </list>
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
	public static Result<T> TryFirst<T>(this IEnumerable<T> source, Func<T, bool> predicate)
	{
		if (source is null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		if (predicate is null)
		{
			throw new ArgumentNullException(nameof(predicate));
		}

		foreach (var item in source)
		{
			if (predicate(item))
			{
				return item;
			}
		}

		return NoMatchingElementFoundError;
	}

	/// <summary>
	/// Get the last element in the collection.
	/// Returns an error if the collection is empty.
	/// </summary>
	/// <remarks>
	/// This method is similar to:
	/// <list type="bullet">
	///   <item>
	///     <see cref="Enumerable.Last{TSource}(IEnumerable{TSource})"/>,
	///     except that this method doesn't throw for empty collections.
	///   </item>
	///   <item>
	///     <see cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource})"/>,
	///     except that this method retains the distinction between: an empty
	///     collection, and: a non-empty collection whose last element happens
	///     to be the <c>default</c> value.
	///   </item>
	/// </list>
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static Result<T> TryLast<T>(this IEnumerable<T> source)
	{
		if (source is null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		if (source is IList<T> list)
		{
			var count = list.Count;
			if (count == 0)
			{
				return ListEmptyError;
			}

			return list[count - 1];
		}

		Result<T> result = SequenceEmptyError;

		foreach (var item in source)
		{
			result = item;
		}

		return result;
	}

	/// <summary>
	/// Get the last element in the collection that satisfies a specified
	/// condition. Returns an error if no element in the collection satisfies
	/// the condition in <paramref name="predicate"/>.
	/// </summary>
	/// <remarks>
	/// This method is similar to:
	/// <list type="bullet">
	///   <item>
	///     <see cref="Enumerable.Last{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>,
	///     except that this method doesn't throw if there were no matches.
	///   </item>
	///   <item>
	///     <see cref="Enumerable.LastOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>,
	///     except that this method retains the distinction between: there were
	///     no matches, and: there was a match but the element happens to be the
	///     <c>default</c> value.
	///   </item>
	/// </list>
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
	public static Result<T> TryLast<T>(this IEnumerable<T> source, Func<T, bool> predicate)
	{
		if (source is null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		if (predicate is null)
		{
			throw new ArgumentNullException(nameof(predicate));
		}

		Result<T> result = NoMatchingElementFoundError;

		foreach (var item in source)
		{
			if (predicate(item))
			{
				result = item;
			}
		}

		return result;
	}

	/// <summary>
	/// Get the only element in the collection.
	/// Returns an error if there is not exactly one element in the collection.
	/// </summary>
	/// <remarks>
	/// This method is similar to:
	/// <list type="bullet">
	///   <item>
	///     <see cref="Enumerable.Single{TSource}(IEnumerable{TSource})"/>,
	///     except that this method doesn't throw for wrongly-sized collections.
	///   </item>
	///   <item>
	///     <see cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource})"/>,
	///     except that this method retains the distinction between:
	///     the collection doesn't contain a single element, and:
	///     the collection has exactly one element that happens to be the
	///     <c>default</c> value.
	///   </item>
	/// </list>
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static Result<T, TrySingleError> TrySingle<T>(this IEnumerable<T> source)
	{
		if (source is null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		if (TryGetNonEnumeratedCount(source, out var count))
		{
			if (count == 0)
			{
				return TrySingleError.NoElements;
			}
			else if (count > 1)
			{
				return TrySingleError.MoreThanOneElement;
			}
			else
			{
				return source.Single();
			}
		}

		using var enumerator = source.GetEnumerator();

		if (!enumerator.MoveNext())
		{
			return TrySingleError.NoElements;
		}

		var item = enumerator.Current;

		if (enumerator.MoveNext())
		{
			return TrySingleError.MoreThanOneElement;
		}

		return item;
	}

	/// <summary>
	/// Get the only element in the collection that satisfies a specified
	/// condition. Returns an error if there is not exactly one element
	/// matching the <paramref name="predicate"/>.
	/// </summary>
	/// <remarks>
	/// This method is similar to:
	/// <list type="bullet">
	///   <item>
	///     <see cref="Enumerable.Single{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>,
	///     except that this method doesn't throw if there wasn't exactly one match.
	///   </item>
	///   <item>
	///     <see cref="Enumerable.SingleOrDefault{TSource}(IEnumerable{TSource}, Func{TSource, bool})"/>,
	///     except that this method retains the distinction between: there wasn't
	///     exactly one match, and: there was a single match but the element
	///     happens to be the <c>default</c> value.
	///   </item>
	/// </list>
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> or <paramref name="predicate"/> is null.</exception>
	public static Result<T, TrySingleError> TrySingle<T>(this IEnumerable<T> source, Func<T, bool> predicate)
	{
		if (source is null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		if (predicate is null)
		{
			throw new ArgumentNullException(nameof(predicate));
		}

		Result<T, TrySingleError> result = TrySingleError.NoElements;

		foreach (var item in source)
		{
			if (predicate(item))
			{
				if (result.IsSuccess)
				{
					return TrySingleError.MoreThanOneElement;
				}

				result = item;
			}
		}

		return result;
	}

	/// <summary>
	/// Get the element at the specified <paramref name="index"/> in the sequence.
	/// Returns an error if the index exceeds the sequence length.
	/// </summary>
	/// <remarks>
	/// This method is similar to:
	/// <list type="bullet">
	///   <item>
	///     <see cref="Enumerable.ElementAt{TSource}(IEnumerable{TSource},int)"/>,
	///     except that this method doesn't throw if the index exceeds the sequence length.
	///   </item>
	///   <item>
	///     <see cref="Enumerable.ElementAtOrDefault{TSource}(IEnumerable{TSource}, int)"/>,
	///     except that this method retains the distinction between: the index
	///     was out of bounds, and: the index exists but happens to contains
	///     the <c>default</c> value.
	///   </item>
	/// </list>
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	/// <exception cref="ArgumentOutOfRangeException"><paramref name="index"/> is negative.</exception>
	public static Result<T> TryElementAt<T>(this IEnumerable<T> source, int index)
	{
		if (source is null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		if (index < 0)
		{
			throw new ArgumentOutOfRangeException(nameof(index));
		}

		if (TryGetNonEnumeratedCount(source, out var count))
		{
			if (index >= count)
			{
				return IndexNotFound;
			}

			return source.ElementAt(index);
		}

		foreach (var item in source)
		{
			if (index == 0)
			{
				return item;
			}

			index--;
		}

		return IndexNotFound;
	}

	/// <summary>
	/// Get the minimum value in the sequence. Returns an error if the sequence
	/// does not contain any non-null elements.
	/// </summary>
	/// <remarks>
	/// Similar to <see cref="Enumerable.Min{TSource}(IEnumerable{TSource})"/>.
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static Result<T> TryMin<T>(this IEnumerable<T> source)
	{
		if (source is null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		// If T is a nullable type (either a reference type or a nullable value type),
		// we can simply defer to LINQ and check for a `null` return value:
		if (default(T) is null)
		{
			var min = source.Min();
			if (min is null)
			{
				return NoNonNullValues;
			}

			return min;
		}

		// Attempt to defer to LINQ's (potentially vectorized) implementation.
		if (TryGetNonEnumeratedCount(source, out var count))
		{
			if (count == 0)
			{
				return NoNonNullValues;
			}

			// This should never throw nor return null, as we've already checked the source isn't empty.
			return source.Min()!;
		}

		Result<T> result = NoNonNullValues;

		foreach (var item in source)
		{
			if (result.IsError || Comparer<T>.Default.Compare(item, result.Value) < 0)
			{
				result = item;
			}
		}

		return result;
	}

	/// <summary>
	/// Get the maximum value in the sequence. Returns an error if the sequence
	/// does not contain any non-null elements.
	/// </summary>
	/// <remarks>
	/// Similar to <see cref="Enumerable.Max{TSource}(IEnumerable{TSource})"/>.
	/// </remarks>
	/// <exception cref="ArgumentNullException"><paramref name="source"/> is null.</exception>
	public static Result<T> TryMax<T>(this IEnumerable<T> source)
	{
		if (source is null)
		{
			throw new ArgumentNullException(nameof(source));
		}

		// If T is a nullable type (either a reference type or a nullable value type),
		// we can simply defer to LINQ and check for a `null` return value:
		if (default(T) is null)
		{
			var max = source.Max();
			if (max is null)
			{
				return NoNonNullValues;
			}

			return max;
		}

		// Attempt to defer to LINQ's (potentially vectorized) implementation.
		if (TryGetNonEnumeratedCount(source, out var count))
		{
			if (count == 0)
			{
				return NoNonNullValues;
			}

			// This should never throw nor return null, as we've already checked the source isn't empty.
			return source.Max()!;
		}

		Result<T> result = NoNonNullValues;

		foreach (var item in source)
		{
			if (result.IsError || Comparer<T>.Default.Compare(item, result.Value) > 0)
			{
				result = item;
			}
		}

		return result;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private static bool TryGetNonEnumeratedCount<T>(IEnumerable<T> source, out int count)
	{
#if NET6_0_OR_GREATER
		return System.Linq.Enumerable.TryGetNonEnumeratedCount(source, out count);
#else
		if (source is ICollection<T> collection)
		{
			count = collection.Count;
			return true;
		}

		count = 0;
		return false;
#endif
	}
}
