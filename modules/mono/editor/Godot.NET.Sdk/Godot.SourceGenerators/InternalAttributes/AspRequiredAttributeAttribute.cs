using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal sealed class AspRequiredAttributeAttribute : Attribute
{
    public AspRequiredAttributeAttribute([NotNull] string attribute)
    {
        Attribute = attribute;
    }

    [NotNull] public string Attribute { get; }
}
