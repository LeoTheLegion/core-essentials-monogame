using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using Microsoft.Xna.Framework;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// Drives one or more named animations on an entity.
/// Each animation is an <see cref="AnimationState"/> backed by a unified <see cref="Sprite"/>.
/// This is a pure controller: it advances playing states each frame and pushes the current
/// frame into the entity's <see cref="SpriteComponent"/>, which owns all rendering and geometry
/// (<see cref="Entity.GetSize"/>, <see cref="Entity.GetOrigin"/>). An entity that uses this
/// component must also attach a <see cref="SpriteComponent"/>.
/// </summary>
public class AnimationComponent : EntityComponent
{
    private readonly Dictionary<string, AnimationState> _animations = new();
    private readonly Dictionary<string, Sprite> _sprites = new();
    private string? _currentAnimation;

    /// <summary>
    /// Gets the names of all animations registered on this component.
    /// </summary>
    public IReadOnlyCollection<string> Animations => _animations.Keys;

    /// <summary>
    /// Gets or sets the name of the currently active animation.
    /// </summary>
    public string? CurrentAnimation
    {
        get => _currentAnimation;
        set => _currentAnimation = value;
    }

    /// <summary>
    /// Gets the <see cref="AnimationState"/> of the current animation, or null if none.
    /// </summary>
    public AnimationState? CurrentAnimationState =>
        _currentAnimation != null && _animations.TryGetValue(_currentAnimation, out var state) ? state : null;

    /// <summary>
    /// Gets the <see cref="Sprite"/> backing the current animation, or null if none.
    /// </summary>
    public Sprite? Sprite =>
        _currentAnimation != null && _sprites.TryGetValue(_currentAnimation, out var sprite) ? sprite : null;

    /// <summary>
    /// Gets or sets the asset name of a sprite to load via the <see cref="AssetManager"/> (e.g. "Sprites/walk.xml").
    /// When set, <see cref="OnAttach"/> loads the asset and registers it as an animation named
    /// <see cref="AnimationName"/>, then starts playing it — so data-driven (XML) entities can declare a
    /// full walk cycle with plain string properties instead of per-game glue components. Only applied
    /// when no animation is already registered under that name (code-registered animations always win).
    /// </summary>
    public string SpriteAsset { get; set; } = "";

    /// <summary>
    /// Gets or sets the name used to register and play the <see cref="SpriteAsset"/> animation (default "walk").
    /// </summary>
    public string AnimationName { get; set; } = "walk";

    /// <summary>
    /// Initializes a new instance of the <see cref="AnimationComponent"/> class.
    /// </summary>
    public AnimationComponent()
    {
    }

    /// <summary>
    /// Called when the component is attached to an entity. Loads and plays a declaratively declared
    /// <see cref="SpriteAsset"/> (if any).
    /// </summary>
    public override void OnAttach()
    {
        base.OnAttach();

        // Resolve a declaratively declared sprite asset: load it, register it under AnimationName,
        // and start playing. Skipped when an animation is already registered under that name —
        // code-registered animations always win over XML-declared ones.
        if (!string.IsNullOrWhiteSpace(SpriteAsset) && !_animations.ContainsKey(AnimationName))
        {
            try
            {
                var sprite = AssetManager.LoadAsset<Sprite>(SpriteAsset);
                AddAnimation(AnimationName, sprite);
                Play(AnimationName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[AnimationComponent] Could not load sprite asset '{SpriteAsset}': {ex.Message}");
            }
        }
    }

    /// <summary>
    /// Adds a named animation backed by the given sprite.
    /// </summary>
    /// <param name="name">A unique name for the animation.</param>
    /// <param name="sprite">The sprite (frame sequence) to animate.</param>
    public void AddAnimation(string name, Sprite sprite)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Animation name cannot be null or empty.", nameof(name));
        if (sprite == null)
            throw new ArgumentNullException(nameof(sprite));
        if (_animations.ContainsKey(name))
            throw new InvalidOperationException($"An animation named '{name}' already exists.");

        _animations[name] = new AnimationState(sprite);
        _sprites[name] = sprite;
    }

    /// <summary>
    /// Plays the named animation and stops all other animations.
    /// </summary>
    /// <param name="name">The name of the animation to play.</param>
    public void Play(string name)
    {
        if (!_animations.TryGetValue(name, out var state))
            throw new KeyNotFoundException($"No animation named '{name}'.");

        foreach (var other in _animations.Values)
            other.Pause();

        state.Play();
        _currentAnimation = name;
    }

    /// <summary>
    /// Stops an animation.
    /// </summary>
    /// <param name="name">The name of the animation to stop. If null, all animations stop.</param>
    public void Stop(string? name = null)
    {
        if (name == null)
        {
            foreach (var animState in _animations.Values)
                animState.Stop();
            return;
        }

        if (!_animations.TryGetValue(name, out var state))
            throw new KeyNotFoundException($"No animation named '{name}'.");

        state.Stop();
    }

    /// <summary>
    /// Sets the playback speed of the named animation.
    /// </summary>
    /// <param name="name">The name of the animation.</param>
    /// <param name="speed">The speed multiplier (values &gt; 1 are faster).</param>
    public void SetSpeed(string name, float speed)
    {
        if (!_animations.TryGetValue(name, out var state))
            throw new KeyNotFoundException($"No animation named '{name}'.");

        state.Speed = speed;
    }

    /// <summary>
    /// Gets the <see cref="AnimationState"/> for the named animation, or null if not found.
    /// </summary>
    public AnimationState? GetAnimation(string name) =>
        _animations.TryGetValue(name, out var state) ? state : null;

    /// <summary>
    /// Advances all playing animations and pushes the current frame into the entity's
    /// <see cref="SpriteComponent"/>.
    /// </summary>
    public override void Update(GameTime gameTime)
    {
        foreach (var state in _animations.Values)
            state.Update(gameTime);

        var current = CurrentAnimationState;
        if (current == null)
            return;

        if (Owner != null && Owner.TryGetComponent<SpriteComponent>(out var spriteComponent) && spriteComponent != null)
        {
            var sprite = current.Sprite;
            int frame = current.CurrentFrame;
            if (sprite != null && sprite.FrameCount > 0)
                frame = MathHelper.Clamp(frame, 0, sprite.FrameCount - 1);
            spriteComponent.AnimationFrame = frame;
        }
    }

}
