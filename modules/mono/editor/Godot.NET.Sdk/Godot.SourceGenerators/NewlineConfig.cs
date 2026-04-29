namespace Godot.SourceGenerators;

public record  NewlineConfig
{
    public string Newline { get; init; }
    public string DefaultIndent { get; init; }
    public bool ApplyIndentToEmptyLines { get; init; }

    public NewlineConfig()
    {
        Newline = "\n";
        DefaultIndent = "    ";
        ApplyIndentToEmptyLines = true;
    }
}