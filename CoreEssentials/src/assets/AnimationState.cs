using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace CoreEssentials.Assets;

/// <summary>
/// Maintains the state of an animation for a specific instance.
/// This class tracks which frame is currently showing and handles timing for frame transitions.
/// </summary>
public class AnimationState
{
    private Sprite _animatedSprite;
    private int _currentFrame;
    private float _animationTimer;
    private float _speed = 1.0f; // Default speed multiplier
    private bool _isPlaying = true;
    private bool _isLooping = true;
    
    /// <summary>
    /// Event raised when an animation completes a full cycle.
    /// Only raised when the animation is not looping.
    /// </summary>
    public event EventHandler? AnimationCompleted;
    
    /// <summary>
    /// Gets or sets the playback speed multiplier.
    /// Values greater than 1 make the animation faster, values less than 1 make it slower.
    /// </summary>
    public float Speed
    {
        get => _speed;
        set => _speed = Math.Max(0.01f, value); // Prevent zero or negative speed
    }
    
    /// <summary>
    /// Gets or sets whether the animation is playing.
    /// </summary>
    public bool IsPlaying
    {
        get => _isPlaying;
        set => _isPlaying = value;
    }
    
    /// <summary>
    /// Gets or sets whether the animation should loop when it reaches the end.
    /// </summary>
    public bool IsLooping
    {
        get => _isLooping;
        set => _isLooping = value;
    }
    
    /// <summary>
    /// Gets the current frame index in the animation sequence.
    /// </summary>
    public int CurrentFrame => _currentFrame;
    
    /// <summary>
    /// Gets the sprite used by this animation state.
    /// </summary>
    public Sprite Sprite => _animatedSprite;
    
    /// <summary>
    /// Gets the effective frame time in seconds, accounting for the speed multiplier.
    /// </summary>
    public float EffectiveFrameTime => _animatedSprite != null ? _animatedSprite.FrameRate / _speed : 1f;
    
    /// <summary>
    /// Gets the progress through the current frame as a value from 0.0 to 1.0.
    /// </summary>
    public float FrameProgress => _animationTimer / EffectiveFrameTime;
    
    /// <summary>
    /// Gets the progress through the entire animation as a value from 0.0 to 1.0.
    /// </summary>
    public float AnimationProgress
    {
        get
        {
            if (_animatedSprite == null || _animatedSprite.FrameCount <= 1)
                return 0f;
                
            return (_currentFrame + FrameProgress) / _animatedSprite.FrameCount;
        }
    }
    
    /// <summary>
    /// Initializes a new instance of the AnimationState class with the specified sprite.
    /// </summary>
    /// <param name="sprite">The sprite to use for this animation state.</param>
    /// <exception cref="ArgumentNullException">Thrown when the sprite parameter is null.</exception>
    public AnimationState(Sprite sprite)
    {
        _animatedSprite = sprite ?? throw new ArgumentNullException(nameof(sprite));
        _currentFrame = 0;
        _animationTimer = 0;
    }
    
    /// <summary>
    /// Replaces the sprite backing this animation state, preserving the current frame and timing.
    /// Used when restoring a deserialized state after the sprite asset has been reloaded.
    /// </summary>
    /// <param name="sprite">The sprite to use for this animation state.</param>
    public void SetSprite(Sprite sprite)
    {
        _animatedSprite = sprite ?? throw new ArgumentNullException(nameof(sprite));
    }

    /// <summary>
    /// Updates the animation state.
    /// </summary>
    /// <param name="gameTime">Provides a snapshot of timing values.</param>
    public void Update(GameTime gameTime)
    {
        if (!_isPlaying || _animatedSprite == null || _animatedSprite.FrameCount <= 1)
            return;
            
        _animationTimer += (float)gameTime.ElapsedGameTime.TotalSeconds;
        
        if (_animationTimer >= EffectiveFrameTime)
        {
            // Advance to next frame
            _currentFrame++;
            
            // Handle end of animation
            if (_currentFrame >= _animatedSprite.FrameCount)
            {
                if (_isLooping)
                {
                    // Loop back to the first frame
                    _currentFrame = 0;
                }
                else
                {
                    // Stay on the last frame
                    _currentFrame = _animatedSprite.FrameCount - 1;
                    _isPlaying = false;
                    
                    // Raise animation completed event
                    AnimationCompleted?.Invoke(this, EventArgs.Empty);
                }
            }
            
            _animationTimer = 0;
        }
    }
    
    /// <summary>
    /// Draws the current frame of the animation.
    /// </summary>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    /// <param name="position">The position to draw the sprite at.</param>
    /// <param name="color">The color to tint the sprite with.</param>
    /// <param name="rotation">The rotation angle of the sprite in radians.</param>
    /// <param name="effects">Sprite effects like flipping horizontally or vertically.</param>
    /// <param name="layerDepth">The layer depth to draw the sprite at (0 to 1).</param>
    public void Draw(SpriteBatch spriteBatch, Vector2 position, Color color, 
                    float rotation = 0f, SpriteEffects effects = SpriteEffects.None, float layerDepth = 0f)
    {
        if (_animatedSprite == null)
            return;
            
        _animatedSprite.DrawFrame(spriteBatch, position, _currentFrame, color, rotation, effects, layerDepth);
    }
    
    /// <summary>
    /// Resets the animation to the first frame.
    /// </summary>
    public void Reset()
    {
        _currentFrame = 0;
        _animationTimer = 0;
        _isPlaying = true;
    }
    
    /// <summary>
    /// Plays the animation from the current frame.
    /// </summary>
    public void Play()
    {
        _isPlaying = true;
    }
    
    /// <summary>
    /// Pauses the animation at the current frame.
    /// </summary>
    public void Pause()
    {
        _isPlaying = false;
    }
    
    /// <summary>
    /// Stops the animation and resets to the first frame.
    /// </summary>
    public void Stop()
    {
        _isPlaying = false;
        _currentFrame = 0;
        _animationTimer = 0;
    }
    
    /// <summary>
    /// Sets the current frame to the specified index.
    /// </summary>
    /// <param name="frameIndex">The index of the frame to set.</param>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the frame index is out of range.</exception>
    public void SetFrame(int frameIndex)
    {
        if (_animatedSprite == null)
            return;
            
        if (frameIndex < 0 || frameIndex >= _animatedSprite.FrameCount)
        {
            throw new ArgumentOutOfRangeException(nameof(frameIndex), $"Frame index {frameIndex} is out of range (0-{_animatedSprite.FrameCount - 1})");
        }
        
        _currentFrame = frameIndex;
        _animationTimer = 0;
    }
}