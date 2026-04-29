using System;

namespace JetBrains.Annotations;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true)]
internal sealed class RazorDirectiveAttribute : Attribute
{
    public RazorDirectiveAttribute([NotNull] string directive)
    {
        Directive = directive;
    }

    [NotNull] public string Directive { get; }
}
