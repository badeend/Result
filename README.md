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

This packages provides a `Result<TValue, TFailure>` type for C#, spiritually similar to those available in [Rust](https://doc.rust-lang.org/std/result/enum.Result.html), [Swift](https://developer.apple.com/documentation/swift/result), [Kotlin](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin/-result/), [C++](https://en.cppreference.com/w/cpp/utility/expected) and basically every functional programming language under the sun.

Results are commonly used in scenarios where failure is anticipated can be handled gracefully by the caller. Examples include:
- Input validation,
- Parsing and conversion,
- Invocation of external services,
- Authentication and authorization,
- and more ...

`Result<TValue, TFailure>` represents the result of a fallible operation as a first class value. A result can be in one of two states: "success" or "failure". Both states have an associated payload of type `TValue` or `TFailure` respectively.

---

**Documentation & more information at: https://badeend.github.io/Result/**

---

#### Why does this package exist?

There are already dozens of similar packages. Yet, surprisingly, none of them provide what I'm looking for:

- **No opinion on what is allowed to be a failure.** In other words: I want the failure type to be parameterized (`TFailure`) without constraints. IMO, hardcoding the failure type to e.g. `Exception` or `string` completely defeats the purpose of using a result type _in C#_.

- **Just Result, nothing else.** I'm not interested in a complete Functional Programming framework that introduces 20-or-so new concepts, pushes all code into lambdas and attempts to redefine what it means to write C#. Not because I don't like FP, but because the language & ecosystem (sadly 🥲) doesn't afford it.

- **"Native" C#.** It should feel as if it is written _by_ C# developers, _for_ C# developers, for use in (existing) C# codebases. Or put differently: if such a type were to be added to the BCL, how would Microsoft design it?

---

### Shameless self-promotion

May I interest you in one of my other packages?

- **[Badeend.ValueCollections](https://badeend.github.io/ValueCollections/)**: _Low overhead immutable collection types with structural equality._
- **[Badeend.EnumClass](https://badeend.github.io/EnumClass/)**: _Discriminated unions for C# with exhaustiveness checking._
- **[Badeend.Result](https://badeend.github.io/Result/)**: _For failures that are not exceptional: `Result<T,E>` for C#._
- **[Badeend.Any](https://badeend.github.io/Any/)**: _Holds any value of any type, without boxing small structs (up to 8 bytes)._
- **[Badeend.Nothing](https://github.com/badeend/Nothing)**: _If you want to use `void` as a type parameter, but C# won't let you._