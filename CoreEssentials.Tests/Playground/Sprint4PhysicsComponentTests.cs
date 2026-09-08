using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.Physics.Engines.Aether;
using CoreEssentials.GameSystems.Physics.Types;
using CoreEssentials.Scenes;
using CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem;
using CoreEssentials.Playground.Components;

namespace CoreEssentials.Tests.Playground
{
    /// <summary>
    /// Sprint 4 — proves the physics-demo ball and world border now run entirely from components,
    /// with no hand-written entity subclass:
    ///   • <see cref="WorldBorderComponent"/> builds four static bodies whose collision mask contains
    ///     BOTH the regular ("Player") and VIP ("Vip") categories resolved from the engine config.
    ///   • <see cref="BallMovementComponent"/> kicks + spins the owning rigidbody on a coroutine and
    ///     stops cleanly on detach.
    ///   • <see cref="BallSaveComponent"/> round-trips a plain GameObjectEntity's transform, tags, and the
    ///     specific component state it needs (sprite color + physics velocity) through the explicit
    ///     save/load path — no entity subclass required.
    /// </summary>
    public class Sprint4PhysicsComponentTests : IDisposable
    {
        private readonly SceneWrapper _scene = null!;
        private readonly EntitySystem _entitySystem = null!;
        private readonly PhysicsEngine _physicsEngine = null!;
        private bool _disposed;

        public Sprint4PhysicsComponentTests()
        {
            (_scene, _entitySystem, _physicsEngine) = BuildWiredSystem(PlayerVipConfig());
        }

        public void Dispose()
        {
            if (_disposed) return;
            CoreEssentials.Coroutines.CoroutineManager.StopAllCoroutines();
            _physicsEngine?.Dispose();
            _disposed = true;
            GC.SuppressFinalize(this);
        }

        #region WorldBorderComponent

        [Fact]
        public void WorldBorder_OnAttach_CreatesFourStaticBodies_WithCombinedPlayerVipMask()
        {
            int baseline = _physicsEngine.GetBodies().Count;

            var border = _entitySystem.CreateEntity<GameObjectEntity>();
            border.Position = Vector2.Zero;
            border.AddComponent(new WorldBorderComponent { Size = new Vector2(320, 180) });

            var bodies = _physicsEngine.GetBodies();
            Assert.Equal(baseline + 4, bodies.Count);

            // Every border body is static and carries the combined Player|Vip mask on its collider.
            var expectedMask = CollisionCategory.Cat1 | CollisionCategory.Cat2;
            foreach (var body in bodies.Skip(baseline))
            {
                Assert.True(body.IsStatic);
                Assert.Single(body.Colliders);
                Assert.Equal(expectedMask, body.Colliders[0].Categories);
                Assert.Equal(expectedMask, body.Colliders[0].CollidesWith);
            }
        }

        [Fact]
        public void WorldBorder_WithInvalidSize_CreatesNoBodies()
        {
            int baseline = _physicsEngine.GetBodies().Count;

            var border = _entitySystem.CreateEntity<GameObjectEntity>();
            border.AddComponent(new WorldBorderComponent { Size = Vector2.Zero });

            Assert.Equal(baseline, _physicsEngine.GetBodies().Count);
        }

        #endregion

        #region BallMovementComponent

