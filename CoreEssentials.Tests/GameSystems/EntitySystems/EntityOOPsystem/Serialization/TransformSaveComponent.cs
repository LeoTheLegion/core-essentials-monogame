using System.Globalization;
using System.Linq;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;

namespace CoreEssentials.Tests.GameSystems.EntitySystems.EntityOOPsystem.Serialization;

/// <summary>
/// Shared test save component (the component-path replacement for the retired entity-level
/// <c>ISaveableEntity</c>). Persists the owner's transform, scale, sort, active flag, and tags —
/// exactly what the legacy per-entity <c>SaveState()</c>/<c>LoadState()</c> bodies did. Attach it to
/// a prefab so an entity is saveable via <see cref="ISaveableComponent"/>.
/// </summary>
public class TransformSaveComponent : EntityComponent, ISaveableComponent
{
    public XElement SaveState()
    {
        var o = Owner;
        return new XElement("Entity",
            new XAttribute("Id", o?.Id ?? string.Empty),
            new XAttribute("Type", o?.GetType().FullName ?? string.Empty),
            new XAttribute("Rotation", (o?.Rotation ?? 0f).ToString(CultureInfo.InvariantCulture)),
            new XAttribute("Sort", o?.GetSort() ?? 0),
            new XAttribute("Active", o?.GetActive() ?? true),
            new XElement("Position",
                new XAttribute("X", (o?.Position.X ?? 0f).ToString(CultureInfo.InvariantCulture)),
                new XAttribute("Y", (o?.Position.Y ?? 0f).ToString(CultureInfo.InvariantCulture))),
            new XElement("Scale",
                new XAttribute("X", (o?.Scale.X ?? 1f).ToString(CultureInfo.InvariantCulture)),
                new XAttribute("Y", (o?.Scale.Y ?? 1f).ToString(CultureInfo.InvariantCulture))),
            new XElement("Tags",
                (o?.Tags ?? Enumerable.Empty<string>()).Select(t => new XElement("Tag", new XAttribute("Name", t)))));
    }

    public void LoadState(XElement element)
    {
        var o = Owner;
        if (o == null)
            return;

        var pos = element.Element("Position");
        if (pos != null)
            o.Position = new Vector2(
                float.Parse(pos.Attribute("X")?.Value ?? "0", CultureInfo.InvariantCulture),
                float.Parse(pos.Attribute("Y")?.Value ?? "0", CultureInfo.InvariantCulture));

        if (float.TryParse(element.Attribute("Rotation")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var rot))
            o.Rotation = rot;

        var sc = element.Element("Scale");
        if (sc != null)
            o.Scale = new Vector2(
                float.Parse(sc.Attribute("X")?.Value ?? "1", CultureInfo.InvariantCulture),
                float.Parse(sc.Attribute("Y")?.Value ?? "1", CultureInfo.InvariantCulture));

        if (int.TryParse(element.Attribute("Sort")?.Value, NumberStyles.Any, CultureInfo.InvariantCulture, out var sort))
            o.SetSort(sort);

        if (bool.TryParse(element.Attribute("Active")?.Value, out var active))
            o.SetActive(active);

        foreach (var t in o.Tags.ToList())
            o.RemoveTag(t);

        var tagsEl = element.Element("Tags");
        if (tagsEl != null)
            foreach (var tag in tagsEl.Elements("Tag"))
                o.SetTag(tag.Attribute("Name")?.Value ?? "default");
    }
}
