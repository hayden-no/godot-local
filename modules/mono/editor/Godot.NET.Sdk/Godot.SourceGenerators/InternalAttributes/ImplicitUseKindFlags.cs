using System;

namespace JetBrains.Annotations;

/// <summary>
/// Specifies the details of an implicitly used symbol when it is marked
/// with <see cref="MeansImplicitUseAttribute"/> or <see cref="UsedImplicitlyAttribute"/>.
/// </summary>
[Flags]
internal enum ImplicitUseKindFlags
{
    Default = Access | Assign | InstantiatedWithFixedConstructorSignature,

    /// <summary>Only entity marked with attribute considered used.</summary>
    Access = 1,

    /// <summary>Indicates implicit assignment to a member.</summary>
    Assign = 2,

    /// <summary>
    /// Indicates implicit instantiation of a type with a fixed constructor signature.
    /// That means any unused constructor parameters will not be reported as such.
    /// </summary>
    InstantiatedWithFixedConstructorSignature = 4,

    /// <summary>Indicates implicit instantiation of a type.</summary>
    InstantiatedNoFixedConstructorSignature = 8,
}
