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
	// keeping the Error case as the enum's `default` value.

	/// <summary>
	/// The operation succeeded.
	/// </summary>
	Success = 1,

	/// <summary>
	/// The operation failed.
	/// </summary>
	Error = 0,
}

/// <summary>
/// Supporting methods for <see cref="Result{TValue}"/> and <see cref="Result{TValue, TError}"/>.
/// </summary>
public static class Result
{
	/// <summary>
	/// Create a successful standard result.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Result<TValue> Success<TValue>(TValue value) => value;

	/// <summary>
	/// Create a successful generic result.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Result<TValue, TError> Success<TValue, TError>(TValue value) => value;

	/// <summary>
	/// Create a failed standard result.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Result<TValue> Error<TValue>() => default(Error);

	/// <summary>
	/// Create a failed standard result.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Result<TValue> Error<TValue>(Error error) => error;

	/// <summary>
	/// Create a failed generic result.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Result<TValue, TError> Error<TValue, TError>(TError error) => error;

	/// <summary>
	/// Attempt to get a readonly reference the operation's success value.
	/// Returns a reference to <typeparamref name="TValue"/>'s <c>default</c>
	/// value when the operation failed.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref readonly TValue GetValueRefOrDefaultRef<TValue, TError>(ref readonly Result<TValue, TError> result) => ref result.value;

	/// <summary>
	/// Attempt to get a readonly reference the operation's success value.
	/// Returns a reference to <typeparamref name="TValue"/>'s <c>default</c>
	/// value when the operation failed.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref readonly TValue GetValueRefOrDefaultRef<TValue>(ref readonly Result<TValue> result) => ref result.inner.value;

	/// <summary>
	/// Attempt to get a readonly reference to the operation's error value.
	/// Returns a reference to <typeparamref name="TError"/>'s <c>default</c>
	/// value when the operation succeeded.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref readonly TError GetErrorRefOrDefaultRef<TValue, TError>(ref readonly Result<TValue, TError> result) => ref result.error;

	/// <summary>
	/// Attempt to get a readonly reference to the operation's error value.
	/// Returns a reference to <see cref="Badeend.Error"/>'s <c>default</c>
	/// value when the operation succeeded.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static ref readonly Error GetErrorRefOrDefaultRef<TValue>(ref readonly Result<TValue> result) => ref result.inner.error;

	/// <summary>
	/// Returns the underlying <c>TValue</c> type argument of the provided
	/// result type. Returns <c>null</c> if <paramref name="resultType"/> is not
	/// a closed generic result type.
	/// </summary>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="resultType"/> is <c>null</c>.
	/// </exception>
	public static Type? GetUnderlyingValueType(Type resultType)
	{
		if (resultType is null)
		{
			throw new ArgumentNullException(nameof(resultType));
		}

		if (resultType.IsGenericType && !resultType.IsGenericTypeDefinition)
		{
			var genericType = resultType.GetGenericTypeDefinition();

			if (genericType == typeof(Result<>) || genericType == typeof(Result<,>))
			{
				return resultType.GetGenericArguments()[0];
			}
		}

		return null;
	}

	/// <summary>
	/// Returns the underlying <c>TError</c> type argument of the provided result
	/// type, or <see cref="Badeend.Error"><c>typeof(Badeend.Error)</c></see>
	/// for the <see cref="Result{TValue}"/> shorthand result types.
	///
	/// Returns <c>null</c> if <paramref name="resultType"/> is not a closed
	/// generic result type.
	/// </summary>
	/// <exception cref="ArgumentNullException">
	/// <paramref name="resultType"/> is <c>null</c>.
	/// </exception>
	public static Type? GetUnderlyingErrorType(Type resultType)
	{
		if (resultType is null)
		{
			throw new ArgumentNullException(nameof(resultType));
		}

		if (resultType.IsGenericType && !resultType.IsGenericTypeDefinition)
		{
			var genericType = resultType.GetGenericTypeDefinition();

			if (genericType == typeof(Result<>))
			{
				return typeof(Badeend.Error);
			}
			else if (genericType == typeof(Result<,>))
			{
				return resultType.GetGenericArguments()[1];
			}
		}

		return null;
	}

	/// <summary>
	/// Change the signature from a generic result into a standard result.
	/// </summary>
	/// <remarks>
	/// This is an <c>O(1)</c> operation and does not allocate any memory.
	///
	/// Typically there's no need to manually call this method because the
	/// operation also exists as an implicit conversion operator on the
	/// <see cref="Result{TValue}"/> type.
	/// </remarks>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Result<TValue> AsStandardResult<TValue>(this Result<TValue, Error> result) => result;

	/// <summary>
	/// Change the signature from a standard result into a generic result.
	/// </summary>
	/// <remarks>
	/// This is an <c>O(1)</c> operation and does not allocate any memory.
	///
	/// Typically there's no need to manually call this method because the
	/// operation also exists as an implicit conversion operator on the
	/// <see cref="Result{T}"/> type.
	/// </remarks>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Result<TValue, Error> AsGenericResult<TValue>(this Result<TValue> result) => result;
}

