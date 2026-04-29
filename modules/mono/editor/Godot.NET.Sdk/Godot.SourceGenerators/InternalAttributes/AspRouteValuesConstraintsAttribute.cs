using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the marked method parameter contains constraints on route values of routing convention for ASP.NET.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class AspRouteValuesConstraintsAttribute : Attribute { }
