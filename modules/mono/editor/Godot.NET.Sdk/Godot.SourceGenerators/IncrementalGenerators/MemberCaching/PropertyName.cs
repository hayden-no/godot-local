using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace Godot.SourceGenerators.MemberCaching;

public record PropertyName : BaseMemberCache
{
    const string CacheName = "PropertyName";

    public PropertyName(TypeName baseType ,IEnumerable<CacheEntry> entries) : base(baseType, entries)
    {
        Definition = new()
        {
            XmlComment = DefaultXmlComment,
            XmlCommentMemberTypes = "properties and fields",
            Declaration = new TypeDeclaration(CacheName, ["public", "new", "partial"], "class", new InheritanceDeclaration(BaseType.CreateChild(CacheName), default), default)
        };
    }

    /// <inheritdoc />
    public override MemberCacheDefinition Definition { get; }




}
