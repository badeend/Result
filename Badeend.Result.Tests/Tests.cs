using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Badeend;

namespace Badeend.Tests;

#pragma warning disable CS1718 // Comparison made to same variable

public class Tests
{
	/// <summary>
	/// Success
	/// </summary>
	private Result<int, string> s = 42;

	/// <summary>
	/// Failure
	/// </summary>
	private Result<int, string> f = "Bad";

	/// <summary>
	/// Default
	/// </summary>
	private Result<int, string> d = default;

	[Fact]
	public void StructuralEquality()
	{
		Result<int, string> s2 = 42;
		Result<int, string> s3 = Result.Success<int, string>(42);

		Result<int, string> f2 = "Bad";
		Result<int, string> f3 = Result.Failure<int, string>("Bad");

		Result<int, string> d2 = Result.Failure<int, string>(null!);

		Assert.True(s == s);
		Assert.True(s == s2);
		Assert.True(s == s3);

		Assert.True(s == 42);
		Assert.True(s != 314);
		Assert.True(s != "Bad");
		Assert.True(s != default);

		Assert.True(f == f);
		Assert.True(f == f2);
		Assert.True(f == f3);

		Assert.True(f == "Bad");
		Assert.True(f != "Worse");
		Assert.True(f != 42);
		Assert.True(f != default);

		Assert.True(d == d);
		Assert.True(d == d2);
		Assert.True(d == default);

		Assert.True(d == null!);
		Assert.True(d != "Bad");
		Assert.True(d != 42);

		Assert.True(s.GetHashCode() == 42.GetHashCode());
		Assert.True(f.GetHashCode() == "Bad".GetHashCode());
		Assert.True(d.GetHashCode() == 0);
	}

	[Fact]
	public void ObjectToString()
	{
		Assert.Equal("Success(42)", s.ToString());
		Assert.Equal("Failure(Bad)", f.ToString());
		Assert.Equal("Failure(null)", d.ToString());
	}

	[Fact]
	public void State()
	{
		Assert.Equal(ResultState.Success, s.State);
		Assert.Equal(ResultState.Failure, f.State);
		Assert.Equal(ResultState.Failure, d.State);
	}

	[Fact]
	public void IsSuccess()
	{
		Assert.True(s.IsSuccess);
		Assert.False(f.IsSuccess);
		Assert.False(d.IsSuccess);
	}

	[Fact]
	public void IsFailure()
	{
		Assert.False(s.IsFailure);
		Assert.True(f.IsFailure);
		Assert.True(d.IsFailure);
	}

	[Fact]
	public void Value()
	{
		Assert.True(s.Value == 42);
		Assert.Throws<InvalidOperationException>(() => f.Value);
		Assert.Throws<InvalidOperationException>(() => d.Value);
	}

	[Fact]
	public void Failure()
	{
		Assert.Throws<InvalidOperationException>(() => s.Failure);
		Assert.True(f.Failure == "Bad");
		Assert.True(d.Failure is null);
	}

	[Fact]
	public void GetValueOrDefault()
	{
		Assert.True(s.GetValueOrDefault() == 42);
		Assert.True(f.GetValueOrDefault() == 0);
		Assert.True(d.GetValueOrDefault() == 0);

		Assert.True(s.GetValueOrDefault(314) == 42);
		Assert.True(f.GetValueOrDefault(314) == 314);
		Assert.True(d.GetValueOrDefault(314) == 314);
	}

	[Fact]
	public void GetFailureOrDefault()
	{
		Assert.True(s.GetFailureOrDefault() is null);
		Assert.True(f.GetFailureOrDefault() == "Bad");
		Assert.True(d.GetFailureOrDefault() is null);

		Assert.True(s.GetFailureOrDefault("Worse") == "Worse");
		Assert.True(f.GetFailureOrDefault("Worse") == "Bad");
		Assert.True(d.GetFailureOrDefault("Worse") is null);
	}

	[Fact]
	public void TryGetValue()
	{
		Assert.True(s.TryGetValue(out var o1) == true && o1 == 42);
		Assert.True(f.TryGetValue(out var o2) == false && o2 == 0);
		Assert.True(d.TryGetValue(out var o3) == false && o3 == 0);
	}

	[Fact]
	public void TryGetFailure()
	{
		Assert.True(s.TryGetFailure(out var o1) == false && o1 is null);
		Assert.True(f.TryGetFailure(out var o2) == true && o2 == "Bad");
		Assert.True(d.TryGetFailure(out var o3) == true && o3 is null);
	}

	[Fact]
	public void GetValueRefOrDefaultRef()
	{
		Assert.True(Result.GetValueRefOrDefaultRef(ref s) == 42);
		Assert.True(Result.GetValueRefOrDefaultRef(ref f) == 0);
		Assert.True(Result.GetValueRefOrDefaultRef(ref d) == 0);
	}

	[Fact]
	public void GetFailureRefOrDefaultRef()
	{
		Assert.True(Result.GetFailureRefOrDefaultRef(ref s) is null);
		Assert.True(Result.GetFailureRefOrDefaultRef(ref f) == "Bad");
		Assert.True(Result.GetFailureRefOrDefaultRef(ref d) is null);
	}

	[Fact]
	public void CompareTo()
	{
		Result<int, string> three = 3; // Success
		Result<int, string> four = 4; // Success

		Result<int, string> cat = "Cat"; // Failure
		Result<int, string> dog = "Dog"; // Failure

		Assert.True(three.CompareTo(four) < 0);
		Assert.True(three.CompareTo(three) == 0);
		Assert.True(four.CompareTo(three) > 0);
		Assert.True(four.CompareTo(four) == 0);

		Assert.True(cat.CompareTo(dog) < 0);
		Assert.True(cat.CompareTo(cat) == 0);
		Assert.True(dog.CompareTo(cat) > 0);
		Assert.True(dog.CompareTo(dog) == 0);

		Assert.True(three.CompareTo(cat) < 0);
		Assert.True(cat.CompareTo(three) > 0);

		Result<int, string>[] unordered = [dog, four, three, cat];
		Result<int, string>[] ordered = unordered.OrderBy(r => r).ToArray();

		Assert.Equal([three, four, cat, dog], ordered);
	}

	[Fact]
	public void GetUnderlyingType()
	{
		Assert.Equal(typeof(int), Result.GetUnderlyingValueType(typeof(Result<int, string>)));
		Assert.Equal(typeof(string), Result.GetUnderlyingFailureType(typeof(Result<int, string>)));

		Assert.Null(Result.GetUnderlyingValueType(typeof(Result)));
		Assert.Null(Result.GetUnderlyingFailureType(typeof(Result)));

		Assert.Null(Result.GetUnderlyingValueType(typeof(Result<,>)));
		Assert.Null(Result.GetUnderlyingFailureType(typeof(Result<,>)));

		Assert.Null(Result.GetUnderlyingValueType(typeof(DateTime)));
		Assert.Null(Result.GetUnderlyingFailureType(typeof(DateTime)));

		Assert.Throws<ArgumentNullException>(() => Result.GetUnderlyingValueType(null!));
		Assert.Throws<ArgumentNullException>(() => Result.GetUnderlyingFailureType(null!));
	}
}
