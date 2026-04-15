using System;

namespace Godot;

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field)]
public class ExportComplexAttribute : Attribute
{
    public ComplexExportMode Mode { get; }

    public ExportComplexAttribute(ComplexExportMode mode = ComplexExportMode.Deconstruct)
    {
        Mode = mode;
    }
}

[AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = true)]
public class ExportComplexMemberAttribute : Attribute
{
    public string MemberPath { get; }

    /// <summary>
    /// Optional hint that determines how the property should be handled by the editor.
    /// </summary>
    public PropertyHint Hint { get; }

    /// <summary>
    /// Optional string that can contain additional metadata for the <see cref="Hint"/>.
    /// </summary>
    public string HintString { get; }

    /// <summary>
    /// Constructs a new ExportAttribute Instance.
    /// </summary>
    /// <param name="hint">The hint for the exported property.</param>
    /// <param name="hintString">A string that may contain additional metadata for the hint.</param>
    public ExportComplexMemberAttribute(string memberPath, PropertyHint hint = PropertyHint.None, string hintString = "")
    {
        MemberPath = memberPath;
        Hint = hint;
        HintString = hintString;
    }
}

public enum ComplexExportMode
{
    Deconstruct,
    Wrap
}
