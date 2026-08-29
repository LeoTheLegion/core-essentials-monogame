using System;
using System.Collections;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.Inputs;
using CoreEssentials.Scenes;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground;

/// <summary>
/// Demonstrates scene-wide SendMessage (Unity-style multi-cast), the Unity-style
/// entity-management one-liners, and prefab templates with declarative &lt;Bind&gt; wiring.
/// All receivers are plain GameObjectEntity shells carrying a PingReceiverComponent —
/// behavior lives on components, entities stay inert (Unity-style).
///
/// Controls:
///   Space — broadcast "OnPing" to the whole scene (every receiver, in every subtree, counts up)
///   P     — spawn a prefab via InstantiateTemplate (the template's &lt;Bind&gt; turns it green)
///   B     — spawn a GameObjectEntity + component via CreateGameObject&lt;GameObjectEntity&gt;()
///   D     — destroy the most recently spawned entity
///   Esc   — back to the character scene
/// </summary>
public class SendMessageDemoScene : Scene
{
    private EntitySystem? _entitySystem;
    private Entity? _lastSpawned;
    private int _spawnCounter;

    // Stored as a field (not a property) so Unload can unsubscribe the exact same delegate instance.
    private EventHandler<CoreEssentials.Inputs.KeyboardEventArgs>? _handleKey;

    protected override GameSystem[] LoadGameSystems() => new GameSystem[] { new EntitySystem() };

    protected override IEnumerator OnStartCoroutine()
    {
        UpdateLoadingProgress(0.2f, "Initializing SendMessage demo...");
        yield return null;

        _entitySystem = GetGameSystem<EntitySystem>();

        // Prefab template from an XML content file: a plain GameObjectEntity shell carrying
        // the receiver component, with a declarative <Bind> wiring Spawned → OnSpawned.
        UpdateLoadingProgress(0.4f, "Registering PingPrefab template...");
        _entitySystem.RegisterTemplate("PingPrefab", "PingPrefabTemplate.xml");

        // Scene entities (root receiver, nested receiver in its own subtree, info line)
        // all come from the scene XML definition file.
        UpdateLoadingProgress(0.7f, "Loading scene entities from XML...");
        LoadEntitiesFromXml("SendMessageDemoScene.xml", _entitySystem);

        _handleKey = (sender, args) => HandleKeyPressed(sender, args);
        Input.Keyboard.KeyReleased += _handleKey;

        UpdateLoadingProgress(1.0f, "SendMessage demo ready!");
        Console.WriteLine("SendMessage demo loaded. Press Space to broadcast.");
        yield break;
    }

    public override void Unload()
    {
        base.Unload();
        if (_handleKey != null)
            Input.Keyboard.KeyReleased -= _handleKey;
        _handleKey = null;
    }

    private void HandleKeyPressed(object sender, CoreEssentials.Inputs.KeyboardEventArgs args)
    {
        if (_entitySystem == null) return;

        switch (args.Key)
        {
            case Microsoft.Xna.Framework.Input.Keys.Space:
                var invoked = _entitySystem.SendMessage("OnPing");
                Console.WriteLine($"Sent 'OnPing' — {invoked} handler(s) fired.");
                break;

            case Microsoft.Xna.Framework.Input.Keys.P:
                // Prefab spawn — the template's <Bind> turns each new instance green.
                _spawnCounter++;
                var prefab = _entitySystem.Instantiate("PingPrefab", new Vector2(640 + (_spawnCounter % 5) * 80, 450));
                prefab.GetComponent<PingReceiverComponent>()!.Label = $"prefab {_spawnCounter}";
                _lastSpawned = prefab;
                Console.WriteLine($"Spawned prefab '{prefab.GetType().Name}'.");
                break;

            case Microsoft.Xna.Framework.Input.Keys.B:
                // Unity-style one-liner from an existing entity, then compose it from a component.
                _spawnCounter++;
                var shell = _entitySystem.FindById("rootReceiver")?.CreateGameObject<GameObjectEntity>();
                if (shell != null)
                {
                    shell.Position = new Vector2(300 + (_spawnCounter % 5) * 80, 450);
                    shell.AddComponent(new PingReceiverComponent { Label = $"typed spawn {_spawnCounter}" });
                    _lastSpawned = shell;
                }
                Console.WriteLine("Created GameObjectEntity via CreateGameObject<T>().");
                break;

            case Microsoft.Xna.Framework.Input.Keys.D:
                _lastSpawned?.Destroy();
                Console.WriteLine("Destroyed last spawned entity.");
                _lastSpawned = null;
                break;

            case Microsoft.Xna.Framework.Input.Keys.Escape:
                SceneManager.LoadScene(new CharacterScene());
                break;
        }
    }
}
