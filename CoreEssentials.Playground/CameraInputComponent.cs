using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Playground;

/// <summary>
/// Input layer for a camera owned by the built-in <see cref="CameraComponent"/> on the same entity.
/// WASD pans the owning entity (the camera follows it via the component's late-update sync), Q/E
/// zoom, and R resets position/rotation/zoom. All keys and speeds are declarative, so a scene can
/// wire a controllable camera purely from data:
/// <code>
/// &lt;Component Type="CameraInputComponent"&gt;
///   &lt;Properties&gt;
///     &lt;Property Name="MoveSpeed" Value="300" /&gt;
///     &lt;Property Name="ZoomSpeed" Value="1" /&gt;
///   &lt;/Properties&gt;
/// &lt;/Component&gt;
/// </code>
/// The component does nothing if no <see cref="CameraComponent"/> is present (panning still moves
/// the entity; zoom is skipped).
/// </summary>
public class CameraInputComponent : EntityComponent
{
    /// <summary>Pan left. Defaults to A.</summary>
    public Keys LeftKey { get; set; } = Keys.A;

    /// <summary>Pan right. Defaults to D.</summary>
    public Keys RightKey { get; set; } = Keys.D;

    /// <summary>Pan up. Defaults to W.</summary>
    public Keys UpKey { get; set; } = Keys.W;

    /// <summary>Pan down. Defaults to S.</summary>
    public Keys DownKey { get; set; } = Keys.S;

    /// <summary>Zoom in. Defaults to Q.</summary>
    public Keys ZoomInKey { get; set; } = Keys.Q;

    /// <summary>Zoom out. Defaults to E.</summary>
    public Keys ZoomOutKey { get; set; } = Keys.E;

    /// <summary>Reset the camera. Defaults to R.</summary>
    public Keys ResetKey { get; set; } = Keys.R;

    /// <summary>Pan speed in world units per second.</summary>
    public float MoveSpeed { get; set; } = 1f;

    /// <summary>Zoom sensitivity factor.</summary>
    public float ZoomSpeed { get; set; } = 1f;

    private const float ActualZoomSpeed = 0.1f; // Actual zoom change per unit of input

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

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (Owner == null) return;
        var dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

        // Pan the owning entity — the CameraComponent anchors the camera to it each late update.
        if (IsKeyHeld(LeftKey)) Owner.Position += new Vector2(-MoveSpeed, 0f) * dt;
        if (IsKeyHeld(RightKey)) Owner.Position += new Vector2(MoveSpeed, 0f) * dt;
        if (IsKeyHeld(UpKey)) Owner.Position += new Vector2(0f, -MoveSpeed) * dt;
        if (IsKeyHeld(DownKey)) Owner.Position += new Vector2(0f, MoveSpeed) * dt;

        var camera = Owner.GetComponent<CameraComponent>();
        if (camera != null)
        {
            if (IsKeyHeld(ZoomInKey)) camera.Zoom += 1f * dt * ActualZoomSpeed * ZoomSpeed;
            if (IsKeyHeld(ZoomOutKey)) camera.Zoom -= 1f * dt * ActualZoomSpeed * ZoomSpeed;
            camera.Zoom = MathHelper.Clamp(camera.Zoom, 0.1f, 3f);
        }
    }

    /// <summary>
    /// Handles a key release: resets the camera when the key matches <see cref="ResetKey"/>.
    /// Exposed publicly so it can be invoked directly (e.g. from tests).
    /// </summary>
    public void HandleKey(Keys key)
    {
        if (key == ResetKey)
            ResetCamera();
    }

    /// <summary>
    /// Resets the owning entity's position and the camera's rotation/zoom to defaults. Virtual so
    /// unit tests can observe the reset without a live camera.
    /// </summary>
    protected virtual void ResetCamera()
    {
        if (Owner == null) return;
        Owner.Position = Vector2.Zero;
        var camera = Owner.GetComponent<CameraComponent>();
        if (camera != null)
        {
            camera.Camera.Rotation = 0f;
            camera.Zoom = 1f;
        }
    }

    /// <summary>
    /// Polls whether a key is currently held. Virtual so unit tests can simulate input without the
    /// live keyboard state.
    /// </summary>
    protected virtual bool IsKeyHeld(Keys key) => Input.Keyboard.IsKeyDown(key);
}
