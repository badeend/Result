using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Badeend;
using Badeend.ResultExtensions;

namespace Badeend.Tests;

#pragma warning disable CS1718 // Comparison made to same variable
#pragma warning disable CA1861 // Avoid constant arrays as arguments
#pragma warning disable CA1825 // Avoid zero-length array allocations

public class CollectionExtensionsTests
{
	[Fact]
	public void TryGetValue()
	{
		var dict = new Dictionary<int, string>()
		{
			[1] = "a",
			[2] = "b",
		};

		Assert.True(dict.TryGetValue(1).IsSuccess);
		Assert.True(dict.TryGetValue(3).IsError);
		Assert.True(dict.TryGetValue(2).IsSuccess);
	}

	[Fact]
	public void TryRemove()
	{
		Test(new Dictionary<int, string>()
		{
			[1] = "a",
			[2] = "b",
		});
		Test(new ConcurrentDictionary<int, string>()
		{
			[1] = "a",
			[2] = "b",
		});
		Test(new SortedDictionary<int, string>()
		{
			[1] = "a",
			[2] = "b",
		});

		static void Test(IDictionary<int, string> dictionary)
		{
			Assert.NotEmpty(dictionary);
			Assert.True(dictionary.TryRemove(1) is { IsSuccess: true, Value: "a" });
			Assert.True(dictionary.TryRemove(3).IsError);
			Assert.True(dictionary.TryRemove(2) is { IsSuccess: true, Value: "b" });
			Assert.Empty(dictionary);
		}
	}

	[Fact]
	public void TryFirstAndLastIndexOf()
	{
		Test([1, 2, 3, 1, 2, 3]);
		Test(new int[] {1, 2, 3, 1, 2, 3 });
		Test(new List<int> {1, 2, 3, 1, 2, 3 });
		Test(new ReadOnlyCollection<int>([1, 2, 3, 1, 2, 3]));

		static void Test(IReadOnlyList<int> items)
		{
			Assert.True(items.TryIndexOf(0).IsError);
			Assert.True(items.TryIndexOf(1) is { Value: 0 });
			Assert.True(items.TryIndexOf(2) is { Value: 1 });
			Assert.True(items.TryIndexOf(3) is { Value: 2 });
			Assert.True(items.TryIndexOf(4).IsError);

			Assert.True(items.TryLastIndexOf(0).IsError);
			Assert.True(items.TryLastIndexOf(1) is { Value: 3 });
			Assert.True(items.TryLastIndexOf(2) is { Value: 4 });
			Assert.True(items.TryLastIndexOf(3) is { Value: 5 });
			Assert.True(items.TryLastIndexOf(4).IsError);
		}
	}

	[Fact]
	public void TryFirstLastAndSingle()
	{
		Test([], [1], [1, 2, 3]);
		Test(new int[] { }, new int[] { 1 }, new int[] { 1, 2, 3 });
		Test(new List<int> { }, new List<int> { 1 }, new List<int> { 1, 2, 3 });
		Test(Enumerable.Empty<int>(), Enumerable.Empty<int>().Append(1), Enumerable.Empty<int>().Append(1).Append(2).Append(3));

		static void Test(IEnumerable<int> zero, IEnumerable<int> one, IEnumerable<int> multiple)
		{
			Assert.True(zero.TryFirst().IsError);
			Assert.True(zero.TryFirst(e => true).IsError);
			Assert.True(zero.TryLast().IsError);
			Assert.True(zero.TryLast(e => true).IsError);
			Assert.True(zero.TrySingle() is { Error: TrySingleError.NoElements });

			Assert.True(one.TryFirst() is { Value: 1 });
			Assert.True(one.TryLast(e => true) is { Value: 1 });
			Assert.True(one.TrySingle() is { Value: 1 });

			Assert.True(multiple.TryFirst() is { Value: 1 });
			Assert.True(multiple.TryFirst(e => true) is { Value: 1 });
			Assert.True(multiple.TryFirst(e => e == 2) is { Value: 2 });
			Assert.True(multiple.TryFirst(e => e == 4).IsError);
			Assert.True(multiple.TryFirst(e => false).IsError);
			Assert.True(multiple.TryLast(e => true) is { Value: 3 });
			Assert.True(multiple.TryLast(e => e == 2) is { Value: 2 });
			Assert.True(multiple.TryLast(e => e == 4).IsError);
			Assert.True(multiple.TryLast(e => false).IsError);
			Assert.True(multiple.TrySingle() is { Error: TrySingleError.MoreThanOneElement });
			Assert.True(multiple.TrySingle(e => true) is { Error: TrySingleError.MoreThanOneElement });
			Assert.True(multiple.TrySingle(e => false) is { Error: TrySingleError.NoElements });
			Assert.True(multiple.TrySingle(e => e == 2) is { Value: 2 });
			Assert.True(multiple.TrySingle(e => e == 4) is { Error: TrySingleError.NoElements });
		}
	}

	[Fact]
	public void TryMinMax()
	{
		Assert.True(Array.Empty<int>().TryMin().IsError);
		Assert.True(Array.Empty<int>().TryMax().IsError);
		Assert.True(Enumerable.Empty<int>().TryMin().IsError);
		Assert.True(Enumerable.Empty<int>().TryMax().IsError);
		Assert.True(new int[] { 3 }.TryMin() is { Value: 3 });
		Assert.True(new int[] { 3 }.TryMax() is { Value: 3 });
		Assert.True(new int[] { 2, 1, 4, 3 }.TryMin() is { Value: 1 });
		Assert.True(new int[] { 2, 1, 4, 3 }.TryMax() is { Value: 4 });
		Assert.True(new int?[] { }.TryMin().IsError);
		Assert.True(new int?[] { }.TryMax().IsError);
		Assert.True(new int?[] { 3 }.TryMin() is { Value: 3 });
		Assert.True(new int?[] { 3 }.TryMax() is { Value: 3 });
		Assert.True(new int?[] { null, null }.TryMin().IsError);
		Assert.True(new int?[] { null, null }.TryMax().IsError);
		Assert.True(new int?[] { null, 2, null, 1, null, 4, null, 3, null }.TryMin() is { Value: 1 });
		Assert.True(new int?[] { null, 2, null, 1, null, 4, null, 3, null }.TryMax() is { Value: 4 });
		Assert.True(Enumerable.Empty<int>().Append(2).Append(1).Append(4).Append(3).TryMin() is { Value: 1 });
		Assert.True(Enumerable.Empty<int>().Append(2).Append(1).Append(4).Append(3).TryMax() is { Value: 4 });
	}

	[Fact]
	public void TryElementAt()
	{
		int[] array = [1, 2, 3];
		var enumerable = Enumerable.Empty<int>().Append(1).Append(2).Append(3);

		Assert.True(array.TryElementAt(0) is { Value: 1 });
		Assert.True(enumerable.TryElementAt(0) is { Value: 1 });

		Assert.True(array.TryElementAt(1) is { Value: 2 });
		Assert.True(enumerable.TryElementAt(1) is { Value: 2 });

		Assert.True(array.TryElementAt(2) is { Value: 3 });
		Assert.True(enumerable.TryElementAt(2) is { Value: 3 });
		
		Assert.True(array.TryElementAt(3).IsError);
		Assert.True(enumerable.TryElementAt(3).IsError);

		Assert.Throws<ArgumentOutOfRangeException>(() => array.TryElementAt(-1));
		Assert.Throws<ArgumentOutOfRangeException>(() => enumerable.TryElementAt(-1));
	}
}
