using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates the condition parameter of the assertion method. The method itself should be
/// marked by the <see cref="AssertionMethodAttribute"/> attribute. The mandatory argument of
/// the attribute is the assertion type.
/// </summary>
[AttributeUsage(AttributeTargets.Parameter)]
internal sealed class AssertionConditionAttribute : Attribute
{
    public AssertionConditionAttribute(AssertionConditionType conditionType)
    {
        ConditionType = conditionType;
    }

    public AssertionConditionType ConditionType { get; }
}
