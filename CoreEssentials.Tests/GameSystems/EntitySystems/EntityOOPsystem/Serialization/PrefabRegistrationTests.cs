using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization
{
    public class PrefabRegistrationTests : IDisposable
    {
        private readonly EntitySystem _system = new();
        private const string AssetName = "LazyProbePrefab.xml";

        [Fact]
        public void InstantiateFromAsset_AutoRegistersOnFirstUse()
        {
            // Arrange
            WriteContentAsset(AssetName, @"<Prefab Type=""LazyProbeEntity"" />");
            AssetManager.Init(new MockContentManager());

            // Act — zero explicit registration calls
            var entity = _system.InstantiateFromAsset(AssetName, Vector2.Zero);

            // Assert — a prefab was registered under the asset's base name and is now resolvable
            Assert.IsType<LazyProbeEntity>(entity);
            Assert.True(_system.HasPrefab("LazyProbePrefab"));

            // Act again — second call reuses the cached prefab even though the file is gone
            File.Delete(System.IO.Path.Combine(AppContext.BaseDirectory, "Content", AssetName));
            var second = _system.InstantiateFromAsset(AssetName, Vector2.Zero);

            // Assert
            Assert.IsType<LazyProbeEntity>(second);
        }

        [Fact]
        public void InstantiateFromAsset_WithNullOverrides_BehavesLikePlainInstantiate()
        {
            // Arrange
            WriteContentAsset(AssetName, @"<Prefab Type=""LazyProbeEntity"" />");
            AssetManager.Init(new MockContentManager());

            // Act — explicit null overrides exercises the 3-arg overload end-to-end
            var entity = _system.InstantiateFromAsset(AssetName, new Vector2(5, 6), null);

            // Assert
            Assert.IsType<LazyProbeEntity>(entity);
            Assert.Equal(new Vector2(5, 6), entity.Position);
        }

        [Fact]
        public void ExplicitRegistration_WinsOverLazyCache()
        {
            // Arrange — an explicit registration under the same base name as the asset
            var explicitPrefab = new Prefab { Type = "ExplicitHostEntity", Sort = 7 };
            _system.RegisterPrefab("ExplicitHost", explicitPrefab);

            WriteContentAsset("ExplicitHost.xml", @"<Prefab Type=""LazyProbeEntity"" />");
            AssetManager.Init(new MockContentManager());

            // Act
            var entity = _system.InstantiateFromAsset("ExplicitHost.xml", Vector2.Zero);

            // Assert — the explicit registration was used, not the asset on disk
            Assert.IsType<ExplicitHostEntity>(entity);
            Assert.Equal(7, entity.GetSort());
        }

        [Fact]
        public void InstantiateFromAsset_EmptyName_Throws()
        {
            // Act & Assert
            Assert.Throws<ArgumentException>(() => _system.InstantiateFromAsset("", Vector2.Zero));
        }

        public void Dispose() => _system.Dispose();

        private static void WriteContentAsset(string fileName, string xml)
        {
            var contentDir = System.IO.Path.Combine(AppContext.BaseDirectory, "Content");
            Directory.CreateDirectory(contentDir);
            File.WriteAllText(System.IO.Path.Combine(contentDir, fileName), xml);
        }

        // ──────────────────────────── Test fixtures ────────────────────────────

        public class LazyProbeEntity : Entity
        {
            public override void Update(GameTime gameTime) { }
            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
        }

        public class ExplicitHostEntity : Entity
        {
            public override void Update(GameTime gameTime) { }
            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
        }
    }
}
