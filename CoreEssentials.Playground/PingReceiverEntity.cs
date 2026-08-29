using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground;

/// <summary>
/// A small entity that reacts to scene-wide SendMessage broadcasts. Used by the
/// SendMessage demo scene: every instance counts "OnPing" messages and re-renders
/// its own text, proving that a single broadcast reaches entities in unrelated subtrees.
/// </summary>
public class PingReceiverEntity : TextEntity
{
    private int _pingCount;

    /// <summary>Raised once on the first update — the prefab template's &lt;Bind&gt; wires this to <see cref="OnSpawned"/>.</summary>
    public event Action? Spawned;

    private bool _spawnSignalSent;

    public PingReceiverEntity(Vector2 position, string label)
        : base(position, label, Color.Cyan, TextAlignment.Center)
    {
    }

    public override void Update(GameTime gameTime)
    {
        base.Update(gameTime);
        if (!_spawnSignalSent)
        {
            _spawnSignalSent = true;
            Spawned?.Invoke();
        }
    }

    /// <summary>Scene-wide message handler — fires for every "OnPing" broadcast.</summary>
    public void OnPing()
    {
        _pingCount++;
        Text = $"[{_pingCount}]";
    }

    /// <summary>Handler wired by the prefab template's &lt;Bind Event="Spawned" Command="OnSpawned" /&gt; — turns the prefab green.</summary>
    public void OnSpawned()
    {
        Color = Color.LightGreen;
    }
}
