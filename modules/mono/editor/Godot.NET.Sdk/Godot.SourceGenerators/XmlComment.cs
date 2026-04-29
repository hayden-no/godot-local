using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Godot.SourceGenerators.MemberCaching;

namespace Godot.SourceGenerators;

public record XmlComment : IArgumentWriteable, IReadOnlyDictionary<XmlCommentTag, string>
{
    public ValueDictionary<XmlCommentTag, string> TagAndContent { get; }
    public IComparer<XmlCommentTag> TagComparer { get; } = new XmlCommentTagSortOrder();

    private ValueArray<(XmlCommentTag tag, string content)>? _sortedCache = null;
    public ValueArray<(XmlCommentTag tag, string content)> SortedTagAndContent => _sortedCache ??= [..TagAndContent.OrderBy(t => t.Key, TagComparer).Select(t => (t.Key, t.Value))];

    public XmlComment(IReadOnlyDictionary<XmlCommentTag, string> tagAndContent, IComparer<XmlCommentTag>? tagComparer = null)
    {
        TagAndContent = tagAndContent.ToImmutableDictionary();
        TagComparer = tagComparer ?? TagComparer;
    }

    public XmlComment(params IEnumerable<(XmlCommentTag tag, string content)> tagAndContent) : this(tagAndContent.ToImmutableDictionary(t => t.tag, t => t.content))
    {
    }

    public XmlComment(IComparer<XmlCommentTag>? tagComparer, params IEnumerable<(XmlCommentTag tag, string content)> tagAndContent) : this(tagAndContent.ToImmutableDictionary(t => t.tag, t => t.content), tagComparer)
    {
    }

    public XmlComment() : this(ValueDictionary<XmlCommentTag, string>.Empty, null)
    {

    }

    public static XmlComment Layer(IComparer<XmlCommentTag>? tagComparer, params IEnumerable<IReadOnlyDictionary<XmlCommentTag, string>?> toLayer)
    {
        return new XmlComment(
            toLayer.Aggregate(
                new Dictionary<XmlCommentTag, string>(),
                (left, right) =>
                {
                    if (right is null) return left;
                    foreach (var kvp in right)
                    {
                        left[kvp.Key] = kvp.Value;
                    }

                    return left;
                }
            ), tagComparer
        );
    }

    public static XmlComment Layer(params IEnumerable<IReadOnlyDictionary<XmlCommentTag, string>?> toLayer)
    {
        return Layer(null, toLayer);
    }

    public bool IsEmpty => TagAndContent.Count == 0;

    public const string SummaryTag = "summary";
    public const string RemarksTag = "remarks";
    public const string ExampleTag = "example";
    public const string ValueTag = "value";
    public const string SeeAlsoTag = "seealso";
    public const string ParamTag = "param";
    public const string TypeParamTag = "typeparam";
    public const string ReturnsTag = "returns";
    public const string ExceptionTag = "exception";

    public const string XmlIndent = "/// ";

    public static XmlComment Create(string? summary = null, string? remarks = null, string? returns = null)
    {
        var list = new List<(XmlCommentTag tag, string content)>();
        if (summary is not null) list.Add((SummaryTag, summary));
        if (remarks is not null) list.Add((RemarksTag, remarks));
        if (returns is not null) list.Add((ReturnsTag, returns));
        return new(list);
    }

    public static XmlComment Create(params IEnumerable<(XmlCommentTag tag, string content)> tagAndContent) => new(tagAndContent);

    /// <inheritdoc />
    public IEnumerator<KeyValuePair<XmlCommentTag, string>> GetEnumerator() => TagAndContent.GetEnumerator();

    /// <inheritdoc />
    IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)TagAndContent).GetEnumerator();

    /// <inheritdoc />
    public int Count => TagAndContent.Count;

    /// <inheritdoc />
    public bool ContainsKey(XmlCommentTag key) => TagAndContent.ContainsKey(key);

    /// <inheritdoc />
    public bool TryGetValue(XmlCommentTag key, out string value) => TagAndContent.TryGetValue(key, out value);

    /// <inheritdoc />
    public string this[XmlCommentTag key] => TagAndContent[key];

    /// <inheritdoc />
    public IEnumerable<XmlCommentTag> Keys => TagAndContent.Keys;

    /// <inheritdoc />
    public IEnumerable<string> Values => TagAndContent.Values;

    /// <inheritdoc />
    public void Write(FormatWriter writer, params IEnumerable<object?> arguments)
    {
        if (IsEmpty) return;

        var args = arguments.ToArray();
        writer.PushIndent(XmlIndent);

        foreach ((XmlCommentTag tag, string content) in SortedTagAndContent)
        {
            writer.WriteStart(tag).WriteLine();
            writer.WriteLine(content, args);
            writer.WriteEnd(tag);
        }

        writer.PopIndent();
    }
}