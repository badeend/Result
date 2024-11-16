using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Badeend;

namespace Badeend.Tests;

#pragma warning disable CS1718 // Comparison made to same variable

/// <summary>
/// Tests for <see cref="Result{TValue}"/>.
/// </summary>
public class Result1Tests
{
	private static readonly Error SomeError = new Error("Bad");
	private static readonly Error OtherError = new Error("Worse");

	/// <summary>
	/// Success
	/// </summary>
	private Result<int> s = 42;

	/// <summary>
	/// Error
	/// </summary>
	private Result<int> f = SomeError;

	/// <summary>
	/// Default
	/// </summary>
	private Result<int> d = default;

	[Fact]
	public void StructuralEquality()
	{
		Result<int> s2 = 42;
		Result<int> s3 = Result.Success<int>(42);

		Result<int> f2 = SomeError;
		Result<int> f3 = Result.Error<int>(SomeError);

		Result<int> d2 = Result.Error<int>(default);

		Assert.True(s == s);
		Assert.True(s == s2);
		Assert.True(s == s3);

		Assert.True(s == 42);
		Assert.True(s != 314);
		Assert.True(s != SomeError);
		Assert.True(s != default);

		Assert.True(f == f);
		Assert.True(f == f2);
		Assert.True(f == f3);

		Assert.True(f == SomeError);
		Assert.True(f != OtherError);
		Assert.True(f != 42);
		Assert.True(f != default);

		Assert.True(d == d);
		Assert.True(d == d2);
		Assert.True(d == default);

		Assert.True(d == default);
		Assert.True(d == default(Error));
		Assert.True(d != SomeError);
		Assert.True(d != 42);

		Assert.True(s.GetHashCode() == 42.GetHashCode());
		Assert.True(f.GetHashCode() == SomeError.GetHashCode());
		Assert.True(d.GetHashCode() == default(Error).GetHashCode());
	}

	[Fact]
	public void ObjectToString()
	{
		Assert.Equal("Success(42)", s.ToString());
		Assert.Equal("Error(Error: Bad)", f.ToString());
		Assert.Equal("Error(Error: Operation did not complete successfully)", d.ToString());
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
	public void Error()
	{
		ref readonly var r = ref f.Error;

		Assert.Throws<InvalidOperationException>(() => s.Error);
		Assert.True(r == SomeError);
		Assert.True(f.Error == SomeError);
		Assert.True(d.Error == default);
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
		Assert.True(s.GetErrorOrDefault() == default);
		Assert.True(f.GetErrorOrDefault() == SomeError);
		Assert.True(d.GetErrorOrDefault() == default);

		Assert.True(s.GetErrorOrDefault(OtherError) == OtherError);
		Assert.True(f.GetErrorOrDefault(OtherError) == SomeError);
		Assert.True(d.GetErrorOrDefault(OtherError) == default);
	}

	[Fact]
	public void TryGetValue()
	{
		Assert.True(s.TryGetValue(out var o1) == true && o1 == 42);
		Assert.True(f.TryGetValue(out var o2) == false && o2 == 0);
		Assert.True(d.TryGetValue(out var o3) == false && o3 == 0);

		Assert.True(s.TryGetValue(out var s1, out var f1) == true && s1 == 42 && f1 == default);
		Assert.True(f.TryGetValue(out var s2, out var f2) == false && s2 == 0 && f2 == SomeError);
		Assert.True(d.TryGetValue(out var s3, out var f3) == false && s3 == 0 && f3 == default);
	}

	[Fact]
	public void TryGetError()
	{
		Assert.True(s.TryGetError(out var o1) == false && o1 == default);
		Assert.True(f.TryGetError(out var o2) == true && o2 == SomeError);
		Assert.True(d.TryGetError(out var o3) == true && o3 == default);
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
		Assert.True(Result.GetErrorRefOrDefaultRef(ref s) == default);
		Assert.True(Result.GetErrorRefOrDefaultRef(ref f) == SomeError);
		Assert.True(Result.GetErrorRefOrDefaultRef(ref d) == default);
	}

	[Fact]
	public void ConversionsWithGenericResult()
	{
		Result<int> s1 = 42;
		Result<int, Error> s2 = 42;
		Result<int> _s3 = s2; // Implicit conversion
		Result<int, Error> _s4 = s1; // Implicit conversion

		Assert.True(s1 == s2.AsStandardResult());
		Assert.True(s2 == s1.AsGenericResult());
	}

	[Fact]
	public void SizeOf()
	{
		Assert.Equal(Unsafe.SizeOf<Result<int, Error>>(), Unsafe.SizeOf<Result<int>>());
	}
}
