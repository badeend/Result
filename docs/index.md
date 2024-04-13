<p align="center">
  <img src="./images/logo.png" alt="Result" width="300"/>
</p>

# Introduction

This packages provides a `Result<TValue, TFailure>` type for C#, spiritually similar to those available in [Rust](https://doc.rust-lang.org/std/result/enum.Result.html), [Swift](https://developer.apple.com/documentation/swift/result), [Kotlin](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin/-result/), [C++](https://en.cppreference.com/w/cpp/utility/expected) and basically every functional programming language under the sun.

Results are commonly used in scenarios where failure is anticipated can be handled gracefully by the caller. Examples include:
- Input validation,
- Parsing and conversion,
- Invocation of external services,
- Authentication and authorization,
- and more ...

`Result<TValue, TFailure>` represents the result of a fallible operation as a first class value. A result can be in one of two states: "success" or "failure". Both states have an associated payload of type `TValue` or `TFailure` respectively.

## Installation

[![NuGet Badeend.Result](https://img.shields.io/nuget/v/Badeend.Result?label=Badeend.Result)](https://www.nuget.org/packages/Badeend.Result)

```sh
dotnet add package Badeend.Result
```

[Full API reference](https://badeend.github.io/Result/api/Badeend.html)

## Basic example

#### Create Result

```cs
public enum SignInError // Failures can be any type you want. For this example I chose a simple enum.
{
    InvalidCredentials,
    LockedOut,
}

/// <summary>
/// Attempt to log the user in and return the newly created user session.
/// </summary>
public Result<Session, SignInError> SignIn(string email, string password) // <--- Notice the return type.
{
    var user = FindUserByEmail(email);
    if (user is null || !user.VerifyPassword(password))
    {
        return SignInError.InvalidCredentials; // Error is implicitly wrapped with `Result.Failure(...)`
    }

    if (user.IsLockedOut)
    {
        return SignInError.LockedOut;
    }

    return user.CreateNewSession(); // Return value is implicitly wrapped with `Result.Success(...)`
}
```

#### Check Result

```cs
public async Task<ActionResult<Session>> PostSignIn(SignInRequest request)
{
    var result = SignIn(request.Email, request.Password); // The SignIn method from above.

    return result.State switch // Tip!: enable CS8509 & disable CS8524 for exhaustiveness checking.
    {
        ResultState.Success => Ok(result.Value),
        ResultState.Failure => BadRequest(result.Failure),
    };


    // Or alternatively, but more verbose:

    if (result.IsSuccess)
    {
        return Ok(result.Value);
    }
    else
    {
        return BadRequest(result.Failure);
    }
}
```

## When should you use Results?

You can use Results when designing fallible methods where:
- failures are part of the domain model and should therefore be part of the regular control flow. And/or:
- the implementation is not in the position to decide whether failures are exceptional or not and you want to leave that up to the caller.

#### Choosing a failure type

It can be anything that describes the failure; an `enum`, a `record`, a list of validation messages, etc... Let your mind run free.

Keep in mind that the failure value should contain enough information for callers of your method to do something meaningful with it. If the caller has no other option than to propagate it up the callstack (recursively), then you might as well throw an exception.

One noteworthy case to watch out for is: `Result<T, Exception>`. There may be legitimate use cases for it, but it smells like it's trying to reinvent exception handling.

#### Results vs. exceptions

Results are not a general purpose replacement for exceptions. Keep using exceptions! Exceptions great for fatal errors, bugs, guard clauses and other non-recoverable errors. That being said, Results _may_ be a suitable replacement for exceptions if you're currently catching exceptions for non-local control flow.

Results don't collect stack traces _by design_.

Ultimately, Results and Exceptions both have their pros and cons, and the choice depends on factors such as the specific requirements of your application, the level of robustness needed, and personal or team preferences.

#### Results vs. the `bool Try***(out T t)` pattern

Results are a generalization of the `bool Try***(out T t)` pattern. The Try pattern still works fine as long as the method:
- has exactly one failure mode, and:
- is not `async`. (`out` parameters don't work on async methods.)

Something to keep in mind when using the Try pattern: you are expected to provide a non-Try variant as well that throws the error instead of returning it.

The advantages of Results over Try methods:
- Works with `async` methods.
- Works with any number/kind of failures. (Technically you could use multiple `out` parameters for the additional error data, but that is [frowned upon](https://learn.microsoft.com/en-us/dotnet/fundamentals/code-analysis/quality-rules/ca1021#how-to-fix-violations:~:text=If%20the%20method%20must%20return%20multiple%20values%2C%20redesign%20it%20to%20return%20a%20single%20instance%20of%20an%20object%20that%20holds%20the%20values))
- No API duplication. You only have to expose one Result-returning method. The caller decides whether failures should throw or not.

#### Results vs. nullables

The cookie cutter answer is: Nullables are about optionality, Results are about fallability.

However, especially when a method has only one failure mode, the distinction can become blurry. Consider the following snippet:

```cs
class MetaData
{
    ??? GetValueByKey(string key);
}
```

What happens when the `key` is not present? It depends on whether the caller is expecting the key to exist or not;
- If the key is expected to exist, a non-existent key should be an error.
- If the key isn't required to exist, a Nullable return type could suffice. Keep in mind that you then loose the distinction between: the key doesn't exist, and: the key exists but its value is null.

FYI, even the BCL isn't consistent in this regard. E.g.:
- [`Dictionary<TKey, TValue>`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.generic.dictionary-2.item?view=net-8.0) fails on non-existing keys.
- [`NameValueCollection`](https://learn.microsoft.com/en-us/dotnet/api/system.collections.specialized.namevaluecollection.item?view=net-8.0#system-collections-specialized-namevaluecollection-item(system-string)) returns `null` for non-existing keys.

#### Are Results effectively Java's "checked exceptions"?

Yes! and: No!

Checked exceptions get a bad rep because they're implemented in only one mainstream language: Java. And Java's implementation of them has turned out to be horrible in practice. In Java, all exceptions are "checked" by default, which means that _every exception_ must be explicitly marked for propagation in _every method_. This makes for an awfully laborious developer experience.

However, concluding that "all checked exceptions must therefore be bad" would be throwing the baby out with the bathwater. Checked exception are still useful: _in moderation_. Java just got their defaults wrong.

Using C# exceptions complemented with Results is the best of both worlds; by default you're never forced to unnecessarily check for errors, except for the few places where you explicitly opted-in to that (by using Result).

## Why does this package exist?

There are already dozens of similar packages. Yet, surprisingly, none of them provided what I'm looking for:

- **No opinion on what is allowed to be a failure.** In other words: I want the failure type to be parameterized (`TFailure`) without constraints. IMO, hardcoding the failure type to e.g. `Exception` or `string` completely defeats the purpose of using a result type _in C#_.

- **Just Result, nothing else.** I'm not interested in a complete Functional Programming framework that introduces 20-or-so of new concepts, pushes all code into lambdas and attempts to redefine what it means to write C#. Speaking from experience, you'll just end up fighting the language every step of the way.

- **"Native" C#.** It should feel as if it is written _by_ C# developers, _for_ C# developers, for use in (existing) C# codebases. Or put differently: if such a type were to be added to the BCL, how would Microsoft design it?

<details>
  <summary>Considered alternatives</summary>

  - [CSharpFunctionalExtensions](https://www.nuget.org/packages/CSharpFunctionalExtensions)
  - [LanguageExt.Core](https://www.nuget.org/packages/LanguageExt.Core)
  - [FluentResults](https://www.nuget.org/packages/FluentResults)
  - [Ardalis.Result](https://www.nuget.org/packages/Ardalis.Result)
  - [DotNext](https://www.nuget.org/packages/DotNext)
  - [ErrorOr](https://www.nuget.org/packages/ErrorOr)
  - [DotNetCore.Results](https://www.nuget.org/packages/DotNetCore.Results)
  - [ResultSharp](https://www.nuget.org/packages/ResultSharp)
  - [Feree.ResultType](https://www.nuget.org/packages/Feree.ResultType)
  - [CSharp-Result](https://www.nuget.org/packages/CSharp-Result)
  - [ResultType](https://www.nuget.org/packages/ResultType)
  - [OperationResult.Net](https://www.nuget.org/packages/OperationResult.Net)
  - [Orx.Fun.Result](https://www.nuget.org/packages/Orx.Fun.Result)
  - [Ergo.Result](https://www.nuget.org/packages/Ergo.Result)
</details>