using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;

namespace Godot.SourceGenerators.MemberCaching;

public abstract record BaseMemberCache
{
    public abstract MemberCacheDefinition Definition { get; }

    public TypeName BaseType { get; }
    public ValueArray<CacheEntry> Entries { get; }

    protected BaseMemberCache(TypeName baseType, IEnumerable<CacheEntry> entries)
    {
        BaseType = baseType;
        Entries = entries.ToImmutableArray();
    }

    public static readonly XmlComment DefaultXmlComment = XmlComment.Create(
        "Cached <see cref=\"Godot.StringName\"/>s for the {0} contained in this class, for fast lookup."
    );

    public void WriteDefaultXmlComment(FormatWriter writer) =>
        writer.Write(Definition.XmlComment, Definition.XmlCommentMemberTypes.ToLower(writer.Culture));

    public void WriteDeclaration(FormatWriter writer)
    {
        writer.WriteJoin(" ", Definition.Declaration.GetKeywordsAndName());

        if (!Definition.Declaration.TypeParameters.IsDefaultOrEmpty)
        {
            writer.Write("<");
            writer.WriteJoin(", ", Definition.Declaration.TypeParameters.Select(t => t.GetShort()));
            writer.Write(">");
        }

        if (Definition.Declaration.Inheritance is not null && Definition.Declaration.Inheritance.Any())
        {
            writer.Write(" : ");
            writer.WriteJoin(", ", Definition.Declaration.Inheritance.Select(t => t.DefaultDisplayString));
        }

        if (!Definition.Declaration.TypeParameters.IsDefaultOrEmpty)
        {
            var arr = Definition.Declaration.TypeParameters.Select(tp =>
                    (tp.Name,
                        string.Join(
                            ", ",
                            tp.GetConstraints(tn => tn.FullyQualifiedNameIncludeGlobal)
                                .Where(s => !string.IsNullOrWhiteSpace(s))
                        ))
                )
                .Where(t => !string.IsNullOrWhiteSpace(t.Item2))
                .ToImmutableArray();

            if (arr.Any())
            {
                writer.WriteLine();

                foreach (var (name, str) in arr)
                {
                    if (string.IsNullOrWhiteSpace(str) || string.IsNullOrWhiteSpace(name)) continue;

                    writer.Write(" where ").Write(name).Write(" : ").WriteLine(str);
                }
            }
        }
    }

    private static readonly XmlComment DefaultCacheXmlComment = XmlComment.Create("Cached name for the {0} '{1}' {2}.");

    public static void WriteNameCacheXmlComment(FormatWriter writer, CacheEntry entry)
    {
        writer.Write(
                XmlComment.Layer(DefaultCacheXmlComment, entry.XmlComment),
                entry.MemberKind,
                entry.Name,
                entry.ValueOrReturnType?.DefaultDisplayString ?? "null"
            )
            .EnsureNewLine();
    }

    public static void WriteStaticStringNameCache(FormatWriter writer, CacheEntry entry)
    {
        WriteNameCacheXmlComment(writer, entry);
        writer.EnsureNewLine();
        writer.Write("public new static readonly global::Godot.StringName @")
            .Write(entry.Name)
            .Write(" = \"")
            .Write(entry.Name)
            .WriteLine("\";");
    }
}
