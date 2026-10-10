#nullable enable
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Xunit;
using CoreEssentials.GameSystems;
using CoreEssentials.Scenes;

namespace CoreEssentials.Tests.GameSystems.SceneManagement
{
    /// <summary>
    /// Device-free tests for the Scene frame dispatch (update / fixed-update / pause / draw),
    /// system lookup, and unload. The registered game systems are lightweight recording stubs, so no
    /// GraphicsDevice or content is required: a null SpriteBatch is passed straight through and never
    /// dereferenced by the fixture. <see cref="Scene.IsLoaded"/> and the per-interface system arrays
    /// are set directly via reflection (the same technique <c>SceneLoadingTests</c> uses for the loading
    /// progress field), sidestepping the coroutine-driven load path without changing production code.
    /// </summary>
    public class SceneTests
    {
        private sealed class TestScene : Scene
        {
            protected override GameSystem[] LoadGameSystems() => Array.Empty<GameSystem>();

            protected override IEnumerator OnStartCoroutine()
            {
                yield break;
            }
        }

        // ───────────── Recording stub game systems (no device dependency) ─────────────

        public class RecordingUpdateSystem : GameSystem, IUpdateGameSystem
        {
            public int UpdateCalls;
            public void Update(GameTime gameTime) => UpdateCalls++;
        }

        public class RecordingDrawSystem : GameSystem, IDrawGameSystem
        {
            public int DrawCalls;
            public void Draw(GameTime gameTime, SpriteBatch spriteBatch) => DrawCalls++;
        }

        public class RecordingFixedUpdateSystem : GameSystem, IFixedUpdateGameSystem
        {
            public int FixedCalls;
            public void FixedUpdate(GameTime gameTime) => FixedCalls++;
        }

        public class RecordingPausableSystem : GameSystem, IPausableGameSystem
        {
            public bool? LastPaused;
            public void OnApplicationPause(bool paused) => LastPaused = paused;
        }

        public class DisposableUpdateSystem : GameSystem, IUpdateGameSystem, IDisposable
        {
            private bool _disposed;

            public int UpdateCalls;
            public bool Disposed => _disposed;
            public void Update(GameTime gameTime) => UpdateCalls++;
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            protected virtual void Dispose(bool disposing)
            {
                if (_disposed) return;
                if (disposing)
                {
                    // Record disposal for the scene-unload assertion.
                }
                _disposed = true;
            }
        }

        private static GameTime Time => new(TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(100));

        // ───────────── Reflection helpers (test-only, no production change) ─────────────

        private static void SetIsLoaded(Scene scene, bool value)
        {
            var back = typeof(Scene).GetFields(BindingFlags.NonPublic | BindingFlags.Instance)
                .First(f => f.FieldType == typeof(bool) && f.Name.Contains("IsLoaded"));
            back.SetValue(scene, value);
        }

        private static void SetSystemArray<T>(Scene scene, string fieldName, T[] value) where T : class
        {
            typeof(Scene).GetField(fieldName, BindingFlags.NonPublic | BindingFlags.Instance)!
                .SetValue(scene, value);
        }

        private static void RegisterSystem(Scene scene, GameSystem system)
        {
            var dict = (System.Collections.Generic.Dictionary<Type, GameSystem>)typeof(Scene)
                .GetField("_gameSystems", BindingFlags.NonPublic | BindingFlags.Instance)!
                .GetValue(scene)!;
            dict[system.GetType()] = system;
        }

        // ───────────── GetGameSystem<T> ─────────────

        [Fact]
        public void GetGameSystem_Registered_ReturnsSameInstance()
        {
            var scene = new TestScene();
            var sys = new RecordingUpdateSystem();
            RegisterSystem(scene, sys);

            Assert.Same(sys, scene.GetGameSystem<RecordingUpdateSystem>());
        }

        [Fact]
        public void GetGameSystem_Unregistered_ThrowsKeyNotFound()
        {
            var scene = new TestScene();

            var ex = Assert.Throws<KeyNotFoundException>(() => scene.GetGameSystem<RecordingFixedUpdateSystem>());
            Assert.Contains("RecordingFixedUpdateSystem", ex.Message);
        }

        // ───────────── Update dispatch (loaded vs not-loaded) ─────────────

