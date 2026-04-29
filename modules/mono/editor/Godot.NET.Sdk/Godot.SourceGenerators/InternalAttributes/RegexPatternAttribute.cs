using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the marked parameter, field, or property is a regular expression pattern.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
internal sealed class RegexPatternAttribute : Attribute { }