/// <summary>
/// Represents the result of a fallible operation.
///
/// A <c>Result</c> can be in one of two states:
/// <see cref="ResultState.Success">Success</see> or
/// <see cref="ResultState.Error">Error</see>.
/// Both states have an associated payload of type <typeparamref name="TValue"/>
/// or <typeparamref name="TError"/> respectively.
/// </summary>
/// <remarks>
/// Because of the implicit conversion operators you typically don't have to
/// manually construct Results. If you do want or need to, you can use
/// <see cref="Result.Success{TValue, TError}(TValue)"><c>Result.Success()</c></see> or
/// <see cref="Result.Error{TValue, TError}(TError)"><c>Result.Error()</c></see> instead.
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
/// A Result's <c>default</c> value is equivalent to <c>Result.Error(default!)</c>.
/// </remarks>
/// <typeparam name="TValue">Type of the result when the operation succeeds.</typeparam>
/// <typeparam name="TError">Type of the result when the operation fails.</typeparam>
[StructLayout(LayoutKind.Auto)]
public readonly struct Result<TValue, TError> : IEquatable<Result<TValue, TError>>, IComparable<Result<TValue, TError>>, IComparable
{
#pragma warning disable SA1304 // Non-private readonly fields should begin with upper-case letter
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
	internal readonly bool isSuccess;
	internal readonly TValue value;
	internal readonly TError error;
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
	private Result(TError error)
	{
		this.isSuccess = false;
		this.value = default!;
		this.error = error;
	}

	/// <summary>
	/// Get the state of the result (<see cref="ResultState.Success">Success</see> or <see cref="ResultState.Error">Error</see>).
	/// </summary>
	[Pure]
	public ResultState State => this.isSuccess ? ResultState.Success : ResultState.Error;

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
	public bool IsError
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => !this.isSuccess;
	}

	/// <summary>
	/// Get the success value.
	/// </summary>
	/// <exception cref="InvalidOperationException">The operation was not successful.</exception>
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
		throw new InvalidOperationException("Operation was not successful.", this.error as Exception);
	}

	/// <summary>
	/// Get the error value.
	/// </summary>
	/// <exception cref="InvalidOperationException">The operation did not fail.</exception>
	[UnscopedRef]
	public ref readonly TError Error
	{
		get
		{
			if (this.isSuccess)
			{
				// Extracted exceptional code path into separate method to aid inlining.
				this.ThrowSuccessfulException();
			}

			return ref this.error;
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
	/// and the error is stored in <paramref name="error"/>.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetValue([MaybeNullWhen(false)] out TValue value, [MaybeNullWhen(true)] out TError error)
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
	public TError? GetErrorOrDefault() => this.error;

	/// <summary>
	/// Attempt to get the operation's error value.
	/// Returns <paramref name="defaultValue"/> when the operation succeeded.
	/// </summary>
	[Pure]
	public TError GetErrorOrDefault(TError defaultValue) => this.isSuccess ? defaultValue : this.error;

	/// <summary>
	/// Attempt to store the operation's error in <paramref name="error"/>.
	/// Returns <see langword="false"/> when the operation succeeded.
	/// </summary>
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Result<TValue, TError>(TValue value) => new(value);
#pragma warning restore CA2225 // Operator overloads have named alternates

#pragma warning disable CA2225 // Operator overloads have named alternates => Result.Error is good enough
	/// <summary>
	/// Create a error result.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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

	/// <summary>
	/// Compare two results.
	///
	/// Successful results precede Error results.
	/// </summary>
	/// <returns>
	/// See <see cref="IComparable{T}.CompareTo(T)"><c>IComparable&lt;T&gt;.CompareTo(T)</c></see> for more information.
	/// </returns>
	[Pure]
	public int CompareTo(Result<TValue, TError> other) => (this.isSuccess, other.isSuccess) switch
	{
		(true, false) => -1,
		(true, true) => Comparer<TValue>.Default.Compare(this.value, other.value),
		(false, false) => Comparer<TError>.Default.Compare(this.error, other.error),
		(false, true) => 1,
	};

	/// <inheritdoc/>
	[Pure]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object? obj)
	{
		return obj is Result<TValue, TError> result && this.Equals(result);
	}

	/// <inheritdoc/>
	int IComparable.CompareTo(object? other) => other switch
	{
		null => 1,
		Result<TValue, TError> otherResult => this.CompareTo(otherResult),

		// FYI, we could additionally match against `TValue` and `TError` directly,
		// but if they represent the same type or are subtypes of each other
		// (e.g. `Result<object, string>`), the outcome would be ill-defined.
		_ => throw new ArgumentException("Comparison with incompatible type", nameof(other)),
	};

	/// <inheritdoc/>
	[Pure]
	public override int GetHashCode() => this.isSuccess switch
	{
		true => this.value?.GetHashCode() ?? 0,
		false => this.error?.GetHashCode() ?? 0,
	};
}

