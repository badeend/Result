using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Badeend;

namespace Badeend.Tests;

#pragma warning disable CS1718 // Comparison made to same variable

public class ErrorTests
{
	private readonly static Error a = new();
	private readonly static Error b = new(message: null);
	private readonly static Error c = new("My message");
	private readonly static Error d = new("My message", innerError: new Error("My inner message"));
	private readonly static Error e = new("My message", innerError: null);
	private readonly static Error f = new("My message", innerError: (Error?)new Error("My inner message"));
	private readonly static Error g = new("My message", null, innerError: new Error("My inner message"));
	private readonly static Error h = new("My message", null, innerError: null);
	private readonly static Error i = new("My message", null, innerError: (Error?)new Error("My inner message"));
	private readonly static Error j = new("My message", 42, innerError: new Error("My inner message"));
	private readonly static Error k = new("My message", 42, innerError: null);
	private readonly static Error l = new("My message", 42, innerError: (Error?)new Error("My inner message"));
	private readonly static Error m = new(exception: null);
	private readonly static Error n = new(new Exception("My message"));
	private readonly static Error o = new(new Exception("My message", new Exception("My inner message")));

	[Fact]
	public void InnerError()
	{
		Assert.Null(a.InnerError);
		Assert.Null(b.InnerError);
		Assert.Null(c.InnerError);
		Assert.NotNull(d.InnerError);
		Assert.Null(d.InnerError.Value.InnerError);
		Assert.Null(e.InnerError);
		Assert.NotNull(f.InnerError);
		Assert.Null(f.InnerError.Value.InnerError);
		Assert.NotNull(g.InnerError);
		Assert.Null(g.InnerError.Value.InnerError);
		Assert.Null(h.InnerError);
		Assert.NotNull(i.InnerError);
		Assert.Null(i.InnerError.Value.InnerError);
		Assert.NotNull(j.InnerError);
		Assert.Null(j.InnerError.Value.InnerError);
		Assert.Null(k.InnerError);
		Assert.NotNull(l.InnerError);
		Assert.Null(l.InnerError.Value.InnerError);
		Assert.Null(m.InnerError);
		Assert.Null(n.InnerError);
		Assert.NotNull(o.InnerError);
		Assert.Null(o.InnerError.Value.InnerError);
	}

	[Fact]
	public void Message()
	{
		Assert.Equal("Operation did not complete successfully", a.Message);
		Assert.Equal("Operation did not complete successfully", b.Message);
		Assert.Equal("My message", c.Message);
		Assert.Equal("My message", d.Message);
		Assert.Equal("My inner message", d.InnerError?.Message);
		Assert.Equal("My message", e.Message);
		Assert.Equal("My message", f.Message);
		Assert.Equal("My inner message", f.InnerError?.Message);
		Assert.Equal("My message", g.Message);
		Assert.Equal("My inner message", g.InnerError?.Message);
		Assert.Equal("My message", h.Message);
		Assert.Equal("My message", i.Message);
		Assert.Equal("My inner message", i.InnerError?.Message);
		Assert.Equal("My message", j.Message);
		Assert.Equal("My inner message", j.InnerError?.Message);
		Assert.Equal("My message", k.Message);
		Assert.Equal("My message", l.Message);
		Assert.Equal("My inner message", l.InnerError?.Message);
		Assert.Equal("Operation did not complete successfully", m.Message);
		Assert.Equal("My message", n.Message);
		Assert.Equal("My message", o.Message);
		Assert.Equal("My inner message", o.InnerError?.Message);
	}

	[Fact]
	public void Data()
	{
		Assert.Null(a.Data);
		Assert.Null(b.Data);
		Assert.Null(c.Data);
		Assert.Null(d.Data);
		Assert.Null(e.Data);
		Assert.Null(f.Data);
		Assert.Null(g.Data);
		Assert.Null(h.Data);
		Assert.Null(i.Data);
		Assert.Equal(42, (int)j.Data!);
		Assert.Equal(42, (int)k.Data!);
		Assert.Equal(42, (int)l.Data!);
		Assert.Null(m.Data);
		Assert.True(n.Data is Exception);
		Assert.True(o.Data is Exception);
		Assert.True(o.InnerError?.Data is Exception);
		Assert.True(((Exception)o.Data).InnerException == (Exception)o.InnerError?.Data!);
	}

