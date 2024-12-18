namespace Badeend.Errors;

/// <summary>
/// Attribute to customize the message of <c>enum</c> errors.
/// </summary>
/// <example>
/// <code>
/// public enum ParseError
/// {
///     [ErrorMessage("The value was too large to fit in the target type.")]
///     Overflow,
///
///     [ErrorMessage("The input string did not match the expected pattern.")]
///     InvalidSyntax,
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Field)]
public sealed class ErrorMessageAttribute : Attribute
{
	/// <summary>
	/// The error message.
	/// </summary>
	public string Message { get; }

	/// <inheritdoc cref="ErrorMessageAttribute"/>
	public ErrorMessageAttribute(string message)
	{
		this.Message = message ?? throw new ArgumentNullException(nameof(message));
	}
}
