using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the marked method declares routing convention for ASP.NET.
/// </summary>
/// <remarks>
/// The IDE will analyze all usages of methods marked with this attribute,
/// and will add all routes to completion, navigation, and other features over URI strings.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class AspRouteConventionAttribute : Attribute
{
    public AspRouteConventionAttribute() { }

    public AspRouteConventionAttribute(string predefinedPattern)
    {
        PredefinedPattern = predefinedPattern;
    }

    [CanBeNull] public string PredefinedPattern { get; }
}
