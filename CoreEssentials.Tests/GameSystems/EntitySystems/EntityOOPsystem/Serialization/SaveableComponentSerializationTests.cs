using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization
{
    /// <summary>
    /// Tests for the component-based, prefab-driven save path (Sprint 1).
    /// An entity is saveable iff it has an <see cref="ISaveableComponent"/>; saves carry a
    /// <c>Prefab</c> attribute and load recreates entities via <c>EntitySystem.Instantiate</c>.
    /// Saves whose entity elements are missing the <c>Prefab</c> attribute fail to load.
    /// </summary>
    public class SaveableComponentSerializationTests : IDisposable
    {
        private readonly EntitySystem _system = new();

        public void Dispose() => _system.Dispose();

        // ──────────────────────────── Detection ────────────────────────────────

        [Fact]
        public void Save_EntityWithSaveComponent_IsIncluded()
        {
            _system.RegisterPrefab("probe", EntityPrefabLoader.LoadFromXml(
                "<Prefab Type=\"SaveableProbeEntity\"><Components>" +
                "<Component Type=\"TestSaveComponent\" /></Components></Prefab>"));

            var entity = _system.Instantiate("probe", new Vector2(10, 20));
            entity.SetId("probe_1");

            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(_system, tempFile);
                var xml = File.ReadAllText(tempFile);

                Assert.Contains("probe_1", xml);
                Assert.Contains("Prefab=\"probe\"", xml);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void Save_EntityWithoutSaveComponent_IsExcluded()
        {
            // A plain entity with no save component and no legacy interface is not saved.
            var entity = _system.CreateEntity<PlainEntity>();
            entity.SetId("plain_1");

            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(_system, tempFile);
                var xml = File.ReadAllText(tempFile);

                Assert.DoesNotContain("plain_1", xml);
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        // ──────────────────────────── Prefab required ──────────────────────────

        [Fact]
        public void Save_SaveComponentWithoutPrefab_Throws()
        {
            // Attach a save component to a directly-created entity (no prefab) → save must fail fast.
            var entity = _system.CreateEntity<SaveableProbeEntity>();
            entity.AddComponent(new TestSaveComponent());
            entity.SetId("noprefab_1");

            Assert.Throws<InvalidOperationException>(() => GameStateSerializer.SaveState(_system, Path.GetTempFileName()));
        }

        // ──────────────────────────── Round-trip via prefab ────────────────────

        [Fact]
        public void Load_SavesWithPrefab_InstantiatesFromPrefabAndRestoresState()
        {
            _system.RegisterPrefab("probe", EntityPrefabLoader.LoadFromXml(
                "<Prefab Type=\"SaveableProbeEntity\"><Components>" +
                "<Component Type=\"TestSaveComponent\" /></Components></Prefab>"));

            var entity = _system.Instantiate("probe", new Vector2(10, 20));
            entity.SetId("probe_rt");
            ((TestSaveComponent)entity.Components.OfType<TestSaveComponent>().Single()).Score = 42;

            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(_system, tempFile);

                // Load into a fresh system that has the same prefab registered.
                var newSystem = new EntitySystem();
                newSystem.RegisterPrefab("probe", EntityPrefabLoader.LoadFromXml(
                    "<Prefab Type=\"SaveableProbeEntity\"><Components>" +
                    "<Component Type=\"TestSaveComponent\" /></Components></Prefab>"));

                try
                {
                    GameStateSerializer.LoadState(newSystem, tempFile);

                    var loaded = newSystem.GetEntities().Single(e => e.Id == "probe_rt");
                    Assert.Equal(10f, loaded.Position.X, 3);
                    Assert.Equal(20f, loaded.Position.Y, 3);

                    var saveComp = loaded.Components.OfType<TestSaveComponent>().Single();
                    Assert.Equal(42, saveComp.Score);
                }
                finally
                {
                    newSystem.Dispose();
                }
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        [Fact]
        public void Load_SaveReferencesUnregisteredPrefab_Throws()
        {
            _system.RegisterPrefab("probe", EntityPrefabLoader.LoadFromXml(
                "<Prefab Type=\"SaveableProbeEntity\"><Components>" +
                "<Component Type=\"TestSaveComponent\" /></Components></Prefab>"));

            var entity = _system.Instantiate("probe", Vector2.Zero);
            entity.SetId("probe_missing");

            var tempFile = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(_system, tempFile);

                // New system WITHOUT the prefab registered → load must throw.
                var newSystem = new EntitySystem();
                try
                {
                    var ex = Assert.Throws<InvalidOperationException>(() => GameStateSerializer.LoadState(newSystem, tempFile));
                    // The actionable root cause is surfaced as the inner exception.
                    Assert.IsType<KeyNotFoundException>(ex.InnerException);
                    Assert.Contains("probe", ex.Message);
                }
                finally
                {
                    newSystem.Dispose();
                }
            }
            finally
            {
                if (File.Exists(tempFile)) File.Delete(tempFile);
            }
        }

        // ──────────────────────────── Legacy path still works ──────────────────

        [Fact]
        public void Load_SaveMissingPrefabAttribute_Throws()
        {
            // A save whose entity element carries no Prefab attribute cannot be recreated.
            var xml = @"<GameState Version=""1.0""><Entities>
  <Entity Id=""no_prefab_1"" Type=""Some.Type"" Rotation=""0"" Sort=""0"" Active=""true"">
    <Position X=""5"" Y=""6"" />
  </Entity>
</Entities></GameState>";

            var newSystem = new EntitySystem();
            try
            {
                var ex = Assert.Throws<InvalidOperationException>(() => GameStateSerializer.LoadStateFromXml(newSystem, xml));
                Assert.IsType<FormatException>(ex.InnerException);
                Assert.Contains("Prefab", ex.Message);
            }
            finally
            {
                newSystem.Dispose();
            }
        }

        // ──────────────────────────── Test fixtures ────────────────────────────

        /// <summary>Plain entity — not saveable on its own.</summary>
        public class PlainEntity : Entity
        {
            public override void Update(GameTime gameTime) { }
            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
        }

        /// <summary>Entity host for the test save component (declared in a prefab).</summary>
        public class SaveableProbeEntity : Entity
        {
            public override void Update(GameTime gameTime) { }
            public override void Render(Microsoft.Xna.Framework.Graphics.SpriteBatch spriteBatch) { }
        }

        /// <summary>
        /// Minimal save component: persists the owner's transform plus a single custom int, and
        /// stamps nothing prefab-related (the serializer adds the Prefab attribute itself).
        /// </summary>
        public class TestSaveComponent : EntityComponent, ISaveableComponent
        {
            public int Score { get; set; }

            public XElement SaveState()
            {
                return new XElement("Entity",
                    new XAttribute("Id", Owner?.Id ?? string.Empty),
                    new XAttribute("Type", Owner?.GetType().FullName ?? string.Empty),
                    new XAttribute("Rotation", (Owner?.Rotation ?? 0f).ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("Sort", Owner?.GetSort() ?? 0),
                    new XAttribute("Active", Owner?.GetActive() ?? true),
                    new XElement("Position",
                        new XAttribute("X", (Owner?.Position.X ?? 0f).ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("Y", (Owner?.Position.Y ?? 0f).ToString(CultureInfo.InvariantCulture))),
                    new XElement("Score", new XAttribute("Value", Score)));
            }

            public void LoadState(XElement element)
            {
                if (Owner == null) return;

                var pos = element.Element("Position");
                if (pos != null)
                    Owner.Position = new Vector2(
                        float.Parse(pos.Attribute("X")?.Value ?? "0", CultureInfo.InvariantCulture),
                        float.Parse(pos.Attribute("Y")?.Value ?? "0", CultureInfo.InvariantCulture));

                var scoreEl = element.Element("Score");
                if (scoreEl != null)
                    Score = int.Parse(scoreEl.Attribute("Value")?.Value ?? "0");
            }
        }
    }
}
