using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the marked attribute is used for attribute routing in ASP.NET.
/// </summary>
/// <remarks>
/// The IDE will analyze all usages of attributes marked with this attribute,
/// and will add all routes to completion, navigation and other features over URI strings.
/// </remarks>
[AttributeUsage(AttributeTargets.Class)]
internal sealed class AspAttributeRoutingAttribute : Attribute
{
    public string HttpVerb { get; set; }
}
