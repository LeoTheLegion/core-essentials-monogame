using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.Physics.Types;

namespace CoreEssentials.Scenes;

/// <summary>
/// A scene that runs entirely from a parsed <see cref="SceneDefinition"/> — no C# subclass needed.
/// Game systems are reflected from the definition, prefab registrations are applied to each entity
/// system (idempotently), and entities — including nested children, per-instance overrides,
/// declarative binds, and cross-entity references — are instantiated during the scene's start phase.
/// </summary>
public class DataDrivenScene : Scene
{
    private SceneDefinition? _definition;
    private readonly string? _assetName;
    private GameSystem[] _systems = Array.Empty<GameSystem>();

    /// <summary>The parsed definition this scene was created from. For scenes built from an asset name,
    /// the definition is resolved lazily on first access (during the scene's load phase).</summary>
    public SceneDefinition Definition => EnsureDefinition();

    /// <summary>Creates a data-driven scene from a parsed definition (see <see cref="SceneParser"/>).</summary>
    /// <exception cref="InvalidOperationException">Thrown when a system that is not an
    /// <see cref="EntitySystem"/> declares prefabs or entities — content can only live inside
    /// <c>&lt;System Type="EntitySystem"&gt;</c>.</exception>
    public DataDrivenScene(SceneDefinition definition)
    {
        _definition = Validate(definition ?? throw new ArgumentNullException(nameof(definition)));
    }

    /// <summary>
    /// Creates a data-driven scene whose definition is parsed from a scene XML asset when the scene
    /// loads. Use this (via <see cref="SceneManager.LoadScene(string)"/> / <see cref="SceneManager.SetLoadingScene(string)"/>)
    /// so a scene can be requested before the <see cref="CoreEssentials.Assets.AssetManager"/> is
    /// initialized — e.g. immediately after game construction, ahead of <c>Run()</c>. The file is read
    /// during the scene's load phase, once assets are available.
    /// </summary>
    /// <param name="sceneAssetName">The name/key of the scene XML asset in the AssetManager (e.g., "HomeScene.xml").</param>
    public DataDrivenScene(string sceneAssetName)
    {
        if (string.IsNullOrWhiteSpace(sceneAssetName))
            throw new ArgumentNullException(nameof(sceneAssetName));

        _assetName = sceneAssetName;
    }

    /// <summary>Resolves the definition on first use, parsing from the asset name when it was not
    /// supplied at construction. Safe to call repeatedly.</summary>
    private SceneDefinition EnsureDefinition()
    {
        if (_definition == null)
            _definition = Validate(SceneParser.LoadFromAsset(_assetName!));

        return _definition;
    }

    private static SceneDefinition Validate(SceneDefinition definition)
    {
        foreach (var systemDef in definition.Systems)
        {
            if ((systemDef.Prefabs.Count > 0 || systemDef.Entities.Count > 0) && systemDef.SystemType != typeof(EntitySystem))
                throw new InvalidOperationException(
                    $"System '{systemDef.TypeName}' declares prefabs or entities but is not an EntitySystem — " +
                    "content can only live inside <System Type=\"EntitySystem\">.");
        }

        return definition;
    }

