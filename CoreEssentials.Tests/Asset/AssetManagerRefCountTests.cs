using System;
using CoreEssentials.Assets;
using Xunit;

namespace CoreEssentials.Tests.Assets
{
    /// <summary>
    /// Exercises the reference-counted load/unload behaviour of the static
    /// <see cref="AssetManager"/>. Each test uses a unique asset name so the manager's
    /// process-wide cache never leaks state between tests. All paths are device-free: the fake
    /// asset overrides Load/Unload and the mock content manager records Unload calls.
    /// </summary>
    public class AssetManagerRefCountTests
    {
        private readonly string _name;
        private readonly MockContentManager _content;

        public AssetManagerRefCountTests()
        {
            // Fresh, unique name per test instance to isolate the static cache state.
            _name = Guid.NewGuid().ToString("N");
            _content = new MockContentManager();
            AssetManager.Init(_content);
            CountingAsset.LoadCalls = 0; // reset so reload-count assertions are order-independent
        }

        [Fact]
        public void LoadAsset_FirstLoad_CreatesAndCaches()
        {
            var first = AssetManager.LoadAsset<CountingAsset>(_name);

            Assert.NotNull(first);
            Assert.Equal(_name, first.Name);
            // A single load should invoke the asset's Load exactly once.
            Assert.Equal(1, CountingAsset.LoadCalls);
        }

        [Fact]
        public void LoadAsset_RepeatLoad_ReturnsCachedInstance_WithoutReloading()
        {
            var first = AssetManager.LoadAsset<CountingAsset>(_name);
            var second = AssetManager.LoadAsset<CountingAsset>(_name);

            Assert.Same(first, second);
            // The cached instance is reused; Load must not run a second time.
            Assert.Equal(1, CountingAsset.LoadCalls);
        }

        [Fact]
        public void UnloadAsset_WhileStillInUse_DoesNotUnload()
        {
            AssetManager.LoadAsset<CountingAsset>(_name); // ref -> 1
            AssetManager.LoadAsset<CountingAsset>(_name); // ref -> 2

            AssetManager.UnloadAsset<CountingAsset>(_name); // ref -> 1 (still in use)

            Assert.False(_content.IsUnloaded(_name));
        }

        [Fact]
        public void UnloadAsset_AtZero_UnloadsFromContent()
        {
            AssetManager.LoadAsset<CountingAsset>(_name); // ref -> 1

            AssetManager.UnloadAsset<CountingAsset>(_name); // ref -> 0

            Assert.True(_content.IsUnloaded(_name));
        }

        [Fact]
        public void UnloadAsset_NeverLoaded_DoesNotThrow()
        {
            var neverLoaded = Guid.NewGuid().ToString("N");

            AssetManager.UnloadAsset<CountingAsset>(neverLoaded); // no-op: unknown key

            Assert.False(_content.IsUnloaded(neverLoaded));
        }

        [Fact]
        public void LoadAsset_WhenNotInitialized_Throws()
        {
            // The manager's content is process-wide state; clear it to exercise the uninit path,
            // then restore neutral initialized state so this never leaks a null into other tests.
            try
            {
                AssetManager.Init(null!);

                Assert.Throws<InvalidOperationException>(
                    () => AssetManager.LoadAsset<CountingAsset>(Guid.NewGuid().ToString("N")));
            }
            finally
            {
                AssetManager.Init(new MockContentManager());
            }
        }

        /// <summary>
        /// An asset that counts how many times Load is invoked across all instances, so tests
        /// can assert the cache reuses rather than reloads.
        /// </summary>
        public class CountingAsset : CoreEssentials.Assets.Asset
        {
            public static int LoadCalls = 0;

            public CountingAsset(string name) : base(name) { }

            public override void Load(IContentManager contentManager) => LoadCalls++;
            public override void Unload(IContentManager contentManager) { }
        }

        private sealed class MockContentManager : IContentManager
        {
            private readonly System.Collections.Generic.HashSet<string> _unloaded = new();

            public T Load<T>(string assetName) => default!;
            public void Unload(string assetName) => _unloaded.Add(assetName);
            public bool IsUnloaded(string assetName) => _unloaded.Contains(assetName);
        }
    }
}
