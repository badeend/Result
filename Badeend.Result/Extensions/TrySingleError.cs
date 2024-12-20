using Badeend.Errors;

namespace Badeend.Extensions;

/// <summary>
/// Error returned by <see cref="CollectionExtensions.TrySingle{T}(IEnumerable{T})"/>.
/// </summary>
public enum TrySingleError
{
	/// <summary>
	/// The sequence did not produce any elements.
	/// </summary>
	[ErrorMessage("Sequence is empty.")]
	NoElements,

	/// <summary>
	/// The sequence produced more than one element.
	/// </summary>
	[ErrorMessage("Sequence contains more than one element.")]
	MoreThanOneElement,
}
