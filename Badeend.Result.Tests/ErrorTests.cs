using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Badeend;
using Badeend.Errors;

namespace Badeend.Tests;

#pragma warning disable CS1718 // Comparison made to same variable

public class ErrorTests
{
	private sealed record MyCustomError(string Message, object? Data = null, Error? InnerError = null) : Badeend.Errors.IError;

	private enum MyCustomEnumError
	{
		[ErrorMessage("My message")]
		MyMessage = 42,

		[ErrorMessage("Other failure")]
		OtherFailure = 314,
	}

	private enum RegularEnum
	{
		A,
		B,
		C,
	}

#pragma warning disable CA1069 // Enums values should not be duplicated
	[Flags]
	private enum FlagsEnum
	{
		None = 0,
		A = 1,
		AAgain = 1, // Intentionally the same as A
		B = 2,
		C = 4,
		ABC = 7,
	}
#pragma warning restore CA1069 // Enums values should not be duplicated

	private readonly static Error a = new();
	private readonly static Error b = new(message: null);
	private readonly static Error c = new("My message");
	private readonly static Error d = new("My message", innerError: new Error("My inner message"));
	private readonly static Error e = new("My message", innerError: null);
	private readonly static Error g = new("My message", null, innerError: new Error("My inner message"));
	private readonly static Error h = new("My message", null, innerError: null);
	private readonly static Error j = new("My message", 42, innerError: new Error("My inner message"));
	private readonly static Error k = new("My message", 42, innerError: null);
	private readonly static Error m = new(exception: null);
	private readonly static Error n = new(new Exception("My message"));
	private readonly static Error o = new(new Exception("My message", new Exception("My inner message")));
	private readonly static Error p = new(new MyCustomError(Message: null!));
	private readonly static Error q = new(new MyCustomError("My message"));
	private readonly static Error r = new(new MyCustomError("My message", InnerError: new Error("My inner message")));
	private readonly static Error s = new(new MyCustomError("My message", Data: 42));
	private readonly static Error t = new(new MyCustomError("My message", Data: 42, InnerError: new Error("My inner message")));
	private readonly static Error u = Error.FromEnum(MyCustomEnumError.MyMessage);

	[Fact]
	public void InnerError()
	{
		Assert.Null(a.InnerError);
		Assert.Null(b.InnerError);
		Assert.Null(c.InnerError);
		Assert.NotNull(d.InnerError);
		Assert.Null(d.InnerError.Value.InnerError);
		Assert.Null(e.InnerError);
		Assert.NotNull(g.InnerError);
		Assert.Null(g.InnerError.Value.InnerError);
		Assert.Null(h.InnerError);
		Assert.NotNull(j.InnerError);
		Assert.Null(j.InnerError.Value.InnerError);
		Assert.Null(k.InnerError);
		Assert.Null(m.InnerError);
		Assert.Null(n.InnerError);
		Assert.NotNull(o.InnerError);
		Assert.Null(o.InnerError.Value.InnerError);
		Assert.Null(p.InnerError);
		Assert.Null(q.InnerError);
		Assert.NotNull(r.InnerError);
		Assert.Null(r.InnerError.Value.InnerError);
		Assert.Null(s.InnerError);
		Assert.NotNull(t.InnerError);
		Assert.Null(t.InnerError.Value.InnerError);
		Assert.Null(u.InnerError);
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
		Assert.Equal("My message", g.Message);
		Assert.Equal("My inner message", g.InnerError?.Message);
		Assert.Equal("My message", h.Message);
		Assert.Equal("My message", j.Message);
		Assert.Equal("My inner message", j.InnerError?.Message);
		Assert.Equal("My message", k.Message);
		Assert.Equal("Operation did not complete successfully", m.Message);
		Assert.Equal("My message", n.Message);
		Assert.Equal("My message", o.Message);
		Assert.Equal("My inner message", o.InnerError?.Message);
		Assert.Equal("Operation did not complete successfully", p.Message);
		Assert.Equal("My message", q.Message);
		Assert.Equal("My message", r.Message);
		Assert.Equal("My inner message", r.InnerError?.Message);
		Assert.Equal("My message", s.Message);
		Assert.Equal("My message", t.Message);
		Assert.Equal("My inner message", t.InnerError?.Message);
		Assert.Equal("My message", u.Message);

		Assert.Equal("My message", Error.FromEnum(MyCustomEnumError.MyMessage).Message);
		Assert.Equal("Other failure", Error.FromEnum(MyCustomEnumError.OtherFailure).Message);
		Assert.Equal("Operation did not complete successfully", Error.FromEnum((MyCustomEnumError)1234).Message);
		Assert.Equal("Operation did not complete successfully", Error.FromEnum(RegularEnum.A).Message);
		Assert.Equal("Operation did not complete successfully", Error.FromEnum(RegularEnum.B).Message);
		Assert.Equal("Operation did not complete successfully", Error.FromEnum(RegularEnum.C).Message);
		Assert.Equal("Operation did not complete successfully", Error.FromEnum((RegularEnum)1234).Message);
		Assert.Equal("Operation did not complete successfully", Error.FromEnum(FlagsEnum.None).Message);
		Assert.Equal("Operation did not complete successfully", Error.FromEnum(FlagsEnum.A).Message);
		Assert.Equal("Operation did not complete successfully", Error.FromEnum(FlagsEnum.B).Message);
		Assert.Equal("Operation did not complete successfully", Error.FromEnum(FlagsEnum.A | FlagsEnum.B).Message);
		Assert.Equal("Operation did not complete successfully", Error.FromEnum(FlagsEnum.ABC).Message);
	}

