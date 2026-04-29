using System;

namespace JetBrains.Annotations;

/// <summary>
/// Can be applied to symbols of types derived from <c>IEnumerable</c> as well as to symbols of <c>Task</c>
/// and <c>Lazy</c> classes to indicate that the value of a collection item, of the <c>Task.Result</c> property,
/// or of the <c>Lazy.Value</c> property can never be null.
/// </summary>
/// <example><code>
/// public void Foo([ItemNotNull]List&lt;string&gt; books)
/// {
///   foreach (var book in books)
///   {
///     if (book != null) // Warning: Expression is always true
///     {
///       Console.WriteLine(book.ToUpper());
///     }
///   }
/// }
/// </code></example>
[AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Parameter | AttributeTargets.Property |
    AttributeTargets.Delegate | AttributeTargets.Field)]
internal sealed class ItemNotNullAttribute : Attribute { }