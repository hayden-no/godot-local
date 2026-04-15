using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Text;

namespace Godot.SourceGenerators;

[Generator]
public class ScriptPropertiesGenerator : IIncrementalGenerator
{
    const string GENERATOR_NAME = "ScriptProperties";

    /// <inheritdoc />
    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        var options = context.AnalyzerConfigOptionsProvider.Select((o, ct) =>
            {
                var GeneratorEnabled = o.IsSourceGenEnabled(GENERATOR_NAME);

                return new { GeneratorEnabled };
            }
        );

        var classes = context.GodotScriptClassProvider();

        var typeCache = MarshalUtils.GetTypeCacheProvider(context);

        // var properties = classes.Select((ctx, ct) =>
        //     {
        //         var propertySymbols = GetPropertySymbols(ctx.Symbol).ToImmutableArray();
        //
        //         return new { ctx = ctx, propertySymbols = propertySymbols };
        //     }
        // );
        //
        // var fields = classes.Select((ctx, ct) =>
        //     {
        //         var fieldSymbols = GetFieldSymbols(ctx.Symbol).ToImmutableArray();
        //
        //         return new { ctx = ctx, fieldSymbols = fieldSymbols };
        //     }
        // );
        //
        // var godotProperties = properties.Combine(typeCache)
        //     .Select((tuple, token) =>
        //         {
        //             var TypeCache = tuple.Right;
        //             var Context = tuple.Left.ctx;
        //             var PropertySymbols =
        //                 GetGodotPropertyData(tuple.Left.propertySymbols, TypeCache).ToImmutableArray();
        //
        //             return new { Context, PropertySymbols, TypeCache };
        //         }
        //     );
        //
        // var godotFields = fields.Combine(typeCache)
        //     .Select((tuple, token) =>
        //         {
        //             var TypeCache = tuple.Right;
        //             var Context = tuple.Left.ctx;
        //             var FieldSymbols = GetGodotFieldData(tuple.Left.fieldSymbols, TypeCache).ToImmutableArray();
        //
        //             return new { Context, FieldSymbols, TypeCache };
        //         }
        //     );
        //
        // var godotPropertyAndFieldData = classes.Combine(typeCache)
        //     .Select((tuple, ct) =>
        //         {
        //             var TypeCache = tuple.Right;
        //             var Context = tuple.Left;
        //             var Data = GetGodotPropertyOrFieldData(Context.Symbol, TypeCache).ToImmutableArray();
        //
        //             return new { Context, Data, TypeCache };
        //         }
        //     );

