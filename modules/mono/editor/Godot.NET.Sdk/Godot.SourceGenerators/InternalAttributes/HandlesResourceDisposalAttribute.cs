using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the method or class instance acquires resource ownership and will dispose it after use.
/// </summary>
/// <remarks>
/// Annotation of <c>out</c> parameters with this attribute is meaningless.<br/>
/// When an instance method is annotated with this attribute,
/// it means that it is handling the resource disposal of the corresponding resource instance.<br/>
/// When a field or a property is annotated with this attribute, it means that this type owns the resource
/// and will handle the resource disposal properly (e.g. in own <c>IDisposable</c> implementation).
/// </remarks>
[AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property)]
internal sealed class HandlesResourceDisposalAttribute : Attribute { }
