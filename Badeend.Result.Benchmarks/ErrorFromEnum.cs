using BenchmarkDotNet.Attributes;

namespace Badeend.Benchmarks;

#pragma warning disable CA1822 // Mark members as static
#pragma warning disable CA1711 // Identifiers should not have incorrect suffix

[MemoryDiagnoser]
[DisassemblyDiagnoser]
public class ErrorFromEnum
{
	private enum Sparse
	{
		One = 1,
		Three = 3,
		Five = 5,
	}

	private enum Dense
	{
		One,
		Two,
		Three,
	}

	public ErrorFromEnum()
	{
		// Frontload one-time reflection cost
		_ = Error.FromEnum(Sparse.Three);
		_ = Error.FromEnum(Dense.Three);
	}

	[Benchmark]
	public Error SparseEnum() => Error.FromEnum(Sparse.Three);

	[Benchmark]
	public Error DenseEnum() => Error.FromEnum(Dense.Three);
}
