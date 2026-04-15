using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators
{
    // Placeholder. Once we switch to native extensions this will act as the registrar for all
    // user Godot classes in the assembly. Think of it as something similar to `register_types`.
    public class ScriptRegistrarGenerator : IIncrementalGenerator
    {
        /// <inheritdoc />
        public void Initialize(IncrementalGeneratorInitializationContext context) => throw new System.NotImplementedException();
    }
}
