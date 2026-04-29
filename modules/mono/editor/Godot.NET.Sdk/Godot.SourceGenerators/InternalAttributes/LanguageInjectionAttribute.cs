using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the marked parameter, field, or property accepts string literals
/// containing code fragments in a specified language.
/// </summary>
/// <example><code>
/// void Foo([LanguageInjection(InjectedLanguage.CSS, Prefix = "body{", Suffix = "}")] string cssProps)
/// {
///   // cssProps should only contain a list of CSS properties
/// }
/// </code></example>
/// <example><code>
/// void Bar([LanguageInjection("json")] string json)
/// {
/// }
/// </code></example>
[AttributeUsage(AttributeTargets.Parameter | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.ReturnValue)]
internal sealed class LanguageInjectionAttribute : Attribute
{
    public LanguageInjectionAttribute(InjectedLanguage injectedLanguage)
    {
        InjectedLanguage = injectedLanguage;
    }

    public LanguageInjectionAttribute([NotNull] string injectedLanguage)
    {
        InjectedLanguageName = injectedLanguage;
    }

    /// <summary>Specifies the language of the injected code fragment.</summary>
    public InjectedLanguage InjectedLanguage { get; }

    /// <summary>Specifies the language name of the injected code fragment.</summary>
    [CanBeNull] public string InjectedLanguageName { get; }

    /// <summary>Specifies the string that 'precedes' the injected string literal.</summary>
    [CanBeNull] public string Prefix { get; set; }

    /// <summary>Specifies the string that 'follows' the injected string literal.</summary>
    [CanBeNull] public string Suffix { get; set; }
}
