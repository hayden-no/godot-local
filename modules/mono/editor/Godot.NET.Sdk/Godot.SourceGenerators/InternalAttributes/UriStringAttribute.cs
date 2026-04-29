using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the marked parameter, field, or property is a URI string.
/// </summary>
/// <remarks>
/// This attribute enables code completion, navigation, renaming, and other features
/// in URI string literals assigned to annotated parameters, fields, or properties.
/// </remarks>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
internal sealed class UriStringAttribute : Attribute
{
    public UriStringAttribute() { }

    public UriStringAttribute(string httpVerb)
    {
        HttpVerb = httpVerb;
    }

    [CanBeNull] public string HttpVerb { get; }
}
