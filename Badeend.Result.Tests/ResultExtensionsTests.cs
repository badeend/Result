using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Badeend;
using Badeend.Results.Extensions;

namespace Badeend.Tests;

#pragma warning disable CS1718 // Comparison made to same variable

public class ResultExtensionsTests
{
	[Fact]
	public void AsGenericResult()
	{
		Result<int> r1 = 42;
		Result<int, Error> o1 = r1.AsGenericResult();
	}

	[Fact]
	public void AsBasicResult()
	{
		Result<int, Error> r1 = new Error("Test");
		Result<int> o1 = r1.AsBasicResult();

		Result<int, MyCustomError> r2 = new MyCustomError("Test");
		Result<int> o2 = r2.AsBasicResult();

		Result<int, SomeEnum> r3 = SomeEnum.A;
		Result<int> o3 = r3.AsBasicResult();

		Result<int, InvalidOperationException> r4 = new InvalidOperationException();
		Result<int> o4 = r4.AsBasicResult();
	}

	private enum SomeEnum
	{
		A,
	}

	private sealed record MyCustomError(string Message, object? Data = null, Error? InnerError = null) : Badeend.Errors.IError;
}
