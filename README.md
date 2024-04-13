<p align="center">
  <img src="./docs/images/logo.png" alt="Result" width="300"/>
</p>

<p align="center">
  <em>For failures that are not exceptional.</em>
</p>

<p align="center">
  <a href="https://www.nuget.org/packages/Badeend.Result"><img src="https://img.shields.io/nuget/v/Badeend.Result" alt="Nuget"/></a>
</p>

---

This packages provides a `Result<TValue, TError>` type for C#, spiritually similar to those available in [Rust](https://doc.rust-lang.org/std/result/enum.Result.html), [Swift](https://developer.apple.com/documentation/swift/result), [Kotlin](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin/-result/), [C++](https://en.cppreference.com/w/cpp/utility/expected) and basically every functional programming language under the sun.

Results are commonly used in scenarios where failure is anticipated can be handled gracefully by the caller. Examples include:
- Input validation,
- Parsing and conversion,
- Invocation of external services,
- Authentication and authorization,
- and more ...

`Result<TValue, TError>` represents the result of a fallible operation as a first class value. A result can be in one of two states: "success" or "error". Both states have an associated payload of type `TValue` or `TError` respectively.

Documentation & more information at: https://badeend.github.io/Result/

---

#### Why does this package exist?

There are already dozens of similar packages. Yet, surprisingly, none of them provided what I had in mind:

- `LanguageExt.Core`, `FluentResults`, `Ardalis.Result`, `DotNext`, `ErrorOr`, `DotNetCore.Results`, `Feree.ResultType`, `CSharp-Result`, `ResultType`, `OperationResult.Net`, `Orx.Fun.Result`, `Ergo.Result`:
    - These all have a hardcoded error type. IMO, this completely obviates the reason to use a result type _in C#_. I want the error type to be parameterized (`TError`) without constraints.
- `CSharpFunctionalExtensions.Result<T, E>`:
    - It comes bundled as part of an entire Functional Programming framework, which is not what I'm looking for.
    - (Nitpick) Its `default` value is a Successful result
    - (Nitpick) It does not implement `IEquatable<..>` or equality operators.
- `ResultSharp.Result<T, E>`:
    - Seems like a 1:1 port from Rust. It doesn't feel very C#-like.

All in all, I want a Result type that is written for __C#__ developers & codebases (not F#, iykwim 😇). My guiding principle when designing this was: "If such a type were to be added to the BCL, how would Microsoft design it?"
