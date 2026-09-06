using System;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Makes the owning entity (a camera host carrying a built-in <c>CameraComponent</c>) smoothly
/// follow another entity: each update the owner's position is lerped toward the target, and while
/// following, manual panning from a sibling <c>CameraInputComponent</c> is ignored. This ports the
/// follow behavior that used to live in the hand-written camera entity, so it can be declared purely
/// from data:
/// <code>
/// &lt;Component Type="CameraFollowComponent" /&gt;
/// ...
/// &lt;References&gt;
///   &lt;Reference Name="FollowTarget" TargetId="player" /&gt;
/// &lt;/References&gt;
/// </code>
/// The target is set via <c>&lt;Reference Name="FollowTarget"/&gt;</c> (which resolves onto this
/// component's <see cref="FollowTarget"/> property) or programmatically via <see cref="SetFollowTarget"/>
/// / <see cref="ToggleFollow"/> — e.g. from a <c>CameraFollowToggleComponent</c>. When no target is
/// set the component is inert.
/// </summary>
public class CameraFollowComponent : EntityComponent
{
    /// <summary>The entity to follow, or null when not following. Settable via &lt;Reference/&gt;.</summary>
    public Entity? FollowTarget { get; private set; }

    /// <summary>Whether the camera is currently following a target.</summary>
    public bool FollowingTarget => FollowTarget != null;

    /// <summary>The per-frame lerp factor toward the target. Defaults to 0.1 (matches the old camera entity).</summary>
    public float LerpFactor { get; set; } = 0.1f;

    /// <inheritdoc />
    public override void Update(GameTime gameTime)
    {
        if (!FollowingTarget || Owner == null) return;

        // If the target was destroyed mid-follow, stop following instead of lerping to a stale position.
        if (FollowTarget.Destroyed)
        {
            FollowTarget = null;
            return;
        }

        Owner.Position = Vector2.Lerp(Owner.Position, FollowTarget.Position, LerpFactor);
    }

    /// <summary>
    /// Sets the entity to follow.
    /// </summary>
    /// <param name="target">The entity to follow, or null to stop.</param>
    /// <param name="startFollowingImmediately">Whether to start following right away.</param>
    public void SetFollowTarget(Entity? target, bool startFollowingImmediately = true)
    {
        FollowTarget = startFollowingImmediately ? target : null;
    }

    /// <summary>
    /// Toggles following of the given target: if already following that exact target, stops;
    /// otherwise starts following it.
    /// </summary>
    public void ToggleFollow(Entity targetToToggle)
    {
        if (FollowingTarget && FollowTarget == targetToToggle)
            SetFollowTarget(null, false);
        else
            SetFollowTarget(targetToToggle, true);
    }
}
