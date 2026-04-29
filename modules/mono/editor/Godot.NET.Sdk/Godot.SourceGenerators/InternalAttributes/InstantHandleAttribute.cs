using System;

namespace JetBrains.Annotations;

/// <summary>
/// Tells the code analysis engine if the parameter is completely handled when the invoked method is on stack.
/// If the parameter is a delegate, indicates that the delegate can only be invoked during method execution
/// (the delegate can be invoked zero or multiple times, but not stored to some field and invoked later,
/// when the containing method is no longer on the execution stack).
/// If the parameter is an enumerable, indicates that it is enumerated while the method is executed.
/// If <see cref="RequireAwait"/> is true, the attribute will only take effect
/// if the method invocation is located under the <c>await</c> expression.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class InstantHandleAttribute : Attribute
{
    /// <summary>
    /// Requires the method invocation to be used under the <c>await</c> expression for this attribute to take effect.
    /// Can be used for delegate/enumerable parameters of <c>async</c> methods.
    /// </summary>
    public bool RequireAwait { get; set; }
}
