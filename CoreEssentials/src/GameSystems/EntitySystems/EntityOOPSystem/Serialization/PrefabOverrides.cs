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
    {
        if (prefab == null) throw new ArgumentNullException(nameof(prefab));
        if (overrides == null || overrides.Count == 0) return prefab;

        var clone = prefab.Clone();

        foreach (var (componentKey, properties) in overrides)
        {
            Type? resolvedType = EntityTemplateLoader.ResolveComponentType(componentKey);
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
