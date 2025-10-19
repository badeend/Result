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

	private static int counter = 0;

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
			if (B().TryGetValue(out var v, out var e))
			{
				return v;
			}
			else
			{
				return new Error("Bad A");
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static Result<string, Error> B()
		{
			if (C().TryGetValue(out var v, out var e))
			{
				return "B";
			}
			else
			{
				return e;
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static Result<string, Error> C()
		{
			if (counter++ % 8 != 0)
			{
				return "C";
			}
			else
			{
				return new Error("Bad C");
			}
		}
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
			if (B().TryGetValue(out var v, out var e))
			{
				return v;
			}
			else
			{
				return new Error("Bad A");
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static Result<string> B()
		{
			if (C().TryGetValue(out var v, out var e))
			{
				return "B";
			}
			else
			{
				return e;
			}
		}

		[MethodImpl(MethodImplOptions.NoInlining)]
		static Result<string> C()
		{
			if (counter++ % 8 != 0)
			{
				return "C";
			}
			else
			{
				return new Error("Bad C");
			}
		}
	}
}
