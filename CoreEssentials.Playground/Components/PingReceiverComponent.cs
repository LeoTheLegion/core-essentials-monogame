using System;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Demo component for the SendMessage scene: renders a short label at the owner's position and
/// reacts to scene-wide broadcasts. Attached to plain GameObjectEntity instances, proving that
/// behavior lives on components while the entity stays a behavior-free shell (Unity-style).
/// </summary>
public class PingReceiverComponent : EntityComponent, IDrawableComponent
{
    private FontAsset? _font;
    private int _pingCount;

    /// <summary>Raised once on the first update — the prefab template's &lt;Bind&gt; wires this to <see cref="OnSpawned"/>.</summary>
    public event Action? Spawned;

    private bool _spawnSignalSent;

    /// <summary>The label text rendered at the owner's position.</summary>
    public string Label { get; set; } = "ping";

    /// <summary>The color used to render the label.</summary>
    public Color Color { get; set; } = Color.Cyan;

    public override void OnAttach()
    {
        base.OnAttach();
        _font = AssetManager.LoadAsset<FontAsset>("base");
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
        Label = $"[{_pingCount}]";
    }

    /// <summary>Handler wired by the prefab template's &lt;Bind Event="Spawned" Command="OnSpawned" /&gt; — turns the label green.</summary>
    public void OnSpawned()
    {
        Color = Color.LightGreen;
    }

    public void Draw(SpriteBatch spriteBatch)
    {
        if (Owner == null || _font?.Font == null) return;

        var size = _font.MeasureStringVector(Label);
        spriteBatch.DrawString(_font.Font, Label, Owner.Position - size / 2f, Color);
    }
}
