using System;

namespace JetBrains.Annotations;

/// <summary>
/// A declaration marked with this attribute will be recognized as a CQRS Query.
/// Its naming and adherence to the CQRS pattern will be checked.
/// </summary>
/// <example><code>
/// public class User
/// {
///   private string Name;
///
///   [CqrsCommand]
///   public void SetUserNameCommand(string newName)
///   {
///     if (newName == GetUserName()) // Warning about 'GetUserName' is called from Command but belongs to the Query
///       return;
///
///     Name = newName;
///   }
///
///   [CqrsQuery]
///   public string GetUserName() // Suggestion to rename it to the 'GetUserNameQuery'
///   {
///     return Name;
///   }
/// }
/// </code></example>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface |
    AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property)]
internal sealed class CqrsQueryAttribute : Attribute { }
