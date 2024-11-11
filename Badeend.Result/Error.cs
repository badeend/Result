namespace Badeend;

public readonly record struct Error(string Message)
{
	/// <inheritdoc/>
	public override string ToString() => this.Message;
}
