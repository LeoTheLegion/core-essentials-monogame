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
///
/// Controls:
///   Space — broadcast "OnPing" to the whole scene (every receiver, in every subtree, counts up)
///   P     — spawn a prefab via InstantiateTemplate (the template's &lt;Bind&gt; turns it green)
///   B     — spawn a typed entity via CreateGameObject&lt;PingReceiverEntity&gt;()
///   D     — destroy the most recently spawned prefab (DestroyOwner from a component / Destroy on the entity)
///   Esc   — back to the character scene
/// </summary>
public class SendMessageDemoScene : Scene
{
    private EntitySystem? _entitySystem;
    private PingReceiverEntity? _lastSpawned;

    // Stored as a field (not a property) so Unload can unsubscribe the exact same delegate instance.
    private EventHandler<CoreEssentials.Inputs.KeyboardEventArgs>? _handleKey;

    protected override GameSystem[] LoadGameSystems() => new GameSystem[] { new EntitySystem() };

    protected override IEnumerator OnStartCoroutine()
    {
        UpdateLoadingProgress(0.2f, "Initializing SendMessage demo...");
        yield return null;

        _entitySystem = GetGameSystem<EntitySystem>();

        // Prefab template built in code — registered through the EntityTemplate overload,
        // with a declarative <Bind> that wires the entity's own Spawned event to OnSpawned.
        var prefab = EntityTemplateLoader.LoadFromXml(
            @"<EntityTemplate Type=""CoreEssentials.Playground.PingReceiverEntity"" Sort=""100"">
                <Bind Event=""Spawned"" Command=""OnSpawned"" />
            </EntityTemplate>");
        _entitySystem.RegisterTemplate("PingPrefab", prefab);

        // Two receivers in unrelated subtrees: one at the root, one nested under a parent.
        var rootReceiver = _entitySystem.CreateEntity<PingReceiverEntity>(new Vector2(400, 300), "root receiver");
        rootReceiver?.SetId("rootReceiver");

        var parent = _entitySystem.CreateEntity<Entity>();
        var nestedReceiver = new PingReceiverEntity(new Vector2(800, 300), "nested receiver (child)");
        parent.AddChild(nestedReceiver);

        var info = _entitySystem.CreateEntity<PingReceiverEntity>(new Vector2(640, 150),
            "Space: broadcast OnPing | P: prefab spawn | B: typed spawn | D: destroy last | Esc: back");
        info?.SetId("info");
        if (info != null) info.Color = Color.White;

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
                // Prefab spawn from a component's owning entity — the template's <Bind> turns it green.
                var prefab = _entitySystem.Instantiate("PingPrefab", new Vector2(640, 450));
                Console.WriteLine($"Spawned prefab '{prefab?.GetType().Name}'.");
                break;

            case Microsoft.Xna.Framework.Input.Keys.B:
                // Unity-style typed one-liner from an existing entity.
                _lastSpawned = (PingReceiverEntity?)_entitySystem.FindById("rootReceiver")
                    ?.CreateGameObject<PingReceiverEntity>(new Vector2(300, 450), "typed spawn");
                Console.WriteLine($"Created {_lastSpawned?.GetType().Name} via CreateGameObject<T>().");
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
