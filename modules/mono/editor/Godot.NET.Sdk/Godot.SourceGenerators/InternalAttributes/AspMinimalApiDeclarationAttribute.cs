using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the marked method declares an ASP.NET Minimal API endpoint.
/// </summary>
/// <remarks>
/// The IDE will analyze all usages of methods marked with this attribute,
/// and will add all routes to completion, navigation and other features over URI strings.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class AspMinimalApiDeclarationAttribute : Attribute
{
    public string HttpVerb { get; set; }
}
