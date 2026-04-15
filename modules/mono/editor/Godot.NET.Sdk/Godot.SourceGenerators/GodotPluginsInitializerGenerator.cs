using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.Text;

namespace Godot.SourceGenerators
{
    [Generator]
    public class GodotPluginsInitializerGenerator : IIncrementalGenerator
    {
        const string GENERATOR_NAME = "GodotPluginsInitializer";

        private const string SCRIPT = """
                                      using System;
                                      using System.Runtime.InteropServices;
                                      using Godot.Bridge;
                                      using Godot.NativeInterop;

                                      namespace GodotPlugins.Game
                                      {
                                          internal static partial class Main
                                          {
                                              [UnmanagedCallersOnly(EntryPoint = "godotsharp_game_main_init")]
                                              private static godot_bool InitializeFromGameProject(IntPtr godotDllHandle, IntPtr outManagedCallbacks,
                                                  IntPtr unmanagedCallbacks, int unmanagedCallbacksSize)
                                              {
                                                  try
                                                  {
                                                      DllImportResolver dllImportResolver = new GodotDllImportResolver(godotDllHandle).OnResolveDllImport;

                                                      var coreApiAssembly = typeof(global::Godot.GodotObject).Assembly;

                                                      NativeLibrary.SetDllImportResolver(coreApiAssembly, dllImportResolver);

                                                      NativeFuncs.Initialize(unmanagedCallbacks, unmanagedCallbacksSize);

                                                      ManagedCallbacks.Create(outManagedCallbacks);

                                                      ScriptManagerBridge.LookupScriptsInAssembly(typeof(global::GodotPlugins.Game.Main).Assembly);

                                                      return godot_bool.True;
                                                  }
                                                  catch (Exception e)
                                                  {
                                                      global::System.Console.Error.WriteLine(e);
                                                      return false.ToGodotBool();
                                                  }
                                              }
                                          }
                                      }

                                      """;

        /// <inheritdoc />
        public void Initialize(IncrementalGeneratorInitializationContext context)
        {
            var settings = context.AnalyzerConfigOptionsProvider.Select((o, ct) =>
                {
                    var IsToolBuild = o.IsToolsProject();
                    var IsEnabled = o.IsSourceGenEnabled(GENERATOR_NAME);

                    return new { IsToolBuild, IsEnabled };
                }
            );

            context.RegisterSourceOutput(settings,
                (ctx, options) =>
                {
                    if (!options.IsEnabled || !options.IsToolBuild) return;

                    ctx.AddSource("GodotPlugins.Game.generated", SourceText.From(SCRIPT, Encoding.UTF8));
                });
        }
    }
}
