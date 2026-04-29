using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the function argument should be a string literal and match
/// one of the parameters of the caller function. This annotation is used for parameters
/// like <c>string paramName</c> parameter of the <see cref="System.ArgumentNullException"/> constructor.
/// </summary>
/// <example><code>
/// void Foo(string param)
/// {
///   if (param == null)
///     throw new ArgumentNullException("par"); // Warning: Cannot resolve symbol
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class InvokerParameterNameAttribute : Attribute { }