	[Fact]
	public void Data()
	{
		Assert.Null(a.Data);
		Assert.Null(b.Data);
		Assert.Null(c.Data);
		Assert.Null(d.Data);
		Assert.Null(e.Data);
		Assert.Null(g.Data);
		Assert.Null(h.Data);
		Assert.Equal(42, (int)j.Data!);
		Assert.Equal(42, (int)k.Data!);
		Assert.Null(m.Data);
		Assert.True(n.Data is Exception);
		Assert.True(o.Data is Exception);
		Assert.True(o.InnerError?.Data is Exception);
		Assert.True(((Exception)o.Data).InnerException == (Exception)o.InnerError?.Data!);
		Assert.Null(p.Data);
		Assert.Null(q.Data);
		Assert.Null(r.Data);
		Assert.Equal(42, (int)s.Data!);
		Assert.Equal(42, (int)t.Data!);
		Assert.Equal(MyCustomEnumError.MyMessage, (MyCustomEnumError)u.Data!);
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
		Assert.True(g.AsException() is { Message: "My message", InnerException: { Message: "My inner message", InnerException: null } });
		Assert.True(h.AsException() is { Message: "My message", InnerException: null });
		Assert.True(j.AsException() is { Message: var m1, InnerException: { Message: "My inner message", InnerException: null } } && m1 == $"My message{Environment.NewLine}Data: 42");
		Assert.True(k.AsException() is { Message: var m2, InnerException: null } && m2 == $"My message{Environment.NewLine}Data: 42");
		Assert.True(m.AsException() is { Message: "Operation did not complete successfully", InnerException: null });
		Assert.True(n.AsException() is { Message: "My message", InnerException: null });
		Assert.True(o.AsException() is { Message: "My message", InnerException: { Message: "My inner message", InnerException: null } });
		Assert.True(p.AsException() is { Message: "Operation did not complete successfully", InnerException: null });
		Assert.True(q.AsException() is { Message: "My message", InnerException: null });
		Assert.True(r.AsException() is { Message: "My message", InnerException: { Message: "My inner message", InnerException: null } });
		Assert.True(s.AsException() is { Message: var m3, InnerException: null } && m3 == $"My message{Environment.NewLine}Data: 42");
		Assert.True(t.AsException() is { Message: var m4, InnerException: { Message: "My inner message", InnerException: null } } && m4 == $"My message{Environment.NewLine}Data: 42");
		Assert.True(u.AsException() is { Message: var m5, InnerException: null } && m5 == $"My message{Environment.NewLine}Data: MyMessage");
	}

