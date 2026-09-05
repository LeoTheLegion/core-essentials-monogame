using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.Inputs;
using CoreEssentials.Playground.Entities;
using Microsoft.Xna.Framework.Input;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Toggles a camera's follow of a target entity when a configured key (default F) is released, and
/// optionally updates an info label to reflect the new state. This ports the per-scene follow toggle
/// that used to live in a scene subclass (the Camera demo), so it can be declared purely from data:
/// <code>
/// &lt;EntityDefinition Type="...GameObjectEntity" Id="followToggle"&gt;
///   &lt;Components&gt;
///     &lt;Component Type="CameraFollowToggleComponent"&gt;
///       &lt;Properties&gt;&lt;Property Name="ToggleKey" Value="F" /&gt;&lt;/Properties&gt;
///     &lt;/Component&gt;
///   &lt;/Components&gt;
///   &lt;References&gt;
///     &lt;Reference Name="Camera" TargetId="camera" /&gt;
///     &lt;Reference Name="FollowTarget" TargetId="player" /&gt;
///     &lt;Reference Name="InfoLabel" TargetId="cameraInfoText" /&gt;
///   &lt;/References&gt;
/// &lt;/EntityDefinition&gt;
/// </code>
/// The linked entities are supplied via &lt;Reference&gt; (the strict parser resolves them into the
/// <see cref="Camera"/>, <see cref="FollowTarget"/> and <see cref="InfoLabel"/> properties). The camera
/// must be a <see cref="CameraEntity"/> and the label a <see cref="TextEntity"/>; anything else is
/// ignored gracefully.
/// </summary>
public class CameraFollowToggleComponent : EntityComponent
{
    /// <summary>The key that toggles follow mode. Defaults to F.</summary>
    public Keys ToggleKey { get; set; } = Keys.F;

    /// <summary>The camera entity (a <see cref="CameraEntity"/>). Set via &lt;Reference Name="Camera"/&gt;.</summary>
    public Entity? Camera { get; set; }

    /// <summary>The entity the camera should follow when toggled on. Set via &lt;Reference Name="FollowTarget"/&gt;.</summary>
    public Entity? FollowTarget { get; set; }

    /// <summary>Optional info label (a <see cref="TextEntity"/>). Set via &lt;Reference Name="InfoLabel"/&gt;.</summary>
    public Entity? InfoLabel { get; set; }

    /// <summary>
    /// The text shown on the info label after a toggle. The literal token <c>{state}</c> is replaced
    /// with "ON" or "OFF". Defaults to the camera demo's control listing.
    /// </summary>
    public string InfoTemplate { get; set; } =
        "Camera Controls:\nWASD: Move Camera\nQ/E: Zoom In/Out\nR: Reset Camera\nF: Follow Player ({state})\nArrow Keys: Move Player";

    private EventHandler<KeyboardEventArgs>? _onKeyReleased;

    /// <inheritdoc />
    public override void OnAttach()
    {
        _onKeyReleased = (_, args) => HandleKey(args.Key);
        Input.Keyboard.KeyReleased += _onKeyReleased;
        // Note: <Reference> links resolve after attach, so Camera/FollowTarget/InfoLabel are null
        // here. The initial info text is authored in the scene XML (via EntityOverrides Text);
        // subsequent toggles keep it up to date. RefreshInfo() can re-sync it on demand.
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        if (_onKeyReleased != null)
            Input.Keyboard.KeyReleased -= _onKeyReleased;
        _onKeyReleased = null;
    }

    /// <summary>
    /// Handles a key release: toggles the camera's follow of <see cref="FollowTarget"/> when the key
    /// matches <see cref="ToggleKey"/>. Exposed publicly so it can be invoked directly (e.g. from tests).
    /// </summary>
    public void HandleKey(Keys key)
    {
        if (key != ToggleKey) return;
        DoToggle();
    }

    // ── Testability seams ────────────────────────────────────────────────────────

    /// <summary>
    /// Performs the follow toggle and info refresh. Virtual so unit tests can observe the request
    /// without a live camera entity.
    /// </summary>
    protected virtual void DoToggle()
    {
        var camera = Camera as CameraEntity;
        if (camera == null || FollowTarget == null) return;

        camera.ToggleFollow(FollowTarget);
        UpdateInfo(camera.FollowingTarget);
    }

    /// <summary>Updates the info label's text to reflect the follow state. Virtual for tests.</summary>
    protected virtual void UpdateInfo(bool following)
    {
        if (InfoLabel is TextEntity text)
            text.Text = InfoTemplate.Replace("{state}", following ? "ON" : "OFF");
    }

    /// <summary>Refreshes the info label to reflect the current follow state (e.g. after references resolve).</summary>
    public void RefreshInfo()
    {
        if (Camera is CameraEntity camera)
            UpdateInfo(camera.FollowingTarget);
    }
}
