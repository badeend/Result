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

---

**Documentation & more information at: https://badeend.github.io/Result/**

---

#### Why does this package exist?

While there are many similar packages available, this one is designed to address specific needs that others did not fully meet:

- **No opinion on what is allowed to be an error.** The error type (`TError`) is parameterized without constraints.
- **Focus on simplicity.** This package is designed to provide just what's needed without introducing an extensive Functional Programming framework. It's about enhancing your existing C# code without overwhelming it with additional concepts.
- **For C# developers.** The goal is to make it feel "native" to the language, designed with C# conventions in mind, and avoiding a paradigm shift in how C# code is written.

---

### Shameless self-promotion

May I interest you in one of my other packages?

- **[Badeend.ValueCollections](https://badeend.github.io/ValueCollections/)**: _Low overhead immutable collection types with structural equality._
- **[Badeend.EnumClass](https://badeend.github.io/EnumClass/)**: _Discriminated unions for C# with exhaustiveness checking._
- **[Badeend.Result](https://badeend.github.io/Result/)**: _For failures that are not exceptional: `Result<T,E>` for C#._
- **[Badeend.Any](https://badeend.github.io/Any/)**: _Holds any value of any type, without boxing small structs (up to 8 bytes)._
- **[Badeend.Nothing](https://github.com/badeend/Nothing)**: _If you want to use `void` as a type parameter, but C# won't let you._