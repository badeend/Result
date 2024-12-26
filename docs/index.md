<p align="center">
  <img src="./images/logo.png" alt="Result" width="300"/>
</p>

# Introduction

This packages provides a `Result<TValue, TError>` type for C#, spiritually similar to those available in [Rust](https://doc.rust-lang.org/std/result/enum.Result.html), [Swift](https://developer.apple.com/documentation/swift/result), [Kotlin](https://kotlinlang.org/api/latest/jvm/stdlib/kotlin/-result/), [C++](https://en.cppreference.com/w/cpp/utility/expected) and basically every functional programming language under the sun.

Results are commonly used in scenarios where failure is anticipated can be handled gracefully by the caller. Examples include:
- Input validation,
- Parsing and conversion,
- Invocation of external services,
- Authentication and authorization,
- and more ...

`Result<TValue, TError>` represents the result of a fallible operation as a first class value. A result can be in one of two states: "success" or "error". Both states have an associated payload of type `TValue` or `TError` respectively. If an operation has exactly one failure mode and/or you don't care about the strongly typed error data, you can also use the `Result<TValue>` shorthand.

## Installation

[![NuGet Badeend.Result](https://img.shields.io/nuget/v/Badeend.Result?label=Badeend.Result)](https://www.nuget.org/packages/Badeend.Result)

```sh
dotnet add package Badeend.Result
```

[Full API reference](https://badeend.github.io/Result/api/Badeend.html)

## Generic results

#### Create Result

```cs
public enum SignInError // Errors can be any type you want. For this example I chose a simple enum.
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
        return SignInError.InvalidCredentials; // Error is implicitly wrapped with `Result.Error(...)`
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
        ResultState.Error => BadRequest(result.Error),
    };


    // Or alternatively, but more verbose:

    if (result.IsSuccess)
    {
        return Ok(result.Value);
    }
    else
    {
        return BadRequest(result.Error);
    }
}
```

## Basic results

Let's say we're building an e-commerce website. Each product page contain a "Recommended for you" section. The data in this section comes from an external Recommendations microservice. Because the data is requested over a network, we must take into consideration that the external request may fail. Yet, we want the product page to remain operational, even in the presence of failure in the recommendations service. In this case we'd conclude that failures are "expected" and should be handled gracefully by the caller:

```cs
public interface IRecommendationsService
{
    /// <summary>
    /// Fetch a personalized recommendations feed based on the user's past
    /// interests and the product they're currently looking at.
    /// </summary>
    Task<Result<List<Recommendation>>> Fetch(int userId, int productId); // <--- Notice the return type.
}
```

As you can see, we've wrapped the recommendations list inside a `Result` to codify the fallibility of this operation. Also, we opted-out of the strongly typed error payload and used the [`Result<T>`](xref:Badeend.Result`1) shorthand instead for two reasons:
- External I/O can fail for a myriad of reasons. Cataloging all the different ways a network request might fail and codifying that in the type system is nigh impossible.
- All that the product page needs to know is whether the operation failed or not and log the error if it did:

```cs
public class ProductPage(IRecommendationsService recommendationsService, ILogger<ProductPage> logger) : PageModel
{
    public async Task OnGet(int productId)
    {
        // (... some code here ...)
        this.Recommendations = await GetRecommendations(productId);
        // (... more code here ...)
    }

    private async Task<List<Recommendation>> GetRecommendations(int productId)
    {
        var result = await recommendationsService.Fetch(this.User.Id, productId);
        if (result.IsError)
        {
            logger.LogInformation("Failed to load recommendations: {ErrorDetails}", result.Error.ToString());
            return new List<Recommendation>();
        }

        return result.Value;
    }
}
```

## Which Result should I use?

This package comes with two `Result` types:
- The fully generic [`Result<TValue, TError>`](xref:Badeend.Result`2): The `TError` type can be anything that describes the failure; an `enum`, a `record`, a list of validation messages, etc... Let your imagination run wild. As long as it contains enough information for callers of your method to take meaningful action.
- The shorthand "basic" [`Result<T>`](xref:Badeend.Result`1): This is essentially an alias for `Result<T, Badeend.Error>`. (See: [`Badeend.Error`](xref:Badeend.Error)). Though for all intents and purposes it should be treated as `Result<T, void>` in that: all that the domain logic should care about is whether the operation failed or not. The Error payload is just a way to _optionally_ carry developer-oriented debug information.

An example of this choice can be seen in practice in the [CollectionExtensions](xref:Badeend.Results.Extensions.CollectionExtensions):
- [`TryFirst`](xref:Badeend.Results.Extensions.CollectionExtensions.TryFirst``1(System.Collections.Generic.IEnumerable{``0})) has only one failure mode: the collection being empty. Therefore it returns: `Result<T>`.
- [`TrySingle`](xref:Badeend.Results.Extensions.CollectionExtensions.TrySingle``1(System.Collections.Generic.IEnumerable{``0})) has two distinct failure modes: the collection being empty, and: the collection containing more than one element. Therefore it returns: `Result<T, TrySingleError>`. If the caller does not care about this distinction, they may simply ignore it or convert the result to shorthand form using [`.AsBasicResult()`](xref:Badeend.Results.Extensions.ResultExtensions.AsBasicResult``1(Badeend.Result{``0,Badeend.Error}))

## When should I use Results?

Of course you're free to do whatever you want, but these guidelines have helped me so far:

First of all: Results are not a general purpose replacement for exceptions. Keep using exceptions! Exceptions great for fatal errors, bugs, guard clauses and other non-recoverable errors. Results don't collect stack traces _by design_.

You can use Results when designing fallible methods where:
- failures are part of the domain model and should therefore be part of the regular control flow. And/or:
- the implementation is not in the position to decide whether failures are exceptional or not and you want to leave that up to the caller.

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

Checked exceptions get a bad reputation because they're implemented in only one mainstream language: Java. And Java's implementation of them has turned out to be horrible in practice. In Java, all exceptions are "checked" by default, which means that _every exception_ must be explicitly marked for propagation in _every method_. This makes for an awfully laborious developer experience.

However, concluding that "all checked exceptions must therefore be bad" would be throwing the baby out with the bathwater. Checked exception are still useful: _in moderation_. Java just got their defaults wrong.

Using C# exceptions complemented with Results is the best of both worlds; by default you're never forced to unnecessarily check for errors, except for the few places where you explicitly opted-in to that (by using Result).

## Why does this package exist?

While there are many similar packages available, this one is designed to address specific needs that others did not fully meet:

- **No opinion on what is allowed to be an error.** The error type (`TError`) is parameterized without constraints.
- **Focus on simplicity.** This package is designed to provide just what's needed without introducing an extensive Functional Programming framework. It's about enhancing your existing C# code without overwhelming it with additional concepts.
- **For C# developers.** The goal is to make it feel "native" to the language, designed with C# conventions in mind, and avoiding a paradigm shift in how C# code is written.

<details>
  <summary>Considered alternatives</summary>

  - [CSharpFunctionalExtensions](https://www.nuget.org/packages/CSharpFunctionalExtensions)
  - [LanguageExt.Core](https://www.nuget.org/packages/LanguageExt.Core)
  - [FluentResults](https://www.nuget.org/packages/FluentResults)
  - [Ardalis.Result](https://www.nuget.org/packages/Ardalis.Result)
  - [DotNext](https://www.nuget.org/packages/DotNext)
  - [ErrorOr](https://www.nuget.org/packages/ErrorOr)
  - [DotNetCore.Results](https://www.nuget.org/packages/DotNetCore.Results)
  - [SuccincT](https://www.nuget.org/packages/SuccincT)
  - [Remora.Results](https://www.nuget.org/packages/Remora.Results)
  - [ResultSharp](https://www.nuget.org/packages/ResultSharp)
  - [Feree.ResultType](https://www.nuget.org/packages/Feree.ResultType)
  - [CSharp-Result](https://www.nuget.org/packages/CSharp-Result)
  - [ResultType](https://www.nuget.org/packages/ResultType)
  - [OperationResult.Net](https://www.nuget.org/packages/OperationResult.Net)
  - [Orx.Fun.Result](https://www.nuget.org/packages/Orx.Fun.Result)
  - [Ergo.Result](https://www.nuget.org/packages/Ergo.Result)
</details>