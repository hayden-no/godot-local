using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
internal sealed class AspMvcAreaViewComponentViewLocationFormatAttribute : Attribute
{
    public AspMvcAreaViewComponentViewLocationFormatAttribute([NotNull] string format)
    {
        Format = format;
    }

    [NotNull] public string Format { get; }
}
