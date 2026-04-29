using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the marked parameter is a message template where placeholders are to be replaced by
/// the following arguments in the order in which they appear.
/// </summary>
/// <example><code>
/// void LogInfo([StructuredMessageTemplate]string message, params object[] args) { ... }
///
/// void Foo()
/// {
///   LogInfo("User created: {username}"); // Warning: Non-existing argument in format string
/// }
/// </code></example>
/// <seealso cref="StringFormatMethodAttribute"/>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class StructuredMessageTemplateAttribute : Attribute { }