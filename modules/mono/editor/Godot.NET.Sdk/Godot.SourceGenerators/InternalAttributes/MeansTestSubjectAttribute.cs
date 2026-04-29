using System;

namespace JetBrains.Annotations;

/// <summary>
/// Marks a generic argument as the test subject for a test class.
/// </summary>
/// <remarks>
/// Can be applied to a generic parameter of a base test class to indicate that
/// the type passed as the argument is the class being tested. This information can be used by the IDE
/// to navigate between tests and tested types,
/// or by test runners to group tests by subject and to provide better test reports.
/// </remarks>
/// <example><code>
/// public class BaseTestClass&lt;[MeansTestSubject] T&gt;
/// {
///   protected T Component { get; }
/// }
///
/// public class CalculatorAdditionTests : BaseTestClass&lt;Calculator&gt;
/// {
///   [Test]
///   public void Should_add_two_numbers()
///   {
///     Assert.That(Component.Add(2, 3), Is.EqualTo(5));
///   }
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.GenericParameter)]
internal sealed class MeansTestSubjectAttribute : Attribute { }
