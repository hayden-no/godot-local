using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the string literal passed as an argument to this parameter
/// should not be checked for spelling or grammar errors.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class IgnoreSpellingAndGrammarErrorsAttribute : Attribute { }
