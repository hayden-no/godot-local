using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the marked parameter contains an ASP.NET Minimal API endpoint handler.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class AspMinimalApiHandlerAttribute : Attribute { }
