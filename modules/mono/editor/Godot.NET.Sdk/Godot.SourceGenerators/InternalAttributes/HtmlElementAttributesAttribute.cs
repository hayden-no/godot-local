using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Property | AttributeTargets.Field)]
internal sealed class HtmlElementAttributesAttribute : Attribute
{
    public HtmlElementAttributesAttribute() { }

    public HtmlElementAttributesAttribute([NotNull] string name)
    {
        Name = name;
    }

    [CanBeNull] public string Name { get; }
}
