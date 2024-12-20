using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Exists as extension method.")]
	public static implicit operator Result<TValue, Error>(Result<TValue> result) => result.inner;

	/// <summary>
	/// Convert from a Result with an explicit error type to a Result with an implicit error type.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	[SuppressMessage("Usage", "CA2225:Operator overloads have named alternates", Justification = "Exists as extension method.")]
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
