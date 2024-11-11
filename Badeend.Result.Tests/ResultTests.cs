using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Badeend;

namespace Badeend.Tests;

#pragma warning disable CS1718 // Comparison made to same variable

/// <summary>
/// Tests for <see cref="Result"/>.
/// </summary>
public class ResultTests
{
	[Fact]
	public void ResultStateBinaryProperties()
	{
		var s = ResultState.Success;
		var f = ResultState.Error;

		Assert.True(ResultState.Error == default(ResultState));
		Assert.True(Unsafe.SizeOf<ResultState>() == Unsafe.SizeOf<bool>());
		Assert.True(Unsafe.As<ResultState, bool>(ref s) == true);
		Assert.True(Unsafe.As<ResultState, bool>(ref f) == false);
	}

	[Fact]
	public void GetUnderlyingType()
	{
		Assert.Equal(typeof(int), Result.GetUnderlyingValueType(typeof(Result<int>)));
		Assert.Equal(typeof(Error), Result.GetUnderlyingErrorType(typeof(Result<int>)));

		Assert.Equal(typeof(int), Result.GetUnderlyingValueType(typeof(Result<int, string>)));
		Assert.Equal(typeof(string), Result.GetUnderlyingErrorType(typeof(Result<int, string>)));

		Assert.Null(Result.GetUnderlyingValueType(typeof(Result)));
		Assert.Null(Result.GetUnderlyingErrorType(typeof(Result)));

		Assert.Null(Result.GetUnderlyingValueType(typeof(Result<>)));
		Assert.Null(Result.GetUnderlyingErrorType(typeof(Result<>)));

		Assert.Null(Result.GetUnderlyingValueType(typeof(Result<,>)));
		Assert.Null(Result.GetUnderlyingErrorType(typeof(Result<,>)));

		Assert.Null(Result.GetUnderlyingValueType(typeof(DateTime)));
		Assert.Null(Result.GetUnderlyingErrorType(typeof(DateTime)));

		Assert.Throws<ArgumentNullException>(() => Result.GetUnderlyingValueType(null!));
		Assert.Throws<ArgumentNullException>(() => Result.GetUnderlyingErrorType(null!));
	}
}
