using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.Inputs;
using CoreEssentials.Timing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Moves the owning entity with the arrow keys — the behavior that used to live inline in the
/// deleted <c>PlayerEntity</c>. Each held key nudges one axis by <see cref="MoveSpeed"/> ×
/// <see cref="Time.DeltaTime"/> (milliseconds), exactly as the original did, so left/right/up/down
/// map to -X/+X/-Y/+Y. Key bindings and speed are declarative, mirroring <see cref="CameraInputComponent"/>.
/// </summary>
public class MoveByKeysComponent : EntityComponent
{
    /// <summary>Move left. Defaults to Left.</summary>
    public Keys LeftKey { get; set; } = Keys.Left;

    /// <summary>Move right. Defaults to Right.</summary>
    public Keys RightKey { get; set; } = Keys.Right;

    /// <summary>Move up. Defaults to Up.</summary>
    public Keys UpKey { get; set; } = Keys.Up;

    /// <summary>Move down. Defaults to Down.</summary>
    public Keys DownKey { get; set; } = Keys.Down;

    /// <summary>Speed in world units per millisecond (matches the original PlayerEntity).</summary>
    public float MoveSpeed { get; set; } = 1f;

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (Owner == null) return;

        float dt = (float)GetDeltaTime();
        var move = new Vector2(0f, 0f);
        if (IsKeyHeld(LeftKey)) move.X -= 1f;
        if (IsKeyHeld(RightKey)) move.X += 1f;
        if (IsKeyHeld(UpKey)) move.Y -= 1f;
        if (IsKeyHeld(DownKey)) move.Y += 1f;

        if (move != Vector2.Zero)
            Owner.Position += move * MoveSpeed * dt;
    }

    /// <summary>Current frame delta in milliseconds. Virtual so tests can pin the value.</summary>
    protected virtual float GetDeltaTime() => (float)Time.DeltaTime;

    /// <summary>Polls whether a key is held. Virtual so tests can simulate input without the live keyboard.</summary>
    protected virtual bool IsKeyHeld(Keys key) => Input.Keyboard.IsKeyDown(key);
}
