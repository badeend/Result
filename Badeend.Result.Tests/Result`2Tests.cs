using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Badeend;

namespace Badeend.Tests;

#pragma warning disable CS1718 // Comparison made to same variable

/// <summary>
/// Tests for <see cref="Result{TValue, TError}"/>.
/// </summary>
public class Result2Tests
{
	/// <summary>
	/// Success
	/// </summary>
	private Result<int, string> s = 42;

	/// <summary>
	/// Error
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
		Result<int, string> f3 = Result.Error<int, string>("Bad");

		Result<int, string> d2 = Result.Error<int, string>(null!);

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

		Assert.True(s.GetHashCode() == s2.GetHashCode());
		Assert.True(s.GetHashCode() == s3.GetHashCode());
		Assert.True(f.GetHashCode() == f2.GetHashCode());
		Assert.True(f.GetHashCode() == f3.GetHashCode());
		Assert.True(s.GetHashCode() != f.GetHashCode());
		Assert.True(s.GetHashCode() != d.GetHashCode());
	}

	[Fact]
	public void ObjectToString()
	{
		Assert.Equal("Success(42)", s.ToString());
		Assert.Equal("Error(Bad)", f.ToString());
		Assert.Equal("Error(null)", d.ToString());
	}

	[Fact]
	public void State()
	{
		Assert.Equal(ResultState.Success, s.State);
		Assert.Equal(ResultState.Error, f.State);
		Assert.Equal(ResultState.Error, d.State);
	}

	[Fact]
	public void IsSuccess()
	{
		Assert.True(s.IsSuccess);
		Assert.False(f.IsSuccess);
		Assert.False(d.IsSuccess);
	}

	[Fact]
	public void IsError()
	{
		Assert.False(s.IsError);
		Assert.True(f.IsError);
		Assert.True(d.IsError);
	}

	[Fact]
	public void Value()
	{
		ref readonly var r = ref s.Value;

		Assert.True(r == 42);
		Assert.True(s.Value == 42);
		Assert.Throws<InvalidOperationException>(() => f.Value);
		Assert.Throws<InvalidOperationException>(() => d.Value);
	}

	[Fact]
	public void ValueInnerException()
	{
		Result<int, Error> e1 = new Error("My message");
		var e1Exception = Assert.Throws<InvalidOperationException>(() => e1.Value);
		Assert.Equal("Operation was not successful. See inner exception for more details.", e1Exception.Message);
		Assert.Equal("My message", e1Exception.InnerException!.Message);

		Result<int, object> e2 = (object)new Error("My message");
		var e2Exception = Assert.Throws<InvalidOperationException>(() => e2.Value);
		Assert.Equal("Operation was not successful.", e2Exception.Message);
		Assert.Null(e2Exception.InnerException);

		Result<int, ArgumentException> e3 = new ArgumentException("My message");
		var e3Exception = Assert.Throws<InvalidOperationException>(() => e3.Value);
		Assert.Equal("Operation was not successful. See inner exception for more details.", e3Exception.Message);
		Assert.Equal("My message", e3Exception.InnerException!.Message);

		Result<int, object> e4 = (object)new ArgumentException("My message");
		var e4Exception = Assert.Throws<InvalidOperationException>(() => e4.Value);
		Assert.Equal("Operation was not successful.", e4Exception.Message);
		Assert.Null(e4Exception.InnerException);
	}

	[Fact]
	public void Error()
	{
		ref readonly var r = ref f.Error;

		Assert.Throws<InvalidOperationException>(() => s.Error);
		Assert.True(r == "Bad");
		Assert.True(f.Error == "Bad");
		Assert.True(d.Error is null);
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
	public void GetErrorOrDefault()
	{
		Assert.True(s.GetErrorOrDefault() is null);
		Assert.True(f.GetErrorOrDefault() == "Bad");
		Assert.True(d.GetErrorOrDefault() is null);

		Assert.True(s.GetErrorOrDefault("Worse") == "Worse");
		Assert.True(f.GetErrorOrDefault("Worse") == "Bad");
		Assert.True(d.GetErrorOrDefault("Worse") is null);
	}

	[Fact]
	public void TryGetValue()
	{
		Assert.True(s.TryGetValue(out var o1) == true && o1 == 42);
		Assert.True(f.TryGetValue(out var o2) == false && o2 == 0);
		Assert.True(d.TryGetValue(out var o3) == false && o3 == 0);

		Assert.True(s.TryGetValue(out var s1, out var f1) == true && s1 == 42 && f1 is null);
		Assert.True(f.TryGetValue(out var s2, out var f2) == false && s2 == 0 && f2 == "Bad");
		Assert.True(d.TryGetValue(out var s3, out var f3) == false && s3 == 0 && f3 is null);
	}

	[Fact]
	public void TryGetError()
	{
		Assert.True(s.TryGetError(out var o1) == false && o1 is null);
		Assert.True(f.TryGetError(out var o2) == true && o2 == "Bad");
		Assert.True(d.TryGetError(out var o3) == true && o3 is null);
	}

	[Fact]
	public void GetValueRefOrDefaultRef()
	{
		Assert.True(Result.GetValueRefOrDefaultRef(ref s) == 42);
		Assert.True(Result.GetValueRefOrDefaultRef(ref f) == 0);
		Assert.True(Result.GetValueRefOrDefaultRef(ref d) == 0);
	}

	[Fact]
	public void GetErrorRefOrDefaultRef()
	{
		Assert.True(Result.GetErrorRefOrDefaultRef(ref s) is null);
		Assert.True(Result.GetErrorRefOrDefaultRef(ref f) == "Bad");
		Assert.True(Result.GetErrorRefOrDefaultRef(ref d) is null);
	}

	[Fact]
	public void CompareTo()
	{
		Result<int, string> three = 3; // Success
		Result<int, string> four = 4; // Success

		Result<int, string> cat = "Cat"; // Error
		Result<int, string> dog = "Dog"; // Error

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
}
