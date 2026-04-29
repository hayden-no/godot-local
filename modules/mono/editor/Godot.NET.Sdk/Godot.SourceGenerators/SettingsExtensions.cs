using System.IO;
using Microsoft.CodeAnalysis.Diagnostics;

namespace Godot.SourceGenerators;

internal static class SettingsExtensions
{
    private const string MsBuildPropertyPrefix = "build_property.";

    private const string Setting_ProjectDir = MsBuildPropertyPrefix + "ProjectDir";
    private const string Setting_ProjectName = MsBuildPropertyPrefix + "ProjectName";
    private const string Setting_SolutionDir = MsBuildPropertyPrefix + "SolutionDir";
    private const string Setting_SolutionName = MsBuildPropertyPrefix + "SolutionName";
    private const string Setting_Configuration = MsBuildPropertyPrefix + "Configuration";
    private const string Setting_Platform = MsBuildPropertyPrefix + "Platform";
    private const string Setting_OutputPath = MsBuildPropertyPrefix + "OutputPath";
    private const string Setting_RootNamespace = MsBuildPropertyPrefix + "RootNameSpace";

    extension(AnalyzerConfigOptionsProvider provider)
    {
        public string? ProjectDir => provider.GlobalOptions.GetValueOrDefault(Setting_ProjectDir);
        public string? ProjectName => provider.GlobalOptions.GetValueOrDefault(Setting_ProjectName);

        public string SolutionDir =>
            provider.GlobalOptions.GetValueOrDefault(Setting_SolutionDir, Path.DirectorySeparatorChar.ToString())!;

        public string? SolutionName => provider.GlobalOptions.GetValueOrDefault(Setting_SolutionName);
        public string? Configuration => provider.GlobalOptions.GetValueOrDefault(Setting_Configuration);
        public string? Platform => provider.GlobalOptions.GetValueOrDefault(Setting_Platform);
        public string? OutputPath => provider.GlobalOptions.GetValueOrDefault(Setting_OutputPath);
        public string? RootNamespace => provider.GlobalOptions.GetValueOrDefault(Setting_RootNamespace);
    }

    extension(AnalyzerConfigOptions options)
    {
        public string? GetValueOrDefault(string key, string? defaultValue = null)
        {
            return options.TryGetValue(key, out var value) ? value : defaultValue;
        }
    }
}