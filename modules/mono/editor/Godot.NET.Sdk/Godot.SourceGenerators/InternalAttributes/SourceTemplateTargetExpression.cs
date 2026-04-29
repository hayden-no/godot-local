namespace JetBrains.Annotations;

/// <summary>
/// Provides a value for the <see cref="SourceTemplateAttribute"/> to define how to capture
/// the expression at the point of expansion
/// </summary>
internal enum SourceTemplateTargetExpression
{
    /// <summary>Selects inner expression</summary>
    /// <example><c>value > 42.{caret}</c> captures <c>42</c></example>
    /// <example><c>_args = args.{caret}</c> captures <c>args</c></example>
    Inner = 0,

    /// <summary>Selects outer expression</summary>
    /// <example><c>value > 42.{caret}</c> captures <c>value > 42</c></example>
    /// <example><c>_args = args.{caret}</c> captures whole assignment</example>
    Outer = 1
}
