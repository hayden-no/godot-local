using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
internal sealed class AspChildControlTypeAttribute : Attribute
{
    public AspChildControlTypeAttribute([NotNull] string tagName, [NotNull] Type controlType)
    {
        TagName = tagName;
        ControlType = controlType;
    }

    [NotNull] public string TagName { get; }

    [NotNull] public Type ControlType { get; }
}
