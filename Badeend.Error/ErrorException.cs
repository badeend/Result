#pragma warning disable SA1649 // File name should match first type name
#pragma warning disable CA1032 // Implement standard exception constructors
#pragma warning disable CA1064 // Exceptions should be public

global using ErrorException = global::Error;

// To play nice with Exception.ToString()'s default implementation, this type deliberately:
// - is not located within any namespace
// - does not have the ***Exception suffix
internal sealed class Error : Exception
{
	internal Badeend.Error ErrorValue { get; }

	internal Error(Badeend.Error error, Exception? innerException)
		: base(null, innerException)
	{
		this.ErrorValue = error;
	}

	public override string Message
	{
		get
		{
			if (this.ErrorValue.Data is { } data)
			{
				return $"{this.ErrorValue.Message}{Environment.NewLine}Data: {Badeend.Error.DataToString(data)}";
			}
			else
			{
				return this.ErrorValue.Message;
			}
		}
	}

	public override string? StackTrace => this.ErrorValue.StackTrace;
}
