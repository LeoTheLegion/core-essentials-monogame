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
    {
        var entityList = entities as List<Entity> ?? new List<Entity>(entities);

        // Draw hierarchy lines first so they appear behind other overlays
        DrawHierarchyIfEnabled(entityList, spriteBatch);

        foreach (var entity in entityList)
        {
            if (!entity.GetActive())
                continue;

            DrawEntityOverlays(entity, spriteBatch, fontAsset);
        }
    }

    /// <summary>
    /// Draws hierarchy lines if enabled in configuration.
    /// </summary>
    private void DrawHierarchyIfEnabled(List<Entity> entities, SpriteBatch spriteBatch)
    {
        if (_config.ShowEntityHierarchy)
        {
            DrawHierarchy(entities, spriteBatch);
        }
    }

    /// <summary>
    /// Draws all per-entity overlays based on configuration.
    /// </summary>
    private void DrawEntityOverlays(Entity entity, SpriteBatch spriteBatch, FontAsset? fontAsset)
    {
        if (_config.ShowEntityBounds)
        {
            DrawBounds(entity, spriteBatch);
        }

        if (_config.ShowEntityPosition)
        {
            DrawPositionMarker(entity, spriteBatch);
        }

        if (fontAsset?.Font != null)
        {
            DrawTextOverlays(entity, spriteBatch, fontAsset);
        }
    }

    /// <summary>
    /// Draws text-based overlays (ID and tags) if enabled.
    /// </summary>
    private void DrawTextOverlays(Entity entity, SpriteBatch spriteBatch, FontAsset fontAsset)
    {
        if (_config.ShowEntityIds)
        {
            DrawId(entity, spriteBatch, fontAsset);
        }

        if (_config.ShowEntityTags)
        {
            DrawTags(entity, spriteBatch, fontAsset);
        }
    }

    /// <summary>
    /// Draws a bounding box around the entity using its position and scale.
    /// </summary>
    private void DrawBounds(Entity entity, SpriteBatch spriteBatch)
    {
        var pos = entity.Position;
        var size = entity.Scale * 64f; // Default entity size estimate
        var bounds = new Rectangle((int)pos.X, (int)pos.Y, (int)size.X, (int)size.Y);
        Debug.Primitives.DrawRectangle(spriteBatch, bounds, _config.BoundsColor, _config.LineThickness);
    }

    /// <summary>
    /// Draws a small crosshair marker at the entity's position.
    /// </summary>
    private void DrawPositionMarker(Entity entity, SpriteBatch spriteBatch)
    {
        var pos = entity.Position;
        const float size = 4f;
        Debug.Primitives.DrawLine(spriteBatch,
            new Vector2(pos.X - size, pos.Y),
            new Vector2(pos.X + size, pos.Y),
            _config.PositionColor, _config.LineThickness);
        Debug.Primitives.DrawLine(spriteBatch,
            new Vector2(pos.X, pos.Y - size),
            new Vector2(pos.X, pos.Y + size),
            _config.PositionColor, _config.LineThickness);
    }

    /// <summary>
    /// Draws the entity's ID above its position.
    /// </summary>
    private void DrawId(Entity entity, SpriteBatch spriteBatch, FontAsset fontAsset)
    {
        if (entity.Id == null || fontAsset.Font == null)
            return;

        var pos = entity.Position;
        var textPos = new Vector2(pos.X, pos.Y - 16f);
        spriteBatch.DrawString(fontAsset.Font, entity.Id, textPos, _config.IdColor);
    }

    /// <summary>
    /// Draws the entity's tags below its position.
    /// </summary>
    private void DrawTags(Entity entity, SpriteBatch spriteBatch, FontAsset fontAsset)
    {
        if (entity.Tags.Count == 0 || fontAsset.Font == null)
            return;

        var pos = entity.Position;
        var textPos = new Vector2(pos.X, pos.Y + 16f);
        var tagText = string.Join(", ", entity.Tags);
        spriteBatch.DrawString(fontAsset.Font, tagText, textPos, _config.TagColor);
    }

    /// <summary>
    /// Draws lines connecting parent entities to their children.
    /// </summary>
    private void DrawHierarchy(List<Entity> entities, SpriteBatch spriteBatch)
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

            Debug.Primitives.DrawLine(spriteBatch, parentPos, childPos, _config.HierarchyColor, _config.LineThickness);
        }
    }
}
