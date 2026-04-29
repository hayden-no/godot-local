using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
internal sealed class RazorImportNamespaceAttribute : Attribute
{
    public RazorImportNamespaceAttribute([NotNull] string name)
    {
        Name = name;
    }

    [NotNull] public string Name { get; }
}