/// <summary>
/// Represents the result of a fallible operation. This type is a shorthand for:
/// <see cref="Result{TValue, TError}"><c>Badeend.Result&lt;TValue, Badeend.Error&gt;</c></see>.
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
/// A Result's <c>default</c> value is equivalent to <c>Result.Error(default!)</c>.
/// </remarks>
/// <typeparam name="TValue">Type of the result when the operation succeeds.</typeparam>
[StructLayout(LayoutKind.Auto)]
public readonly struct Result<TValue> : IEquatable<Result<TValue>>
{
#pragma warning disable SA1304 // Non-private readonly fields should begin with upper-case letter
#pragma warning disable SA1307 // Accessible fields should begin with upper-case letter
	internal readonly Result<TValue, Error> inner;
#pragma warning restore SA1307 // Accessible fields should begin with upper-case letter
#pragma warning restore SA1304 // Non-private readonly fields should begin with upper-case letter

	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	private Result(Result<TValue, Error> inner)
	{
		this.inner = inner;
	}

	/// <inheritdoc cref="Result{TValue,TError}.State"/>
	[Pure]
	public ResultState State
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => this.inner.State;
	}

	/// <inheritdoc cref="Result{TValue,TError}.IsSuccess"/>
	[Pure]
	public bool IsSuccess
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => this.inner.IsSuccess;
	}

	/// <inheritdoc cref="Result{TValue,TError}.IsError"/>
	[Pure]
	public bool IsError
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => this.inner.IsError;
	}

	/// <inheritdoc cref="Result{TValue,TError}.Value"/>
	[UnscopedRef]
	public ref readonly TValue Value
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ref this.inner.Value;
	}

	/// <inheritdoc cref="Result{TValue,TError}.Error"/>
	[UnscopedRef]
	public ref readonly Error Error => ref this.inner.Error;

	/// <inheritdoc cref="Result{TValue,TError}.GetValueOrDefault()"/>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TValue? GetValueOrDefault() => this.inner.GetValueOrDefault();

	/// <inheritdoc cref="Result{TValue,TError}.GetValueOrDefault(TValue)"/>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public TValue GetValueOrDefault(TValue defaultValue) => this.inner.GetValueOrDefault(defaultValue);

	/// <summary>
	/// Attempt to store the operation's success value in <paramref name="value"/>.
	/// Returns <see langword="false"/> when the operation failed.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetValue([MaybeNullWhen(false)] out TValue value) => this.inner.TryGetValue(out value);

	/// <summary>
	/// Attempt to store the operation's success value in <paramref name="value"/>.
	/// If the operation failed, this method returns <see langword="false"/>
	/// and the error is stored in <paramref name="error"/>.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetValue([MaybeNullWhen(false)] out TValue value, [MaybeNullWhen(true)] out Error error) => this.inner.TryGetValue(out value, out error);

	/// <summary>
	/// Attempt to get the operation's error value.
	/// Returns <see langword="default"/> when the operation succeeded.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Error GetErrorOrDefault() => this.inner.GetErrorOrDefault();

	/// <summary>
	/// Attempt to get the operation's error value.
	/// Returns <paramref name="defaultValue"/> when the operation succeeded.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Error GetErrorOrDefault(Error defaultValue) => this.inner.GetErrorOrDefault(defaultValue);

	/// <summary>
	/// Attempt to store the operation's error in <paramref name="error"/>.
	/// Returns <see langword="false"/> when the operation succeeded.
	/// </summary>
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool TryGetError([MaybeNullWhen(false)] out Error error) => this.inner.TryGetError(out error);

	/// <summary>
	/// Get a string representation of the result for debugging purposes.
	/// The format is not stable and may change without prior notice.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override string ToString() => this.inner.ToString();

#pragma warning disable CA2225 // Operator overloads have named alternates => Result.Success is good enough
	/// <summary>
	/// Create a successful result.
	/// </summary>
	[Pure]
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Result<TValue, Error>(Result<TValue> result) => result.inner;

	/// <summary>
	/// Convert from a Result with an explicit error type to a Result with an implicit error type.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static implicit operator Result<TValue>(Result<TValue, Error> result) => new(result);

	/// <summary>
	/// Check for equality.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Result<TValue> left, Result<TValue> right) => left.inner == right.inner;

	/// <summary>
	/// Check for inequality.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Result<TValue> left, Result<TValue> right) => left.inner != right.inner;

	/// <summary>
	/// Check for equality.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public bool Equals(Result<TValue> other) => this.inner.Equals(other.inner);

	/// <inheritdoc/>
	[Pure]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object? obj)
	{
		return obj is Result<TValue> result && this.Equals(result);
	}

	/// <inheritdoc/>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public override int GetHashCode() => this.inner.GetHashCode();
}
