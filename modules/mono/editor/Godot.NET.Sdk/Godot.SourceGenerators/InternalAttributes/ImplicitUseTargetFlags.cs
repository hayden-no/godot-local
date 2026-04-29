using System;

namespace JetBrains.Annotations;

/// <summary>
/// Specifies what is considered to be used implicitly when marked
/// with <see cref="MeansImplicitUseAttribute"/> or <see cref="UsedImplicitlyAttribute"/>.
/// </summary>
[Flags]
internal enum ImplicitUseTargetFlags
{
    Default = Itself,

    /// <summary>Code entity itself.</summary>
    Itself = 1,

    /// <summary>Members of the type marked with the attribute are considered used.</summary>
    Members = 2,

    /// <summary> Inherited entities are considered used. </summary>
    WithInheritors = 4,

    /// <summary>Entity marked with the attribute and all its members considered used.</summary>
    WithMembers = Itself | Members
}
