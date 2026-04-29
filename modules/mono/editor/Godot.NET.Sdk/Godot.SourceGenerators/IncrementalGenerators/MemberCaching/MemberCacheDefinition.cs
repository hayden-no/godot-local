using System.Collections.Generic;

namespace Godot.SourceGenerators.MemberCaching;

public record MemberCacheDefinition
{
    public TypeDeclaration Declaration { get; set; }

    public IList<Pragma> Pragmas { get; } = [Pragma.Warnings.CS0109];
    public XmlComment? XmlComment { get; set; }
    public string XmlCommentMemberTypes { get; set; } = "[null]";
}