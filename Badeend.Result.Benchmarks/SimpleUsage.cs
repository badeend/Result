using System.Runtime.CompilerServices;
using BenchmarkDotNet.Attributes;

namespace Badeend.Benchmarks;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix

[MemoryDiagnoser]
[DisassemblyDiagnoser]
public class SimpleUsage
{
	private const int Iterations = 1_000_000;

	[Benchmark]
	public Result<string, Error> Generic()
	{
		Result<string, Error> result = "init";

		for (int i = 0; i < Iterations; i++)
		{
			var a = A();
			if (a.IsSuccess)
			{
				result = a.Value;
			}
		}

		return result;

		[MethodImpl(MethodImplOptions.NoInlining)]
		static Result<string, Error> A()
		{
			if (B().TryGetValue(out var a))
			{
				return "yay";
			}
			else
			{
				return new Error("meh");
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static Result<int, Error> B() => 42;
	}

	[Benchmark]
	public Result<string> Basic()
	{
		Result<string> result = "init";

		for (int i = 0; i < Iterations; i++)
		{
			var a = A();
			if (a.IsSuccess)
			{
				result = a.Value;
			}
		}

		return result;

		[MethodImpl(MethodImplOptions.NoInlining)]
		static Result<string> A()
		{
			if (B().TryGetValue(out var a))
			{
				return "yay";
			}
			else
			{
				return new Error("meh");
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static Result<int> B() => 42;
	}
}
