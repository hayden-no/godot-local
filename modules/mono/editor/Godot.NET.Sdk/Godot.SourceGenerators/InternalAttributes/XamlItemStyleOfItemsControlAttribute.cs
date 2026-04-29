using System;

namespace JetBrains.Annotations;

/// <summary>
/// XAML attribute. Indicates the property of some <c>Style</c>-derived type that
/// is used to style items of an <c>ItemsControl</c>-derived type. This annotation will
/// enable the <c>DataContext</c> type resolution in XAML bindings for such properties.
/// </summary>
/// <remarks>
/// Property should have a tree ancestor of the <c>ItemsControl</c> type or
/// marked with the <see cref="XamlItemsControlAttribute"/> attribute.
/// </remarks>
[AttributeUsage(AttributeTargets.Property)]
internal sealed class XamlItemStyleOfItemsControlAttribute : Attribute { }
