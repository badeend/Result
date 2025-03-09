using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using Badeend.Results.Extensions;

namespace Badeend;

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
[SuppressMessage("Naming", "CA1708:Identifiers should differ by more than case", Justification = "Internal")]
[SuppressMessage("Design", "CA1036:Override methods on comparable types", Justification = "Result is only comparable if TValue and TError are, which we can't know at compile time. Don't want to promote the comparable stuff too much.")]
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
		Exception? innerException = null;

		if (typeof(TError) == typeof(Error))
		{
			innerException = (this.error as Error?)?.AsException();
		}
		else if (typeof(Exception).IsAssignableFrom(typeof(TError)))
		{
			innerException = this.error as Exception;
		}

		if (innerException is not null)
		{
			throw new InvalidOperationException("Operation was not successful. See inner exception for more details.", innerException);
		}
		else
		{
			throw new InvalidOperationException("Operation was not successful.");
		}
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
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Result<TValue, TError> left, Result<TValue, TError> right) => left.Equals(right);

	/// <summary>
	/// Check for inequality.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
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
	/// <exception cref="ArgumentException"><typeparamref name="TValue"/> or <typeparamref name="TError"/> does not implement IComparable.</exception>
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
	public override bool Equals(object? obj) => obj switch
	{
		Result<TValue, TError> otherResult => this.Equals(otherResult),
		Result<TValue> otherResult when this is Result<TValue, Error> thisResult => thisResult.Equals(otherResult.AsGenericResult()),
		_ => false,
	};

	/// <inheritdoc/>
	int IComparable.CompareTo(object? other) => other switch
	{
		null => 1,
		Result<TValue, TError> otherResult => this.CompareTo(otherResult),
		Result<TValue> otherResult when this is Result<TValue, Error> thisResult => thisResult.CompareTo(otherResult.AsGenericResult()),

		// FYI, we could additionally match against `TValue` and `TError` directly,
		// but if they represent the same type or are subtypes of each other
		// (e.g. `Result<object, string>`), the outcome would be ill-defined.
		_ => throw new ArgumentException("Comparison with incompatible type", nameof(other)),
	};

	/// <inheritdoc/>
	[Pure]
	public override int GetHashCode() => HashCode.Combine(typeof(Result<TValue, TError>), this.isSuccess, this.value, this.error);
}
