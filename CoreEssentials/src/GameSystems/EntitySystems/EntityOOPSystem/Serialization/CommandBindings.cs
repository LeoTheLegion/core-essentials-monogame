using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// Declarative event-to-command wiring for XML-driven entities, mirroring how
/// Unity wires component events in scene files. A &lt;Bind&gt; element on an
/// &lt;EntityDefinition&gt; subscribes a public method (the command handler) to a public
/// event, so interactive scenes can be fully data-driven without per-button
/// FindById + subscribe code in a scene class.
/// </summary>
/// <remarks>
/// Two equivalent forms are supported:
/// <code>
/// &lt;!-- Explicit target + member (Unity PersistentCall style) --&gt;
/// &lt;Bind Event="Clicked" Target="MenuActions" Member="StartGame" /&gt;
///
/// &lt;!-- Named command resolved by search (Unity SendMessage style) --&gt;
/// &lt;Bind Event="Clicked" Command="StartGame" /&gt;
/// </code>
/// Resolution searches the owning entity first, then its components, then ancestors
/// (nearest first); the explicit form restricts the search to the named component type.
/// Handler methods must be public instance methods with zero parameters, or one parameter
/// that receives the event's payload. Supported event signatures: <c>Action</c>,
/// <c>Action&lt;T&gt;</c> and <c>EventHandler</c>. Unresolvable binds log a
/// [Serialization] console warning and are skipped — they never throw.
/// </remarks>
public static class CommandBindings
{
    /// <summary>
    /// Applies all &lt;Bind&gt; elements declared on the given entity definition to the entity.
    /// Binds may be direct children of the definition or nested inside its &lt;Components&gt;
    /// element (either as siblings of &lt;Component&gt; elements or inside one).
    /// Must be called after all components on the entity are attached.
    /// </summary>
    /// <param name="entity">The entity to wire up.</param>
    /// <param name="entityDef">The &lt;EntityDefinition&gt;/&lt;Entity&gt; element it was loaded from.</param>
    public static void ApplyBindings(Entity entity, XElement entityDef)
    {
        var binds = new List<XElement>(entityDef.Elements("Bind"));

        var componentsElement = entityDef.Element("Components");
        if (componentsElement != null)
        {
            foreach (var child in componentsElement.Elements())
            {
                if (child.Name.LocalName == "Bind")
                    binds.Add(child);
                else if (child.Name.LocalName == "Component")
                    binds.AddRange(child.Elements("Bind"));
            }
        }

        foreach (var bind in binds)
            ApplySingleBinding(entity, bind);
    }

    private static void ApplySingleBinding(Entity entity, XElement bind)
    {
        var eventName = bind.Attribute("Event")?.Value;
        if (string.IsNullOrWhiteSpace(eventName))
        {
            Console.WriteLine($"[Serialization] <Bind> on entity {Describe(entity)} is missing the required Event attribute — skipped.");
            return;
        }

        var sourceName = bind.Attribute("Source")?.Value;
        if (!TryResolveEvent(entity, eventName, sourceName, out var owner, out var eventInfo))
        {
            Console.WriteLine($"[Serialization] Could not find public event '{eventName}'{(string.IsNullOrWhiteSpace(sourceName) ? "" : $" on '{sourceName}'")} for entity {Describe(entity)} — bind skipped.");
            return;
        }

        if (!TryResolveHandler(entity, bind, out var target, out var method))
            return; // logs its own warning

        var bridge = CreateBridge(eventInfo, target, method);
        if (bridge == null)
        {
            Console.WriteLine($"[Serialization] Event '{eventName}' ({eventInfo.EventHandlerType}) and handler '{method.DeclaringType?.Name}.{method.Name}' have incompatible signatures — bind skipped.");
            return;
        }

        eventInfo.AddEventHandler(owner, bridge);
    }

    /// <summary>
    /// Resolves the public event to subscribe to. When a source name is given, only that
    /// component (or the entity itself) is considered; otherwise the entity and all of its
    /// components are searched.
    /// </summary>
    private static bool TryResolveEvent(Entity entity, string eventName, string? sourceName, out object owner, out EventInfo eventInfo)
    {
        var candidates = new List<object>();
        if (string.IsNullOrWhiteSpace(sourceName))
        {
            candidates.Add(entity);
            candidates.AddRange(entity.Components.Cast<object>());
        }
        else
        {
            if (MatchesTypeName(entity.GetType(), sourceName))
                candidates.Add(entity);

            var component = FindComponentByName(entity, sourceName);
            if (component != null)
                candidates.Add(component);
        }

        foreach (var candidate in candidates)
        {
            var info = candidate.GetType().GetEvent(eventName, BindingFlags.Public | BindingFlags.Instance);
            if (info != null)
            {
                owner = candidate;
                eventInfo = info;
                return true;
            }
        }

        owner = null!;
        eventInfo = null!;
        return false;
    }

