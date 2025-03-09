using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Badeend.Results.Extensions;

namespace Badeend;

/// <summary>
/// Represents the result of a fallible operation. This type is a shorthand for:
/// <see cref="Result{TValue, TError}"><c>Result&lt;TValue, Badeend.Error&gt;</c></see>.
/// </summary>
/// <remarks>
/// A <c>Result</c> can be in one of two states:
/// <see cref="ResultState.Success">Success</see> or
/// <see cref="ResultState.Error">Error</see>.
/// Both states have an associated payload of type <typeparamref name="TValue"/>
/// or <see cref="Badeend.Error"/> respectively.
///
/// Because of the implicit conversion operators you typically don't have to
/// manually construct Results. If you do want or need to, you can use
/// <see cref="Result.Success{TValue}(TValue)"><c>Result.Success()</c></see> or
/// <see cref="Result.Error{TValue}(Error)"><c>Result.Error()</c></see> instead.
///
/// You can examine a result like this:
/// <code>
/// _ = myResult.State switch
/// {
///   ResultState.Success => $"Something successful: {myResult.Value}",
///   ResultState.Error => $"Something failed: {myResult.Error}",
/// };
/// </code>
///
/// Or alternatively using
/// <see cref="IsSuccess"><c>IsSuccess</c></see>,
/// <see cref="IsError"><c>IsError</c></see>,
/// <see cref="TryGetValue(out TValue)"><c>TryGetValue</c></see>,
/// <see cref="TryGetError"><c>TryGetError</c></see>,
/// <see cref="GetValueOrDefault()"><c>GetValueOrDefault</c></see> or
/// <see cref="GetErrorOrDefault()"><c>GetErrorOrDefault</c></see>.
///
/// Note that you should generally not attempt to
/// derive any semantic meaning from the <see cref="Badeend.Error"/>'s content.
/// This type (or its longhand form <c>Result&lt;T, Badeend.Error&gt;</c>)
/// is semantically the same as <c>Result&lt;T, void&gt;</c> in that: all that the
/// domain logic should care about is whether the operation succeeded or failed.
/// The Error data is just a way to carry additional developer-oriented context.
///
/// A Result's <c>default</c> value is equivalent to <c>Result.Error(default!)</c>.
/// </remarks>
/// <typeparam name="TValue">Type of the result when the operation succeeds.</typeparam>
[StructLayout(LayoutKind.Auto)]
[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "Internal")]
[SuppressMessage("Design", "CA1036:Override methods on comparable types", Justification = "Result is only comparable if TValue and Error are, which we can't know at compile time. Don't want to promote the comparable stuff too much.")]
public readonly struct Result<TValue> : IEquatable<Result<TValue>>, IComparable<Result<TValue>>, IComparable
{
#pragma warning disable SA1304 // Non-private readonly fields should begin with upper-case letter
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
	internal readonly bool isSuccess;
	internal readonly TValue value;
	internal readonly Error error;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter
#pragma warning restore SA1304 // Non-private readonly fields should begin with upper-case letter

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Result(TValue value)
	{
		this.isSuccess = true;
		this.value = value;
		this.error = default!;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Result(Error error)
	{
		this.isSuccess = false;
		this.value = default!;
		this.error = error;
	}

	/// <inheritdoc cref="Result{TValue,TError}.State"/>
	[Pure]
	public ResultState State => this.isSuccess ? ResultState.Success : ResultState.Error;

	/// <inheritdoc cref="Result{TValue,TError}.IsSuccess"/>
	[Pure]
	public bool IsSuccess
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => this.isSuccess;
	}

	/// <inheritdoc cref="Result{TValue,TError}.IsError"/>
	[Pure]
	public bool IsError
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => !this.isSuccess;
	}

	/// <inheritdoc cref="Result{TValue,TError}.Value"/>
	[UnscopedRef]
	public ref readonly TValue Value
	{
		get
		{
			if (!this.isSuccess)
			{
				// Extracted exceptional code path into separate method to aid inlining.
				this.ThrowNotSuccessfulException();
			}

			return ref this.value;
		}
	}

	[DoesNotReturn]
	private void ThrowNotSuccessfulException()
	{
		throw new InvalidOperationException("Operation was not successful. See inner exception for more details.", this.error.AsException());
	}

	/// <inheritdoc cref="Result{TValue,TError}.Error"/>
	[UnscopedRef]
	public ref readonly Error Error
	{
		get
		{
			if (this.isSuccess)
			{
				// Extracted exceptional code path into separate method to aid inlining.
				ThrowSuccessfulException();
			}

			return ref this.error;
		}
	}

	[DoesNotReturn]
	private static void ThrowSuccessfulException()
	{
		throw new InvalidOperationException("Operation did not fail.");
	}

	/// <inheritdoc cref="Result{TValue,TError}.GetValueOrDefault()"/>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TValue? GetValueOrDefault() => this.value;

	/// <inheritdoc cref="Result{TValue,TError}.GetValueOrDefault(TValue)"/>
	[Pure]
	public TValue GetValueOrDefault(TValue defaultValue) => this.isSuccess ? this.value : defaultValue;

	/// <summary>
	/// Attempt to store the operation's success value in <paramref name="value"/>.
	/// Returns <see langword="false"/> when the operation failed.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
	{
		value = this.value;
		return this.isSuccess;
	}

	/// <summary>
	/// Attempt to store the operation's success value in <paramref name="value"/>.
	/// If the operation failed, this method returns <see langword="false"/>
	/// and the error is stored in <paramref name="error"/>.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetValue([MaybeNullWhen(false)] out TValue value, [MaybeNullWhen(true)] out Error error)
	{
		value = this.value;
		error = this.error;
		return this.isSuccess;
	}

	/// <summary>
	/// Attempt to get the operation's error value.
	/// Returns <see langword="default"/> when the operation succeeded.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Error GetErrorOrDefault() => this.error;

	/// <summary>
	/// Attempt to get the operation's error value.
	/// Returns <paramref name="defaultValue"/> when the operation succeeded.
	/// </summary>
	[Pure]
	public Error GetErrorOrDefault(Error defaultValue) => this.isSuccess ? defaultValue : this.error;

	/// <summary>
	/// Attempt to store the operation's error in <paramref name="error"/>.
	/// Returns <see langword="false"/> when the operation succeeded.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetError([MaybeNullWhen(false)] out Error error)
	{
		error = this.error;
		return !this.isSuccess;
	}

	/// <summary>
	/// Get a string representation of the result for debugging purposes.
	/// The format is not stable and may change without prior notice.
	/// </summary>
	[Pure]
	public override string ToString() => this.isSuccess switch
	{
		true => $"Success({this.value?.ToString() ?? "null"})",
		false => $"Error({this.error.ToString() ?? "null"})",
	};

