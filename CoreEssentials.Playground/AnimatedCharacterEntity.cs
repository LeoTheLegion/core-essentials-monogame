using System;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using Microsoft.Xna.Framework;

namespace CoreEssentials.Playground
{
    /// <summary>
    /// A character entity that demonstrates animated sprite functionality.
    /// Animation is driven entirely by an <see cref="AnimationComponent"/> (no Render/GetSize/Update overrides).
    /// </summary>
    public class AnimatedCharacterEntity : Entity
    {
        // Parameterless constructor for XML-based entity loading
        public AnimatedCharacterEntity()
        {
        }

        public AnimatedCharacterEntity(Vector2 position)
        {
            Position = position;
        }

        public override void OnStart()
        {
            base.OnStart();

            // Load the animated sprite. The SpriteComponent owns rendering + geometry;
            // the AnimationComponent is a pure controller that drives its frames.
            var sprite = AssetManager.LoadAsset<Sprite>("character_anim_walk.xml");
            AddComponent(new SpriteComponent(sprite));
            var animation = AddComponent(new AnimationComponent());
            animation.AddAnimation("walk", sprite);
            animation.Play("walk");

            Console.WriteLine($"Animation has {sprite.FrameCount} frames with base frame rate of {sprite.FrameRate}s per frame");
            Console.WriteLine("Animated character entity created!");
        }

        public override void OnDestroy()
        {
            base.OnDestroy();

            Console.WriteLine("Animated character entity destroyed!");
        }
    }
}