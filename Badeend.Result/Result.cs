using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Badeend;

/// <summary>
/// State of a result.
/// </summary>
public enum ResultState : byte
{
	// The enum cases appear in reverse order with explicitly defined values.
	// This was done so that the "Add missing cases" code fix (IDE0072)
	// automatically puts the happy path (Success) first. While also still
	// keeping the Failure case as the enum's `default` value.

	/// <summary>
	/// The operation succeeded.
	/// </summary>
	Success = 1,

	/// <summary>
	/// The operation failed.
	/// </summary>
	Failure = 0,
}

/// <summary>
/// Supporting methods for <see cref="Result{TValue, TFailure}"/>.
/// </summary>
public static class Result
{
	/// <summary>
	/// Create a successful result.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Result<TValue, TFailure> Success<TValue, TFailure>(TValue value) => value;

	/// <summary>
	/// Create a failure result.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Result<TValue, TFailure> Failure<TValue, TFailure>(TFailure failure) => failure;

	/// <summary>
	/// Attempt to get a readonly reference the operation's success value.
	/// Returns a reference to <typeparamref name="TValue"/>'s <c>default</c>
	/// value when the operation failed.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref readonly TValue GetValueRefOrDefaultRef<TValue, TFailure>(ref readonly Result<TValue, TFailure> result) => ref result.value;

	/// <summary>
	/// Attempt to get a readonly reference to the operation's failure value.
	/// Returns a reference to <typeparamref name="TFailure"/>'s <c>default</c>
	/// value when the operation succeeded.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref readonly TFailure GetFailureRefOrDefaultRef<TValue, TFailure>(ref readonly Result<TValue, TFailure> result) => ref result.failure;

	/// <summary>
	/// Returns the underlying <c>TValue</c> type argument of the provided
	/// result type. Returns <c>null</c> if <paramref name="resultType"/> is not
	/// a closed generic result type.
	/// </summary>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="resultType"/> is <c>null</c>.
	/// </exception>
	public static Type? GetUnderlyingValueType(Type resultType) => GetTypeArgument(resultType, 0);

	/// <summary>
	/// Returns the underlying <c>TFailure</c> type argument of the provided
	/// result type. Returns <c>null</c> if <paramref name="resultType"/> is not
	/// a closed generic result type.
	/// </summary>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="resultType"/> is <c>null</c>.
	/// </exception>
	public static Type? GetUnderlyingFailureType(Type resultType) => GetTypeArgument(resultType, 1);

	private static Type? GetTypeArgument(Type resultType, int index)
	{
		if (resultType is null)
		{
			throw new ArgumentNullException(nameof(resultType));
		}

		if (resultType.IsGenericType
			&& !resultType.IsGenericTypeDefinition
			&& resultType.GetGenericTypeDefinition() == typeof(Result<,>))
		{
			return resultType.GetGenericArguments()[index];
		}

		return null;
	}
}

/// <summary>
/// Represents the result of a fallible operation.
///
/// A <c>Result</c> can be in one of two states:
/// <see cref="ResultState.Success">Success</see> or
/// <see cref="ResultState.Failure">Failure</see>.
/// Both states have an associated payload of type <typeparamref name="TValue"/>
/// or <typeparamref name="TFailure"/> respectively.
/// </summary>
/// <remarks>
/// Because of the implicit conversion operators you typically don't have to
/// manually construct Results. If you do want or need to, you can use
/// <see cref="Result.Success"><c>Result.Success()</c></see> or
/// <see cref="Result.Failure"><c>Result.Failure()</c></see> instead.
///
/// You can examine a result like this:
/// <code>
/// _ = myResult.State switch
/// {
///   ResultState.Success => $"Something successful: {myResult.Value}",
///   ResultState.Failure => $"Something failed: {myResult.Failure}",
/// };
/// </code>
///
/// Or alternatively using
/// <see cref="IsSuccess"><c>IsSuccess</c></see>,
/// <see cref="IsFailure"><c>IsFailure</c></see>,
/// <see cref="TryGetValue(out TValue)"><c>TryGetValue</c></see>,
/// <see cref="TryGetFailure"><c>TryGetFailure</c></see>,
/// <see cref="GetValueOrDefault()"><c>GetValueOrDefault</c></see> or
/// <see cref="GetFailureOrDefault()"><c>GetFailureOrDefault</c></see>.
///
/// A Result's <c>default</c> value is equivalent to <c>Result.Failure(default!)</c>.
/// </remarks>
/// <typeparam name="TValue">Type of the result when the operation succeeds.</typeparam>
/// <typeparam name="TFailure">Type of the result when the operation fails.</typeparam>
[StructLayout(LayoutKind.Auto)]
public readonly struct Result<TValue, TFailure> : IEquatable<Result<TValue, TFailure>>, IComparable<Result<TValue, TFailure>>, IComparable
{
#pragma warning disable SA1304 // Non-private readonly fields should begin with upper-case letter
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
	internal readonly bool isSuccess;
	internal readonly TValue value;
	internal readonly TFailure failure;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter
#pragma warning restore SA1304 // Non-private readonly fields should begin with upper-case letter

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Result(TValue value)
	{
		this.isSuccess = true;
		this.value = value;
		this.failure = default!;
	}

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Result(TFailure failure)
	{
		this.isSuccess = false;
		this.value = default!;
		this.failure = failure;
	}

	/// <summary>
	/// Get the state of the result (<see cref="ResultState.Success">Success</see> or <see cref="ResultState.Failure">Failure</see>).
	/// </summary>
	[Pure]
	public ResultState State => this.isSuccess ? ResultState.Success : ResultState.Failure;

	/// <summary>
	/// Check whether the operation succeeded.
	/// </summary>
	[Pure]
	public bool IsSuccess
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => this.isSuccess;
	}

	/// <summary>
	/// Check whether the operation failed.
	/// </summary>
	[Pure]
	public bool IsFailure
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => !this.isSuccess;
	}

	/// <summary>
	/// Get the success value.
	/// </summary>
	/// <exception cref="InvalidOperationException">The operation was not successful.</exception>
	public TValue Value
	{
		get
		{
			if (!this.isSuccess)
			{
				// Extracted exceptional code path into separate method to aid inlining.
				this.ThrowNotSuccessfulException();
			}

			return this.value;
		}
	}

	[DoesNotReturn]
	private void ThrowNotSuccessfulException()
	{
		throw new InvalidOperationException("Operation was not successful.", this.failure as Exception);
	}

	/// <summary>
	/// Get the failure value.
	/// </summary>
	/// <exception cref="InvalidOperationException">The operation did not fail.</exception>
	public TFailure Failure
	{
		get
		{
			if (this.isSuccess)
			{
				// Extracted exceptional code path into separate method to aid inlining.
				this.ThrowSuccessfulException();
			}

			return this.failure;
		}
	}

	[DoesNotReturn]
	private void ThrowSuccessfulException()
	{
		throw new InvalidOperationException("Operation did not fail.");
	}

	/// <summary>
	/// Attempt to get the operation's success value.
	/// Returns <see langword="default"/> when the operation failed.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TValue? GetValueOrDefault() => this.value;

	/// <summary>
	/// Attempt to get the operation's success value.
	/// Returns <paramref name="defaultValue"/> when the operation failed.
	/// </summary>
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
	/// and the error is stored in <paramref name="failure"/>.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetValue([MaybeNullWhen(false)] out TValue value, [MaybeNullWhen(true)] out TFailure failure)
	{
		value = this.value;
		failure = this.failure;
		return this.isSuccess;
	}

	/// <summary>
	/// Attempt to get the operation's failure value.
	/// Returns <see langword="default"/> when the operation succeeded.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TFailure? GetFailureOrDefault() => this.failure;

	/// <summary>
	/// Attempt to get the operation's failure value.
	/// Returns <paramref name="defaultValue"/> when the operation succeeded.
	/// </summary>
	[Pure]
	public TFailure GetFailureOrDefault(TFailure defaultValue) => this.isSuccess ? defaultValue : this.failure;

	/// <summary>
	/// Attempt to store the operation's failure in <paramref name="failure"/>.
	/// Returns <see langword="false"/> when the operation succeeded.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetFailure([MaybeNullWhen(false)] out TFailure failure)
	{
		failure = this.failure;
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
		false => $"Failure({this.failure?.ToString() ?? "null"})",
	};

