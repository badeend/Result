using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Badeend;

/// <summary>
/// Supporting methods for <see cref="Result{TValue, TError}"/>.
/// </summary>
public static class Result
{
	/// <summary>
	/// Create a successful result.
	/// </summary>
	[Pure]
	public static Result<TValue, TError> Success<TValue, TError>(TValue value) => value;

	/// <summary>
	/// Create an error result.
	/// </summary>
	[Pure]
	public static Result<TValue, TError> Error<TValue, TError>(TError error) => error;

	/// <summary>
	/// Attempt to get a readonly reference the operation's success value.
	/// Returns a reference to to <typeparamref name="TValue"/>'s <c>default</c>
	/// value when the operation failed.
	/// </summary>
	[Pure]
	public static ref readonly TValue GetValueRefOrDefaultRef<TValue, TError>(ref readonly Result<TValue, TError> result) => ref result.value;

	/// <summary>
	/// Attempt to get a readonly reference to the operation's error value.
	/// Returns a reference to to <typeparamref name="TError"/>'s <c>default</c>
	/// value when the operation succeeded.
	/// </summary>
	[Pure]
	public static ref readonly TError GetErrorRefOrDefaultRef<TValue, TError>(ref readonly Result<TValue, TError> result) => ref result.error;
}

/// <summary>
/// Represents the result of a fallible operation.
///
/// A <c>Result</c> can be in one of two states: "success" or "error". Both states
/// have an associated payload of type <typeparamref name="TValue"/> or
/// <typeparamref name="TError"/> respectively.
/// </summary>
/// <remarks>
/// Because of the implicit conversion operators you typically don't have to
/// manually construct Results. If for some reason you do want or need to, you
/// can use <see cref="Result.Success"><c>Result.Success()</c></see>,
/// <see cref="Result.Error"><c>Result.Error()</c></see>
/// or one of the constructors.
///
/// The state can be inspected with
/// <see cref="IsSuccess"><c>IsSuccess</c></see>,
/// <see cref="Value"><c>Value</c></see>,
/// <see cref="TryGetValue"><c>TryGetValue</c></see>, and
/// <see cref="GetValueOrDefault()"><c>GetValueOrDefault</c></see>
/// for successful operations. Failures can be inspected similarly using
/// <see cref="IsError"><c>IsError</c></see>,
/// <see cref="Error"><c>Error</c></see>,
/// <see cref="TryGetError"><c>TryGetError</c></see>,
/// <see cref="GetErrorOrDefault()"><c>GetErrorOrDefault</c></see>.
///
/// A Result's <c>default</c> value is equivalent to <c>Result.Error(default!)</c>.
/// </remarks>
/// <typeparam name="TValue">Type of the result when the operation succeeds.</typeparam>
/// <typeparam name="TError">Type of the result when the operation fails.</typeparam>
[StructLayout(LayoutKind.Auto)]
public readonly struct Result<TValue, TError> : IEquatable<Result<TValue, TError>>
{
#pragma warning disable SA1304 // Non-private readonly fields should begin with upper-case letter
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
	internal readonly bool isSuccess;
	internal readonly TValue value;
	internal readonly TError error;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter
#pragma warning restore SA1304 // Non-private readonly fields should begin with upper-case letter

	/// <summary>
	/// Create a successful result.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Result(TValue value)
	{
		this.isSuccess = true;
		this.value = value;
		this.error = default!;
	}

	/// <summary>
	/// Create an error result.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Result(TError error)
	{
		this.isSuccess = false;
		this.value = default!;
		this.error = error;
	}

	/// <summary>
	/// Check whether the operation succeeded.
	/// </summary>
	public bool IsSuccess
	{
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => this.isSuccess;
	}

	/// <summary>
	/// Check whether the operation failed.
	/// </summary>
	[Pure]
	public bool IsError
	{
		[Pure]
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => !this.isSuccess;
	}

	/// <summary>
	/// Get the success value.
	/// </summary>
	/// <exception cref="InvalidOperationException">The operation was not successful.</exception>
	[Pure]
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
		throw new InvalidOperationException("Operation was not successful.", this.error as Exception);
	}

	/// <summary>
	/// Get the error value.
	/// </summary>
	/// <exception cref="InvalidOperationException">The operation did not fail.</exception>
	[Pure]
	public TError Error
	{
		get
		{
			if (this.isSuccess)
			{
				// Extracted exceptional code path into separate method to aid inlining.
				this.ThrowSuccessfulException();
			}

			return this.error;
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TValue GetValueOrDefault(TValue defaultValue) => this.isSuccess ? this.value : defaultValue;

	/// <summary>
	/// Attempt to store the operation's success value in <paramref name="value"/>.
	/// Returns <see langword="false"/> when the operation failed.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
	{
		value = this.value;
		return this.isSuccess;
	}

	/// <summary>
	/// Attempt to get the operation's error value.
	/// Returns <see langword="default"/> when the operation succeeded.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TError? GetErrorOrDefault() => this.error;

	/// <summary>
	/// Attempt to get the operation's error value.
	/// Returns <paramref name="defaultValue"/> when the operation succeeded.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TError GetErrorOrDefault(TError defaultValue) => this.isSuccess ? defaultValue : this.error;

	/// <summary>
	/// Attempt to store the operation's failure in <paramref name="error"/>.
	/// Returns <see langword="false"/> when the operation succeeded.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetError([MaybeNullWhen(false)] out TError error)
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
		false => $"Error({this.error?.ToString() ?? "null"})",
	};

#pragma warning disable CA2225 // Operator overloads have named alternates => Result.Success is good enough
	/// <summary>
	/// Create a successful result.
	/// </summary>
	[Pure]
	public static implicit operator Result<TValue, TError>(TValue value) => new(value);
#pragma warning restore CA2225 // Operator overloads have named alternates

#pragma warning disable CA2225 // Operator overloads have named alternates => Result.Error is good enough
	/// <summary>
	/// Create an error result.
	/// </summary>
	[Pure]
	public static implicit operator Result<TValue, TError>(TError error) => new(error);
#pragma warning restore CA2225 // Operator overloads have named alternates

	/// <summary>
	/// Check for equality.
	/// </summary>
	[Pure]
	public static bool operator ==(Result<TValue, TError> left, Result<TValue, TError> right) => left.Equals(right);

	/// <summary>
	/// Check for inequality.
	/// </summary>
	[Pure]
	public static bool operator !=(Result<TValue, TError> left, Result<TValue, TError> right) => !left.Equals(right);

	/// <summary>
	/// Check for equality.
	/// </summary>
	[Pure]
	public bool Equals(Result<TValue, TError> other) => (this.isSuccess, other.isSuccess) switch
	{
		(true, true) => EqualityComparer<TValue>.Default.Equals(this.value, other.value),
		(false, false) => EqualityComparer<TError>.Default.Equals(this.error, other.error),
		_ => false,
	};

	/// <inheritdoc/>
	[Pure]
	public override bool Equals(object? obj)
	{
		return obj is Result<TValue, TError> result && this.Equals(result);
	}

	/// <inheritdoc/>
	[Pure]
	public override int GetHashCode() => this.isSuccess switch
	{
		true => this.value?.GetHashCode() ?? 0,
		false => this.error?.GetHashCode() ?? 0,
	};
}
