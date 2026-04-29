using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the marked method declares an ASP.NET Minimal API endpoints group.
/// </summary>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class AspMinimalApiGroupAttribute : Attribute { }
