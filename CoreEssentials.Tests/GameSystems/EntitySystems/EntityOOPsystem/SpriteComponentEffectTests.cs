#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    /// <summary>
    /// Tests for the sprite renderer's companion-shader guarantee and the effect-based render partitioning in
    /// <see cref="EntitySystem"/>. The shader (effect + uniforms) now lives entirely on a
    /// <see cref="ShaderComponent"/>; a <see cref="SpriteComponent"/> guarantees such a sibling exists,
    /// auto-creating a basic one (no effect = the SpriteBatch default batch) when missing. Real MonoGame
    /// <see cref="Effect"/> objects cannot be created without a live GraphicsDevice, so these use
    /// uninitialized-effect fakes — enough to assert identity/precedence/partition behavior headlessly.
    /// </summary>
    public class SpriteComponentEffectTests
    {
        private sealed class TestEntity : Entity
        {
            public override void Render(SpriteBatch _spriteBatch) { }
        }

        // ===== Sprite renderer guarantees a companion ShaderComponent (code path) =====

        [Fact]
        public void AddSpriteComponent_AutoCreatesBasicShader()
        {
            var entity = new TestEntity();

            entity.AddComponent(new SpriteComponent());

            // A sibling shader now exists, with no effect (the default batch).
            var shader = entity.GetComponent<ShaderComponent>();
            Assert.NotNull(shader);
            Assert.Null(shader!.EffectiveEffect);
        }

        [Fact]
        public void AddSpriteComponent_WithExistingShader_DoesNotDuplicate()
        {
            var entity = new TestEntity();
            var userShader = new ShaderComponent { Effect = CreateFakeEffect() };
            entity.AddComponent(userShader);

            entity.AddComponent(new SpriteComponent());

            // The user-declared shader is kept, not replaced or duplicated.
            Assert.Same(userShader, entity.GetComponent<ShaderComponent>());
        }

        [Fact]
        public void AddSpriteComponent_WithExistingShaderKeepsItsEffect()
        {
            var entity = new TestEntity();
            var effect = CreateFakeEffect();
            entity.AddComponent(new ShaderComponent { Effect = effect });

            entity.AddComponent(new SpriteComponent());

            Assert.Same(effect, entity.GetComponent<ShaderComponent>()!.EffectiveEffect);
        }

        [Fact]
        public void EnsureShaderComponent_IsIdempotent()
        {
            var entity = new TestEntity();
            var first = SpriteComponent.EnsureShaderComponent(entity);
            var second = SpriteComponent.EnsureShaderComponent(entity);

            Assert.Same(first, second);
            // Only one shader component exists on the entity.
            Assert.Single(entity.Components);
        }

        // ===== Prefab/scene finish pass guarantees a companion ShaderComponent =====

        [Fact]
        public void Prefab_SpriteWithoutShader_AutoCreatesBasicShader()
        {
            var system = new EntitySystem();
            var prefab = new Prefab
            {
                Type = nameof(TestEntity),
                Components =
                {
                    new Prefab.ComponentDefinition { Type = nameof(SpriteComponent) }
                }
            };
            system.RegisterPrefab("sprite_only", prefab);

            var entity = system.Instantiate("sprite_only", Vector2.Zero);

            // The finish pass created a basic (no-effect) companion shader.
            var shader = entity.GetComponent<ShaderComponent>();
            Assert.NotNull(shader);
            Assert.Null(shader!.EffectiveEffect);
        }

        [Fact]
        public void Prefab_SpriteWithDeclaredShader_DoesNotDuplicateAndSeedsVars()
        {
            var system = new EntitySystem();
            var prefab = new Prefab
            {
                Type = nameof(TestEntity),
                Components =
                {
                    new Prefab.ComponentDefinition { Type = nameof(SpriteComponent) },
                    new Prefab.ComponentDefinition
                    {
                        Type = nameof(ShaderComponent),
                        EffectParameters = { ["GlowStrength"] = "1.0" }
                    }
                }
            };
            system.RegisterPrefab("sprite_with_shader", prefab);

            var entity = system.Instantiate("sprite_with_shader", Vector2.Zero);

            // Exactly one shader (the declared one, not a duplicate) and its vars were seeded.
            var shaders = new List<ShaderComponent>();
            foreach (var c in entity.Components)
                if (c is ShaderComponent s) shaders.Add(s);
            Assert.Single(shaders);
            Assert.Equal(1.0f, shaders[0].Get<float>("GlowStrength"));
        }

        [Fact]
        public void XmlOnlyShader_UniformsParsedFromEffectParameter_ReachComponent()
        {
            // Reproduces a data-driven (XML-only) shader: uniforms declared purely via
            // <EffectParameter> on the ShaderComponent, with no code controller overriding them.
            var system = new EntitySystem();
            var prefab = EntityPrefabLoader.LoadFromXml(@"
                <Prefab Type=""" + nameof(TestEntity) + @""">
                    <Components>
                        <Component Type=""SpriteComponent"">
                            <Properties>
                                <Property Name=""SpriteAsset"" Value=""Sprites/target_sprite.xml"" />
                            </Properties>
                        </Component>
                        <Component Type=""ShaderComponent"">
                            <Properties>
                                <Property Name=""EffectAsset"" Value=""Effects/GlowRadioactive"" />
                            </Properties>
                            <EffectParameter Name=""GlowStrength"" Value=""1.0"" />
                        </Component>
                    </Components>
                </Prefab>");

            system.RegisterPrefab("xml_only_shader", prefab);

            var entity = system.Instantiate("xml_only_shader", Vector2.Zero);

            // The XML-declared base value must reach the component — before, ParseComponentDefinition
            // never read <EffectParameter>, so this stayed at 0 (no glow).
            var shader = entity.GetComponent<ShaderComponent>();
            Assert.NotNull(shader);
            Assert.Equal(1.0f, shader!.Get<float>("GlowStrength"));

            system.Dispose();
        }

        // ===== PartitionByEffect (batcher effect grouping key) =====

        [Fact]
        public void Partition_AllNullEffects_SingleRun_NoRegression()
        {
            var entities = new List<Entity> { NewEffectEntity(null), NewEffectEntity(null), NewEffectEntity(null) };

            var runs = InvokePartitionByEffect(entities);

            // No effect anywhere -> exactly one null-effect run (the previous single Begin/End behavior).
            Assert.Single(runs);
            Assert.Null(runs[0].Effect);
            Assert.Equal(3, runs[0].Entities.Count);
        }

        [Fact]
        public void Partition_DistinctEffects_SplitIntoRuns_InOrder()
        {
            var a = CreateFakeEffect();
            var b = CreateFakeEffect();
            var entities = new List<Entity>
            {
                NewEffectEntity(a),
                NewEffectEntity(b),
                NewEffectEntity(a)
            };

            var runs = InvokePartitionByEffect(entities);

            Assert.Equal(3, runs.Count);
            Assert.Same(a, runs[0].Effect);
            Assert.Same(b, runs[1].Effect);
            Assert.Same(a, runs[2].Effect);
            Assert.All(runs, r => Assert.Single(r.Entities));
        }

        [Fact]
        public void Partition_AdjacentSameEffect_Coalesce()
        {
            var a = CreateFakeEffect();
            var b = CreateFakeEffect();
            var entities = new List<Entity>
            {
                NewEffectEntity(a),
                NewEffectEntity(a),
                NewEffectEntity(b)
            };

            var runs = InvokePartitionByEffect(entities);

            Assert.Equal(2, runs.Count);
            Assert.Same(a, runs[0].Effect);
            Assert.Equal(2, runs[0].Entities.Count);
            Assert.Same(b, runs[1].Effect);
            Assert.Single(runs[1].Entities);
        }

        [Fact]
        public void Partition_PreservesEntityOrderWithinAndAcrossRuns()
        {
            var a = CreateFakeEffect();
            var e1 = NewEffectEntity(a);
            var e2 = NewEffectEntity(null);
            var e3 = NewEffectEntity(a);

            var runs = InvokePartitionByEffect(new List<Entity> { e1, e2, e3 });

            Assert.Equal(3, runs.Count);
            Assert.Same(e1, runs[0].Entities[0]);
            Assert.Same(e2, runs[1].Entities[0]);
            Assert.Same(e3, runs[2].Entities[0]);
        }

        [Fact]
        public void Partition_EmptyList_NoRuns()
        {
            var runs = InvokePartitionByEffect(new List<Entity>());

            Assert.Empty(runs);
        }

        // ===== Partition with shader-uniform signatures =====

        [Fact]
        public void Partition_SameEffectDifferentVars_SplitIntoRuns()
        {
            var a = CreateFakeEffect();
            var e1 = NewEffectEntity(a);
            e1.GetComponent<ShaderComponent>()!.SetFloat("Strength", 1f);
            var e2 = NewEffectEntity(a);
            e2.GetComponent<ShaderComponent>()!.SetFloat("Strength", 2f);

            var runs = InvokePartitionByEffect(new List<Entity> { e1, e2 });

            // Same effect but different uniform values must split into separate runs.
            Assert.Equal(2, runs.Count);
            Assert.Same(a, runs[0].Effect);
            Assert.Same(a, runs[1].Effect);
            Assert.NotEqual(runs[0].Signature, runs[1].Signature);
            Assert.All(runs, r => Assert.Single(r.Entities));
        }

        [Fact]
        public void Partition_SameEffectSameVars_Coalesce()
        {
            var a = CreateFakeEffect();
            var e1 = NewEffectEntity(a);
            e1.GetComponent<ShaderComponent>()!.SetFloat("Strength", 1f);
            var e2 = NewEffectEntity(a);
            e2.GetComponent<ShaderComponent>()!.SetFloat("Strength", 1f);

            var runs = InvokePartitionByEffect(new List<Entity> { e1, e2 });

            // Same effect AND same uniform values coalesce into one run.
            Assert.Single(runs);
            Assert.Same(a, runs[0].Effect);
            Assert.Equal(2, runs[0].Entities.Count);
        }

        [Fact]
        public void Partition_EffectWithoutParams_SignatureIsEmpty()
        {
            var a = CreateFakeEffect();
            var e1 = NewEffectEntity(a); // no uniforms set

            var runs = InvokePartitionByEffect(new List<Entity> { e1 });

            Assert.Single(runs);
            Assert.Equal(string.Empty, runs[0].Signature);
        }

        [Fact]
        public void Partition_MixedWithAndWithoutParams_Split()
        {
            var a = CreateFakeEffect();
            var withParams = NewEffectEntity(a);
            withParams.GetComponent<ShaderComponent>()!.SetFloat("Strength", 1f);
            var withoutParams = NewEffectEntity(a);

            var runs = InvokePartitionByEffect(new List<Entity> { withParams, withoutParams });

            // A parameterized entity and a bare entity on the same effect split ("" != "Strength=1").
            Assert.Equal(2, runs.Count);
            Assert.Same(withParams, runs[0].Entities[0]);
            Assert.Same(withoutParams, runs[1].Entities[0]);
        }

        // ===== Helpers =====

        private static Entity NewEffectEntity(Effect? effect)
        {
            var entity = new TestEntity();
            entity.AddComponent(new ShaderComponent { Effect = effect });
            return entity;
        }

        private static Effect CreateFakeEffect() =>
            (Effect)RuntimeHelpers.GetUninitializedObject(typeof(Effect));

        private static List<(Effect? Effect, string Signature, List<Entity> Entities)> InvokePartitionByEffect(List<Entity> entities)
        {
            var method = typeof(EntitySystem).GetMethod(
                "PartitionByEffect",
                BindingFlags.NonPublic | BindingFlags.Static);

            return (List<(Effect? Effect, string Signature, List<Entity> Entities)>)method!.Invoke(null, new object[] { entities })!;
        }
    }
}
