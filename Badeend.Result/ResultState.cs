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
