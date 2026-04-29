using System;

namespace JetBrains.Annotations;

/// <summary>
/// XAML attribute. Indicates that DependencyProperty has <c>OneWay</c> binding mode by default.
/// </summary>
/// <remarks>
/// This attribute must be applied to DependencyProperty's CLR accessor property if it is present,
/// or to a DependencyProperty descriptor field otherwise.
/// </remarks>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
internal sealed class XamlOneWayBindingModeByDefaultAttribute : Attribute { }
