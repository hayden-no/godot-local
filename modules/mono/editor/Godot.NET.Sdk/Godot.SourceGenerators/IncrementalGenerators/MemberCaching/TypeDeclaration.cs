using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Godot.SourceGenerators.MemberCaching;

public record TypeDeclaration
{
    public ValueArray<string> Keywords { get; init; }
    public string DeclarationKeyword { get; init; }
    public string Name { get; init; }
    public InheritanceDeclaration? Inheritance { get; init; }
    public ValueArray<TypeParameterDeclaration> TypeParameters { get; init; }

    public TypeDeclaration(
        string name,
        ValueArray<string> keywords,
        string declarationKeyword,
        InheritanceDeclaration? inheritance,
        ValueArray<TypeParameterDeclaration> typeParameters
    )
    {
        Name = name;
        Keywords = keywords;
        DeclarationKeyword = declarationKeyword;
        Inheritance = inheritance;
        TypeParameters = typeParameters;
    }

    public IEnumerable<string> GetKeywordsAndName()
    {
        foreach (var keyword in Keywords)
        {
            yield return keyword;
        }

        yield return DeclarationKeyword;
        yield return Name;
    }

    public IBeginEndWritable GetNamedTypeDeclaration()
    {
        return new ClassBlock { Declaration = this };
    }

    record ClassBlock : IBeginEndWritable
    {
        public TypeDeclaration Declaration { get; init; }

        /// <inheritdoc />
        public void Begin(FormatWriter writer, CancellationToken cancellationToken = default)
        {
            writer.EnsureNewLine();
            writer.WriteJoin(" ", Declaration.GetKeywordsAndName())
                .WriteIfNoneNullOrEmpty(
                    "<",
                    string.Join(", ", Declaration.TypeParameters.Select(t => t.GetShort())),
                    ">"
                );

            if (Declaration.Inheritance?.IsEmpty == false)
            {
                writer.Write(" : ").WriteJoin<TypeName>(", ", Declaration.Inheritance);
            }

            if (!Declaration.TypeParameters.IsEmpty && Declaration.TypeParameters.Any(t => t.HasConstraints))
            {
                writer.EnsureNewLine();
                writer.PushIndent("  ");
                writer.WriteJoin("\n", Declaration.TypeParameters.Select(t => t.GetConstraints()));
                writer.PopIndent();
            }

            writer.EnsureNewLine();
            writer.WriteLine("{");
            writer.PushIndent();
        }

        /// <inheritdoc />
        public void End(FormatWriter writer, CancellationToken cancellationToken = default)
        {
            writer.PopIndent();
            writer.WriteLine("}");
        }
    }
}