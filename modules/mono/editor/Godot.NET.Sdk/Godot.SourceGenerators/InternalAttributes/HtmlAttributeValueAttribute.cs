using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
internal sealed class HtmlAttributeValueAttribute : Attribute
{
    public HtmlAttributeValueAttribute([NotNull] string name)
    {
        Name = name;
    }

    [NotNull] public string Name { get; }
}
