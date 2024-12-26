A lightweight immutable error representation that contains:
- A human-readable error description. (`Message`)
- (Optional) A custom data payload, used to provide additional context beyond the message. (`Data`)
- (Optional) An inner error, used to build up a "chain" of context. (`InnerError`)

This Error type is designed to be a close relative to the built-in Exception class, but with a focus on being lightweight and suitable for situations where errors need to be reported frequently and/or where performance is critical.

This package is developed as part of [Badeend.Result](https://www.nuget.org/packages/Badeend.Result), though it can be used standalone as well.