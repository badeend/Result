using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using Badeend.Errors;

namespace Badeend.Extensions;

// The extension methods have been split out into separate classes because
// .NET does not support overloading methods based on generic constraints only.

/// <summary>
/// Extension methods for Results.
/// </summary>
public static class ResultExtensions
{
	/// <summary>
	/// Change the signature from a generic result into a standard result.
	/// </summary>
	/// <remarks>
	/// This is an <c>O(1)</c> operation and does not allocate any memory.
	///
	/// Typically there's no need to manually call this method because the
	/// operation also exists as an implicit conversion operator on the
	/// <see cref="Result{TValue}"/> type. However, sometimes C#'s type inference
	/// gets confused and needs a little help. In those cases, you can use this
	/// method to explicitly convert from one type to the other.
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
	/// <see cref="Result{TValue}"/> type. However, sometimes C#'s type inference
	/// gets confused and needs a little help. In those cases, you can use this
	/// method to explicitly convert from one type to the other.
	/// </remarks>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static Result<TValue, Error> AsGenericResult<TValue>(this Result<TValue> result) => result;
}

/// <summary>
/// Extensions for <c>Result&lt;T, Badeend.IError&gt;</c>.
/// </summary>
public static class IErrorResultExtensions
{
	/// <summary>
	/// Convert the result's error value into a new <see cref="Error"/>.
	/// If the result is successful, this method is a no-op.
	/// </summary>
	/// <remarks>
	/// This is an <c>O(1)</c> operation and does not allocate any memory.
	/// </remarks>
	[Pure]
	public static Result<TValue> AsStandardResult<TValue, TError>(this Result<TValue, TError> result)
		where TError : IError
	{
		return result.TryGetValue(out var value, out var error) ? value : new Error(error);
	}
}

/// <summary>
/// Extensions for Results where the error type is an enum.
/// </summary>
public static class EnumResultExtensions
{
	/// <summary>
	/// Convert the result's error value into a new <see cref="Error"/>.
	/// If the result is successful, this method is a no-op.
	/// </summary>
	/// <remarks>
	/// If the error value is a regular declared enum member (i.e. it is returned by <c>Enum.GetValues</c>),
	/// then this is an <c>O(1)</c> operation and does not allocate any memory.
	/// </remarks>
	[Pure]
	public static Result<TValue> AsStandardResult<TValue, TError>(this Result<TValue, TError> result)
		where TError : struct, Enum
	{
		return result.TryGetValue(out var value, out var error) ? value : Error.FromEnum(error);
	}
}

/// <summary>
/// Extensions for <c>Result&lt;T, Exception&gt;</c>.
/// </summary>
public static class ExceptionResultExtensions
{
	/// <summary>
	/// Convert the result's error value into a new <see cref="Error"/>.
	/// If the result is successful, this method is a no-op.
	/// </summary>
	/// <remarks>
	/// This is an <c>O(1)</c> operation and does not allocate any memory.
	/// </remarks>
	[Pure]
	public static Result<TValue> AsStandardResult<TValue, TError>(this Result<TValue, TError> result)
		where TError : Exception
	{
		return result.TryGetValue(out var value, out var error) ? value : new Error(error);
	}
}
