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
	/// Error
	/// </summary>
	private Result<int, string> e = "Bad";

	/// <summary>
	/// Default
	/// </summary>
	private Result<int, string> d = default;

	[Fact]
	public void StructuralEquality()
	{
		Result<int, string> s2 = new(42);
		Result<int, string> s3 = Result.Success<int, string>(42);

		Result<int, string> e2 = new("Bad");
		Result<int, string> e3 = Result.Error<int, string>("Bad");

		Result<int, string> d2 = Result.Error<int, string>(null!);

		Assert.True(s == s);
		Assert.True(s == s2);
		Assert.True(s == s3);

		Assert.True(s == 42);
		Assert.True(s != 314);
		Assert.True(s != "Bad");
		Assert.True(s != default);

		Assert.True(e == e);
		Assert.True(e == e2);
		Assert.True(e == e3);

		Assert.True(e == "Bad");
		Assert.True(e != "Worse");
		Assert.True(e != 42);
		Assert.True(e != default);

		Assert.True(d == d);
		Assert.True(d == d2);
		Assert.True(d == default);

		Assert.True(d == null!);
		Assert.True(d != "Bad");
		Assert.True(d != 42);

		Assert.True(s.GetHashCode() == 42.GetHashCode());
		Assert.True(e.GetHashCode() == "Bad".GetHashCode());
		Assert.True(d.GetHashCode() == 0);
	}

	[Fact]
	public void ObjectToString()
	{
		Assert.Equal("Success(42)", s.ToString());
		Assert.Equal("Error(Bad)", e.ToString());
		Assert.Equal("Error(null)", d.ToString());
	}

	[Fact]
	public void IsSuccess()
	{
		Assert.True(s.IsSuccess);
		Assert.False(e.IsSuccess);
		Assert.False(d.IsSuccess);
	}

	[Fact]
	public void IsError()
	{
		Assert.False(s.IsError);
		Assert.True(e.IsError);
		Assert.True(d.IsError);
	}

	[Fact]
	public void Value()
	{
		Assert.True(s.Value == 42);
		Assert.Throws<InvalidOperationException>(() => e.Value);
		Assert.Throws<InvalidOperationException>(() => d.Value);
	}

	[Fact]
	public void Error()
	{
		Assert.Throws<InvalidOperationException>(() => s.Error);
		Assert.True(e.Error == "Bad");
		Assert.True(d.Error is null);
	}

	[Fact]
	public void GetValueOrDefault()
	{
		Assert.True(s.GetValueOrDefault() == 42);
		Assert.True(e.GetValueOrDefault() == 0);
		Assert.True(d.GetValueOrDefault() == 0);

		Assert.True(s.GetValueOrDefault(314) == 42);
		Assert.True(e.GetValueOrDefault(314) == 314);
		Assert.True(d.GetValueOrDefault(314) == 314);
	}

	[Fact]
	public void GetErrorOrDefault()
	{
		Assert.True(s.GetErrorOrDefault() is null);
		Assert.True(e.GetErrorOrDefault() == "Bad");
		Assert.True(d.GetErrorOrDefault() is null);

		Assert.True(s.GetErrorOrDefault("Worse") == "Worse");
		Assert.True(e.GetErrorOrDefault("Worse") == "Bad");
		Assert.True(d.GetErrorOrDefault("Worse") is null);
	}

	[Fact]
	public void TryGetValue()
	{
		Assert.True(s.TryGetValue(out var o1) == true && o1 == 42);
		Assert.True(e.TryGetValue(out var o2) == false && o2 == 0);
		Assert.True(d.TryGetValue(out var o3) == false && o3 == 0);
	}

	[Fact]
	public void TryGetError()
	{
		Assert.True(s.TryGetError(out var o1) == false && o1 is null);
		Assert.True(e.TryGetError(out var o2) == true && o2 == "Bad");
		Assert.True(d.TryGetError(out var o3) == true && o3 is null);
	}

	[Fact]
	public void GetValueRefOrDefaultRef()
	{
		Assert.True(Result.GetValueRefOrDefaultRef(ref s) == 42);
		Assert.True(Result.GetValueRefOrDefaultRef(ref e) == 0);
		Assert.True(Result.GetValueRefOrDefaultRef(ref d) == 0);
	}

	[Fact]
	public void GetErrorRefOrDefaultRef()
	{
		Assert.True(Result.GetErrorRefOrDefaultRef(ref s) is null);
		Assert.True(Result.GetErrorRefOrDefaultRef(ref e) == "Bad");
		Assert.True(Result.GetErrorRefOrDefaultRef(ref d) is null);
	}
}
