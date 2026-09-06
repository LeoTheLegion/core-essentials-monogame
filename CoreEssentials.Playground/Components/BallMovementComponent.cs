using System.Collections;
using CoreEssentials.Coroutines;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.Utils;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground.Components;

/// <summary>
/// Gives the owning entity (a physics ball carrying a built-in <c>RigidbodyComponent</c>) the
/// random "kick + spin" movement that used to live in the hand-written ball entity. On attach it rolls
/// a random display scale for the ball (so balls come in varied sizes) — but only when the owner's
/// scale is still the default <see cref="Vector2.One"/>, so an explicit scale set by the spawner (VIP
/// balls) or restored from a save wins — and starts a coroutine that repeatedly applies a random-direction
/// impulse plus a random angular impulse, waiting a random number of seconds between kicks; on detach it
/// stops all its coroutines. Declared purely from data:
/// <code>
/// &lt;Component Type="BallMovementComponent" /&gt;
/// </code>
/// When no <c>RigidbodyComponent</c> is attached the component is inert.
/// </summary>
public class BallMovementComponent : EntityComponent
{
    private CoroutineOwner? _coroutineOwner;

    /// <summary>The magnitude of each random-direction kick impulse.</summary>
    public float ImpulseStrength { get; set; } = 500000f;

    /// <summary>Half-range of the random angular (spin) impulse applied with each kick.</summary>
    public float SpinImpulseHalfRange { get; set; } = 5f;

    /// <summary>Inclusive minimum seconds to wait between kicks.</summary>
    public int MinWaitSeconds { get; set; } = 1;

    /// <summary>Inclusive maximum seconds to wait between kicks.</summary>
    public int MaxWaitSeconds { get; set; } = 4;

    /// <inheritdoc />
    public override void OnAttach()
    {
        // Balls come in random sizes (matches the old ball entity's constructor behavior). A loaded ball
        // is unaffected: LoadState restores its saved scale after OnAttach has run, and a VIP/explicit
        // scale set by the spawner before attach is non-Unit so it wins here too.
        if (Owner.Scale == Vector2.One)
        {
            float randomScale = GameRandom.NextFloat(0.5f, 1.5f);
            Owner.Scale = new Vector2(randomScale, randomScale);
        }

        _coroutineOwner ??= new CoroutineOwner();
        _coroutineOwner.StartCoroutine(RandomMovementCoroutine());
    }

    /// <inheritdoc />
    public override void OnDetach()
    {
        _coroutineOwner?.StopAllCoroutines();
        _coroutineOwner = null;
    }

    private IEnumerator RandomMovementCoroutine()
    {
        while (!Owner.Destroyed)
        {
            var direction = GameRandom.RandomVector2();

            var rigidbody = Owner.GetComponent<RigidbodyComponent>();
            rigidbody?.ApplyImpulse(direction * ImpulseStrength);
            rigidbody?.ApplyAngularImpulse(GameRandom.NextSignedFloat() * SpinImpulseHalfRange);

            yield return new WaitForSeconds(GameRandom.Next(MinWaitSeconds, MaxWaitSeconds + 1));
        }
    }
}
