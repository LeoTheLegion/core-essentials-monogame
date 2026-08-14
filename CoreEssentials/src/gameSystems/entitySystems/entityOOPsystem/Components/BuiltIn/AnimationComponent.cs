using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// Drives one or more named animations on an entity.
/// Each animation is an <see cref="AnimationState"/> backed by a unified <see cref="Sprite"/>.
/// The component advances playing states each frame and pushes the current frame into the
/// entity's <see cref="SpriteComponent"/> (when present). If no <see cref="SpriteComponent"/> is
/// attached, the component renders the current frame directly as a fallback.
/// </summary>
public class AnimationComponent : EntityComponent, ISerializableComponent, IDrawableComponent
{
    private readonly Dictionary<string, AnimationState> _animations = new();
    private readonly Dictionary<string, Sprite> _sprites = new();
    private readonly Dictionary<string, string> _assetNames = new();
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
    /// Initializes a new instance of the <see cref="AnimationComponent"/> class.
    /// </summary>
    public AnimationComponent()
    {
    }

    /// <summary>
    /// Called when the component is attached to an entity.
    /// Reloads sprite assets for animations that were restored from deserialization
    /// (which happens before attachment, when the <see cref="AssetManager"/> may not have
    /// been able to resolve them yet).
    /// </summary>
    public override void OnAttach()
    {
        base.OnAttach();

        // Reload sprites for animations restored from deserialization (their states are backed
        // by the shared placeholder until the real asset is resolved here).
        foreach (var name in _assetNames.Keys.ToList())
        {
            if (!_animations.TryGetValue(name, out var state))
                continue;

            if (state.Sprite == null || state.Sprite.Name == PlaceholderSprite.Name)
            {
                try
                {
                    var sprite = AssetManager.LoadAsset<Sprite>(_assetNames[name]);
                    _sprites[name] = sprite;
                    state.SetSprite(sprite);
                }
                catch (Exception)
                {
                    // Asset unavailable (e.g. AssetManager not initialized); leave unresolved.
                }
            }
        }

        // If a current animation was restored, start it playing.
        if (CurrentAnimationState != null)
            CurrentAnimationState.Play();
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
        _assetNames[name] = sprite.Name;
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
    /// <see cref="SpriteComponent"/> (if present).
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

    /// <summary>
    /// Draws the current animation frame.
    /// If the entity has a <see cref="SpriteComponent"/>, that component is responsible for
    /// rendering (driven by <see cref="Update"/>), so this method does nothing to avoid
    /// double-drawing. Otherwise it renders the current frame directly as a fallback.
    /// </summary>
    public void Draw(SpriteBatch spriteBatch)
    {
        if (Owner == null)
            return;

        if (Owner.TryGetComponent<SpriteComponent>(out var spriteComponent) && spriteComponent != null)
            return;

        var current = CurrentAnimationState;
        if (current == null)
            return;

        current.Draw(spriteBatch, Owner.Position, Color.White, Owner.Rotation, SpriteEffects.None, 0f);
    }

    /// <summary>
    /// Gets the size of the current animation frame, scaled by the owning entity.
    /// </summary>
    public Vector2 GetSize()
    {
        var current = CurrentAnimationState;
        if (current == null || current.Sprite == null)
            return Vector2.Zero;

        try
        {
            return current.Sprite.GetSize() * (Owner?.Scale ?? Vector2.One);
        }
        catch (InvalidOperationException)
        {
            return Vector2.Zero;
        }
    }

    /// <summary>
    /// Serializes the component's state to an XML element.
    /// Persists animation names + asset names, the current animation name, and per-animation
    /// speed/loop state.
    /// </summary>
    public XElement SerializeToXml()
    {
        return new XElement("AnimationComponentState",
            new XAttribute("CurrentAnimation", _currentAnimation ?? ""),
            _animations.Select(pair =>
            {
                var name = pair.Key;
                var animState = pair.Value;
                return new XElement("Animation",
                    new XAttribute("Name", name),
                    new XAttribute("AssetName", _assetNames.TryGetValue(name, out var asset) ? asset : ""),
                    new XAttribute("Speed", animState.Speed.ToString(System.Globalization.CultureInfo.InvariantCulture)),
                    new XAttribute("Looping", animState.IsLooping));
            })
        );
    }

    /// <summary>
    /// Deserializes the component's state from an XML element.
    /// Restores animation names + asset names (sprites are reloaded in <see cref="OnAttach"/>),
    /// the current animation name, and per-animation speed/loop state.
    /// </summary>
    public void DeserializeFromXml(XElement element)
    {
        _animations.Clear();
        _sprites.Clear();
        _assetNames.Clear();

        string current = element.Attribute("CurrentAnimation")?.Value ?? "";
        _currentAnimation = string.IsNullOrEmpty(current) ? null : current;

        foreach (var animElement in element.Elements("Animation"))
        {
            string? name = animElement.Attribute("Name")?.Value;
            string? assetName = animElement.Attribute("AssetName")?.Value;
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(assetName))
                continue;

            _assetNames[name] = assetName;

            // Create a placeholder state until the sprite is reloaded in OnAttach.
            if (!_animations.ContainsKey(name))
            {
                _animations[name] = new AnimationState(PlaceholderSprite);
            }

            var state = _animations[name];
            if (float.TryParse(animElement.Attribute("Speed")?.Value, System.Globalization.NumberStyles.Any, System.Globalization.CultureInfo.InvariantCulture, out float speed))
                state.Speed = speed;
            if (bool.TryParse(animElement.Attribute("Looping")?.Value, out bool looping))
                state.IsLooping = looping;
        }
    }

    /// <summary>
    /// A shared placeholder sprite used to back deserialized animation states until their real
    /// sprite asset is reloaded in <see cref="OnAttach"/>.
    /// </summary>
    private static readonly Sprite PlaceholderSprite = new Sprite("__placeholder__");
}
