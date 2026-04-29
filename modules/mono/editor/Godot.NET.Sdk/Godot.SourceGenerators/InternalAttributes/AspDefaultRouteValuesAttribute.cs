using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the marked method parameter contains default route values of routing convention for ASP.NET.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class AspDefaultRouteValuesAttribute : Attribute { }
