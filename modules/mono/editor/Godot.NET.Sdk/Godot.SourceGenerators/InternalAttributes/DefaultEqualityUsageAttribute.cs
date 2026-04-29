using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the method or type uses equality members of the annotated element.
/// </summary>
/// <remarks>
/// When applied to the method's generic parameter, indicates that the equality of the annotated type is used,
/// unless a custom equality comparer is passed when calling this method. The attribute can also be applied
/// directly to the method's parameter or return type to specify equality usage for it.
/// When applied to the type's generic parameter, indicates that type equality usage can happen anywhere
/// inside this type, so the instantiation of this type is treated as equality usage, unless a custom
/// equality comparer is passed to the constructor.
/// </remarks>
/// <example><code>
/// struct StructWithDefaultEquality { /* no Equals &amp; GetHashCode override */ }
///
/// class MySet&lt;[DefaultEqualityUsage] T&gt; { ... }
///
/// static class Extensions
/// {
///   public static MySet&lt;T&gt; ToMySet&lt;[DefaultEqualityUsage] T&gt;(this IEnumerable&lt;T&gt; items) { ... }
/// }
///
/// class MyList&lt;T&gt;
/// {
///   public int IndexOf([DefaultEqualityUsage] T item) { ... }
/// }
///
/// class UsesDefaultEquality
/// {
///   void Test()
///   {
///     var list = new MyList&lt;StructWithDefaultEquality&gt;();
///
///     // Warning: Default equality of struct 'StructWithDefaultEquality' is used
///     list.IndexOf(new StructWithDefaultEquality());
///
///     // Warning: Default equality of struct 'StructWithDefaultEquality' is used
///     var set1 = new MySet&lt;StructWithDefaultEquality&gt;();
///
///     // Warning: Default equality of struct 'StructWithDefaultEquality' is used
///     var set2 = new StructWithDefaultEquality[1].ToMySet();
///   }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.GenericParameter | AttributeTargets.Parameter | AttributeTargets.ReturnValue)]
internal sealed class DefaultEqualityUsageAttribute : Attribute { }