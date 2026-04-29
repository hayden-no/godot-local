using System;

namespace JetBrains.Annotations;

/// <summary>
/// XAML attribute. Indicates the type that has an <c>ItemsSource</c> property and should be treated
/// as an <c>ItemsControl</c>-derived type, to enable inner items <c>DataContext</c> type resolution.
/// </summary>
[AttributeUsage(AttributeTargets.Class)]
internal sealed class XamlItemsControlAttribute : Attribute { }
