using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;

namespace Badeend;

/// <summary>
/// Initialization methods for <see cref="Result{TValue, TError}"/>.
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
/// A Result's <c>default</c> value is an uninitialized Result and will throw on
/// any attempt to access the success or error state.
/// </remarks>
/// <typeparam name="TValue">Type of the result when the operation succeeds.</typeparam>
/// <typeparam name="TError">Type of the result when the operation fails.</typeparam>
public readonly struct Result<TValue, TError> : IEquatable<Result<TValue, TError>>
{
	private enum ResultState
	{
		Uninitialized, // Must be the first item in the enum!
		Success,
		Error,
	}

	private readonly ResultState state;
	private readonly TValue value;
	private readonly TError error;

	/// <summary>
	/// Create a successful result.
	/// </summary>
	[Pure]
	public Result(TValue value)
	{
		this.state = ResultState.Success;
		this.value = value;
		this.error = default!;
	}

	/// <summary>
	/// Create an error result.
	/// </summary>
	[Pure]
	public Result(TError error)
	{
		this.state = ResultState.Error;
		this.value = default!;
		this.error = error;
	}

	/// <summary>
	/// Check whether the operation succeeded.
	/// </summary>
	[Pure]
	public bool IsSuccess => this.state switch
	{
		ResultState.Success => true,
		ResultState.Error => false,
		ResultState.Uninitialized => throw UninitializedException(),
	};

	/// <summary>
	/// Check whether the operation failed.
	/// </summary>
	[Pure]
	public bool IsError => this.state switch
	{
		ResultState.Success => false,
		ResultState.Error => true,
		ResultState.Uninitialized => throw UninitializedException(),
	};

	/// <summary>
	/// Get the success value.
	/// </summary>
	/// <exception cref="InvalidOperationException">The operation was not successful.</exception>
	[Pure]
	public TValue Value => this.state switch
	{
		ResultState.Success => this.value,
		ResultState.Error => throw new InvalidOperationException("Can't get success value from failed result.", this.error as Exception),
		ResultState.Uninitialized => throw UninitializedException(),
	};

	/// <summary>
	/// Get the error value.
	/// </summary>
	/// <exception cref="InvalidOperationException">The operation did not fail.</exception>
	[Pure]
	public TError Error => this.state switch
	{
		ResultState.Success => throw new InvalidOperationException("Can't get error value from successful result."),
		ResultState.Error => this.error,
		ResultState.Uninitialized => throw UninitializedException(),
	};

	/// <summary>
	/// Attempt to get the operation's success value.
	/// Returns <see langword="default"/> when the operation failed.
	/// </summary>
	[Pure]
	public TValue? GetValueOrDefault() => this.GetValueOrDefault(default!);

	/// <summary>
	/// Attempt to get the operation's success value.
	/// Returns <paramref name="defaultValue"/> when the operation failed.
	/// </summary>
	[Pure]
	public TValue GetValueOrDefault(TValue defaultValue) => this.state switch
	{
		ResultState.Success => this.value,
		ResultState.Error => defaultValue,
		ResultState.Uninitialized => throw UninitializedException(),
	};

	/// <summary>
	/// Attempt to store the operation's success value in <paramref name="value"/>.
	/// Returns <see langword="false"/> when the operation failed.
	/// </summary>
	[Pure]
	public bool TryGetValue([MaybeNullWhen(false)] out TValue value)
	{
		if (this.state is ResultState.Success)
		{
			value = this.value;
			return true;
		}
		else if (this.state is ResultState.Error)
		{
			value = default;
			return false;
		}
		else
		{
			throw UninitializedException();
		}
	}

	/// <summary>
	/// Attempt to get the operation's error value.
	/// Returns <see langword="default"/> when the operation succeeded.
	/// </summary>
	[Pure]
	public TError? GetErrorOrDefault() => this.GetErrorOrDefault(default!);

	/// <summary>
	/// Attempt to get the operation's error value.
	/// Returns <paramref name="defaultValue"/> when the operation succeeded.
	/// </summary>
	[Pure]
	public TError GetErrorOrDefault(TError defaultValue) => this.state switch
	{
		ResultState.Success => defaultValue,
		ResultState.Error => this.error,
		ResultState.Uninitialized => throw UninitializedException(),
	};

	/// <summary>
	/// Attempt to store the operation's failure in <paramref name="error"/>.
	/// Returns <see langword="false"/> when the operation succeeded.
	/// </summary>
	[Pure]
	public bool TryGetError([MaybeNullWhen(false)] out TError error)
	{
		if (this.state is ResultState.Error)
		{
			error = this.error;
			return true;
		}
		else if (this.state is ResultState.Success)
		{
			error = default;
			return false;
		}
		else
		{
			throw UninitializedException();
		}
	}

	/// <summary>
	/// Get a string representation of the result for debugging purposes.
	/// The format is not stable and may change without prior notice.
	/// </summary>
	[Pure]
	public override string ToString() => this.state switch
	{
		ResultState.Success => $"Success({this.value?.ToString() ?? "null"})",
		ResultState.Error => $"Error({this.error?.ToString() ?? "null"})",
		ResultState.Uninitialized => string.Empty,
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
	public bool Equals(Result<TValue, TError> other) => (this.state, other.state) switch
	{
		(ResultState.Success, ResultState.Success) => EqualityComparer<TValue>.Default.Equals(this.value, other.value),
		(ResultState.Error, ResultState.Error) => EqualityComparer<TError>.Default.Equals(this.error, other.error),
		(ResultState.Uninitialized, ResultState.Uninitialized) => true,
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
	public override int GetHashCode() => this.state switch
	{
		ResultState.Success => this.value?.GetHashCode() ?? 0,
		ResultState.Error => this.error?.GetHashCode() ?? 0,
		ResultState.Uninitialized => 0,
	};

	private static InvalidOperationException UninitializedException() => new("Uninitialized result.");
}