    /// <summary>Reflectively instantiates every system declared by the definition, in document order.
    /// Systems with a <c>Config</c> attribute are created from their configuration asset instead of
    /// their parameterless constructor.</summary>
    protected override GameSystem[] LoadGameSystems()
    {
        var defs = EnsureDefinition().Systems;
        _systems = new GameSystem[defs.Count];
        for (int i = 0; i < defs.Count; i++)
        {
            var def = defs[i];
            object? instance;
            if (def.ConfigAsset != null)
            {
                instance = CreateSystemWithConfig(def);
            }
            else
            {
                try
                {
                    instance = Activator.CreateInstance(def.SystemType);
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        $"Could not create game system '{def.SystemType.Name}' from the scene definition — it needs a public parameterless constructor.", ex);
                }
            }
            _systems[i] = (GameSystem)instance!;
        }
        return _systems;
    }

    /// <summary>
    /// Maps configuration parameter types to the loader that builds one from an XML asset name.
    /// A system whose single-argument constructor accepts a registered type can be created from a
    /// &lt;System Config="..."&gt; attribute.
    /// </summary>
    private static readonly Dictionary<Type, Func<string, object>> _configLoaders = new()
    {
        { typeof(PhysicsConfig), name => PhysicsConfig.LoadFromAsset(name) }
    };

    /// <summary>Creates a system from its configuration asset via its single-argument constructor.</summary>
    private static object CreateSystemWithConfig(SystemDefinition def)
    {
        var candidates = def.SystemType.GetConstructors()
            .Where(c => c.GetParameters().Length == 1 && _configLoaders.ContainsKey(c.GetParameters()[0].ParameterType))
            .ToList();

        if (candidates.Count != 1)
            throw new InvalidOperationException(
                $"System '{def.TypeName}' declares Config=\"{def.ConfigAsset}\" but has no single-argument constructor " +
                $"accepting a known configuration type ({string.Join(", ", _configLoaders.Keys.Select(t => t.Name))}).");

        var ctor = candidates[0];
        return ctor.Invoke(new object[] { _configLoaders[ctor.GetParameters()[0].ParameterType](def.ConfigAsset!) });
    }

    /// <summary>
    /// Registers each entity system's prefabs, then instantiates its entities. Runs in the
    /// 50%→100% loading phase after all systems have started.
    /// </summary>
    protected override IEnumerator OnStartCoroutine()
    {
        var defs = EnsureDefinition().Systems;
        for (int i = 0; i < defs.Count; i++)
        {
            var systemDef = defs[i];
            if (systemDef.Prefabs.Count == 0 && systemDef.Entities.Count == 0)
                continue;

            if (_systems[i] is not EntitySystem entitySystem)
                throw new InvalidOperationException(
                    $"System '{systemDef.TypeName}' declares content but was not created as an EntitySystem.");

            var fraction = (float)i / Math.Max(1, defs.Count);
            UpdateLoadingProgress(0.5f + 0.45f * fraction, $"Registering prefabs for {systemDef.TypeName}...");
            foreach (var registration in systemDef.Prefabs)
                entitySystem.RegisterPrefab(registration.Name, registration.Prefab!);
            yield return null;

            UpdateLoadingProgress(0.5f + 0.45f * ((float)(i + 1) / Math.Max(1, defs.Count)), $"Creating entities for {systemDef.TypeName}...");
            var roots = new List<Entity>();
            foreach (var def in systemDef.Entities)
                roots.Add(InstantiateDefinition(def, entitySystem));
            ResolveReferences(systemDef.Entities, roots);
            yield return null;
        }

        UpdateLoadingProgress(1.0f, "Scene content ready");
        yield break;
    }

    // ──────────────────────────── Entity instantiation ────────────────────────────

    /// <summary>
    /// Instantiates a single entity definition and its nested &lt;Children&gt; as one tree. The whole
    /// subtree is built and linked first, then components attach pre-order (parents before children),
    /// so hierarchy-dependent components — e.g. a child's LabelComponent finding its ancestor
    /// CanvasComponent — resolve correctly. This mirrors EntityPrefabLoader's two-phase design.
    /// </summary>
    private static Entity InstantiateDefinition(EntityDefinition def, EntitySystem system)
    {
        var combined = BuildCombinedPrefab(def, system);
        var root = EntityPrefabLoader.Instantiate(combined, system, def.Position);

        ApplyIdsAndBinds(root, def, system);
        return root;
    }

    /// <summary>
    /// Builds a single prefab tree for a definition and all of its nested scene &lt;Children&gt;. A
    /// &lt;Source&gt; node resolves to its registered prefab (cloned); a plain-class (&lt;Type&gt;) node builds
    /// an ad-hoc prefab from its declared components. Per-instance overrides are applied at each node,
    /// and nested children are inlined so the loader can build + link + attach the whole tree in one
    /// pass. A child's &lt;Position&gt; is carried on its prefab node as an offset from its parent.
    /// </summary>
    private static Prefab BuildCombinedPrefab(EntityDefinition def, EntitySystem system)
    {
        Prefab rootPrefab;
        if (def.Source != null)
        {
            if (!system.TryGetPrefab(def.Source, out var registered))
                throw new KeyNotFoundException($"Prefab '{def.Source}' is not registered.");
            rootPrefab = registered!.Clone();
        }
        else
        {
            rootPrefab = BuildAdHocPrefab(def);
        }

        // Apply returns a clone when overrides are present and the (already disposable) input when they
        // are not — either way `node` is safe to extend without touching any registered prefab.
        var node = PrefabOverrides.Apply(rootPrefab, def.ResolvedOverrides, def.EntityOverrides);
        foreach (var child in def.Children)
        {
            var childNode = BuildCombinedPrefab(child, system);
            // A nested <Position> is an offset from this node, not a world position.
            childNode.Position = child.Position;
            node.Children.Add(childNode);
        }
        return node;
    }

    /// <summary>
    /// Assigns stable ids and declarative binds to the built tree, walking in lockstep with the
    /// definition. For a &lt;Source&gt; node whose registered prefab has its own children, those
    /// prefab-internal children precede the scene-level ones in the built entity's child list, so we
    /// skip past them before descending into the scene children.
    /// </summary>
    private static void ApplyIdsAndBinds(Entity entity, EntityDefinition def, EntitySystem system)
    {
        if (!string.IsNullOrEmpty(def.Id))
            entity.SetId(def.Id);

        ApplyDefinitionBinds(entity, def.Binds);

        int skip = def.Source != null && system.TryGetPrefab(def.Source, out var registered)
            ? registered!.Children.Count
            : 0;

        for (int i = 0; i < def.Children.Count; i++)
        {
            var builtChild = entity.Children[skip + i];
            ApplyIdsAndBinds(builtChild, def.Children[i], system);
        }
    }

    /// <summary>Builds a throwaway prefab from a plain-class (Type=) definition's declared components.</summary>
    private static Prefab BuildAdHocPrefab(EntityDefinition def)
    {
        return new Prefab
        {
            Type = def.Type!,
            Rotation = def.Rotation ?? 0f,
            Sort = def.Sort ?? 0,
            Active = def.Active ?? true,
            Tags = new List<string>(def.Tags),
            Components = def.DeclaredComponents
                .Select(c => new Prefab.ComponentDefinition
                {
                    Type = c.Type,
                    Properties = new Dictionary<string, string>(c.Properties)
                })
                .ToList()
        };
    }

    /// <summary>Applies a definition's declarative &lt;Bind&gt; wiring (deep-copied so the stored
    /// elements are never mutated), mirroring the prefab loader's bind application.</summary>
    private static void ApplyDefinitionBinds(Entity entity, List<XElement> binds)
    {
        if (binds.Count == 0) return;

        var wrapper = new XElement("EntityDefinition");
        foreach (var bind in binds)
            wrapper.Add(new XElement(bind));

        CommandBindings.ApplyBindings(entity, wrapper);
    }

    // ──────────────────────────── Reference resolution ────────────────────────────

    /// <summary>Resolves &lt;Reference Name= TargetId=/&gt; links once every entity in the system
    /// exists, mirroring <see cref="EntitySerializer"/> semantics: an entity property first, then a
    /// component property or public field whose type accepts <see cref="Entity"/>.</summary>
    private static void ResolveReferences(List<EntityDefinition> defs, List<Entity> roots)
    {
        var idToEntity = new Dictionary<string, Entity>(StringComparer.Ordinal);
        CollectById(roots, idToEntity);

        foreach (var def in defs)
            ResolveReferences(def, idToEntity);
    }

    private static void ResolveReferences(EntityDefinition def, Dictionary<string, Entity> idToEntity)
    {
        if (!string.IsNullOrEmpty(def.Id) && idToEntity.TryGetValue(def.Id!, out var target))
        {
            foreach (var reference in def.References)
            {
                var name = reference.Attribute("Name")?.Value;
                var targetId = reference.Attribute("TargetId")?.Value;
                if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(targetId))
                    continue;

                if (idToEntity.TryGetValue(targetId!, out var referenced))
                    SetReference(target, name!, referenced);
            }
        }

        foreach (var child in def.Children)
            ResolveReferences(child, idToEntity);
    }

    private static void CollectById(List<Entity> roots, Dictionary<string, Entity> map)
    {
        foreach (var root in roots)
            CollectById(root, map);
    }

    private static void CollectById(Entity entity, Dictionary<string, Entity> map)
    {
        if (!string.IsNullOrEmpty(entity.Id))
            map[entity.Id!] = entity;

        foreach (var child in entity.Children)
            CollectById(child, map);
    }

    private static void SetReference(Entity target, string name, Entity reference)
    {
        var property = target.GetType().GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
        if (property != null && property.PropertyType.IsAssignableFrom(typeof(Entity)))
        {
            property.SetValue(target, reference);
            return;
        }

        foreach (var component in target.Components)
        {
            var componentType = component.GetType();

            var componentProperty = componentType.GetProperty(name, BindingFlags.Instance | BindingFlags.Public);
            if (componentProperty != null && componentProperty.CanWrite && componentProperty.PropertyType.IsAssignableFrom(typeof(Entity)))
            {
                componentProperty.SetValue(component, reference);
                return;
            }

            var componentField = componentType.GetField(name, BindingFlags.Instance | BindingFlags.Public);
            if (componentField != null && componentField.FieldType.IsAssignableFrom(typeof(Entity)))
            {
                componentField.SetValue(component, reference);
                return;
            }
        }
    }
}