        [Fact]
        public void BallMovement_AppliesImpulseOnTick_AndStopsOnDetach()
        {
            var helper = new CoreEssentials.Tests.Coroutines.CoroutineTestHelper();
            try
            {
                var ball = _entitySystem.CreateEntity<GameObjectEntity>();
                var rigidbody = ball.AddComponent(new RigidbodyComponent(RigidbodyType.Dynamic));
                rigidbody.CreateBody();

                ball.AddComponent(new BallMovementComponent { ImpulseStrength = 1000f });

                // First tick runs the coroutine body once: a random-direction impulse is applied.
                helper.Tick();
                var afterFirstKick = rigidbody.LinearVelocity;
                Assert.NotEqual(Vector2.Zero, afterFirstKick);

                // Detach stops the coroutine — no further kicks even after the wait window elapses.
                ball.RemoveComponent<BallMovementComponent>();
                for (int i = 0; i < 5; i++)
                    helper.AdvanceTime(3f);

                Assert.Equal(afterFirstKick, rigidbody.LinearVelocity);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        [Fact]
        public void BallMovement_WithoutRigidbody_IsInert()
        {
            var helper = new CoreEssentials.Tests.Coroutines.CoroutineTestHelper();
            try
            {
                int baseline = _physicsEngine.GetBodies().Count;
                var ball = _entitySystem.CreateEntity<GameObjectEntity>();
                ball.AddComponent(new BallMovementComponent());

                for (int i = 0; i < 3; i++) helper.Tick();

                // No rigidbody means no body was created and no impulse could land.
                Assert.Equal(baseline, _physicsEngine.GetBodies().Count);
            }
            finally
            {
                helper.Cleanup();
            }
        }

        #endregion

        #region Ball save/load round-trip (component + prefab)

        [Fact]
        public void Ball_SaveLoad_RoundTrips_Transform_Tags_SpriteColor_AndVelocity_ViaComponentAndPrefab()
        {
            var (scene, system, engine) = BuildWiredSystem(PlayerVipConfig());
            RegisterBallPrefab(system);
            var tempFile = Path.GetTempFileName();
            try
            {
                // Source: a ball instantiated from the prefab, then given deterministic state. The
                // random-scale roll in BallMovementComponent.OnAttach runs at instantiate time (scale is
                // still One), so we override it explicitly afterward — mirroring a VIP/explicit scale.
                var source = system.Instantiate("BallTest", Vector2.Zero);
                source.SetId("ball_test");
                source.Position = new Vector2(100, 200);
                source.Rotation = 30f;
                source.Scale = new Vector2(2, 2);
                source.SetSort(5);
                source.SetActive(true);

                source.GetComponent<SpriteComponent>()!.Color = new Color(10, 20, 30);

                var rigidbody = source.GetComponent<RigidbodyComponent>()!;
                rigidbody.CreateBody();
                rigidbody.SetLinearVelocity(new Vector2(5, 7));
                rigidbody.AngularVelocity = 2f;

                GameStateSerializer.SaveState(system, tempFile);

                // Load into a fresh system with the same prefab registered — the serializer instantiates
                // from the prefab (OnAttach runs), sets the saved Id, then BallSaveComponent.LoadState
                // restores the saved state on top of it.
                var (loadScene, loadSystem, loadEngine) = BuildWiredSystem(PlayerVipConfig());
                RegisterBallPrefab(loadSystem);
                GameStateSerializer.LoadState(loadSystem, tempFile);

                var loaded = loadSystem.GetEntities().FirstOrDefault(e => e.Id == "ball_test");
                Assert.NotNull(loaded);
                Assert.IsType<GameObjectEntity>(loaded);
                // The save component is what makes the plain entity saveable.
                Assert.NotNull(loaded!.GetComponent<BallSaveComponent>());

                Assert.Equal(new Vector2(100, 200), loaded.Position);
                Assert.Equal(30f, loaded.Rotation, 0.01f);
                Assert.Equal(new Vector2(2, 2), loaded.Scale);
                Assert.Equal(5, loaded.GetSort());
                Assert.Contains("Ball", loaded.Tags);

                Assert.Equal(new Color(10, 20, 30), loaded.GetComponent<SpriteComponent>()!.Color);

                var loadedBody = loaded.GetComponent<RigidbodyComponent>()!;
                Assert.True(loadedBody.IsBodyCreated);
                var restoredVelocity = loadedBody.LinearVelocity;
                Assert.Equal(5f, restoredVelocity.X, 0.01f);
                Assert.Equal(7f, restoredVelocity.Y, 0.01f);
                Assert.Equal(2f, loadedBody.AngularVelocity, 0.01f);

                loadEngine.Dispose();
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
                engine.Dispose();
            }
        }

        [Fact]
        public void Ball_SaveComponentWithoutPrefab_FailsFastOnSave()
        {
            var (scene, system, engine) = BuildWiredSystem(PlayerVipConfig());
            try
            {
                // A hand-created ball carrying a save component but NO registered prefab must fail fast.
                var ball = system.CreateEntity<GameObjectEntity>();
                ball.SetId("ball_noprefab");
                ball.AddComponent(new BallSaveComponent());

                Assert.Throws<InvalidOperationException>(() => GameStateSerializer.SaveState(system, Path.GetTempFileName()));
            }
            finally
            {
                engine.Dispose();
            }
        }

        #endregion

        #region Helpers

        /// <summary>A config with the same named categories as the shipping PhysicsConfig.xml.</summary>
        private static PhysicsConfig PlayerVipConfig() => PhysicsConfig.LoadFromXml(
            "<PhysicsConfig><Categories><Category Name=\"Player\" /><Category Name=\"Vip\" /></Categories></PhysicsConfig>");

        /// <summary>
        /// Registers a ball prefab (plain GameObjectEntity + BallSaveComponent) for round-trip tests.
        /// The sprite asset is intentionally omitted so no content load is required in the headless test.
        /// </summary>
        private static void RegisterBallPrefab(EntitySystem system)
        {
            system.RegisterPrefab("BallTest", EntityPrefabLoader.LoadFromXml(
                "<Prefab Type=\"CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.GameObjectEntity\">" +
                "<Tags><Tag Name=\"Ball\" /><Tag Name=\"Physical\" /></Tags>" +
                "<Components>" +
                "<Component Type=\"SpriteComponent\"><Properties><Property Name=\"Color\" Value=\"White\" /><Property Name=\"Origin\" Value=\"0.5,0.5\" /></Properties></Component>" +
                "<Component Type=\"RigidbodyComponent\"><Properties><Property Name=\"FixedRotation\" Value=\"false\" /><Property Name=\"Mass\" Value=\"1.0\" /></Properties></Component>" +
                "<Component Type=\"ColliderComponent\"><Properties><Property Name=\"ShapeType\" Value=\"Circle\" /><Property Name=\"Restitution\" Value=\"1.0\" /><Property Name=\"Offset\" Value=\"0,1\" /></Properties></Component>" +
                "<Component Type=\"BallSaveComponent\" />" +
                "</Components></Prefab>"));
        }

        /// <summary>
        /// Builds a headless scene with an EntitySystem + PhysicsEngine wired together, mirroring the
        /// CollisionFilteringTests fixture so components can resolve their sibling engine.
        /// </summary>
        private static (SceneWrapper scene, EntitySystem entitySystem, PhysicsEngine engine) BuildWiredSystem(PhysicsConfig? config = null)
        {
            var scene = new SceneWrapper();
            scene.SetSceneManager(new SceneManager());

            var dict = (Dictionary<Type, GameSystem>)typeof(Scene).GetField("_gameSystems", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(scene)!;

            var engine = config != null ? new PhysicsEngine(config) : new PhysicsEngine(Vector2.Zero);
            var entitySystem = new EntitySystem();

            dict.Add(typeof(PhysicsEngine), engine);
            dict.Add(typeof(EntitySystem), entitySystem);

            entitySystem.SetScene(scene);
            return (scene, entitySystem, engine);
        }

        #endregion
    }
}
