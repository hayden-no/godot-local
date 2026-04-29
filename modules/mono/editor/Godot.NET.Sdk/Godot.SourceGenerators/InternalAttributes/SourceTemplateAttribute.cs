using System;

namespace JetBrains.Annotations;

/// <summary>
/// An extension method marked with this attribute is processed by code completion
/// as a 'Source Template'. When the extension method is completed over some expression, its source code
/// is automatically expanded like a template at the call site.
/// </summary>
/// <remarks>
/// Template method bodies can contain valid source code and/or special comments starting with '$'.
/// Text inside these comments is added as source code when the template is applied. Template parameters
/// can be used either as additional method parameters or as identifiers wrapped in two '$' signs.
/// Use the <see cref="MacroAttribute"/> attribute to specify macros for parameters.
/// The expression to be used in the expansion can be adjusted
/// by the <see cref="SourceTemplateAttribute.Target"/> parameter.
/// </remarks>
/// <example>
/// In this example, the <c>forEach</c> method is a source template available over all values
/// of enumerable types, producing an ordinary C# <c>foreach</c> statement and placing the caret inside the block:
/// <code>
/// [SourceTemplate]
/// public static void forEach&lt;T&gt;(this IEnumerable&lt;T&gt; xs)
/// {
///   foreach (var x in xs)
///   {
///     //$ $END$
///   }
/// }
/// </code>
/// </example>
[AttributeUsage(AttributeTargets.Method)]
internal sealed class SourceTemplateAttribute : Attribute
{
    /// <summary>
    /// Allows specifying the expression to capture for template execution if more than one expression
    /// is available at the expansion point.
    /// If not specified, <see cref="SourceTemplateTargetExpression.Inner"/> is assumed.
    /// </summary>
    public SourceTemplateTargetExpression Target { get; set; }
}
