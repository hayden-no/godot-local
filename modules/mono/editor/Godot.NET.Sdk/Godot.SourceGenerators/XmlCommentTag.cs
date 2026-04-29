using System;
using System.Linq;
using System.Threading;
using Godot.SourceGenerators.MemberCaching;

namespace Godot.SourceGenerators;

public readonly struct XmlCommentTag : IEquatable<XmlCommentTag>, IBeginEndWritable
{
    public enum Ref
    {
        Cref,
        Href
    }

    public readonly string Name;
    public readonly ValueDictionary<string, string> Attributes;

    public static readonly XmlCommentTag Summary = new(XmlComment.SummaryTag);
    public static readonly XmlCommentTag Remarks = new(XmlComment.RemarksTag);
    public static readonly XmlCommentTag Example = new(XmlComment.ExampleTag);
    public static readonly XmlCommentTag Value = new(XmlComment.ValueTag);
    public static readonly XmlCommentTag Returns = new(XmlComment.ReturnsTag);

    public static XmlCommentTag SeeAlso(string refName, Ref refType) => new(XmlComment.SeeAlsoTag, new ValueDictionary<string, string> { { refType == Ref.Cref ? "cref" : "href", refName } });
    public static XmlCommentTag Param(string name) => new(XmlComment.ParamTag, new ValueDictionary<string, string> { { "name", name } });
    public static XmlCommentTag TypeParam(string name) => new(XmlComment.TypeParamTag, new ValueDictionary<string, string> { { "name", name } });
    public static XmlCommentTag Exception(string name) => new(XmlComment.ExceptionTag, new ValueDictionary<string, string> { { "cref", name } });


    public bool IsSummary => Name == XmlComment.SummaryTag;
    public bool IsRemarks => Name == XmlComment.RemarksTag;
    public bool IsExample => Name == XmlComment.ExampleTag;
    public bool IsValue => Name == XmlComment.ValueTag;
    public bool IsSeeAlso => Name == XmlComment.SeeAlsoTag;
    public bool IsParam => Name == XmlComment.ParamTag;
    public bool IsTypeParam => Name == XmlComment.TypeParamTag;
    public bool IsReturns => Name == XmlComment.ReturnsTag;
    public bool IsException => Name == XmlComment.ExceptionTag;
    public bool IsOther => !IsSummary && !IsRemarks && !IsExample && !IsValue && !IsSeeAlso && !IsParam && !IsTypeParam && !IsReturns && !IsException;

    public XmlCommentTag(string name) : this(name, ValueDictionary<string, string>.Empty) { }

    public XmlCommentTag(string name, ValueDictionary<string, string> attributes)
    {
        Name = name;
        Attributes = attributes;
    }

    /// <inheritdoc />
    public bool Equals(XmlCommentTag other) => Name == other.Name && Attributes.Equals(other.Attributes);

    /// <inheritdoc />
    public override bool Equals(object? obj) => obj is XmlCommentTag other && Equals(other);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            return (Name.GetHashCode() * 397) ^ Attributes.GetHashCode();
        }
    }

    public static bool operator ==(XmlCommentTag left, XmlCommentTag right) => left.Equals(right);
    public static bool operator !=(XmlCommentTag left, XmlCommentTag right) => !left.Equals(right);

    public static implicit operator XmlCommentTag(string name) => new(name);

    /// <inheritdoc />
    public void Begin(FormatWriter writer, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) return;
        // assume triple '/' is already on the indent stack
        writer.Write('<').Write(Name);
        writer.WriteJoinSkipNull(" ", Attributes.Select(static kvp => kvp.Key + "=\"" + kvp.Value + "\""));
        writer.Write('>');
    }

    /// <inheritdoc />
    public void End(FormatWriter writer, CancellationToken cancellationToken = default)
    {
        if (cancellationToken.IsCancellationRequested) return;
        writer.Write("</").Write(Name).Write('>');
    }
}