	[Fact]
	public void FindData()
	{
		var x = new Error();
		var a = new Error(message: null, data: (int) 1, innerError: null);
		var b = new Error(message: null, data: (long)2, innerError: a);
		var c = new Error(message: null, data: (int) 3, innerError: b);

		Assert.True(x.TryFindData(out int i0) == false && i0 == default);
		Assert.True(a.TryFindData(out int i1) == true && i1 == 1);
		Assert.True(b.TryFindData(out int i2) == true && i2 == 1);
		Assert.True(c.TryFindData(out int i3) == true && i3 == 3);

		Assert.True(x.TryFindData(out int? ni0) == false && ni0 == default);
		Assert.True(a.TryFindData(out int? ni1) == true && ni1 == 1);
		Assert.True(b.TryFindData(out int? ni2) == true && ni2 == 1);
		Assert.True(c.TryFindData(out int? ni3) == true && ni3 == 3);

		Assert.True(x.TryFindData(out long l0) == false && l0 == default);
		Assert.True(a.TryFindData(out long l1) == false && l1 == default);
		Assert.True(b.TryFindData(out long l2) == true && l2 == 2);
		Assert.True(c.TryFindData(out long l3) == true && l3 == 2);

		Assert.True(x.TryFindData<object>(out var o0) == false && o0 == null);
		Assert.True(a.TryFindData<object>(out var o1) == true && (int)o1 == 1);
		Assert.True(b.TryFindData<object>(out var o2) == true && (long)o2 == 2);
		Assert.True(c.TryFindData<object>(out var o3) == true && (int)o3 == 3);
	}

	[Fact]
	public void AsException()
	{
		Assert.True(a.AsException() is { Message: "Operation did not complete successfully", InnerException: null });
		Assert.True(b.AsException() is { Message: "Operation did not complete successfully", InnerException: null });
		Assert.True(c.AsException() is { Message: "My message", InnerException: null });
		Assert.True(d.AsException() is { Message: "My message", InnerException: { Message: "My inner message", InnerException: null } });
		Assert.True(e.AsException() is { Message: "My message", InnerException: null });
		Assert.True(f.AsException() is { Message: "My message", InnerException: { Message: "My inner message", InnerException: null } });
		Assert.True(g.AsException() is { Message: "My message", InnerException: { Message: "My inner message", InnerException: null } });
		Assert.True(h.AsException() is { Message: "My message", InnerException: null });
		Assert.True(i.AsException() is { Message: "My message", InnerException: { Message: "My inner message", InnerException: null } });
		Assert.True(j.AsException() is { Message: var m1, InnerException: { Message: "My inner message", InnerException: null } } && m1 == $"My message{Environment.NewLine}Data: 42");
		Assert.True(k.AsException() is { Message: var m2, InnerException: null } && m2 == $"My message{Environment.NewLine}Data: 42");
		Assert.True(l.AsException() is { Message: var m3, InnerException: { Message: "My inner message", InnerException: null } } && m3 == $"My message{Environment.NewLine}Data: 42");
		Assert.True(m.AsException() is { Message: "Operation did not complete successfully", InnerException: null });
		Assert.True(n.AsException() is { Message: "My message", InnerException: null });
		Assert.True(o.AsException() is { Message: "My message", InnerException: { Message: "My inner message", InnerException: null } });
	}

	[Fact]
	public void _ToString()
	{
		AssertEqual(a, """
		Error: Operation did not complete successfully
		""");
		AssertEqual(b, """
		Error: Operation did not complete successfully
		""");
		AssertEqual(c, """
		Error: My message
		""");
		AssertEqual(d, """
		Error: My message

		--- Caused by: ---
		Error: My inner message
		""");
		AssertEqual(e, """
		Error: My message
		""");
		AssertEqual(f, """
		Error: My message

		--- Caused by: ---
		Error: My inner message
		""");
		AssertEqual(g, """
		Error: My message

		--- Caused by: ---
		Error: My inner message
		""");
		AssertEqual(h, """
		Error: My message
		""");
		AssertEqual(i, """
		Error: My message

		--- Caused by: ---
		Error: My inner message
		""");
		AssertEqual(j, """
		Error: My message
		Data: 42

		--- Caused by: ---
		Error: My inner message
		""");
		AssertEqual(k, """
		Error: My message
		Data: 42
		""");
		AssertEqual(l, """
		Error: My message
		Data: 42

		--- Caused by: ---
		Error: My inner message
		""");
		AssertEqual(m, """
		Error: Operation did not complete successfully
		""");
		AssertEqual(n, """
		System.Exception: My message
		""");
		AssertEqual(o, """
		System.Exception: My message

		--- Caused by: ---
		System.Exception: My inner message
		""");

		static void AssertEqual(Error error, string expected)
		{
			Assert.Equal(expected.Replace("\r\n", "\n"), error.ToString().Replace("\r\n", "\n"));
		}
	}

