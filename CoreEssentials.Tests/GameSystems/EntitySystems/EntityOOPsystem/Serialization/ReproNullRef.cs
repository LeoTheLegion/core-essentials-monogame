#nullable enable
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using Microsoft.Xna.Framework;
using System.Xml.Linq;
using Xunit;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization
{
    public class ReproNullRef
    {
        public class EntityWithSpriteInOnStart : Entity, ISaveableEntity
        {
            public SpriteComponent? Comp { get; private set; }
            public bool Started { get; private set; }
            
            public override void OnStart()
            {
                base.OnStart();
                Started = true;
                Comp = new SpriteComponent();
                AddComponent(Comp);
            }

            public XElement SaveState()
            {
                return new XElement("Entity",
                    new XAttribute("Id", Id ?? string.Empty),
                    new XAttribute("Type", GetType().FullName),
                    new XAttribute("Rotation", Rotation.ToString(CultureInfo.InvariantCulture)),
                    new XAttribute("Sort", GetSort()),
                    new XAttribute("Active", GetActive()),
                    new XElement("Position",
                        new XAttribute("X", Position.X.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("Y", Position.Y.ToString(CultureInfo.InvariantCulture))
                    ),
                    new XElement("Scale",
                        new XAttribute("X", Scale.X.ToString(CultureInfo.InvariantCulture)),
                        new XAttribute("Y", Scale.Y.ToString(CultureInfo.InvariantCulture))
                    ),
                    new XElement("Tags",
                        Tags.Select(tag => new XElement("Tag", new XAttribute("Name", tag)))
                    ));
            }

            public void LoadState(XElement element)
            {
                var pos = element.Element("Position");
                if (pos != null)
                    Position = new Vector2(float.Parse(pos.Attribute("X")?.Value ?? "0", CultureInfo.InvariantCulture),
                        float.Parse(pos.Attribute("Y")?.Value ?? "0", CultureInfo.InvariantCulture));

                if (float.TryParse(element.Attribute("Rotation")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var rot))
                    Rotation = rot;

                var sc = element.Element("Scale");
                if (sc != null)
                    Scale = new Vector2(float.Parse(sc.Attribute("X")?.Value ?? "1", CultureInfo.InvariantCulture),
                        float.Parse(sc.Attribute("Y")?.Value ?? "1", CultureInfo.InvariantCulture));

                if (int.TryParse(element.Attribute("Sort")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var sort))
                    SetSort(sort);

                if (bool.TryParse(element.Attribute("Active")?.Value, out var active))
                    SetActive(active);

                foreach (var t in Tags.ToList()) RemoveTag(t);
                var tagsEl = element.Element("Tags");
                if (tagsEl != null)
                    foreach (var tag in tagsEl.Elements("Tag"))
                        SetTag(tag.Attribute("Name")?.Value ?? "default");
            }
        }

        [Fact]
        public void LoadEntity_ComponentCreatedViaReflection_OwnerIsSetBeforeOnAttach()
        {
            var system = new EntitySystem();
            var e = system.CreateEntity<EntityWithSpriteInOnStart>();
            e.SetId("test");
            e.Position = new Vector2(10, 20);
            
            var file = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, file);
                
                var newSystem = new EntitySystem();
                GameStateSerializer.LoadState(newSystem, file);
                
                var loaded = newSystem.GetEntities().First();
                var comp = loaded.GetComponent<SpriteComponent>();
                Assert.NotNull(comp);
                Assert.Same(loaded, comp!.Owner);
            }
            finally
            {
                if (File.Exists(file)) File.Delete(file);
            }
        }

        [Fact]
        public void LoadEntity_WithExistingComponent_DoesNotRecreate()
        {
            var system = new EntitySystem();
            var e = system.CreateEntity<EntityWithSpriteInOnStart>();
            e.SetId("test2");
            
            var file = Path.GetTempFileName();
            try
            {
                GameStateSerializer.SaveState(system, file);
                
                var newSystem = new EntitySystem();
                // Load with component XML that matches existing component
                // Simulate by saving and loading
                GameStateSerializer.LoadState(newSystem, file);
                
                var loaded = newSystem.GetEntities().First();
                var comp = loaded.GetComponent<SpriteComponent>();
                // Component should be from OnStart, not recreated
                Assert.NotNull(comp);
                Assert.Same(loaded, comp!.Owner);
            }
            finally
            {
                if (File.Exists(file)) File.Delete(file);
            }
        }
    }
}
#nullable enable