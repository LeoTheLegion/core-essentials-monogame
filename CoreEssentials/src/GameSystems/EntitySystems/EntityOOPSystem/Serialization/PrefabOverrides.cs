using System;
using System.Collections.Generic;
using System.Linq;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

/// <summary>
/// Applies per-instantiation property overrides to a prefab. Overrides are merged into a
/// deep copy of the prefab so the cached prefab is never mutated — every instantiation
/// sees its own final values, and components observe them in <c>OnAttach</c>.
/// </summary>
public static class PrefabOverrides
{
    /// <summary>
    /// Returns a copy of <paramref name="prefab"/> with the given overrides merged into its
    /// component definitions. Component keys may be short names or fully-qualified type names;
    /// resolution reuses the same reflection path as normal prefab instantiation.
    /// </summary>
    /// <param name="prefab">The prefab to override (never mutated).</param>
    /// <param name="overrides">
    /// Map of component type name → property name → value string. A key matching an existing
    /// component merges into it; a key matching no component adds a new component definition.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="prefab"/> is null.</exception>
    /// <exception cref="FormatException">Thrown when a component type cannot be resolved.</exception>
    public static Prefab Apply(Prefab prefab, IReadOnlyDictionary<string, Dictionary<string, string>>? overrides)
        => Apply(prefab, overrides, null);

    /// <summary>
    /// Returns a copy of <paramref name="prefab"/> with the given component and entity-level
    /// overrides merged into it. Component keys may be short names or fully-qualified type names;
    /// resolution reuses the same reflection path as normal prefab instantiation. Entity-level
    /// overrides target writable public properties on the entity itself (not a component).
    /// </summary>
    /// <param name="prefab">The prefab to override (never mutated).</param>
    /// <param name="overrides">
    /// Map of component type name → property name → value string. A key matching an existing
    /// component merges into it; a key matching no component adds a new component definition.
    /// </param>
    /// <param name="entityOverrides">
    /// Optional map of entity property name → value string, applied to the created entity before
    /// <c>OnStart</c>/<c>OnAttach</c>. Merged into the prefab's <see cref="Prefab.EntityOverrides"/>.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="prefab"/> is null.</exception>
    /// <exception cref="FormatException">Thrown when a component type cannot be resolved.</exception>
    public static Prefab Apply(Prefab prefab,
        IReadOnlyDictionary<string, Dictionary<string, string>>? overrides,
        IReadOnlyDictionary<string, string>? entityOverrides)
    {
        if (prefab == null) throw new ArgumentNullException(nameof(prefab));
        var hasComponent = overrides != null && overrides.Count > 0;
        var hasEntity = entityOverrides != null && entityOverrides.Count > 0;
        if (!hasComponent && !hasEntity) return prefab;

        var clone = prefab.Clone();

        foreach (var (name, value) in entityOverrides ?? new Dictionary<string, string>())
            clone.EntityOverrides[name] = value;

        if (overrides == null) return clone;

        foreach (var (componentKey, properties) in overrides)
        {
            Type? resolvedType = EntityPrefabLoader.ResolveComponentType(componentKey);
            if (resolvedType == null)
                throw new FormatException($"Prefab override references unresolvable component type '{componentKey}'.");

            var target = clone.Components.FirstOrDefault(c => string.Equals(c.Type, resolvedType.FullName, StringComparison.Ordinal));
            if (target == null)
            {
                target = new Prefab.ComponentDefinition { Type = resolvedType.FullName };
                clone.Components.Add(target);
            }

            foreach (var (name, value) in properties)
                target.Properties[name] = value;
        }

        return clone;
    }
}
