using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Playground;

/// <summary>
/// Drives the SendMessage demo commands from a single entity, so the scene can be pure data.
/// All keys are declarative:
/// <code>
/// &lt;Component Type="PingControlComponent"&gt;
///   &lt;Properties&gt;
///     &lt;Property Name="BroadcastKey" Value="Space" /&gt;
///     &lt;Property Name="SpawnPrefabKey" Value="P" /&gt;
///     &lt;Property Name="PrefabName" Value="PingPrefab" /&gt;
///     &lt;Property Name="SpawnTypedKey" Value="B" /&gt;
///     &lt;Property Name="DestroyLastKey" Value="D" /&gt;
///   &lt;/Properties&gt;
/// &lt;/Component&gt;
/// </code>
/// Space broadcasts a scene-wide message; P spawns a registered prefab; B spawns a typed
/// GameObjectEntity + <see cref="PingReceiverComponent"/>; D destroys the most recently spawned
/// entity. Spawned entities are tracked so D can remove them.
/// </summary>
public class PingControlComponent : EntityComponent
{
    /// <summary>Key that broadcasts the scene-wide message. Defaults to Space.</summary>
    public Keys BroadcastKey { get; set; } = Keys.Space;

    /// <summary>The scene-wide message name to broadcast. Defaults to "OnPing".</summary>
    public string MessageName { get; set; } = "OnPing";

    /// <summary>Key that spawns a prefab. Defaults to P.</summary>
    public Keys SpawnPrefabKey { get; set; } = Keys.P;

    /// <summary>The registered prefab name to spawn on <see cref="SpawnPrefabKey"/>.</summary>
    public string PrefabName { get; set; } = "PingPrefab";

    /// <summary>Key that spawns a typed GameObjectEntity + PingReceiverComponent. Defaults to B.</summary>
    public Keys SpawnTypedKey { get; set; } = Keys.B;

    /// <summary>Key that destroys the most recently spawned entity. Defaults to D.</summary>
    public Keys DestroyLastKey { get; set; } = Keys.D;

    /// <summary>The base position for spawned entities (staggered by spawn count).</summary>
    public Vector2 SpawnPosition { get; set; } = new(640, 450);

    private Entity? _lastSpawned;
    private int _spawnCounter;
    private EventHandler<KeyboardEventArgs>? _onKeyReleased;

    /// <inheritdoc />
    public override void OnAttach()
    {
        _onKeyReleased = (_, args) => HandleKey(args.Key);
        Input.Keyboard.KeyReleased += _onKeyReleased;
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        if (_onKeyReleased != null)
            Input.Keyboard.KeyReleased -= _onKeyReleased;
        _onKeyReleased = null;
    }

    /// <summary>
    /// Handles a key release, dispatching to the matching demo command. Exposed publicly so it can
    /// be invoked directly (e.g. from tests).
    /// </summary>
    public void HandleKey(Keys key)
    {
        if (key == BroadcastKey)
            Broadcast();
        else if (key == SpawnPrefabKey)
            TrackSpawn(SpawnPrefab(NextPosition()));
        else if (key == SpawnTypedKey)
            TrackSpawn(SpawnTyped(NextPosition()));
        else if (key == DestroyLastKey)
            DestroyLast();
    }

    private Vector2 NextPosition()
    {
        _spawnCounter++;
        return SpawnPosition + new Vector2((_spawnCounter % 5) * 80f, 0f);
    }

    private void TrackSpawn(Entity? spawned) => _lastSpawned = spawned;

    /// <summary>
    /// Broadcasts <see cref="MessageName"/> to the whole scene. Virtual so unit tests can observe
    /// the broadcast without a live EntitySystem.
    /// </summary>
    protected virtual int Broadcast()
        => EntitySystem?.SendMessage(MessageName) ?? -1;

    /// <summary>
    /// Spawns the registered <see cref="PrefabName"/> at the given position. Virtual so unit tests
    /// can observe the spawn without a live EntitySystem.
    /// </summary>
    protected virtual Entity? SpawnPrefab(Vector2 position)
        => EntitySystem?.Instantiate(PrefabName, position);

    /// <summary>
    /// Spawns a typed GameObjectEntity carrying a <see cref="PingReceiverComponent"/> at the given
    /// position. Virtual so unit tests can observe the spawn without loading demo assets.
    /// </summary>
    protected virtual Entity? SpawnTyped(Vector2 position)
    {
        var system = EntitySystem;
        if (system == null) return null;

        var shell = system.CreateEntity<GameObjectEntity>();
        shell.Position = position;
        shell.AddComponent(new PingReceiverComponent { Label = $"typed spawn {_spawnCounter}" });
        return shell;
    }

    /// <summary>
    /// Destroys the most recently spawned entity. Virtual so unit tests can observe the call.
    /// </summary>
    protected virtual void DestroyLast()
    {
        _lastSpawned?.Destroy();
        _lastSpawned = null;
    }
}
