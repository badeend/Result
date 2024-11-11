using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;

namespace Badeend;

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