	[Fact]
	public void Equality()
	{
		var defaultError = new Error();
		var messageError = new Error("My message");
		var messageAndDataError = new Error("My message", data: 42);
		var messageAndInnerErrorError = new Error("My message", new Error("My inner message"));
		var messageAndDataAndInnerErrorError = new Error("My message", 42, new Error("My inner message"));

		HashSet<Error> errors = [defaultError, messageError, messageAndDataError, messageAndInnerErrorError, messageAndDataAndInnerErrorError];
		HashSet<int> hashCodes = errors.Select(e => e.GetHashCode()).ToHashSet();
		Assert.Equal(5, errors.Count); // Distinct error values must be distinct :)
		Assert.Equal(5, hashCodes.Count); // Distinct error values must produce distinct hash codes.


		AssertEqual(a, defaultError);
		AssertEqual(b, defaultError);
		AssertEqual(c, messageError);
		AssertEqual(d, messageAndInnerErrorError);
		AssertEqual(e, messageError);
		AssertEqual(f, messageAndInnerErrorError);
		AssertEqual(g, messageAndInnerErrorError);
		AssertEqual(h, messageError);
		AssertEqual(i, messageAndInnerErrorError);
		AssertEqual(j, messageAndDataAndInnerErrorError);
		AssertEqual(k, messageAndDataError);
		AssertEqual(l, messageAndDataAndInnerErrorError);
		AssertEqual(m, defaultError);
		AssertNotEqual(n, messageError); // Equality should include the exception.
		AssertNotEqual(o, messageAndInnerErrorError); // Equality should include the exception.

		static void AssertEqual(Error left, Error right)
		{
			Assert.Equal(left.GetHashCode(), right.GetHashCode());
			Assert.True(left.Equals(right));
			Assert.True(left == right);
			Assert.False(left != right);
		}

		static void AssertNotEqual(Error left, Error right)
		{
			Assert.NotEqual(left.GetHashCode(), right.GetHashCode());
			Assert.False(left.Equals(right));
			Assert.False(left == right);
			Assert.True(left != right);
		}
	}

	[Fact]
	public void SizeOf()
	{
		Assert.Equal(Unsafe.SizeOf<object>(), Unsafe.SizeOf<Error>());
	}

	[Fact]
	public void AllocationFreeOperations()
	{
		var accumulator = 0; // Ensure the operations do not get optimizated away.

		var someString = "My message";
		var someException = new Exception("My message");

		var before = GC.GetAllocatedBytesForCurrentThread();

		int iterations = 10_000;
		for (int i = 0; i < iterations; i++)
		{
			accumulator += new Error().Message.Length;
			accumulator += new Error().Data is null ? 0 : 1;
			accumulator += new Error().ToString().Length;
			accumulator += new Error(someString).Message.Length;
			accumulator += new Error(someString).Data is null ? 0 : 1;
			accumulator += new Error(someString, innerError: null).Message.Length;
			accumulator += new Error(someString, innerError: null).Data is null ? 0 : 1;
			accumulator += new Error(someString, data: null, innerError: null).Message.Length;
			accumulator += new Error(someString, data: null, innerError: null).Data is null ? 0 : 1;
			accumulator += new Error(someException).Message.Length;
			accumulator += new Error(someException).Data is null ? 0 : 1;
		}

		var actual = GC.GetAllocatedBytesForCurrentThread() - before;

		// This is an imperfect science, so we'll allow some margin. As long as
		// the jitter is effectively less than 1 byte per iteration, we should
		// be fine:
		Assert.InRange(actual, 0, iterations / 10);

		Assert.True(accumulator != 0); // Ensure the operations do not get optimizated away.
	}
}
