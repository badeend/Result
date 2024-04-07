using System.Diagnostics.CodeAnalysis;
using System.Runtime.InteropServices;
using Badeend;

namespace Badeend.Tests;

#pragma warning disable CS1718 // Comparison made to same variable

public class Tests
{
	/// <summary>
	/// Default
	/// </summary>
	private readonly Result<int, string> d = default;

	/// <summary>
	/// Success
	/// </summary>
	private readonly Result<int, string> s = 42;

	/// <summary>
	/// Error
	/// </summary>
	private readonly Result<int, string> e = "Bad";

	[Fact]
	public void StructuralEquality()
	{
		Result<int, string> s2 = new(42);
		Result<int, string> s3 = Result.Success<int, string>(42);

		Result<int, string> e2 = new("Bad");
		Result<int, string> e3 = Result.Error<int, string>("Bad");

		Assert.True(d == d);
		Assert.True(d == default);

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

		Assert.True(d.GetHashCode() == 0);
		Assert.True(s.GetHashCode() == 42.GetHashCode());
		Assert.True(e.GetHashCode() == "Bad".GetHashCode());
	}

	[Fact]
	public void ObjectToString()
	{
		Assert.Equal("", d.ToString());
		Assert.Equal("Success(42)", s.ToString());
		Assert.Equal("Error(Bad)", e.ToString());
	}

	[Fact]
	public void IsSuccess()
	{
		Assert.Throws<InvalidOperationException>(() => d.IsSuccess);
		Assert.True(s.IsSuccess);
		Assert.False(e.IsSuccess);
	}

	[Fact]
	public void IsError()
	{
		Assert.Throws<InvalidOperationException>(() => d.IsError);
		Assert.False(s.IsError);
		Assert.True(e.IsError);
	}

	[Fact]
	public void Value()
	{
		Assert.Throws<InvalidOperationException>(() => d.Value);
		Assert.True(s.Value == 42);
		Assert.Throws<InvalidOperationException>(() => e.Value);
	}

	[Fact]
	public void Error()
	{
		Assert.Throws<InvalidOperationException>(() => d.Error);
		Assert.Throws<InvalidOperationException>(() => s.Error);
		Assert.True(e.Error == "Bad");
	}

	[Fact]
	public void GetValueOrDefault()
	{
		Assert.Throws<InvalidOperationException>(() => d.GetValueOrDefault());
		Assert.True(s.GetValueOrDefault() == 42);
		Assert.True(e.GetValueOrDefault() == 0);

		Assert.Throws<InvalidOperationException>(() => d.GetValueOrDefault(314));
		Assert.True(s.GetValueOrDefault(314) == 42);
		Assert.True(e.GetValueOrDefault(314) == 314);
	}

	[Fact]
	public void GetErrorOrDefault()
	{
		Assert.Throws<InvalidOperationException>(() => d.GetErrorOrDefault());
		Assert.True(s.GetErrorOrDefault() == null);
		Assert.True(e.GetErrorOrDefault() == "Bad");

		Assert.Throws<InvalidOperationException>(() => d.GetErrorOrDefault("Worse"));
		Assert.True(s.GetErrorOrDefault("Worse") == "Worse");
		Assert.True(e.GetErrorOrDefault("Worse") == "Bad");
	}

	[Fact]
	public void TryGetValue()
	{
		Assert.Throws<InvalidOperationException>(() => d.TryGetValue(out _));
		Assert.True(s.TryGetValue(out var value1) == true && value1 == 42);
		Assert.True(e.TryGetValue(out var default1) == false && default1 == default);
	}

	[Fact]
	public void TryGetError()
	{
		Assert.Throws<InvalidOperationException>(() => d.TryGetError(out _));
		Assert.True(s.TryGetError(out var default2) == false && default2 == default);
		Assert.True(e.TryGetError(out var value2) == true && value2 == "Bad");
	}
}
