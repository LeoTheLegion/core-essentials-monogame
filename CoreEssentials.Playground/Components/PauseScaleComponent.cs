using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Scales the owning entity up while the application is paused (window unfocused) and restores it on
/// resume — the small pause behavior both deleted character entities shared via their
/// <c>OnApplicationPause</c> overrides. Because <see cref="Entity.OnApplicationPause"/> forwards to
/// every attached component, a single declarative instance of this component reproduces that effect
/// without any entity subclass. The scale factor is configurable; it defaults to the original 1.5×.
/// </summary>
public class PauseScaleComponent : EntityComponent
{
    /// <summary>Scale applied while paused (default 1.5, matching the original characters).</summary>
    public float PausedScale { get; set; } = 1.5f;

    /// <inheritdoc />
    public override void OnApplicationPause(bool paused)
    {
        if (Owner == null) return;
        Owner.Scale = paused ? new Vector2(PausedScale, PausedScale) : Vector2.One;
    }
}
