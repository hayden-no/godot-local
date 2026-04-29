using System.Collections.Generic;

namespace Godot.SourceGenerators;

public record XmlCommentTagSortOrder : IComparer<XmlCommentTag>
{
    // common
    public int Summary { get; set; } = 0;
    public int Remarks { get; set; } = 100;
    public int Example { get; set; } = 200;
    public int Value { get; set; } = 300;
    public int SeeAlso { get; set; } = 400;
    public int Param { get; set; } = 500;
    public int TypeParam { get; set; } = 600;
    public int Returns { get; set; } = 700;
    public int Exception { get; set; } = 800;
    public int Other { get; set; } = 999;

    public int Compare(XmlCommentTag x, XmlCommentTag y)
    {
        var xValue = GetValue(x);
        var yValue = GetValue(y);
        return xValue.CompareTo(yValue);
    }

    public int GetValue(in XmlCommentTag tag)
    {
        return tag.IsSummary ? Summary :
            tag.IsRemarks ? Remarks :
            tag.IsExample ? Example :
            tag.IsValue ? Value :
            tag.IsSeeAlso ? SeeAlso :
            tag.IsParam ? Param :
            tag.IsTypeParam ? TypeParam :
            tag.IsReturns ? Returns :
            tag.IsException ? Exception :
            Other;
    }

    public static readonly XmlCommentTagSortOrder Default = new();
}