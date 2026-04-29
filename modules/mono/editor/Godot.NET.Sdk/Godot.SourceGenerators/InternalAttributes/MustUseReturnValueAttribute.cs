using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the return value of the method invocation must be used.
/// </summary>
/// <remarks>
/// Methods decorated with this attribute (in contrast to pure methods) might change state,
/// but make no sense without using their return value. <br/>
/// Similarly to <see cref="PureAttribute"/>, this attribute
/// will help detect usages of the method when the return value is not used.
/// Optionally, you can specify a message to use when showing warnings, e.g.
/// <code>[MustUseReturnValue("Use the return value to...")]</code>.
/// </remarks>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class MustUseReturnValueAttribute : Attribute
{
    public MustUseReturnValueAttribute() { }

    public MustUseReturnValueAttribute([NotNull] string justification)
    {
        Justification = justification;
    }

    [CanBeNull] public string Justification { get; }

    /// <summary>
    /// Enables the special handling of the "fluent" APIs that perform mutations and return 'this' object.
    /// In this case the analysis checks the fluent invocations chain and only warns if the initial receiver value
    /// is probably a temporary value - in this case the very last fluent method return assumed to be temporary as well,
    /// therefore is a subject of warning if unused. If the initial receiver is a local variable or 'this' reference
    /// the analysis assumes that fluent invocations were used to mutate the existing value and warning will not be shown.
    /// </summary>
    /// <remarks>
    /// This property must only be used for methods with the return type matching the receiver type.
    /// </remarks>
    public bool IsFluentBuilderMethod { get; set; }
}
