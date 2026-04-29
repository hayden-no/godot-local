using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the value of the marked type (or its derivatives)
/// cannot be compared using <c>==</c> or <c>!=</c> operators, and <c>Equals()</c>
/// should be used instead. However, using <c>==</c> or <c>!=</c> for comparison
/// with <c>null</c> is always permitted.
/// </summary>
/// <example><code>
/// [CannotApplyEqualityOperator]
/// class NoEquality { }
///
/// class UsesNoEquality
/// {
///   void Test()
///   {
///     var instance1 = new NoEquality();
///     var instance2 = new NoEquality();
///     if (instance1 != null) // OK
///     {
///       bool condition = instance1 == instance2; // Warning
///     }
///   }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Interface | AttributeTargets.Class | AttributeTargets.Struct)]
internal sealed class CannotApplyEqualityOperatorAttribute : Attribute { }