#pragma warning disable CA2225 // Operator overloads have named alternates => Result.Success is good enough
	/// <summary>
	/// Create a successful result.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Result<TValue>(TValue value) => new(value);
#pragma warning restore CA2225 // Operator overloads have named alternates

#pragma warning disable CA2225 // Operator overloads have named alternates => Result.Error is good enough
	/// <summary>
	/// Create a error result.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Result<TValue>(Error error) => new(error);
#pragma warning restore CA2225 // Operator overloads have named alternates

	/// <summary>
	/// Convert from a Result with an implicit error type to a Result with an explicit error type.
	/// </summary>
	[Pure]
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Exists as extension method.")]
	public static implicit operator Result<TValue, Error>(Result<TValue> result) => result.IsSuccess ? result.value : result.error;

	/// <summary>
	/// Convert from a Result with an explicit error type to a Result with an implicit error type.
	/// </summary>
	[Pure]
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Exists as extension method.")]
	public static implicit operator Result<TValue>(Result<TValue, Error> result) => result.IsSuccess ? result.value : result.error;

	/// <summary>
	/// Check for equality.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Result<TValue> left, Result<TValue> right) => left.Equals(right);

	/// <summary>
	/// Check for inequality.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Result<TValue> left, Result<TValue> right) => !left.Equals(right);

	/// <summary>
	/// Check for equality.
	/// </summary>
	[Pure]
	public bool Equals(Result<TValue> other) => (this.isSuccess, other.isSuccess) switch
	{
		(true, true) => EqualityComparer<TValue>.Default.Equals(this.value, other.value),
		(false, false) => EqualityComparer<Error>.Default.Equals(this.error, other.error),
		_ => false,
	};

	/// <inheritdoc/>
	[Pure]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object? obj) => obj switch
	{
		Result<TValue> result => this.Equals(result),
		Result<TValue, Error> result => this.Equals(result.AsBasicResult()),
		_ => false,
	};

	/// <inheritdoc/>
	[Pure]
	public override int GetHashCode() => HashCode.Combine(typeof(Result<TValue, Error>), this.isSuccess, this.value, this.error);

	/// <inheritdoc cref="Result{TValue,TError}.CompareTo"/>
	[Pure]
	public int CompareTo(Result<TValue> other) => (this.isSuccess, other.isSuccess) switch
	{
		(true, false) => -1,
		(true, true) => Comparer<TValue>.Default.Compare(this.value, other.value),
		(false, false) => Comparer<Error>.Default.Compare(this.error, other.error),
		(false, true) => 1,
	};

	/// <inheritdoc/>
	int IComparable.CompareTo(object? other) => other switch
	{
		null => 1,
		Result<TValue> otherResult => this.CompareTo(otherResult),
		Result<TValue, Error> otherResult => this.CompareTo(otherResult.AsBasicResult()),

		// FYI, we could additionally match against `TValue` and `Error` directly,
		// but this would be incompatible with the CompareTo implementation of
		// the fully generic Result<TValue, TError> type.
		_ => throw new ArgumentException("Comparison with incompatible type", nameof(other)),
	};
}
