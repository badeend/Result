namespace Badeend.Errors;

/// <summary>
/// An custom error implementation that can be freely converted into an <see cref="Error"/>.
/// </summary>
public interface IError
{
	/// <summary>
	/// Human-readable description of the error.
	/// </summary>
	string Message { get; }

	/// <summary>
	/// The payload attached at this specific point in the error chain.
	/// </summary>
	/// <remarks>
	/// The default implementation returns <c>this</c>.
	/// </remarks>
	object? Data
	{
#if NETCOREAPP3_0_OR_GREATER
		get => this;
#else
		get;
#endif
	}

	/// <summary>
	/// Gets the inner error associated with the current <see cref="Error"/>
	/// instance, or <c>null</c> if this is a "root" error.
	/// </summary>
	/// <remarks>
	/// The default implementation returns <c>null</c>.
	/// </remarks>
	Error? InnerError
	{
#if NETCOREAPP3_0_OR_GREATER
		get => null;
#else
		get;
#endif
	}
}
