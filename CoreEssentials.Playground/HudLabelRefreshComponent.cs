using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground;

/// <summary>
/// Periodically rewrites its host label's text to embed the label's own measured size, so the demo
/// shows that AutoWidth/AutoHeight labels report their real content size. This ports the throttled
/// HUD refresh that used to live in a scene subclass (the LabelAlignment demo), where three labels
/// each showed e.g. <c>LEFT   (W=120 H=24)</c>. Attach one per label host:
/// <code>
/// &lt;Component Type="HudLabelRefreshComponent"&gt;
///   &lt;Properties&gt;
///     &lt;Property Name="TextTemplate" Value="LEFT   (W={w} H={h})" /&gt;
///     &lt;Property Name="IntervalSeconds" Value="0.5" /&gt;
///   &lt;/Properties&gt;
/// &lt;/Component&gt;
/// </code>
/// The <c>{w}</c> and <c>{h}</c> tokens are replaced with the label's current measured width/height
/// (rounded). Refreshing a couple of times a second keeps the text visible without thrashing every
/// frame.
/// </summary>
public class HudLabelRefreshComponent : EntityComponent
{
    /// <summary>
    /// The text template for the host label. The tokens <c>{w}</c> and <c>{h}</c> are replaced with
    /// the label's measured width/height each refresh.
    /// </summary>
    public string TextTemplate { get; set; } = "(W={w} H={h})";

    /// <summary>The minimum seconds between refreshes. Defaults to 0.5.</summary>
    public float IntervalSeconds { get; set; } = 0.5f;

    private float _elapsed;

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (Owner == null) return;

        _elapsed += (float)gameTime.ElapsedGameTime.TotalSeconds;
        if (_elapsed < IntervalSeconds) return;
        _elapsed = 0f;

        Refresh();
    }

    // ── Testability seams ────────────────────────────────────────────────────────

    /// <summary>
    /// Rewrites the host label's text from <see cref="TextTemplate"/>, substituting its measured
    /// width/height. Virtual so unit tests can observe the produced text without a live canvas.
    /// </summary>
    protected virtual void Refresh()
    {
        var label = Owner?.GetComponent<LabelComponent>();
        if (label == null) return;

        label.Text = Format(label.Width, label.Height);
    }

    /// <summary>
    /// Formats the template with the given measured size. Pure string work so unit tests can assert
    /// the exact output without a live label.
    /// </summary>
    public string Format(float width, float height)
        => TextTemplate
            .Replace("{w}", ((int)width).ToString())
            .Replace("{h}", ((int)height).ToString());

    /// <summary>The time accumulated since the last refresh (seconds).</summary>
    public float ElapsedSinceRefresh => _elapsed;
}
