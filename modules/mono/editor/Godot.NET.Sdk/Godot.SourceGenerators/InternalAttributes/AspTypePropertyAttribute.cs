using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Property)]
internal sealed class AspTypePropertyAttribute : Attribute
{
    public bool CreateConstructorReferences { get; }

    public AspTypePropertyAttribute(bool createConstructorReferences)
    {
        CreateConstructorReferences = createConstructorReferences;
    }
}