	[Fact]
	public void AsExceptionReturnsOriginalException()
	{
		var x1 = new Exception("My inner message");
		var x2 = new Exception("My message", x1);

		var e = new Error(x2);

		Assert.True(object.ReferenceEquals(e.AsException(), x2));
		Assert.True(object.ReferenceEquals(e.InnerError!.Value.AsException(), x1));
	}

	[Fact]
	public void ExceptionConstructorUnwrapsError()
	{
		var e1 = new Error("My inner message");
		var e2 = new Error("My message", e1);

		var w1 = new Error(e1.AsException());
		var w2 = new Error(e2.AsException());

		Assert.True(w1 == e1);
		Assert.True(w2 == e2);
		Assert.True(w2.InnerError == e1);
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
		AssertEqual(g, """
		Error: My message

		--- Caused by: ---
		Error: My inner message
		""");
		AssertEqual(h, """
		Error: My message
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
		AssertEqual(p, """
		Badeend.Tests.ErrorTests+MyCustomError: Operation did not complete successfully
		""");
		AssertEqual(q, """
		Badeend.Tests.ErrorTests+MyCustomError: My message
		""");
		AssertEqual(r, """
		Badeend.Tests.ErrorTests+MyCustomError: My message

		--- Caused by: ---
		Error: My inner message
		""");
		AssertEqual(s, """
		Badeend.Tests.ErrorTests+MyCustomError: My message
		Data: 42
		""");
		AssertEqual(t, """
		Badeend.Tests.ErrorTests+MyCustomError: My message
		Data: 42

		--- Caused by: ---
		Error: My inner message
		""");
		AssertEqual(u, """
		Badeend.Tests.ErrorTests+MyCustomEnumError.MyMessage: My message
		""");



		AssertEqual(Error.FromEnum(MyCustomEnumError.MyMessage), """
		Badeend.Tests.ErrorTests+MyCustomEnumError.MyMessage: My message
		""");
		AssertEqual(Error.FromEnum(MyCustomEnumError.OtherFailure), """
		Badeend.Tests.ErrorTests+MyCustomEnumError.OtherFailure: Other failure
		""");
		AssertEqual(Error.FromEnum((MyCustomEnumError)1234), """
		Badeend.Tests.ErrorTests+MyCustomEnumError: Operation did not complete successfully
		Data: 1234
		""");
		AssertEqual(Error.FromEnum(RegularEnum.A), """
		Badeend.Tests.ErrorTests+RegularEnum.A: Operation did not complete successfully
		""");
		AssertEqual(Error.FromEnum(RegularEnum.B), """
		Badeend.Tests.ErrorTests+RegularEnum.B: Operation did not complete successfully
		""");
		AssertEqual(Error.FromEnum(RegularEnum.C), """
		Badeend.Tests.ErrorTests+RegularEnum.C: Operation did not complete successfully
		""");
		AssertEqual(Error.FromEnum((RegularEnum)1234), """
		Badeend.Tests.ErrorTests+RegularEnum: Operation did not complete successfully
		Data: 1234
		""");
		AssertEqual(Error.FromEnum(FlagsEnum.None), """
		Badeend.Tests.ErrorTests+FlagsEnum: Operation did not complete successfully
		Data: None
		""");
		AssertEqual(Error.FromEnum(FlagsEnum.A), """
		Badeend.Tests.ErrorTests+FlagsEnum: Operation did not complete successfully
		Data: AAgain
		""");
		AssertEqual(Error.FromEnum(FlagsEnum.B), """
		Badeend.Tests.ErrorTests+FlagsEnum: Operation did not complete successfully
		Data: B
		""");
		AssertEqual(Error.FromEnum(FlagsEnum.A | FlagsEnum.B), """
		Badeend.Tests.ErrorTests+FlagsEnum: Operation did not complete successfully
		Data: AAgain, B
		""");
		AssertEqual(Error.FromEnum(FlagsEnum.A | FlagsEnum.B | FlagsEnum.C), """
		Badeend.Tests.ErrorTests+FlagsEnum: Operation did not complete successfully
		Data: ABC
		""");
		AssertEqual(Error.FromEnum(FlagsEnum.ABC), """
		Badeend.Tests.ErrorTests+FlagsEnum: Operation did not complete successfully
		Data: ABC
		""");

		Assert.Equal("Operation did not complete successfully", Error.FromEnum(FlagsEnum.ABC).Message);

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
		AssertEqual(g, messageAndInnerErrorError);
		AssertEqual(h, messageError);
		AssertEqual(j, messageAndDataAndInnerErrorError);
		AssertEqual(k, messageAndDataError);
		AssertEqual(m, defaultError);
		AssertNotEqual(n, messageError); // Equality should include the exception.
		AssertNotEqual(o, messageAndInnerErrorError); // Equality should include the exception.
		AssertEqual(p, defaultError);
		AssertEqual(q, messageError);
		AssertEqual(r, messageAndInnerErrorError);
		AssertEqual(s, messageAndDataError);
		AssertEqual(t, messageAndDataAndInnerErrorError);
		AssertEqual(u, new Error("My message", data: MyCustomEnumError.MyMessage));

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
	public void CompareTo()
	{
		Error a1 = new("a");
		Error a2 = new("a");
		Error b = new("b");
		Error c1 = new("c", data: 42);
		Error c2 = new("c", data: 314);
		Error c3 = new("c", innerError: new("3"));
		Error c4 = new("c", innerError: new("4"));

		Assert.True(a1.CompareTo(b) < 0);
		Assert.True(a1.CompareTo(a2) == 0);
		Assert.True(b.CompareTo(a1) > 0);
		Assert.True(c1.CompareTo(c2) < 0);
		Assert.True(c2.CompareTo(c3) > 0);
		Assert.True(c3.CompareTo(c4) < 0);

		Error[] unordered = [c3, b, c1, a1, c4, a2, c2];
		Error[] ordered = unordered.OrderBy(r => r).ToArray();

		Assert.Equal([a1, a2, b, c3, c4, c1, c2], ordered);
	}

	[Fact]
	public void EnumErrorPropertiesAreCached()
	{
		var messageA = Error.FromEnum(MyCustomEnumError.MyMessage).Message;
		var messageB = Error.FromEnum(MyCustomEnumError.MyMessage).Message;

		Assert.True(object.ReferenceEquals(messageA, messageB));

		var dataA = Error.FromEnum(MyCustomEnumError.MyMessage).Data;
		var dataB = Error.FromEnum(MyCustomEnumError.MyMessage).Data;

		Assert.True(object.ReferenceEquals(dataA, dataB));
	}

	[Fact]
	public void Int32EnumSpecialization()
	{
		var a1 = Error.FromEnum(RegularEnum.A).Data;
		var a2 = Error.FromEnum(RegularEnum.A).Data;

		Assert.True(object.ReferenceEquals(a1, a2));

		var tooLow = (RegularEnum)(-1);
		var tooHigh = (RegularEnum)9999;

		Assert.True((RegularEnum)Error.FromEnum(tooLow).Data! == tooLow);
		Assert.True((RegularEnum)Error.FromEnum(tooHigh).Data! == tooHigh);
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
		var someIError = new MyCustomError("My message");
		_ = Error.FromEnum(MyCustomEnumError.MyMessage).ToString(); // Preload type-level cache.
		_ = Error.FromEnum(RegularEnum.B).ToString(); // Preload type-level cache.

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
			accumulator += new Error(someIError).Message.Length;
			accumulator += Error.FromEnum(MyCustomEnumError.MyMessage).Message.Length;
			accumulator += Error.FromEnum(MyCustomEnumError.MyMessage).Data is null ? 0 : 1;
			accumulator += Error.FromEnum(RegularEnum.B).Message.Length;
			accumulator += Error.FromEnum(RegularEnum.B).Data is null ? 0 : 1;
		}

		var actual = GC.GetAllocatedBytesForCurrentThread() - before;

		// This is an imperfect science, so we'll allow some margin. As long as
		// the variance is effectively less than 1 byte per iteration, we should
		// be fine:
		Assert.InRange(actual, 0, iterations / 10);

		Assert.True(accumulator != 0); // Ensure the operations do not get optimizated away.
	}

#if NETCOREAPP3_0_OR_GREATER
	private sealed record DefaultInterfaceImplementations(string Message) : Badeend.Errors.IError;
#endif
}
