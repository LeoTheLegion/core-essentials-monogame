using Microsoft.Xna.Framework;
using Cam = CoreEssentials.Camera.Camera;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// Built-in camera anchor component. The owning entity becomes the thing the camera is attached
/// to: every frame the camera's position (and optionally rotation) is synced from the owner, so
/// moving the entity moves the camera with it. This component intentionally contains no input
/// handling — movement belongs to whatever drives the entity (player code, a follow routine,
/// WASD handlers, physics, etc.).
/// </summary>
/// <remarks>
/// On attach a new <see cref="Camera"/> is created and registered as
/// <see cref="Camera.MainCamera"/>; on detach it is disposed and removed from the main-camera
/// slot. The sync happens in <see cref="LateUpdate"/>, i.e. after all regular updates of the
/// frame, so the camera always sees the entity's final position for that frame.
/// Scenes can therefore get a camera by attaching this component to any plain entity — including
/// one loaded from XML — with zero game-layer code.
/// </remarks>
public class CameraComponent : EntityComponent
{
    private readonly Cam _camera;

    /// <summary>
    /// Gets the camera owned by this component.
    /// </summary>
    public Cam Camera => _camera;

    /// <summary>
    /// Gets or sets whether the camera's rotation is synced from the owner's rotation each frame.
    /// Defaults to true.
    /// </summary>
    public bool SyncRotation { get; set; } = true;

    /// <summary>
    /// Gets or sets the camera zoom. Not driven by the owner — set it once (or at runtime) and
    /// it is preserved across per-frame position syncs. Defaults to 1.
    /// </summary>
    public float Zoom
    {
        get => _camera.Zoom;
        set => _camera.Zoom = value;
    }

    /// <summary>
    /// Gets or sets the orthographic half-height of the visible area in world units (Unity's
    /// <c>orthographicSize</c>). Zero keeps the legacy 1-world-unit-per-pixel behavior.
    /// </summary>
    public float OrthographicSize
    {
        get => _camera.OrthographicSize;
        set => _camera.OrthographicSize = value;
    }

    /// <summary>
    /// Gets or sets the logical (game) resolution the camera projects into — for pixel-art games
    /// this is typically smaller than the backbuffer (e.g. 320x180 on a 1280x720 window).
    /// </summary>
    public Vector2 ViewportSize
    {
        get => _camera.ViewportSize;
        set => _camera.ViewportSize = value;
    }

    /// <summary>
    /// Gets or sets the render-to-game resolution ratio for pixel-art upscaling. Defaults to 1.
    /// </summary>
    public float RenderScale
    {
        get => _camera.RenderScale;
        set => _camera.RenderScale = value;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="CameraComponent"/> class.
    /// </summary>
    public CameraComponent()
    {
        _camera = new Cam();
    }

    /// <inheritdoc />
    public override void OnAttach()
    {
        base.OnAttach();
        _camera.SetAsMainCamera();
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        if (Cam.MainCamera == _camera)
            Cam.SetMainCamera(null);

        _camera.Dispose();
        base.OnDetach();
    }

    /// <inheritdoc />
    public override void LateUpdate(GameTime gameTime)
    {
        if (Owner == null)
            return;

        // The whole job of this component: keep the camera anchored to the owning entity,
        // after everything else in the frame has moved.
        _camera.Position = Owner.Position;
        if (SyncRotation)
            _camera.Rotation = Owner.Rotation;
    }
}
