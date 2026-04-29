using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators;

public static class GenUtil
{
    public record FileHintOptions
    {
        public bool EnsureUniqueWithRandomSuffix { get; set; } = true;
        public Func<string> RandomSuffixGenerator { get; set; } = DefaultRandomSuffixGenerator;
        public Func<INamedTypeSymbol, string> SymbolFormatter { get; set; } = DefaultSymbolFormatter;
        public Func<string, string> GeneratorNameFormatter { get; set; } = DefaultGeneratorNameFormatter;
        public string? Prefix { get; set; } = null;
        public string? Suffix { get; set; } = ".generated";


        public static string DefaultGeneratorNameFormatter(string arg)
        {
            return $"_{arg}";
        }

        public static string DefaultSymbolFormatter(INamedTypeSymbol symbol) =>
            symbol.FullQualifiedNameOmitGlobal().SanitizeQualifiedNameForUniqueHint();

        public static string DefaultRandomSuffixGenerator()
        {
            return "_" + Guid.NewGuid().ToString("N");
        }

        public string GetPrefix() => Prefix ?? string.Empty;

        public string GetSuffix()
        {
            string result = string.Empty;
            if (EnsureUniqueWithRandomSuffix) result += RandomSuffixGenerator();
            if (Suffix != null) result += Suffix;
            return result;
        }
    }

    public static readonly FileHintOptions DefaultFileHintOptions = new();

    public static string GetFileHint(string generatorName, INamedTypeSymbol symbol, FileHintOptions? options = null)
    {
        options ??= DefaultFileHintOptions;

        return
            $"{options.GetPrefix()}{options.SymbolFormatter(symbol)}{options.GeneratorNameFormatter(generatorName)}{options.GetSuffix()}";
    }

    public record LookupClass
    {
        public LookupClassCreationOptions Options { get; }
        public string Name { get; }
        public INamedTypeSymbol Symbol { get; }
        public INamedTypeSymbol BaseType { get; }
        public ImmutableArray<ISymbol> Symbols { get; }
        public bool IsValid { get; }

        public string GetDeclaration()
        {
            if (!IsValid) return string.Empty;

            return
                $"{String.Join(" ", Options.Keywords)} {Options.DeclarationKeyword} {Name} : {Options.BaseTypeFormatter(BaseType)}.{Name}";
        }
    }

    public record LookupClassCreationOptions
    {
        public string[] Keywords { get; set; } = ["public", "new", "partial"];
        public string DeclarationKeyword { get; set; } = "class";

        public Func<INamedTypeSymbol, string> BaseTypeFormatter { get; set; } =
          static s => s.FullQualifiedNameIncludeGlobal();

        public IList<Pragma> Pragmas { get; } = [Pragma.Warnings.CS0109];
    }

    public static readonly LookupClassCreationOptions DefaultLookupClassCreationOptions = new();

    public static LookupClass CreateLookup(string lookupName, INamedTypeSymbol symbol, IEnumerable<ISymbol> symbols, LookupClassCreationOptions? options = null)
    {
        throw new NotImplementedException();
    }
}