    /// <summary>
    /// Resolves the handler method to invoke when the bound event fires.
    /// Both forms walk the entity, its components, and then ancestors (nearest first):
    /// the Command form searches every owner for a method with that name, while the
    /// Target+Member form restricts the search to the named type.
    /// </summary>
    private static bool TryResolveHandler(Entity entity, XElement bind, out object target, out MethodInfo method)
    {
        var member = bind.Attribute("Member")?.Value;
        var command = bind.Attribute("Command")?.Value;
        var targetName = bind.Attribute("Target")?.Value;

        string memberName;
        if (!string.IsNullOrWhiteSpace(member))
        {
            if (string.IsNullOrWhiteSpace(targetName))
            {
                Console.WriteLine($"[Serialization] <Bind> on entity {Describe(entity)} specifies Member='{member}' without a Target — skipped.");
                target = null!;
                method = null!;
                return false;
            }
            memberName = member;
        }
        else if (!string.IsNullOrWhiteSpace(command))
        {
            memberName = command;
            targetName = null; // search all owners
        }
        else
        {
            Console.WriteLine($"[Serialization] <Bind> on entity {Describe(entity)} must specify either Command or Target+Member — skipped.");
            target = null!;
            method = null!;
            return false;
        }

        var current = entity;
        while (current != null)
        {
            // 1. The entity itself (explicit form: only if its type matches Target).
            if (string.IsNullOrWhiteSpace(targetName) || MatchesTypeName(current.GetType(), targetName))
            {
                var entityMethod = FindHandlerMethod(current.GetType(), memberName);
                if (entityMethod != null)
                {
                    target = current;
                    method = entityMethod;
                    return true;
                }
            }

            // 2. Its components (explicit form: only the matching type).
            foreach (var component in current.Components)
            {
                if (!string.IsNullOrWhiteSpace(targetName) && !MatchesTypeName(component.GetType(), targetName))
                    continue;

                var componentMethod = FindHandlerMethod(component.GetType(), memberName);
                if (componentMethod != null)
                {
                    target = component;
                    method = componentMethod;
                    return true;
                }
            }

            // 3. Walk up so a single behavior component on a parent can serve every
            //    entity underneath it (shared state stays in one place).
            current = current.Parent;
        }

        Console.WriteLine($"[Serialization] Command '{memberName}'{(string.IsNullOrWhiteSpace(targetName) ? "" : $" on target '{targetName}'")} not found on entity {Describe(entity)} or its ancestors — bind skipped.");
        target = null!;
        method = null!;
        return false;
    }

    /// <summary>
    /// Builds a delegate matching the event's signature that adapts the invocation to the
    /// handler method (zero parameters, or one parameter receiving the event payload).
    /// Returns null when the signatures are incompatible. Exceptions thrown by the handler
    /// are caught and logged so a bad command can never take down the game loop.
    /// </summary>
    private static Delegate? CreateBridge(EventInfo eventInfo, object target, MethodInfo method)
    {
        var delegateType = eventInfo.EventHandlerType;
        if (delegateType == null)
            return null;

        var parameters = method.GetParameters();

        var hasPayload = false;
        Type? payloadType = null;

        if (delegateType == typeof(Action))
        {
            // No payload.
        }
        else if (delegateType.IsGenericType && delegateType.GetGenericTypeDefinition() == typeof(Action<>))
        {
            hasPayload = true;
            payloadType = delegateType.GetGenericArguments()[0];
        }
        else if (delegateType == typeof(EventHandler))
        {
            hasPayload = true;
            payloadType = typeof(EventArgs);
        }
        else
        {
            return null; // e.g. Func<T, bool> — not supported
        }

        if (parameters.Length > 1)
            return null;

        if (!hasPayload && parameters.Length == 1)
            return null; // nothing to deliver to a single-parameter handler

        if (hasPayload && parameters.Length == 1)
        {
            var paramType = parameters[0].ParameterType;
            if (!(paramType == typeof(object) || paramType.IsAssignableFrom(payloadType!)))
                return null;
        }

        var invoker = BuildInvoker(target, method);

        if (delegateType == typeof(Action))
            return new Action(() => invoker(null));

        if (delegateType == typeof(EventHandler))
            return new EventHandler((_, e) => invoker(e));

        // Action<T>
        var createOfT = typeof(CommandBindings).GetMethod(nameof(CreateActionOf), BindingFlags.NonPublic | BindingFlags.Static)!;
        var generic = createOfT.MakeGenericMethod(payloadType!);
        return (Delegate)generic.Invoke(null, new object[] { invoker })!;
    }

    private static Action<object?> BuildInvoker(object target, MethodInfo method)
    {
        var hasParam = method.GetParameters().Length > 0;

        return payload =>
        {
            try
            {
                if (hasParam)
                    method.Invoke(target, new[] { payload });
                else
                    method.Invoke(target, null);
            }
            catch (Exception ex)
            {
                var cause = ex is TargetInvocationException tie && tie.InnerException != null ? tie.InnerException : ex;
                Console.WriteLine($"[Serialization] Command handler '{method.DeclaringType?.Name}.{method.Name}' threw: {cause.Message}");
            }
        };
    }

    private static Action<T> CreateActionOf<T>(Action<object?> invoker) => payload => invoker(payload);

    private static MethodInfo? FindHandlerMethod(Type type, string name) =>
        type.GetMethods(BindingFlags.Public | BindingFlags.Instance)
            .FirstOrDefault(m => m.Name == name && !m.IsGenericMethodDefinition && m.GetParameters().Length <= 1);

    private static EntityComponent? FindComponentByName(Entity entity, string name) =>
        entity.Components.FirstOrDefault(c => MatchesTypeName(c.GetType(), name));

    private static bool MatchesTypeName(Type type, string name) =>
        type.Name.Equals(name, StringComparison.OrdinalIgnoreCase) || type.FullName == name;

    private static string Describe(Entity entity) =>
        string.IsNullOrEmpty(entity.Id) ? $"(type {entity.GetType().Name})" : $"'{entity.Id}'";
}
