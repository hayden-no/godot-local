using System;

namespace JetBrains.Annotations;

/// <summary>
/// Indicates that the marked element must be excluded from CQRS-related analysis.
/// </summary>
/// <example><code>
/// public class User
/// {
///   private string Name;
///
///   [CqrsCommand]
///   public void SetUserNameCommand(string newName)
///   {
///     if (newName == GetUserName()) // Warning about 'GetUserName' being called from Command but belongs to the Query
///       return;
///
///     Name = newName;
///     CheckName();
///   }
///
///   [CqrsQuery]
///   public string GetUserName() // Suggestion to rename it to the 'GetUserNameQuery'
///   {
///     CheckName();
///     return Name;
///   }
///
///   [CqrsExcludeFromAnalysis]
///   public bool CheckName() // Although this method is used both in Command and Query, will be ignored in all CQRS analyzes
///   {
///     ...
///   }
/// }
/// </code></example>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface |
    AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property)]
internal sealed class CqrsExcludeFromAnalysisAttribute : Attribute { }
