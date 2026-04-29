using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the marked method contains Minimal API endpoint declaration.
/// </summary>
/// <remarks>
/// The IDE will analyze all usages of methods marked with this attribute,
/// and will add all declared in attributes routes to completion, navigation and other features over URI strings.
/// </remarks>
[AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
internal sealed class AspMinimalApiImplicitEndpointDeclarationAttribute : Attribute
{
    public string HttpVerb { get; set; }

    public string RouteTemplate { get; set; }

    public Type BodyType { get; set; }

    /// <summary>
    /// Comma-separated list of query parameters defined for endpoint
    /// </summary>
    public string QueryParameters { get; set; }
}