#pragma warning disable CA2225 // Operator overloads have named alternates => Result.Success is good enough
	/// <summary>
	/// Create a successful result.
	/// </summary>
	[Pure]
	public static implicit operator Result<TValue, TFailure>(TValue value) => new(value);
#pragma warning restore CA2225 // Operator overloads have named alternates

#pragma warning disable CA2225 // Operator overloads have named alternates => Result.Failure is good enough
	/// <summary>
	/// Create a failure result.
	/// </summary>
	[Pure]
	public static implicit operator Result<TValue, TFailure>(TFailure failure) => new(failure);
#pragma warning restore CA2225 // Operator overloads have named alternates

	/// <summary>
	/// Check for equality.
	/// </summary>
	[Pure]
	public static bool operator ==(Result<TValue, TFailure> left, Result<TValue, TFailure> right) => left.Equals(right);

	/// <summary>
	/// Check for inequality.
	/// </summary>
	[Pure]
	public static bool operator !=(Result<TValue, TFailure> left, Result<TValue, TFailure> right) => !left.Equals(right);

	/// <summary>
	/// Check for equality.
	/// </summary>
	[Pure]
	public bool Equals(Result<TValue, TFailure> other) => (this.isSuccess, other.isSuccess) switch
	{
		(true, true) => EqualityComparer<TValue>.Default.Equals(this.value, other.value),
		(false, false) => EqualityComparer<TFailure>.Default.Equals(this.failure, other.failure),
		_ => false,
	};

	/// <summary>
	/// Compare two results.
	///
	/// Successful results precede failed results.
	/// </summary>
	/// <returns>
	/// See <see cref="IComparable{T}.CompareTo(T)"><c>IComparable&lt;T&gt;.CompareTo(T)</c></see> for more information.
	/// </returns>
	[Pure]
	public int CompareTo(Result<TValue, TFailure> other) => (this.isSuccess, other.isSuccess) switch
	{
		(true, false) => -1,
		(true, true) => Comparer<TValue>.Default.Compare(this.value, other.value),
		(false, false) => Comparer<TFailure>.Default.Compare(this.failure, other.failure),
		(false, true) => 1,
	};

	/// <inheritdoc/>
	[Pure]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object? obj)
	{
		return obj is Result<TValue, TFailure> result && this.Equals(result);
	}

	/// <inheritdoc/>
	int IComparable.CompareTo(object? other) => other switch
	{
		null => 1,
		Result<TValue, TFailure> otherResult => this.CompareTo(otherResult),

		// FYI, we could additionally match against `TValue` and `TFailure` directly,
		// but if they represent the same type or are subtypes of each other
		// (e.g. `Result<object, string>`), the outcome would be ill-defined.
		_ => throw new ArgumentException("Comparison with incompatible type", nameof(other)),
	};

	/// <inheritdoc/>
	[Pure]
	public override int GetHashCode() => this.isSuccess switch
	{
		true => this.value?.GetHashCode() ?? 0,
		false => this.failure?.GetHashCode() ?? 0,
	};
}
