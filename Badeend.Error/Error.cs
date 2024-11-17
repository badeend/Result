using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

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
/// The most commonly used constructor (<c><see cref="Error(string?)"/></c>) is
/// even allocation free.
///
/// This type has the size of a single machine word (4 or 8 bytes), making
/// it a good fit for applications where errors are treated as first-class
/// values, are copied frequently and/or are propagated through regular control
/// flow patterns instead of stack unwinding.
///
/// This type does not collect stack traces <i>by design</i>. Any additional
/// context that you want to attach along the way must be added manually by
/// wrapping it inside a new error using one of constructors that take an
/// <c>InnerError</c> parameter, e.g. <c><see cref="Error(string?,object?,Error)"/></c>.
///
/// The <c>default</c> Error contains only a predefined default message and is
/// equivalent to using the <see cref="Error()">parameterless constructor</see>.
/// </remarks>
[StructLayout(LayoutKind.Auto)]
public readonly struct Error : IEquatable<Error>
{
	private const string MessagePrefix = $"Error: ";
	private const string DefaultErrorMessage = "Operation did not complete successfully";
	private const string DefaultErrorToString = MessagePrefix + DefaultErrorMessage;
	private const string DefaultExceptionMessage = "An exception was thrown";

	/// <summary>
	/// Is one of the following:
	/// - `null`:      Empty error (`default`).
	/// - `string`:    Message-only error.
	/// - `Exception`: Error created through the special constructor. The exception instance acts as both `Data` and a `Message`. The InnerException is the InnerError.
	/// - `FatError`:  Any kind of error.
	/// </summary>
	private readonly object? obj;

	private sealed record FatError(
		string? Message,
		object? Data,
		Error? InnerError);

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
		FatError f => f.Message ?? DefaultErrorMessage,
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
		FatError f => f.Data,
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
	/// using e.g. <see cref="Error(string?, object?, Error)"/>, the pre-existing
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
		FatError f => f.InnerError,
		Exception { InnerException: { } innerException } => new Error(innerException),
		_ => null,
	};

	[Pure]
	internal string? StackTrace => this.obj switch
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
	public Error(string? message, Error innerError)
	{
		this.obj = new FatError(
			Message: message,
			Data: null,
			InnerError: innerError);
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
			this.obj = new FatError(
				Message: message,
				Data: null,
				InnerError: innerError);
		}
	}

	/// <summary>
	/// Create a new <see cref="Error"/> with the provided <paramref name="message"/>
	/// and/or <paramref name="data"/> that wraps the <paramref name="innerError"/>.
	/// </summary>
	[Pure]
	public Error(string? message, object? data, Error innerError)
	{
		this.obj = new FatError(
			Message: message,
			Data: data,
			InnerError: innerError);
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
			this.obj = new FatError(
				Message: message,
				Data: data,
				InnerError: innerError);
		}
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
			if (d is T match)
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
	/// The return value should therefore be considered as "already thrown" and
	/// should not be thrown directly again. The only supported method for
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
		else
		{
			result.Append(MessagePrefix);
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
		try
		{
			return data?.ToString() ?? "null";
		}
		catch (Exception)
		{
			return "<<.ToString() threw an exception>>";
		}
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
	public override bool Equals(object? other)
	{
		return other is Error otherError && this.Equals(otherError);
	}
#pragma warning restore CS0809 // Obsolete member overrides non-obsolete member

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
}
