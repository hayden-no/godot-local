using System;

namespace JetBrains.Annotations;

/// <summary>
/// Specifies a type being tested by a test class or a test method.
/// </summary>
/// <remarks>
/// This information can be used by the IDE to navigate between tests and tested types,
/// or by test runners to group tests by subject and to provide better test reports.
/// </remarks>
[AttributeUsage(
    AttributeTargets.Method | AttributeTargets.Property | AttributeTargets.Class |
    AttributeTargets.Interface,
    AllowMultiple = true)]
internal sealed class TestSubjectAttribute : Attribute
{
    /// <summary>
    /// Gets the type being tested.
    /// </summary>
    [NotNull] public Type Subject { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestSubjectAttribute"/> class with the specified tested type.
    /// </summary>
    /// <param name="subject">The type being tested.</param>
    public TestSubjectAttribute([NotNull] Type subject)
    {
        Subject = subject;
    }
}
