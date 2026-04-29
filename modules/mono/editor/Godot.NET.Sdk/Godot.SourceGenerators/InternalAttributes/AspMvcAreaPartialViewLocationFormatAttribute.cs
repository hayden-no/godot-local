using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Assembly | AttributeTargets.Field | AttributeTargets.Property, AllowMultiple = true)]
internal sealed class AspMvcAreaPartialViewLocationFormatAttribute : Attribute
{
    public AspMvcAreaPartialViewLocationFormatAttribute([NotNull] string format)
    {
        Format = format;
    }

    [NotNull] public string Format { get; }
}
