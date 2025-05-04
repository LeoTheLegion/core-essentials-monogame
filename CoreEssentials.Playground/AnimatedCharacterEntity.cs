using System;
using System.Collections;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Debugging;
using CoreEssentials.Inputs;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using MonoGame.Extended.Input.InputListeners;

namespace CoreEssentials.Playground
{
    /// <summary>
    /// A simple character entity that demonstrates animated sprite functionality.
    /// </summary>
    public class AnimatedCharacterEntity : Entity
    {
        private AnimatedSprite _animatedSprite;
        private AnimationState _animationState;
        
        public AnimatedCharacterEntity(Vector2 position)
        {
            _position = position;
            
            // Load the animated sprite
            _animatedSprite = (AnimatedSprite)AssetManager.LoadAsset<AnimatedSprite>("character_anim_walk.xml");
            
            // Create animation state for this instance
            _animationState = new AnimationState(_animatedSprite);
            
            Debug.Console.WriteLine($"Animation has {_animatedSprite.FrameCount} frames with base frame rate of {_animatedSprite.FrameRate}s per frame");
            Debug.Console.WriteLine($"Effective frame time with speed {_animationState.Speed}: {_animationState.EffectiveFrameTime}s per frame");
        }
        
        public override void OnStart()
        {
            base.OnStart();
            Debug.Console.WriteLine("Animated character entity created!");
        }
        
        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);
            
            // Update the animation
            _animationState.Update(gameTime);
        }
        
        public override void Render(SpriteBatch spriteBatch)
        {
            SpriteEffects effects = SpriteEffects.None;
            
            // Draw the animated character using the current animation state
            _animationState.Draw(
                spriteBatch, 
                _position, 
                Color.White,
                0f,
                effects,
                0f
            );
        }
        
        public override void OnDestroy()
        {
            base.OnDestroy();
            
            Debug.Console.WriteLine("Animated character entity destroyed!");
        }
    }
}