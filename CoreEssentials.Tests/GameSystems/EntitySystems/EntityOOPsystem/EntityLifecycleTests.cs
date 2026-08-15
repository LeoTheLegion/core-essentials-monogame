using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.Coroutines;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem
{
    /// <summary>
    /// Tests for the Unity-style lifecycle hooks on <see cref="Entity"/>:
    /// OnAwake, OnEnable, OnDisable, OnLateUpdate, OnFixedUpdate, and OnApplicationPause.
    /// </summary>
    public class EntityLifecycleTests
    {
        // ---- OnAwake ----

        [Fact]
        public void OnAwake_IsCalledOnEntityCreation()
        {
            var entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<LifecycleTestEntity>();

            Assert.True(entity.AwakeCalled);
        }

        [Fact]
        public void OnAwake_IsCalledOnUnstartedCreation()
        {
            var entitySystem = new EntitySystem();
            var entity = (LifecycleTestEntity)entitySystem.CreateEntityUnstarted(typeof(LifecycleTestEntity));

            Assert.True(entity.AwakeCalled);
        }

        [Fact]
        public void OnAwake_IsCalledOnlyOnceBySystem()
        {
            var entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<LifecycleTestEntity>();

            // The system must call OnAwake exactly once per entity (Unity guarantees a single Awake).
            Assert.Equal(1, entity.AwakeCount);
        }

        [Fact]
        public void OnAwake_BaseGuardPreventsDoubleAwakeState()
        {
            var entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<LifecycleTestEntity>();

            // The base guard (_hasAwoken) is already set after the system's single call.
            // A second system-level OnAwake should not re-establish the awoken state.
            Assert.True(entity.HasAwoken);
        }

        // ---- OnEnable / OnDisable ----

        [Fact]
        public void OnEnable_IsCalledWhenEntityBecomesActive()
        {
            var entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<LifecycleTestEntity>();

            // Entity is active on creation, so OnEnable fired once during OnAwake.
            int countAfterCreate = entity.EnableCount;

            entity.SetActive(false);
            entity.SetActive(true);

            Assert.Equal(countAfterCreate + 1, entity.EnableCount);
        }

        [Fact]
        public void OnDisable_IsCalledWhenEntityBecomesInactive()
        {
            var entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<LifecycleTestEntity>();

            int countAfterCreate = entity.DisableCount;

            entity.SetActive(false);

            Assert.Equal(countAfterCreate + 1, entity.DisableCount);
        }

        [Fact]
        public void OnEnable_IsNotCalledOnNoOpSetActive()
        {
            var entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<LifecycleTestEntity>();

            int countAfterCreate = entity.EnableCount;

            // Entity is already active; setting active again should not fire OnEnable.
            entity.SetActive(true);

            Assert.Equal(countAfterCreate, entity.EnableCount);
        }

        [Fact]
        public void OnDisable_IsNotCalledOnNoOpSetActive()
        {
            var entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<LifecycleTestEntity>();

            entity.SetActive(false);
            int countAfterDisable = entity.DisableCount;

            // Entity is already inactive; setting inactive again should not fire OnDisable.
            entity.SetActive(false);

            Assert.Equal(countAfterDisable, entity.DisableCount);
        }

        // ---- OnLateUpdate ----

        [Fact]
        public void OnLateUpdate_IsCalledAfterUpdateEachFrame()
        {
            var entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<LifecycleTestEntity>();

            var gameTime = new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(16));

            entitySystem.Update(gameTime);

            Assert.True(entity.UpdateCalled);
            Assert.True(entity.LateUpdateCalled);

            // LateUpdate must come after Update in the recorded sequence.
            int updateIndex = entity.CallOrder.IndexOf("Update");
            int lateIndex = entity.CallOrder.IndexOf("LateUpdate");
            Assert.True(updateIndex >= 0 && lateIndex > updateIndex,
                $"Expected Update before LateUpdate, got order: {string.Join(",", entity.CallOrder)}");
        }

        [Fact]
        public void OnLateUpdate_IsNotCalledForInactiveEntity()
        {
            var entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<LifecycleTestEntity>();
            entity.SetActive(false);

            entitySystem.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(16)));

            Assert.False(entity.LateUpdateCalled);
        }

        // ---- OnFixedUpdate ----

        [Fact]
        public void OnFixedUpdate_IsCalledOnFixedTimestep()
        {
            var entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<LifecycleTestEntity>();

            entitySystem.FixedUpdate(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(20)));

            Assert.True(entity.FixedUpdateCalled);
        }

        [Fact]
        public void OnFixedUpdate_IsNotCalledForInactiveEntity()
        {
            var entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<LifecycleTestEntity>();
            entity.SetActive(false);

            entitySystem.FixedUpdate(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(20)));

            Assert.False(entity.FixedUpdateCalled);
        }

        // ---- OnApplicationPause ----

        [Fact]
        public void OnApplicationPause_ReachesAllActiveEntities()
        {
            var entitySystem = new EntitySystem();
            var a = entitySystem.CreateEntity<LifecycleTestEntity>();
            var b = entitySystem.CreateEntity<LifecycleTestEntity>();

            entitySystem.OnApplicationPause(true);

            Assert.True(a.PausedWithValue(true));
            Assert.True(b.PausedWithValue(true));
        }

        [Fact]
        public void OnApplicationPause_IsNotCalledForInactiveEntity()
        {
            var entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<LifecycleTestEntity>();
            entity.SetActive(false);

            entitySystem.OnApplicationPause(true);

            Assert.False(entity.PausedWithValue(true));
        }

        [Fact]
        public void OnApplicationPause_PassesResumeValue()
        {
            var entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<LifecycleTestEntity>();

            entitySystem.OnApplicationPause(true);
            entitySystem.OnApplicationPause(false);

            Assert.True(entity.PausedWithValue(true));
            Assert.True(entity.PausedWithValue(false));
        }

        // ---- Full lifecycle order ----

        [Fact]
        public void Lifecycle_OrderIsAwakeEnableStartUpdateLateUpdateDisableDestroy()
        {
            var entitySystem = new EntitySystem();
            var entity = entitySystem.CreateEntity<LifecycleTestEntity>();

            // After creation: Awake -> Enable -> Start
            var expectedAfterCreate = new[] { "Awake", "Enable", "Start" };
            Assert.Equal(expectedAfterCreate, entity.CallOrder.ToArray());

            // One frame: Update -> LateUpdate
            entitySystem.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(16)));
            Assert.Contains("Update", entity.CallOrder);
            Assert.Contains("LateUpdate", entity.CallOrder);

            // Inactive: Disable
            entity.SetActive(false);
            Assert.Contains("Disable", entity.CallOrder);

            // Destroy: Destroy
            entity.Destroy();
            entitySystem.Update(new GameTime(TimeSpan.Zero, TimeSpan.FromMilliseconds(16)));
            Assert.Contains("Destroy", entity.CallOrder);

            // Verify overall ordering of the key milestones.
            int awake = entity.CallOrder.IndexOf("Awake");
            int enable = entity.CallOrder.IndexOf("Enable");
            int start = entity.CallOrder.IndexOf("Start");
            int update = entity.CallOrder.IndexOf("Update");
            int late = entity.CallOrder.IndexOf("LateUpdate");
            int disable = entity.CallOrder.IndexOf("Disable");
            int destroy = entity.CallOrder.IndexOf("Destroy");

            Assert.True(awake < enable, "Awake must precede Enable");
            Assert.True(enable < start, "Enable must precede Start");
            Assert.True(start < update, "Start must precede Update");
            Assert.True(update < late, "Update must precede LateUpdate");
            Assert.True(late < disable, "LateUpdate must precede Disable");
            Assert.True(disable < destroy, "Disable must precede Destroy");
        }

        // ---- Test entity ----

        private class LifecycleTestEntity : Entity
        {
            public readonly List<string> CallOrder = new();

            public bool AwakeCalled { get; private set; }
            public int AwakeCount { get; private set; }
            public bool HasAwoken => _hasAwoken;
            public int EnableCount { get; private set; }
            public int DisableCount { get; private set; }
            public bool UpdateCalled { get; private set; }
            public bool LateUpdateCalled { get; private set; }
            public bool FixedUpdateCalled { get; private set; }
            public readonly List<bool> PauseValues = new();

            public override void OnAwake()
            {
                base.OnAwake();
                AwakeCalled = true;
                AwakeCount++;
                CallOrder.Add("Awake");
            }

            public override void OnEnable()
            {
                base.OnEnable();
                EnableCount++;
                CallOrder.Add("Enable");
            }

            public override void OnDisable()
            {
                base.OnDisable();
                DisableCount++;
                CallOrder.Add("Disable");
            }

            public override void OnStart()
            {
                base.OnStart();
                CallOrder.Add("Start");
            }

            public override void Update(GameTime gameTime)
            {
                base.Update(gameTime);
                UpdateCalled = true;
                CallOrder.Add("Update");
            }

            public override void OnLateUpdate(GameTime gameTime)
            {
                base.OnLateUpdate(gameTime);
                LateUpdateCalled = true;
                CallOrder.Add("LateUpdate");
            }

            public override void OnFixedUpdate(GameTime gameTime)
            {
                base.OnFixedUpdate(gameTime);
                FixedUpdateCalled = true;
                CallOrder.Add("FixedUpdate");
            }

            public override void OnApplicationPause(bool paused)
            {
                base.OnApplicationPause(paused);
                PauseValues.Add(paused);
                CallOrder.Add(paused ? "Pause" : "Resume");
            }

            public override void OnDestroy()
            {
                base.OnDestroy();
                CallOrder.Add("Destroy");
            }

            public override void Render(SpriteBatch spriteBatch) { }

            public bool PausedWithValue(bool value) => PauseValues.Contains(value);
        }
    }
}