        [Fact]
        public void Update_Loaded_ForwardsToEveryUpdateSystem()
        {
            var scene = new TestScene();
            var a = new RecordingUpdateSystem();
            var b = new RecordingUpdateSystem();
            SetIsLoaded(scene, true);
            SetSystemArray(scene, "_updateSystems", new IUpdateGameSystem[] { a, b });

            scene.Update(Time);

            Assert.Equal(1, a.UpdateCalls);
            Assert.Equal(1, b.UpdateCalls);
        }

        [Fact]
        public void Update_NotLoaded_SkipsDispatch()
        {
            var scene = new TestScene();
            var a = new RecordingUpdateSystem();
            SetIsLoaded(scene, false);
            SetSystemArray(scene, "_updateSystems", new IUpdateGameSystem[] { a });

            scene.Update(Time); // must not throw and must not dispatch

            Assert.Equal(0, a.UpdateCalls);
        }

        // ───────────── FixedUpdate dispatch ─────────────

        [Fact]
        public void FixedUpdate_Loaded_ForwardsToEveryFixedUpdateSystem()
        {
            var scene = new TestScene();
            var sys = new RecordingFixedUpdateSystem();
            SetIsLoaded(scene, true);
            SetSystemArray(scene, "_fixedUpdateSystems", new IFixedUpdateGameSystem[] { sys });

            scene.FixedUpdate(Time);

            Assert.Equal(1, sys.FixedCalls);
        }

        [Fact]
        public void FixedUpdate_NotLoaded_SkipsDispatch()
        {
            var scene = new TestScene();
            var sys = new RecordingFixedUpdateSystem();
            SetIsLoaded(scene, false);
            SetSystemArray(scene, "_fixedUpdateSystems", new IFixedUpdateGameSystem[] { sys });

            scene.FixedUpdate(Time);

            Assert.Equal(0, sys.FixedCalls);
        }

        // ───────────── OnApplicationPause dispatch ─────────────

        [Fact]
        public void OnApplicationPause_Loaded_ForwardsFlagToEveryPausableSystem()
        {
            var scene = new TestScene();
            var sys = new RecordingPausableSystem();
            SetIsLoaded(scene, true);
            SetSystemArray(scene, "_pausableSystems", new IPausableGameSystem[] { sys });

            scene.OnApplicationPause(true);

            Assert.Equal(true, sys.LastPaused);
        }

        [Fact]
        public void OnApplicationPause_NotLoaded_SkipsDispatch()
        {
            var scene = new TestScene();
            var sys = new RecordingPausableSystem();
            SetIsLoaded(scene, false);
            SetSystemArray(scene, "_pausableSystems", new IPausableGameSystem[] { sys });

            scene.OnApplicationPause(true);

            Assert.Null(sys.LastPaused);
        }

        // ───────────── Draw dispatch (SpriteBatch passed through, never dereferenced) ─────────────

        [Fact]
        public void Draw_Loaded_ForwardsToEveryDrawSystem()
        {
            var scene = new TestScene();
            var sys = new RecordingDrawSystem();
            SetIsLoaded(scene, true);
            SetSystemArray(scene, "_drawSystems", new IDrawGameSystem[] { sys });

            // A null SpriteBatch is fine — the stub ignores it and Scene never touches it.
            scene.Draw(Time, (SpriteBatch?)null!);

            Assert.Equal(1, sys.DrawCalls);
        }

        [Fact]
        public void Draw_NotLoaded_SkipsDispatch()
        {
            var scene = new TestScene();
            var sys = new RecordingDrawSystem();
            SetIsLoaded(scene, false);
            SetSystemArray(scene, "_drawSystems", new IDrawGameSystem[] { sys });

            scene.Draw(Time, (SpriteBatch?)null!);

            Assert.Equal(0, sys.DrawCalls);
        }

        // ───────────── Unload: disposes systems and resets state ─────────────

        [Fact]
        public void Unload_DisposesSystems_ClearsLookupAndResetsLoaded()
        {
            var scene = new TestScene();
            var disposable = new DisposableUpdateSystem();
            RegisterSystem(scene, disposable);
            SetIsLoaded(scene, true);
            SetSystemArray(scene, "_updateSystems", new IUpdateGameSystem[] { disposable });

            scene.Unload();

            Assert.True(disposable.Disposed);
            Assert.False(scene.IsLoaded);
            // The system registry was cleared, so lookups no longer resolve.
            Assert.Throws<KeyNotFoundException>(() => scene.GetGameSystem<DisposableUpdateSystem>());
        }
    }
}