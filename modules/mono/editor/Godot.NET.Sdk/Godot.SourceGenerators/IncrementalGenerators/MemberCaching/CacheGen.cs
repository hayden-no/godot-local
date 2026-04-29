using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Godot.SourceGenerators.MemberCaching;

[Generator]
public class CacheGen : IIncrementalGenerator
{
    const string GENERATOR_NAME = "CacheGen";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // check is enabled
        var options = context.AnalyzerConfigOptionsProvider.Select((o, ct) => { return new { o.SolutionDir }; });

        var classes = context.SyntaxProvider.CreateSyntaxProvider(
                (static (node, token) => node is ClassDeclarationSyntax cds &&
                    cds.AncestorsAndSelf()
                        .OfType<ClassDeclarationSyntax>()
                        .All(c => c.Modifiers.Any(SyntaxKind.PartialKeyword))),
                static (syntaxContext, token) =>
                {
                    if (token.IsCancellationRequested) return null;

                    ClassDeclarationSyntax cds = (ClassDeclarationSyntax)syntaxContext.Node;
                    var symbol = syntaxContext.SemanticModel.GetDeclaredSymbol(cds, token);

                    if (symbol is null || !symbol.IsGodotObjectType()) return null;

                    var typeCache = new MarshalUtils.TypeCache(syntaxContext.SemanticModel.Compilation);

                    var location = TypeLocation.FromType(symbol);
                    var baseTypeName = TypeName.CreateOrNull(symbol.BaseType);
                    var name = symbol.Name;
                    var declaration = symbol.ToDeclarationString();
                    var members = CacheEntry.FromMembers(symbol, typeCache, memberPredicate: MemberPredicate);
                    var filename = syntaxContext.SemanticModel.SyntaxTree.FilePath;
                    var marshal = MarshalUtils.ConvertManagedTypeToMarshalType(symbol, typeCache);
                    var variant = marshal.HasValue ? MarshalUtils.ConvertMarshalTypeToVariantType(marshal.Value) : null;

                    bool MemberPredicate(ISymbol obj)
                    {
                        if (obj.IsImplicitlyDeclared) return false;
                        if (!obj.IsDefinition) return false;
                        if (!obj.Locations.Any(l =>
                                l.IsInSource && l.SourceTree.FilePath == syntaxContext.SemanticModel.SyntaxTree.FilePath
                            ))
                            return false;

                        return true;
                    }

                    return new
                    {
                        Location = location,
                        BaseTypeName = baseTypeName,
                        Name = name,
                        Declaration = declaration,
                        Members = members,
                        Filename = filename
                    };
                }
            )
            .WhereNotNull();

        var fieldsAndProperties = classes.Select((obj, token) =>
                {
                    if (token.IsCancellationRequested) return null;

                    return obj with
                    {
                        Members = obj.Members.Where(static m => m.MemberKind is SymbolKind.Field or SymbolKind.Property)
                            .ToImmutableArray()
                    };
                }
            )
            .WhereNotNull()
            .Where(static obj => obj.Members.Length > 0);

        var fieldAndPropertyLookupMembers = fieldsAndProperties.Select(static (obj, ct) =>
                {
                    if (ct.IsCancellationRequested) return null;
                    if (obj.BaseTypeName is null) return null;

                    string baseTypeName = obj.BaseTypeName.FullyQualifiedNameIncludeGlobal;
                    var members = obj.Members
                        .Select(m => (Name: m.Name,
                            Comment: FormatWriter.Execute(BaseMemberCache.WriteNameCacheXmlComment, m))
                        )
                        .Distinct(
                            IEqualityComparer.CreateAnon<(string Name, string Comment)>(
                                (arg1, arg2) => string.Equals(arg1.Name, arg2.Name),
                                arg => arg.Name.GetHashCode()
                            )
                        );

                    return new
                    {
                        Location = obj.Location.NewChild("partial " + obj.Declaration),
                        BaseTypeName = baseTypeName,
                        Members = members,
                        FileHintName = obj.Declaration.TrimStart("class").ToString(),
                        obj.Filename
                    };
                }
            )
            .WhereNotNull();

        context.RegisterSourceOutput(
            fieldAndPropertyLookupMembers.Combine(options),
            (productionContext, arg) =>
            {
                var (data, options) = arg;

                var solutionRelativeDir = data.Filename.TrimStart(options.SolutionDir)
                    .TrimEnd(Path.DirectorySeparatorChar)
                    .ToString();

                var comment = new FileHeaderComment()
                {
                    GenTime = DateTime.Now,
                    GenName = GENERATOR_NAME,
                    FilenameSources = [solutionRelativeDir],
                    Description = "Cached StringName values for performance."
                };

                const string className = "PropertyName";
                const string cacheTypes = "properties and fields";
                const string stringName = "global::Godot.StringName";

                string fileHint = $"{data.FileHintName.SanitizeQualifiedNameForUniqueHint()}.{className}.{Guid.NewGuid():N}.g";

                var decl = new TypeDeclaration(
                    className,
                    ["public", "new", "partial"],
                    "class",
                    new(new TypeName.Pseudo($"{data.BaseTypeName}.{className}"), ValueArray<TypeName>.Empty),
                    ValueArray<TypeParameterDeclaration>.Empty
                );
                Pragma[] pragmas = [Pragma.Warnings.CS0109];
                XmlComment classComment = BaseMemberCache.DefaultXmlComment;

                FormatWriter writer = new(productionContext.CancellationToken);
                writer.Write(comment);
                writer.EnsureNewLines(2);
                writer.PushBlock(data.Location);
                writer.EnsureNewLine();
                writer.PushBlock(pragmas.Aggregate());
                classComment.Write(writer, cacheTypes);
                writer.PushBlock(decl.GetNamedTypeDeclaration());

                foreach (var member in data.Members)
                {
                    writer.EnsureNewLines(2);
                    writer.WriteLine(member.Comment);
                    writer.Write("public new static readonly ")
                        .Write(stringName)
                        .Write(" @")
                        .Write(member.Name)
                        // .Write(" = \"")
                        // .Write(member.Name)
                        // .WriteLine("\";");
                        .Write(" = nameof(")
                        .Write(data.FileHintName)
                        .Write(".")
                        .Write(member.Name)
                        .WriteLine(");");
                }

                writer.PopBlock();
                writer.PopBlock();
                writer.PopBlock();

                productionContext.AddSource(fileHint, writer.ToSourceText());
            }
        );
    }
}
