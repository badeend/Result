using System.ComponentModel;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;
using Badeend.Errors;

namespace Badeend;

/// <summary>
/// A lightweight immutable error representation that contains:
/// <list type="bullet">
///   <item>A human-readable error description. (<see cref="Message"/>)</item>
///   <item>(Optional) A custom data payload, used to provide additional context beyond the message. (<see cref="Data"/>)</item>
///   <item>(Optional) An inner error, used to build up a "chain" of context. (<see cref="InnerError"/>)</item>
/// </list>
/// </summary>
/// <remarks>
/// This Error type is designed to be a close relative to the built-in Exception
/// class, but with a focus on being lightweight and suitable for situations where
/// errors need to be reported frequently and/or where performance is critical.
///
/// The four primary methods for creating new errors are:
/// <list type="bullet">
///   <item>From a message string (<see cref="Error(string?)"/>)</item>
///   <item>From an existing Exception instance (<see cref="Error(Exception?)"/>)</item>
///   <item>From an <c>enum</c> value (<see cref="FromEnum"/>)</item>
///   <item>From a custom <see cref="IError"/> implementation (<see cref="Error(IError?)"/>)</item>
/// </list>
///
/// All of these are <c>O(1)</c> and allocation free.
///
/// This type is designed to be used with <c>Badeend.Result</c>, though it can
/// be used standalone as well. Note that you should generally not attempt to
/// derive any semantic meaning from the Error's content.
/// I.e., <c>Result&lt;T, Badeend.Error&gt;</c> (or its shorthand <c>Result&lt;T&gt;</c>)
/// is semantically the same as <c>Result&lt;T, void&gt;</c> in that: all that the
/// domain logic should care about is whether the operation succeeded or failed.
/// The Error data is just a way to carry additional developer-oriented context.
///
/// This type has the size of just a single machine word (4 or 8 bytes), making
/// it a good fit for applications where errors are treated as first-class
/// values, are copied frequently and are propagated through regular control
/// flow patterns instead of stack unwinding.
///
/// This type does not collect stack traces <i>by design</i>. Any additional
/// context that you want to attach along the way must be added manually by
/// wrapping it inside a new error using one of constructors that take an
/// <c>InnerError</c> parameter, e.g. <c><see cref="Error(string?,object?,Error?)"/></c>.
///
/// The <c>default</c> Error contains only a predefined default message and is
/// equivalent to using the <see cref="Error()">parameterless constructor</see>.
/// </remarks>
[StructLayout(LayoutKind.Auto)]
[SuppressMessage("Design", "CA1036:Override methods on comparable types", Justification = "Error is only comparable if its Data is too, which we can't know at compile time. Don't want to promote the comparable stuff too much.")]
#pragma warning disable CA1716 // Identifiers should not match keywords. => Don't care about VB.
public readonly struct Error : IEquatable<Error>, IComparable<Error>, IComparable
#pragma warning restore CA1716 // Identifiers should not match keywords.
{
	private const string MessagePrefix = $"Error: ";
	private const string DefaultErrorMessage = "Operation did not complete successfully";
	private const string DefaultErrorToString = MessagePrefix + DefaultErrorMessage;
	private const string DefaultExceptionMessage = "An exception was thrown";

	/// <summary>
	/// Is one of the following:
	/// - `null`:      Empty error (`default`).
	/// - `string`:    Message-only error.
	/// - `IError`:    Error created through the special constructor.
	/// - `Exception`: Error created through the special constructor. The exception instance acts as both `Data` and a `Message`. The InnerException is the InnerError.
	/// </summary>
	private readonly object? obj;

	/// <summary>
	/// Human-readable description of the error.
	/// A generic fallback string will be returned if no message was provided.
	/// </summary>
	/// <remarks>
	/// Error messages are intended for human consumption only and
	/// should not be parsed or relied on programmatically. Parsing this
	/// message may lead to breaking changes if the message format changes in
	/// the future.
	/// </remarks>
	[Pure]
	public string Message => this.obj switch
	{
		string s => s,
		IError e => e.Message ?? DefaultErrorMessage,
		Exception e => e.Message ?? DefaultExceptionMessage,
		_ => DefaultErrorMessage,
	};

	/// <summary>
	/// The payload attached at this specific point in the error chain.
	/// </summary>
	/// <remarks>
	/// Generally, you should not depend on specific payloads existing at
	/// specific levels in the error chain as they may change over time due to
	/// e.g. internal refactorings or other updates to the code that produces
	/// the errors.
	///
	/// If you need to retrieve a specific type of payload, consider using the
	/// <see cref="TryFindData{T}"/> method instead.
	/// </remarks>
	[Pure]
	public object? Data => this.obj switch
	{
		IError e => e.Data,
		Exception e => e,
		_ => null,
	};

	/// <summary>
	/// Gets the inner error associated with the current <see cref="Error"/>
	/// instance, or <c>null</c> if this is a "root" error.
	/// </summary>
	/// <remarks>
	/// This property is similar to the <c>InnerException</c> property of
	/// regular .NET exceptions. When a new <see cref="Error"/> is created by
	/// using e.g. <see cref="Error(string?, object?, Error?)"/>, the pre-existing
	/// <see cref="Error"/> instance becomes the <c>InnerError</c> of the new
	/// instance. This chaining allows for capturing contextual information at
	/// each layer where the error is encountered, while retaining the original
	/// error details.
	///
	/// Accessing <see cref="InnerError"/> provides a way to traverse the chain of
	/// errors in reverse order, from the most recently appended error back to the
	/// original root error.
	/// </remarks>
	[Pure]
	public Error? InnerError => this.obj switch
	{
		IError e => e.InnerError,
		Exception { InnerException: { } innerException } => new Error(innerException),
		_ => null,
	};

	[Pure]
	private string? StackTrace => this.obj switch
	{
		Exception e => e.StackTrace,
		_ => null,
	};

	/// <summary>
	/// Create a new empty <see cref="Error"/>.
	/// </summary>
	/// <remarks>
	/// Errors created by this constructor can be used as a "marker" to signal
	/// that an operation did not succeed even in the absence of a descriptive
	/// message.
	///
	/// This is an <c>O(1)</c> operation, does not allocate any memory and is
	/// equivalent to the <c>default</c> value.
	/// </remarks>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Error()
	{
	}

	/// <summary>
	/// Create a new <see cref="Error"/> using the provided <paramref name="error"/>
	/// as the backing implementation.
	/// </summary>
	/// <remarks>
	/// This is an <c>O(1)</c> operation and does not allocate any memory.
	/// </remarks>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Error(IError? error)
	{
		this.obj = error;
	}

	/// <summary>
	/// Create a new <see cref="Error"/> from the provided <paramref name="exception"/>.
	/// </summary>
	/// <remarks>
	/// If the provided <paramref name="exception"/> was obtained through
	/// <see cref="AsException"/>, the original Error is returned to prevent
	/// double wrapping.
	///
	/// Otherwise, this is functionally equivalent to creating an Error with:
	/// <list type="bullet">
	///   <item>the <see cref="Message"/> set to the exception's <see cref="Exception.Message"/>,</item>
	///   <item>the <see cref="Data"/> set to the exception itself</item>
	///   <item>the <see cref="InnerError"/> set to the exception's <see cref="Exception.InnerException"/> that has been recursively converted using the preceding logic.</item>
	/// </list>
	/// </remarks>
	[Pure]
	public Error(Exception? exception)
	{
		if (exception is ErrorException { ErrorValue: var error })
		{
			this.obj = error.obj;
		}
		else
		{
			this.obj = exception;
		}
	}

	/// <summary>
	/// Create a new <see cref="Error"/> with the provided <paramref name="message"/>.
	/// </summary>
	/// <remarks>
	/// This is an <c>O(1)</c> operation and does not allocate any memory.
	/// </remarks>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public Error(string? message)
	{
		this.obj = message;
	}

	/// <summary>
	/// Create a new <see cref="Error"/> with the provided <paramref name="message"/>
	/// that wraps the <paramref name="innerError"/>.
	/// </summary>
	[Pure]
	public Error(string? message, Error? innerError)
	{
		if (innerError is null)
		{
			this.obj = message;
		}
		else
		{
			this.obj = new FatError
			{
				Message = message,
				Data = null,
				InnerError = innerError,
			};
		}
	}

	/// <summary>
	/// Create a new <see cref="Error"/> with the provided <paramref name="message"/>
	/// and/or <paramref name="data"/> that wraps the <paramref name="innerError"/>.
	/// </summary>
	[Pure]
	public Error(string? message, object? data = null, Error? innerError = null)
	{
		if (data is null && innerError is null)
		{
			this.obj = message;
		}
		else
		{
			this.obj = new FatError
			{
				Message = message,
				Data = data,
				InnerError = innerError,
			};
		}
	}

	/// <summary>
	/// Create a new <see cref="Error"/> from the provided enum <paramref name="value"/>.
	/// </summary>
	/// <remarks>
	/// The error message can be customized by annotating the enum members with the
	/// <see cref="ErrorMessageAttribute">[ErrorMessage]</see> attribute.
	///
	/// If the value is a regular declared enum member (i.e. it is returned by <c>Enum.GetValues</c>),
	/// then this is an <c>O(1)</c> operation and does not allocate any memory.
	/// </remarks>
	public static Error FromEnum<TEnum>(TEnum value)
		where TEnum : struct, Enum
	{
		return new(EnumError<TEnum>.Lookup.Instance.GetError(value));
	}

	/// <summary>
	/// Search through the error chain and attempt to retrieve the payload that
	/// matches the specified type <typeparamref name="T"/>.
	/// </summary>
	/// <typeparam name="T">The type of the payload object to retrieve from the Error.</typeparam>
	/// <remarks>
	/// This method provides a type-safe way to access metadata without overly
	/// relying on the order in which the errors are nested. If multiple items
	/// of type <typeparamref name="T"/> exist, the top-most instance in the
	/// chain is returned.
	/// </remarks>
	public bool TryFindData<T>([MaybeNullWhen(false)] out T data)
	{
		Error? current = this;
		while (current is { Data: var d, InnerError: var inner })
		{
#pragma warning disable CA1508 // Avoid dead conditional code. => Analyzer is confused.
			if (d is T match)
#pragma warning restore CA1508 // Avoid dead conditional code.
			{
				data = match;
				return true;
			}

			current = inner;
		}

		data = default;
		return false;
	}

	/// <summary>
	/// Convert the error into an exception. This may return a previously
	/// <see cref="Error(Exception?)">Error-wrapped-Exception</see> as-is.
	/// The returned exception should therefore be considered as "already thrown"
	/// and should not be thrown directly again. The only supported method for
	/// throwing the returned value is as the inner exception of a fresh
	/// exception instance.
	/// </summary>
	/// <remarks>
	/// The returned Exception hierarchy is for debugging purposes only.
	/// The format is not stable and may change without prior notice.
	/// </remarks>
	[Pure]
	public Exception AsException()
	{
		if (this.obj is Exception e)
		{
			return e;
		}

		return new ErrorException(this, this.InnerError?.AsException());
	}

	/// <summary>
	/// Get a string representation of the error for debugging purposes.
	/// The format is not stable and may change without prior notice.
	/// </summary>
	[Pure]
	public override string ToString()
	{
		if (this.obj is null)
		{
			return DefaultErrorToString;
		}
		else if (this.obj is string s)
		{
			return $"{MessagePrefix}{s}";
		}

		var result = new StringBuilder();

		var message = this.Message;
		var data = this.Data;
		var stackTrace = this.StackTrace;
		var inner = this.InnerError;
		var innerErrorString = inner?.ToString();

		// Special case for exceptions as their .ToString() implementation
		// already prints out the message and inner exceptions. We don't want
		// that info to get duplicated:
		if (this.obj is Exception exception)
		{
			var className = exception.GetType().ToString();

			result.Append(className);
			result.Append(": ");
			result.Append(message);
		}
		else if (this.obj is SpecialError specialError)
		{
			specialError.SerializeInto(result);
		}
		else
		{
			if (this.obj is IError ierror)
			{
				var className = ierror.GetType().ToString();

				result.Append(className);
				result.Append(": ");
			}
			else
			{
				result.Append(MessagePrefix);
			}

			result.Append(message);

			if (data is not null)
			{
				result.AppendLine();
				result.Append("Data: ");
				result.Append(DataToString(data));
			}
		}

		if (innerErrorString is not null)
		{
			result.AppendLine();
			result.AppendLine();
			result.AppendLine("--- Caused by: ---");
			result.Append(innerErrorString);

			if (inner?.StackTrace is not null)
			{
				result.AppendLine();
				result.Append("--- End of inner stack trace ---");
			}
		}

		if (stackTrace is not null)
		{
			if (innerErrorString is null)
			{
				result.AppendLine();
			}

			result.AppendLine();
			result.Append(stackTrace);
		}

		return result.ToString();
	}

	internal static string DataToString(object? data)
	{
#pragma warning disable CA1031 // Do not catch general exception types.
		try
		{
			return data?.ToString() ?? "null";
		}
		catch (Exception)
		{
			return "<<.ToString() threw an exception>>";
		}
#pragma warning restore CA1031 // Do not catch general exception types.
	}

	/// <inheritdoc/>
	[Pure]
	public override int GetHashCode() => HashCode.Combine(
		this.Message,
		this.Data,
		this.InnerError);

	/// <summary>
	/// Check two <see cref="Error"/> instances for equality. Errors use
	/// structural equality and are considered "equal" when their
	/// <c>(Message, Data, InnerError)</c> components are equal. The <c>Data</c>
	/// component is compared using the <see cref="object.Equals(object)"/> method.
	/// </summary>
	[Pure]
	public bool Equals(Error other)
	{
		if (object.ReferenceEquals(this.obj, other.obj))
		{
			return true;
		}

		return this.Message == other.Message
			&& DataEquals(this.Data, other.Data)
			&& this.InnerError == other.InnerError;
	}

	private static bool DataEquals(object? left, object? right)
	{
		if (left is null)
		{
			return right is null;
		}
		else
		{
			return right is not null && left.Equals(right);
		}
	}

#pragma warning disable CS0809 // Obsolete member overrides non-obsolete member
	/// <inheritdoc/>
	[Pure]
	[Obsolete("Avoid boxing. Use == instead.")]
	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool Equals(object? obj)
	{
		return obj is Error otherError && this.Equals(otherError);
	}
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

	/// <summary>
	/// Compare two errors.
	///
	/// Errors are compared by their components: <c>(Message, Data, InnerError)</c>.
	/// </summary>
	/// <returns>
	/// See <see cref="IComparable{T}.CompareTo(T)"><c>IComparable&lt;T&gt;.CompareTo(T)</c></see> for more information.
	/// </returns>
	/// <exception cref="ArgumentException">The Data object does not implement IComparable.</exception>
	[Pure]
	public int CompareTo(Error other)
	{
		if (object.ReferenceEquals(this.obj, other.obj))
		{
			return 0;
		}

		var messageComparison = string.CompareOrdinal(this.Message, other.Message);
		if (messageComparison != 0)
		{
			return messageComparison;
		}

		var dataComparison = Comparer<object>.Default.Compare(this.Data!, other.Data!);
		if (dataComparison != 0)
		{
			return dataComparison;
		}

		return Compare(this.InnerError, other.InnerError);
	}

	private static int Compare(Error? left, Error? right)
	{
		if (left is null)
		{
			return right is null ? 0 : -1;
		}
		else
		{
			return right is null ? 1 : left.Value.CompareTo(right.Value);
		}
	}

	/// <inheritdoc/>
	int IComparable.CompareTo(object? other) => other switch
	{
		null => 1,
		Error otherError => this.CompareTo(otherError),
		_ => throw new ArgumentException("Comparison with incompatible type", nameof(other)),
	};

	/// <inheritdoc cref="Equals(Error)"/>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator ==(Error left, Error right) => left.Equals(right);

	/// <summary>
	/// Check for inequality. See <see cref="Equals(Error)"/> for more info.
	/// </summary>
	[Pure]
	[MethodImpl(MethodImplOptions.AggressiveInlining)]
	public static bool operator !=(Error left, Error right) => !left.Equals(right);

	private abstract class SpecialError
	{
		internal abstract void SerializeInto(StringBuilder output);
	}

	private sealed class FatError : SpecialError, IError
	{
#pragma warning disable CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).
		public required string? Message { get; init; }
#pragma warning restore CS8766 // Nullability of reference types in return type doesn't match implicitly implemented member (possibly because of nullability attributes).

		public required object? Data { get; init; }

		public required Error? InnerError { get; init; }

		internal override void SerializeInto(StringBuilder output)
		{
			output.Append(MessagePrefix);
			output.Append(this.Message ?? DefaultErrorMessage);

			if (this.Data is not null)
			{
				output.AppendLine();
				output.Append("Data: ");
				output.Append(DataToString(this.Data));
			}
		}
	}

	private sealed class EnumError<TEnum>(TEnum value, bool isDefined) : SpecialError, IError
		where TEnum : struct, Enum
	{
		private static readonly bool IsFlags = Attribute.IsDefined(typeof(TEnum), typeof(FlagsAttribute), inherit: false);

		// Beware that these may be accessed from multiple threads:
		private string? cachedMessage;
		private object? cachedDataObject;

		public string Message => this.cachedMessage ??= this.GetCustomMessage() ?? DefaultErrorMessage;

		public Error? InnerError => null;

		public object? Data => this.cachedDataObject ??= (object)value;

		private string? GetCustomMessage()
		{
			if (!isDefined)
			{
				return null;
			}

			var name = GetEnumName(value);
			if (name is null)
			{
				return null;
			}

			var field = typeof(TEnum).GetField(name);
			if (field is null)
			{
				return null;
			}

			var attribute = (ErrorMessageAttribute?)Attribute.GetCustomAttribute(field, typeof(ErrorMessageAttribute), inherit: false);
			if (attribute is null)
			{
				return null;
			}

			return attribute.Message;
		}

		internal override void SerializeInto(StringBuilder output)
		{
			var name = GetEnumName(value);
			var useSimpleFormat = name is not null && isDefined && !IsFlags;

			output.Append(typeof(TEnum).ToString());

			if (useSimpleFormat)
			{
				output.Append('.');
				output.Append(name);
			}

			output.Append(": ");
			output.Append(this.Message);

			if (!useSimpleFormat)
			{
				output.AppendLine();
				output.Append("Data: ");
				output.Append(DataToString(value));
			}
		}

		internal abstract class Lookup
		{
			internal static readonly Lookup Instance = CreateInstance();

			private static Lookup CreateInstance()
			{
				var values = GetEnumValues();
				var underlyingType = Enum.GetUnderlyingType(typeof(TEnum));

				// If the enum is backed by an `int` and all its values are
				// laid out sequentially starting at 0 (which is the default),
				// we can use a more efficient array-based lookup.
				// Benchmarking shows that this is approximately 20x faster than
				// the dictionary-based lookup.
				if (underlyingType == typeof(int) && Int32ArrayLookup.TryCreate(values) is { } int32Lookup)
				{
					return int32Lookup;
				}

				return DictionaryLookup.Create(values);
			}

			internal abstract IError GetError(TEnum value);

			[MethodImpl(MethodImplOptions.NoInlining)]
			private static EnumError<TEnum> GetErrorSlow(TEnum value) => new(value, isDefined: false);

			private sealed class Int32ArrayLookup(IError[] declaredErrors) : Lookup
			{
				internal static Int32ArrayLookup? TryCreate(TEnum[] values)
				{
					var errors = new IError[values.Length];

					for (int i = 0; i < values.Length; i++)
					{
						var value = values[i];

						if (GetInt32Value(value) != i)
						{
							return null;
						}

						errors[i] = new EnumError<TEnum>(value, isDefined: true);
					}

					return new Int32ArrayLookup(errors);
				}

				internal override IError GetError(TEnum value)
				{
					var intValue = GetInt32Value(value);

					if ((uint)intValue < (uint)declaredErrors.Length)
					{
						return declaredErrors[intValue];
					}

					return GetErrorSlow(value);
				}

				[MethodImpl(MethodImplOptions.AggressiveInlining)]
				private static int GetInt32Value(TEnum value)
				{
					Debug.Assert(Enum.GetUnderlyingType(typeof(TEnum)) == typeof(int));

					return Unsafe.As<TEnum, int>(ref value);
				}
			}

			private sealed class DictionaryLookup(Dictionary<TEnum, IError> declaredErrors) : Lookup
			{
				internal static DictionaryLookup Create(TEnum[] values)
				{
					var errors = new Dictionary<TEnum, IError>(capacity: values.Length + 1);

					// Always add the default value. This may be overwritten within the loop.
					errors[default] = new EnumError<TEnum>(default, isDefined: false);

					foreach (var value in values)
					{
						errors[value] = new EnumError<TEnum>(value, isDefined: true);
					}

					return new DictionaryLookup(errors);
				}

				internal override IError GetError(TEnum value)
				{
					if (declaredErrors.TryGetValue(value, out var existingError))
					{
						return existingError;
					}

					return GetErrorSlow(value);
				}
			}
		}

		private static string? GetEnumName(TEnum value)
		{
#if NET5_0_OR_GREATER
			return Enum.GetName<TEnum>(value);
#else
			return Enum.GetName(typeof(TEnum), value);
#endif
		}

		private static TEnum[] GetEnumValues()
		{
#if NET5_0_OR_GREATER
			return Enum.GetValues<TEnum>();
#else
			return (TEnum[])Enum.GetValues(typeof(TEnum));
#endif
		}
	}

	/// <summary>
	/// Get the special "success" marker instance.
	/// </summary>
	/// <remarks>
	/// This is used internally by Result`1 to represent successful results,
	/// preventing the need for an additional `isSuccess` boolean field.
	/// </remarks>
	internal static Error Success
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => new(SuccessMarker.Singleton);
	}

	/// <summary>
	/// Is this the special "success" marker instance?.
	/// </summary>
	internal bool IsSuccess
	{
		[MethodImpl(MethodImplOptions.AggressiveInlining)]
		get => ReferenceEquals(this.obj, SuccessMarker.Singleton);
	}

	/// <summary>
	/// Instances of this error type should behave exactly the same as the
	/// `default` Error value as instances of this error type could be exposed
	/// through e.g. `result.TryGetError(out var error)` or
	/// `Result.GetErrorRefOrDefaultRef`, etc.
	/// </summary>
	private sealed class SuccessMarker : SpecialError, IError
	{
		internal static readonly IError Singleton = new SuccessMarker();

		/// <inheritdoc/>
		public string Message => DefaultErrorMessage;

		/// <inheritdoc/>
		public object? Data => null;

		/// <inheritdoc/>
		public Error? InnerError => null;

		internal override void SerializeInto(StringBuilder output)
		{
			output.Append(DefaultErrorToString);
		}
	}
}
