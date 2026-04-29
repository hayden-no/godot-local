namespace JetBrains.Annotations;

/// <summary>
/// Language of the injected code fragment inside a string literal marked by the <see cref="LanguageInjectionAttribute"/>.
/// </summary>
internal enum InjectedLanguage
{
    CSS = 0,
    HTML = 1,
    JAVASCRIPT = 2,
    JSON = 3,
    XML = 4
}
