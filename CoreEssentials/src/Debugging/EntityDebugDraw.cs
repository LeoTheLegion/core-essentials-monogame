using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using CoreEssentials.Assets;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem;

namespace CoreEssentials.Debugging;

/// <summary>
/// Draws entity metadata overlays for visual debugging.
/// Bounding boxes, IDs, tags, hierarchy lines, and position markers.
/// </summary>
public class EntityDebugDraw
{
    private readonly DebugConfig _config;

    /// <summary>
    /// Initializes a new instance of the <see cref="EntityDebugDraw"/> class.
    /// </summary>
    /// <param name="config">The debug configuration controlling what overlays to draw.</param>
    public EntityDebugDraw(DebugConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Renders all enabled debug overlays for the given entities.
    /// </summary>
    /// <param name="entities">The entities to draw debug overlays for.</param>
    /// <param name="spriteBatch">The SpriteBatch used for drawing.</param>
    /// <param name="fontAsset">Optional font asset for rendering text overlays. If null, text overlays will be skipped.</param>
    public void DrawOverlays(IEnumerable<Entity> entities, SpriteBatch spriteBatch, FontAsset? fontAsset = null)
        => DrawOverlays(entities, new SpriteBatchDebugTarget(spriteBatch), fontAsset);

    /// <summary>
    /// Renders all enabled debug overlays for the given entities using the supplied draw target.
    /// Exposed internally so tests can drive the overlay geometry with a recording fake instead of a live <see cref="SpriteBatch"/>.
    /// </summary>
    internal void DrawOverlays(IEnumerable<Entity> entities, IEntityDebugTarget target, FontAsset? fontAsset = null)
    {
        var entityList = entities as List<Entity> ?? new List<Entity>(entities);

        // Draw hierarchy lines first so they appear behind other overlays
        DrawHierarchyIfEnabled(entityList, target);

        foreach (var entity in entityList)
        {
            if (!entity.GetActive())
                continue;

            DrawEntityOverlays(entity, target, fontAsset);
        }
    }

    /// <summary>
    /// Draws hierarchy lines if enabled in configuration.
    /// </summary>
    private void DrawHierarchyIfEnabled(List<Entity> entities, IEntityDebugTarget target)
    {
        if (_config.ShowEntityHierarchy)
        {
            DrawHierarchy(entities, target);
        }
    }

    /// <summary>
    /// Draws all per-entity overlays based on configuration.
    /// </summary>
    private void DrawEntityOverlays(Entity entity, IEntityDebugTarget target, FontAsset? fontAsset)
    {
        if (_config.ShowEntityBounds)
        {
            DrawBounds(entity, target);
        }

        if (_config.ShowEntityPosition)
        {
            DrawPositionMarker(entity, target);
        }

        if (fontAsset?.Font != null)
        {
            DrawTextOverlays(entity, target, fontAsset);
        }
    }

    /// <summary>
    /// Draws text-based overlays (ID and tags) if enabled.
    /// </summary>
    private void DrawTextOverlays(Entity entity, IEntityDebugTarget target, FontAsset fontAsset)
    {
        if (_config.ShowEntityIds)
        {
            DrawId(entity, target, fontAsset);
        }

        if (_config.ShowEntityTags)
        {
            DrawTags(entity, target, fontAsset);
        }
    }

    /// <summary>
    /// Draws a bounding box around the entity using its position, size, and sprite origin.
    /// The entity's <see cref="Entity.Position"/> is where the sprite's origin (pivot) sits, so the
    /// top-left corner of the rendered sprite is at <c>Position - GetOrigin()</c>. Offsetting the
    /// box by the origin keeps it centered on the sprite instead of anchored at its local (0,0).
    /// Entities that report a zero size (no sprite available) are skipped.
    /// </summary>
    private void DrawBounds(Entity entity, IEntityDebugTarget target)
    {
        var size = entity.GetSize();
        if (size == Vector2.Zero)
            return;

        var origin = entity.GetOrigin();
        var topLeft = entity.Position - origin;
        var bounds = new Rectangle((int)topLeft.X, (int)topLeft.Y, (int)size.X, (int)size.Y);
        target.DrawRectangle(bounds, _config.BoundsColor, _config.LineThickness);
    }

    /// <summary>
    /// Draws a small crosshair marker at the entity's position.
    /// </summary>
    private void DrawPositionMarker(Entity entity, IEntityDebugTarget target)
    {
        var pos = entity.Position;
        const float size = 4f;
        target.DrawLine(
            new Vector2(pos.X - size, pos.Y),
            new Vector2(pos.X + size, pos.Y),
            _config.PositionColor, _config.LineThickness);
        target.DrawLine(
            new Vector2(pos.X, pos.Y - size),
            new Vector2(pos.X, pos.Y + size),
            _config.PositionColor, _config.LineThickness);
    }

    /// <summary>
    /// Draws the entity's ID above its position.
    /// </summary>
    private void DrawId(Entity entity, IEntityDebugTarget target, FontAsset fontAsset)
    {
        if (entity.Id == null || fontAsset.Font == null)
            return;

        var pos = entity.Position;
        var textPos = new Vector2(pos.X, pos.Y - 16f);
        target.DrawText(fontAsset.Font!, entity.Id, textPos, _config.IdColor);
    }

    /// <summary>
    /// Draws the entity's tags below its position.
    /// </summary>
    private void DrawTags(Entity entity, IEntityDebugTarget target, FontAsset fontAsset)
    {
        if (entity.Tags.Count == 0 || fontAsset.Font == null)
            return;

        var pos = entity.Position;
        var textPos = new Vector2(pos.X, pos.Y + 16f);
        var tagText = string.Join(", ", entity.Tags);
        target.DrawText(fontAsset.Font!, tagText, textPos, _config.TagColor);
    }

    /// <summary>
    /// Draws lines connecting parent entities to their children.
    /// </summary>
    private void DrawHierarchy(List<Entity> entities, IEntityDebugTarget target)
    {
        var entityDict = new Dictionary<string, Entity>();
        foreach (var e in entities)
        {
            if (e.Id != null)
                entityDict[e.Id] = e;
        }

        foreach (var entity in entities)
        {
            if (entity.Parent == null)
                continue;

            var parentPos = entity.Parent.Position;
            var childPos = entity.Position;

            target.DrawLine(parentPos, childPos, _config.HierarchyColor, _config.LineThickness);
        }
    }
}

/// <summary>
/// The set of drawing operations required to render debug overlays. Production uses a
/// <see cref="SpriteBatch"/>-backed implementation; tests supply a recording fake so the
/// overlay geometry can be asserted without a graphics device.
/// </summary>
internal interface IEntityDebugTarget
{
    void DrawLine(Vector2 start, Vector2 end, Color color, float thickness);

    void DrawRectangle(Rectangle bounds, Color color, float thickness);

    void DrawText(SpriteFont font, string text, Vector2 position, Color color);
}

/// <summary>
/// Default <see cref="IEntityDebugTarget"/> that forwards to <see cref="Debug.Primitives"/> and a <see cref="SpriteBatch"/>.
/// </summary>
internal sealed class SpriteBatchDebugTarget : IEntityDebugTarget
{
    private readonly SpriteBatch _spriteBatch;

    public SpriteBatchDebugTarget(SpriteBatch spriteBatch)
    {
        _spriteBatch = spriteBatch;
    }

    public void DrawLine(Vector2 start, Vector2 end, Color color, float thickness)
        => Debug.Primitives.DrawLine(_spriteBatch, start, end, color, thickness);

    public void DrawRectangle(Rectangle bounds, Color color, float thickness)
        => Debug.Primitives.DrawRectangle(_spriteBatch, bounds, color, thickness);

    public void DrawText(SpriteFont font, string text, Vector2 position, Color color)
        => _spriteBatch.DrawString(font, text, position, color);
}