        context.RegisterSourceOutput(classes.Combine(options.Combine(typeCache).Combine(context.CompilationProvider)),
            (productionContext, tuple) =>
            {
                var (Context, ((Options, TypeCache), Compilation)) = tuple;
                var (symbol, syntax) = (Context.Symbol, Context.Syntax);
                if (!Context.IsValid || !Options.GeneratorEnabled)
                    return;

                string namespaceName = symbol.ContainingNamespace is { IsGlobalNamespace: false }
                    ? symbol.ContainingNamespace.FullQualifiedNameOmitGlobal()
                    : string.Empty;

                bool isInnerClass = symbol.ContainingType is {};
                bool isToolClass = symbol.HasAttribute(GodotClasses.ToolAttr);
                string fileHint = symbol.FullQualifiedNameOmitGlobal().SanitizeQualifiedNameForUniqueHint() +
                    "_ScriptProperties.generated";

                var writer = new FormatWriter();
                writer.WriteUsingStatements("Godot", "Godot.NativeInterop");
                writer.WriteNamespace(namespaceName);

                if (isInnerClass)
                {
                    var containingType = symbol.ContainingType;

                    void WritePartialContainingTypeDeclarations(INamedTypeSymbol? containingTypes)
                    {
                        if (containingType is null)
                            return;

                        WritePartialContainingTypeDeclarations(containingType.ContainingType);

                        writer.WriteTypeDeclaration(
                            containingType.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat),
                            ["partial", containingType.GetDeclarationKeyword()]
                        );
                        writer.WriteBlockStart();
                    }
                }

                writer.WriteTypeDeclaration(symbol.ToDisplayString(SymbolDisplayFormat.MinimallyQualifiedFormat), ["partial", "class"]);
                writer.WriteBlockStart();

                var properties = GetGodotPropertyData(GetPropertySymbols(symbol), TypeCache).ToImmutableArray();
                var fields = GetGodotFieldData(GetFieldSymbols(symbol), TypeCache).ToImmutableArray();
                var propertyAndFieldSymbols = GetGodotPropertyOrFieldData(symbol, TypeCache).ToImmutableArray();


                // Cached StringNames for the properties and fields
                WritePropertyNameClass(writer, symbol, properties.Select(p => p.PropertySymbol as ISymbol).Concat(fields.Select(f => f.FieldSymbol).OrderBy(s => s.Name)));

                WriteSetGodotClassPropertyValue(writer, properties, fields);

                WriteGetGodotClassPropertyValue(writer, properties, fields);

                WriteGetPropertyList(writer, productionContext, TypeCache, isToolClass, Compilation, propertyAndFieldSymbols);

                writer.WriteBlockEnd(); // partial class

                if (isInnerClass)
                {
                    var containingType = symbol.ContainingType;

                    while (containingType is not null)
                    {
                        writer.WriteBlockEnd(); // outer class
                        containingType = containingType.ContainingType;
                    }
                }

                productionContext.AddSource(fileHint, writer.ToSourceText());
            });
    }

    static IEnumerable<IPropertySymbol> GetPropertySymbols(INamedTypeSymbol symbol)
    {
        return symbol.GetMembers()
            .Where(s => !s.IsStatic && s.Kind == SymbolKind.Property)
            .Cast<IPropertySymbol>()
            .Where(s => !s.IsIndexer && s.ExplicitInterfaceImplementations.Length == 0);
    }

    static IEnumerable<IFieldSymbol> GetFieldSymbols(INamedTypeSymbol symbol)
    {
        return symbol.GetMembers()
            .Where(s => !s.IsStatic && s.Kind == SymbolKind.Field && !s.IsImplicitlyDeclared)
            .Cast<IFieldSymbol>();
    }

    static IEnumerable<GodotPropertyData> GetGodotPropertyData(
        IEnumerable<IPropertySymbol> propertySymbols,
        MarshalUtils.TypeCache typeCache
    ) =>
        propertySymbols.WhereIsGodotCompatibleType(typeCache);

    static IEnumerable<GodotFieldData> GetGodotFieldData(
        IEnumerable<IFieldSymbol> fieldSymbols,
        MarshalUtils.TypeCache typeCache
    ) =>
        fieldSymbols.WhereIsGodotCompatibleType(typeCache);

    static IEnumerable<GodotPropertyOrFieldData> GetGodotPropertyOrFieldData(
        INamedTypeSymbol symbol,
        MarshalUtils.TypeCache typeCache
    ) =>
        GetGodotPropertyData(GetPropertySymbols(symbol), typeCache)
            .Select(p => new GodotPropertyOrFieldData(p))
            .Concat(
                (GetGodotFieldData(GetFieldSymbols(symbol), typeCache)).Select(f => new GodotPropertyOrFieldData(f))
            )
            .OrderBy(data => data.Symbol.Locations[0].Path())
            .ThenBy(data => data.Symbol.Locations[0].StartLine());

    static FormatWriter WritePropertyNameClass(FormatWriter sb, INamedTypeSymbol symbol, IEnumerable<ISymbol> data)
    {
        if (symbol.BaseType is null) return sb;

        sb.Write(
            $@"""
            #pragma warning disable CS0109 // Disable warning about redundant 'new' keyword
                /// <summary>
                /// Cached StringNames for the properties and fields contained in this class, for fast lookup.
                /// </summary>
                public new class PropertyName : {symbol.BaseType.FullQualifiedNameIncludeGlobal()}.PropertyName
                {{
                    {string.Join("\n", data.Select(data => {
                        string name = data.Name;
                        string type = data switch {
                            IPropertySymbol p => p.Type.ToDisplayString(),
                            IFieldSymbol f => f.Type.ToDisplayString(),
                            _ => throw new NotImplementedException() };

                        return $"""
                                        /// <summary>
                                        /// Cached name for the '{name}' {type}.
                                        /// </summary>
                                        public new static readonly global::Godot.StringName @{name} = ""{name}"";
                                """;
                    }))}
                }}
            """
        );

        return sb;
    }

    static FormatWriter WriteSetGodotClassPropertyValue(
        FormatWriter sb,
        ImmutableArray<GodotPropertyData> properties,
        ImmutableArray<GodotFieldData> fields
    )
    {
        bool allReadOnly = properties.All(d => d.PropertySymbol.IsReadOnly || d.PropertySymbol.SetMethod!.IsInitOnly) &&
            fields.All(d => d.FieldSymbol.IsReadOnly);

        if (!allReadOnly)
        {
            sb.PushIndent();
            sb.Write(
                """
                /// <inheritdoc/>
                [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
                protected override bool SetGodotClassPropertyValue(in godot_string_name name, godot_variant value)
                {
                """
            );
            sb.PushIndent();

            foreach (var property in properties)
            {
                if (property.PropertySymbol.IsReadOnly || property.PropertySymbol.SetMethod.IsInitOnly) continue;

                WritePropertySetter(sb, property.PropertySymbol.Name, property.PropertySymbol.Type, property.Type);
            }

            foreach (var field in fields)
            {
                if (field.FieldSymbol.IsReadOnly) continue;

                WritePropertySetter(sb, field.FieldSymbol.Name, field.FieldSymbol.Type, field.Type);
            }

            sb.PopIndent();

            sb.Write(
                """
                    return base.SetGodotClassPropertyValue(name, value);
                }
                """
            );
            sb.PopIndent();
        }

        return sb;
    }

    static FormatWriter WriteGetGodotClassPropertyValue(
        FormatWriter writer,
        ImmutableArray<GodotPropertyData> properties,
        ImmutableArray<GodotFieldData> fields
    )
    {
        bool allWriteOnly = fields.IsDefaultOrEmpty && properties.All(d => d.PropertySymbol.IsWriteOnly);

        if (!allWriteOnly)
        {
            writer.PushIndent();
            writer.Write(
                """
                /// <inheritdoc/>
                [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
                protected override bool GetGodotClassPropertyValue(in godot_string_name name, out godot_variant value)
                {
                """
            );
            writer.PushIndent();

            foreach (var property in properties)
            {
                if (property.PropertySymbol.IsWriteOnly) continue;

                WritePropertyGetter(writer, property.PropertySymbol.Name, property.PropertySymbol.Type, property.Type);
            }

            foreach (var field in fields)
            {
                WritePropertyGetter(writer, field.FieldSymbol.Name, field.FieldSymbol.Type, field.Type);
            }

            writer.PopIndent();
            writer.Write(
                """
                   return base.GetGodotClassPropertyValue(name, out value);
                }
                """
            );
            writer.PopIndent();
        }

        return writer;
    }

    static FormatWriter WriteGetPropertyList(
        FormatWriter writer,
        SourceProductionContext context,
        MarshalUtils.TypeCache typeCache,
        bool isToolClass,
        Compilation compilation,
        ImmutableArray<GodotPropertyOrFieldData> data
    )
    {
        const string DictionaryType = "global::System.Collections.Generic.List<global::Godot.Bridge.PropertyInfo>";

        writer.Write(
            """
            /// <summary>
            /// Get the property information for all the properties declared in this class.
            /// This method is used by Godot to register the available properties in the editor.
            /// Do not call this method.
            /// </summary>
            [global::System.ComponentModel.EditorBrowsable(global::System.ComponentModel.EditorBrowsableState.Never)]
            internal new static {0} GetGodotPropertyList()
            {
                var properties = new {0}();
            """,
            DictionaryType
        );
        writer.PushIndent();

        foreach (var member in data)
        {
            foreach (var groupingInfo in DetermineGroupingPropertyInfo(member.Symbol))
                WriteGroupingPropertyInfo(writer, groupingInfo);

            var info = DeterminePropertyInfo(context, typeCache, member.Symbol, member.Type, compilation);

            if (info is null) continue;

            if (info.Value.Hint == PropertyHint.ToolButton && !isToolClass)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Common.OnlyToolClassesShouldUseExportToolButtonRule,
                        member.Symbol.Locations.FirstLocationWithSourceTreeOrDefault(),
                        member.Symbol.ToDisplayString()
                    )
                );

                continue;
            }

            WritePropertyInfo(writer, info.Value);
        }

        writer.PopIndent();
        writer.Write(
            """
               return properties;
            }
            #pragma warning restore CS0109
            """
        );

        return writer;
    }

    static FormatWriter WritePropertySetter(
        FormatWriter writer,
        string propertyMemberName,
        ITypeSymbol propertyType,
        MarshalType marshalType
    )
    {
        writer.Write(
            """
            if (name == PropertyName.@{0})
            {
               this.@{0} = {1};
               return true;
            }
            """,
            propertyMemberName,
            MarshalUtils.GetNativeVariantToManagedExpr("value" + propertyMemberName, propertyType, marshalType)
        );

        return writer;
    }

    static FormatWriter WritePropertyGetter(
        FormatWriter writer,
        string propertyName,
        ITypeSymbol symbolType,
        MarshalType marshalType
    )
    {
        writer.Write(
            """
            if (name == PropertyName.@{0})
            {
                value = {1};
                return true;
            }
            """,
            propertyName,
            MarshalUtils.GetNativeVariantToManagedExpr("this.@" + propertyName, symbolType, marshalType)
        );

        return writer;
    }

    private static IEnumerable<PropertyInfo> DetermineGroupingPropertyInfo(ISymbol memberSymbol)
    {
        foreach (var attr in memberSymbol.GetAttributes())
        {
            PropertyUsageFlags? propertyUsage = attr.AttributeClass?.FullQualifiedNameOmitGlobal() switch
            {
                GodotClasses.ExportCategoryAttr => PropertyUsageFlags.Category,
                GodotClasses.ExportGroupAttr => PropertyUsageFlags.Group,
                GodotClasses.ExportSubgroupAttr => PropertyUsageFlags.Subgroup,
                _ => null
            };

            if (propertyUsage is null) continue;

            if (attr.ConstructorArguments.Length > 0 && attr.ConstructorArguments[0].Value is string name)
            {
                string? hintString = null;
                if (propertyUsage != PropertyUsageFlags.Category && attr.ConstructorArguments.Length > 1)
                    hintString = attr.ConstructorArguments[1].Value?.ToString();

                yield return new PropertyInfo(
                    VariantType.Nil,
                    name,
                    PropertyHint.None,
                    hintString,
                    propertyUsage.Value,
                    true
                );
            }
        }
    }

    private static FormatWriter WritePropertyInfo(FormatWriter writer, PropertyInfo info)
    {
        writer.Write(
            """
            properties.Add(new(type: (global::Godot.Variant.Type){0},
               name: "{1}",
               hint: (global::Godot.PropertyHint){2},
               hintString: "{3}",
               usage: (global::Godot.PropertyUsageFlags){4},
               exported: {5}));
            """,
            (int)info.Type,
            info.Name,
            (int)info.Hint,
            info.HintString ?? "",
            (int)info.Usage,
            info.Exported ? "true" : "false"
        );

        return writer;
    }

    private static FormatWriter WriteGroupingPropertyInfo(FormatWriter writer, PropertyInfo info) =>
        WritePropertyInfo(
            writer,
            new(VariantType.Nil, info.Name, PropertyHint.None, info.HintString, info.Usage, true)
        );

    static PropertyInfo? DeterminePropertyInfo(
        SourceProductionContext context,
        MarshalUtils.TypeCache typeCache,
        ISymbol memberSymbol,
        MarshalType marshalType,
        Compilation compilation
    )
    {
        var exportAttr = memberSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.IsGodotExportAttribute() ?? false);

        var exportToolButtonAttr = memberSymbol.GetAttributes()
            .FirstOrDefault(a => a.AttributeClass?.IsGodotExportToolButtonAttribute() ?? false);

        if (exportAttr != null && exportToolButtonAttr != null)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    Common.ExportToolButtonShouldNotBeUsedWithExportRule,
                    memberSymbol.Locations.FirstLocationWithSourceTreeOrDefault(),
                    memberSymbol.ToDisplayString()
                )
            );

            return null;
        }

        var propertySymbol = memberSymbol as IPropertySymbol;
        var fieldSymbol = memberSymbol as IFieldSymbol;

        if (exportAttr != null && propertySymbol != null)
        {
            if (propertySymbol.GetMethod == null ||
                propertySymbol.SetMethod == null ||
                propertySymbol.SetMethod.IsInitOnly)
            {
                // Exports can be neither read-only nor write-only but the diagnostic errors for properties are already
                // reported by ScriptPropertyDefValGenerator.cs so just quit early here.
                return null;
            }
        }

        if (exportToolButtonAttr != null && propertySymbol != null && propertySymbol.GetMethod == null)
        {
            context.ReportDiagnostic(
                Diagnostic.Create(
                    Common.ExportedPropertyIsWriteOnlyRule,
                    propertySymbol.Locations.FirstLocationWithSourceTreeOrDefault(),
                    propertySymbol.ToDisplayString()
                )
            );

            return null;
        }

        if (exportToolButtonAttr != null && propertySymbol != null)
        {
            if (!PropertyIsExpressionBodiedAndReturnsNewCallable(compilation, propertySymbol))
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Common.ExportToolButtonMustBeExpressionBodiedProperty,
                        propertySymbol.Locations.FirstLocationWithSourceTreeOrDefault(),
                        propertySymbol.ToDisplayString()
                    )
                );

                return null;
            }

            static bool PropertyIsExpressionBodiedAndReturnsNewCallable(
                Compilation compilation,
                IPropertySymbol? propertySymbol
            )
            {
                if (propertySymbol == null)
                {
                    return false;
                }

                var propertyDeclarationSyntax = propertySymbol.DeclaringSyntaxReferences
                    .Select(r => r.GetSyntax() as PropertyDeclarationSyntax)
                    .FirstOrDefault();

                if (propertyDeclarationSyntax == null || propertyDeclarationSyntax.Initializer != null)
                {
                    return false;
                }

                if (propertyDeclarationSyntax.AccessorList != null)
                {
                    var accessors = propertyDeclarationSyntax.AccessorList.Accessors;

                    foreach (var accessor in accessors)
                    {
                        if (!accessor.IsKind(SyntaxKind.GetAccessorDeclaration))
                        {
                            // Only getters are allowed.
                            return false;
                        }

                        if (!ExpressionBodyReturnsNewCallable(compilation, accessor.ExpressionBody))
                        {
                            return false;
                        }
                    }
                }
                else if (!ExpressionBodyReturnsNewCallable(compilation, propertyDeclarationSyntax.ExpressionBody))
                {
                    return false;
                }

                return true;
            }

            static bool ExpressionBodyReturnsNewCallable(
                Compilation compilation,
                ArrowExpressionClauseSyntax? expressionSyntax
            )
            {
                if (expressionSyntax == null)
                {
                    return false;
                }

                var semanticModel = compilation.GetSemanticModel(expressionSyntax.SyntaxTree);

                switch (expressionSyntax.Expression)
                {
                    case ImplicitObjectCreationExpressionSyntax creationExpression:
                        // We already validate that the property type must be 'Callable'
                        // so we can assume this constructor is valid.
                        return true;

                    case ObjectCreationExpressionSyntax creationExpression:
                        var typeSymbol = ModelExtensions.GetSymbolInfo(semanticModel, creationExpression.Type).Symbol as ITypeSymbol;

                        if (typeSymbol != null)
                        {
                            return typeSymbol.FullQualifiedNameOmitGlobal() == GodotClasses.Callable;
                        }

                        break;

                    case InvocationExpressionSyntax invocationExpression:
                        var methodSymbol = ModelExtensions.GetSymbolInfo(semanticModel, invocationExpression).Symbol as IMethodSymbol;

                        if (methodSymbol != null && methodSymbol.Name == "From")
                        {
                            return methodSymbol.ContainingType.FullQualifiedNameOmitGlobal() == GodotClasses.Callable;
                        }

                        break;
                }

                return false;
            }
        }

        var memberType = propertySymbol?.Type ?? fieldSymbol!.Type;

        var memberVariantType = MarshalUtils.ConvertMarshalTypeToVariantType(marshalType)!.Value;
        string memberName = memberSymbol.Name;

        string? hintString = null;

        if (exportToolButtonAttr != null)
        {
            if (memberVariantType != VariantType.Callable)
            {
                context.ReportDiagnostic(
                    Diagnostic.Create(
                        Common.ExportToolButtonIsNotCallableRule,
                        memberSymbol.Locations.FirstLocationWithSourceTreeOrDefault(),
                        memberSymbol.ToDisplayString()
                    )
                );

                return null;
            }

            hintString = exportToolButtonAttr.ConstructorArguments[0].Value?.ToString() ?? "";

            foreach (var namedArgument in exportToolButtonAttr.NamedArguments)
            {
                if (namedArgument is { Key: "Icon", Value.Value: string { Length: > 0 } })
                {
                    hintString += $",{namedArgument.Value.Value}";
                }
            }

            return new PropertyInfo(
                memberVariantType,
                memberName,
                PropertyHint.ToolButton,
                hintString: hintString,
                PropertyUsageFlags.Editor,
                exported: true
            );
        }

        if (exportAttr == null)
        {
            return new PropertyInfo(
                memberVariantType,
                memberName,
                PropertyHint.None,
                hintString: hintString,
                PropertyUsageFlags.ScriptVariable,
                exported: false
            );
        }

        if (!TryGetMemberExportHint(
                typeCache,
                memberType,
                exportAttr,
                memberVariantType,
                isTypeArgument: false,
                out var hint,
                out hintString
            ))
        {
            var constructorArguments = exportAttr.ConstructorArguments;

            if (constructorArguments.Length > 0)
            {
                var hintValue = exportAttr.ConstructorArguments[0].Value;

                hint = hintValue switch
                {
                    null => PropertyHint.None,
                    int intValue => (PropertyHint)intValue,
                    _ => (PropertyHint)(long)hintValue
                };

                hintString = constructorArguments.Length > 1
                    ? exportAttr.ConstructorArguments[1].Value?.ToString()
                    : null;
            }
            else
            {
                hint = PropertyHint.None;
            }
        }

        var propUsage = PropertyUsageFlags.Default | PropertyUsageFlags.ScriptVariable;

        if (memberVariantType == VariantType.Nil) propUsage |= PropertyUsageFlags.NilIsVariant;

        return new PropertyInfo(memberVariantType, memberName, hint, hintString, propUsage, exported: true);
    }

    private static bool TryGetMemberExportHint(
        MarshalUtils.TypeCache typeCache,
        ITypeSymbol type,
        AttributeData exportAttr,
        VariantType variantType,
        bool isTypeArgument,
        out PropertyHint hint,
        out string? hintString
    )
    {
        hint = PropertyHint.None;
        hintString = null;

        if (variantType == VariantType.Nil) return true; // Variant, no export hint

        if (variantType == VariantType.Int && type.IsValueType && type.TypeKind == TypeKind.Enum)
        {
            bool hasFlagsAttr = type.GetAttributes().Any(a => a.AttributeClass?.IsSystemFlagsAttribute() ?? false);

            hint = hasFlagsAttr ? PropertyHint.Flags : PropertyHint.Enum;

            var members = type.GetMembers();

            var enumFields = members
                .Where(s => s.Kind == SymbolKind.Field &&
                    s.IsStatic &&
                    s.DeclaredAccessibility == Accessibility.Public &&
                    !s.IsImplicitlyDeclared
                )
                .Cast<IFieldSymbol>()
                .ToArray();

            var hintStringBuilder = new StringBuilder();
            var nameOnlyHintStringBuilder = new StringBuilder();

            // True: enum Foo { Bar, Baz, Qux }
            // True: enum Foo { Bar = 0, Baz = 1, Qux = 2 }
            // False: enum Foo { Bar = 0, Baz = 7, Qux = 5 }
            bool usesDefaultValues = true;

            for (int i = 0; i < enumFields.Length; i++)
            {
                var enumField = enumFields[i];

                if (i > 0)
                {
                    hintStringBuilder.Append(",");
                    nameOnlyHintStringBuilder.Append(",");
                }

                string enumFieldName = enumField.Name;
                hintStringBuilder.Append(enumFieldName);
                nameOnlyHintStringBuilder.Append(enumFieldName);

                long val = enumField.ConstantValue switch
                {
                    sbyte v => v,
                    short v => v,
                    int v => v,
                    long v => v,
                    byte v => v,
                    ushort v => v,
                    uint v => v,
                    ulong v => (long)v,
                    _ => 0
                };

                uint expectedVal = (uint)(hint == PropertyHint.Flags ? 1 << i : i);
                if (val != expectedVal) usesDefaultValues = false;

                hintStringBuilder.Append(":");
                hintStringBuilder.Append(val);
            }

            hintString = !usesDefaultValues
                ? hintStringBuilder.ToString()
                :
                // If we use the format NAME:VAL, that's what the editor displays.
                // That's annoying if the user is not using custom values for the enum constants.
                // This may not be needed in the future if the editor is changed to not display values.
                nameOnlyHintStringBuilder.ToString();

            return true;
        }

        if (variantType == VariantType.Object && type is INamedTypeSymbol memberNamedType)
        {
            if (TryGetNodeOrResourceType(exportAttr, out hint, out hintString))
            {
                return true;
            }

            if (memberNamedType.InheritsFrom("GodotSharp", "Godot.Resource"))
            {
                hint = PropertyHint.ResourceType;
                hintString = GetTypeName(memberNamedType);

                return true;
            }

            if (memberNamedType.InheritsFrom("GodotSharp", "Godot.Node"))
            {
                hint = PropertyHint.NodeType;
                hintString = GetTypeName(memberNamedType);

                return true;
            }
        }

        static bool TryGetNodeOrResourceType(AttributeData exportAttr, out PropertyHint hint, out string? hintString)
        {
            hint = PropertyHint.None;
            hintString = null;

            if (exportAttr.ConstructorArguments.Length <= 1) return false;

            var hintValue = exportAttr.ConstructorArguments[0].Value;

            var hintEnum = hintValue switch
            {
                null => PropertyHint.None,
                int intValue => (PropertyHint)intValue,
                _ => (PropertyHint)(long)hintValue
            };

            if (!hintEnum.HasFlag(PropertyHint.NodeType) && !hintEnum.HasFlag(PropertyHint.ResourceType)) return false;

            var hintStringValue = exportAttr.ConstructorArguments[1].Value?.ToString();

            if (string.IsNullOrWhiteSpace(hintStringValue))
            {
                return false;
            }

            hint = hintEnum;
            hintString = hintStringValue;

            return true;
        }

        static string GetTypeName(INamedTypeSymbol memberSymbol)
        {
            if (memberSymbol.GetAttributes().Any(a => a.AttributeClass?.IsGodotGlobalClassAttribute() ?? false))
            {
                return memberSymbol.Name;
            }

            return memberSymbol.GetGodotScriptNativeClassName()!;
        }

        static bool GetStringArrayEnumHint(
            VariantType elementVariantType,
            AttributeData exportAttr,
            out string? hintString
        )
        {
            var constructorArguments = exportAttr.ConstructorArguments;

            if (constructorArguments.Length > 0)
            {
                var presetHintValue = exportAttr.ConstructorArguments[0].Value;

                PropertyHint presetHint = presetHintValue switch
                {
                    null => PropertyHint.None,
                    int intValue => (PropertyHint)intValue,
                    _ => (PropertyHint)(long)presetHintValue
                };

                if (presetHint == PropertyHint.Enum)
                {
                    string? presetHintString = constructorArguments.Length > 1
                        ? exportAttr.ConstructorArguments[1].Value?.ToString()
                        : null;

                    hintString = (int)elementVariantType + "/" + (int)PropertyHint.Enum + ":";

                    if (presetHintString != null) hintString += presetHintString;

                    return true;
                }
            }

            hintString = null;

            return false;
        }

        if (!isTypeArgument && variantType == VariantType.Array)
        {
            var elementType = MarshalUtils.GetArrayElementType(type);

            if (elementType == null) return false; // Non-generic Array, so there's no hint to add.

            if (elementType.TypeKind == TypeKind.TypeParameter)
                return false; // The generic is not constructed, we can't really hint anything.

            var elementMarshalType = MarshalUtils.ConvertManagedTypeToMarshalType(elementType, typeCache)!.Value;
            var elementVariantType = MarshalUtils.ConvertMarshalTypeToVariantType(elementMarshalType)!.Value;

            bool isPresetHint = false;

            if (elementVariantType == VariantType.String || elementVariantType == VariantType.StringName)
                isPresetHint = GetStringArrayEnumHint(elementVariantType, exportAttr, out hintString);

            if (!isPresetHint)
            {
                bool hintRes = TryGetMemberExportHint(
                    typeCache,
                    elementType,
                    exportAttr,
                    elementVariantType,
                    isTypeArgument: true,
                    out var elementHint,
                    out var elementHintString
                );

                // Format: type/hint:hint_string
                if (hintRes)
                {
                    hintString = (int)elementVariantType + "/" + (int)elementHint + ":";

                    if (elementHintString != null) hintString += elementHintString;
                }
                else
                {
                    hintString = (int)elementVariantType + "/" + (int)PropertyHint.None + ":";
                }
            }

            hint = PropertyHint.TypeString;

            return hintString != null;
        }

        if (!isTypeArgument && variantType == VariantType.PackedStringArray)
        {
            if (GetStringArrayEnumHint(VariantType.String, exportAttr, out hintString))
            {
                hint = PropertyHint.TypeString;

                return true;
            }
        }

        if (!isTypeArgument && variantType == VariantType.Dictionary)
        {
            var elementTypes = MarshalUtils.GetGenericElementTypes(type);

            if (elementTypes == null) return false; // Non-generic Dictionary, so there's no hint to add

            Debug.Assert(elementTypes.Length == 2);

            var keyElementMarshalType = MarshalUtils.ConvertManagedTypeToMarshalType(elementTypes[0], typeCache);
            var valueElementMarshalType = MarshalUtils.ConvertManagedTypeToMarshalType(elementTypes[1], typeCache);

            if (keyElementMarshalType == null || valueElementMarshalType == null)
            {
                // To maintain compatibility with previous versions of Godot before 4.4,
                // we must preserve the old behavior for generic dictionaries with non-marshallable
                // generic type arguments.
                return false;
            }

            var keyElementVariantType =
                MarshalUtils.ConvertMarshalTypeToVariantType(keyElementMarshalType.Value)!.Value;
            var keyIsPresetHint = false;
            var keyHintString = (string?)null;

            if (keyElementVariantType == VariantType.String || keyElementVariantType == VariantType.StringName)
                keyIsPresetHint = GetStringArrayEnumHint(keyElementVariantType, exportAttr, out keyHintString);

            if (!keyIsPresetHint)
            {
                bool hintRes = TryGetMemberExportHint(
                    typeCache,
                    elementTypes[0],
                    exportAttr,
                    keyElementVariantType,
                    isTypeArgument: true,
                    out var keyElementHint,
                    out var keyElementHintString
                );

                // Format: type/hint:hint_string
                if (hintRes)
                {
                    keyHintString = (int)keyElementVariantType + "/" + (int)keyElementHint + ":";

                    if (keyElementHintString != null) keyHintString += keyElementHintString;
                }
                else
                {
                    keyHintString = (int)keyElementVariantType + "/" + (int)PropertyHint.None + ":";
                }
            }

            var valueElementVariantType =
                MarshalUtils.ConvertMarshalTypeToVariantType(valueElementMarshalType.Value)!.Value;
            var valueIsPresetHint = false;
            var valueHintString = (string?)null;

            if (valueElementVariantType == VariantType.String || valueElementVariantType == VariantType.StringName)
                valueIsPresetHint = GetStringArrayEnumHint(valueElementVariantType, exportAttr, out valueHintString);

            if (!valueIsPresetHint)
            {
                bool hintRes = TryGetMemberExportHint(
                    typeCache,
                    elementTypes[1],
                    exportAttr,
                    valueElementVariantType,
                    isTypeArgument: true,
                    out var valueElementHint,
                    out var valueElementHintString
                );

                // Format: type/hint:hint_string
                if (hintRes)
                {
                    valueHintString = (int)valueElementVariantType + "/" + (int)valueElementHint + ":";

                    if (valueElementHintString != null) valueHintString += valueElementHintString;
                }
                else
                {
                    valueHintString = (int)valueElementVariantType + "/" + (int)PropertyHint.None + ":";
                }
            }

            hint = PropertyHint.TypeString;

            hintString = keyHintString != null && valueHintString != null ? $"{keyHintString};{valueHintString}" : null;

            return hintString != null;
        }

        return false;
    }
}
