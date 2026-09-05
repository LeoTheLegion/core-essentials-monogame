using CoreEssentials.Scenes;
using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// Tracks the active scene transition's progress so a data-driven loading screen can display it.
/// Every frame the component mirrors <see cref="SceneManager.TransitionProgress"/> into
/// <see cref="Progress"/>, raises <see cref="ProgressChanged"/> when the value moves, and — if the
/// owning entity also carries a <see cref="LabelComponent"/> — keeps its text in sync as a
/// percentage. Custom bars can subscribe declaratively with
/// <c>&lt;Bind Event="ProgressChanged" Command="SetFill" /&gt;</c>.
/// </summary>
public class TransitionProgressComponent : EntityComponent
{
    private float _progress;
    private LabelComponent? _label;

    /// <summary>The last observed transition progress (0.0 to 1.0). Holds its value while no
    /// scene manager is reachable.</summary>
    public float Progress => _progress;

    /// <summary>Raised whenever the tracked progress changes, carrying the new value.</summary>
    public event Action<float>? ProgressChanged;

    /// <summary>Finds an optional <see cref="LabelComponent"/> on the owning entity so the
    /// component can render a live percentage without extra wiring.</summary>
    public override void OnAttach()
    {
        Owner.TryGetComponent(out _label);
    }

    /// <summary>Mirrors the scene manager's transition progress into this component each frame.</summary>
    public override void Update(GameTime gameTime)
    {
        var source = EntitySystem?.Scene?.SceneManagerOrNull;
        if (source == null) return; // No manager reachable — hold the last value.

        var next = Math.Clamp(source.TransitionProgress, 0f, 1f);
        if (Math.Abs(next - _progress) < 0.0001f)
            return;

        _progress = next;
        if (_label != null)
            _label.Text = $"{(int)MathF.Round(_progress * 100)}%";
        ProgressChanged?.Invoke(_progress);
    }
}
