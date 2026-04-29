using System;

namespace JetBrains.Annotations;

/// <summary>
/// This attribute is intended to mark publicly available APIs
/// that should not be removed and therefore should never be reported as unused.
/// </summary>
[MeansImplicitUse(ImplicitUseTargetFlags.WithMembers)]
[AttributeUsage(AttributeTargets.All, Inherited = false)]
internal sealed class PublicAPIAttribute : Attribute
{
    public PublicAPIAttribute() { }

    public PublicAPIAttribute([NotNull] string comment)
    {
        Comment = comment;
    }

    [CanBeNull] public string Comment { get; }
}
