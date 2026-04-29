using System;

namespace JetBrains.Annotations;

/// <summary>
/// A declaration marked with this attribute will be recognized as a CQRS QueryHandler.
/// Its naming and adherence to the CQRS pattern will be checked.
/// </summary>
/// <example><code>
/// [CqrsCommandHandler]
/// public class UserCommandHandler
/// {
///   public void Handle(SetUserNameCommand command)
///   {
///     var query = new GetUserNameQuery() { Id = command.Id }; // Warning about using Query inside Command
///     var handler = new UserQuery(); // Warning about using Query inside Command
///
///     if (command.Name == handler.Handle(query)) // Warning about using Query inside Command
///       return;
///
///     // ...
///   }
/// }
///
/// [CqrsQueryHandler]
/// public class UserQuery // Suggestion to rename to the 'UserQueryHandler'
/// {
///   public string Handle(GetUserNameQuery query)
///   {
///     return ...;
///   }
/// }
/// </code></example>
[AttributeUsage(
    AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Interface |
    AttributeTargets.Constructor | AttributeTargets.Method | AttributeTargets.Property)]
internal sealed class CqrsQueryHandlerAttribute : Attribute { }
