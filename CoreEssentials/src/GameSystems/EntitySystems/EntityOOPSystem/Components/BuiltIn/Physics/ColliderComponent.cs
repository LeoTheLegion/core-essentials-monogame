using System;
using System.Xml.Linq;
using Microsoft.Xna.Framework;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;
using CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Serialization;
using CoreEssentials.GameSystems.Physics.Types;

namespace CoreEssentials.GameSystems.EntitySystems.EntityOOPSystem.Components.BuiltIn;

/// <summary>
/// The type of collider shape.
/// </summary>
public enum ColliderShapeType
{
    /// <summary>
    /// Circle shape.
    /// </summary>
    Circle,

    /// <summary>
    /// Rectangle shape.
    /// </summary>
    Rectangle,

    /// <summary>
    /// Polygon shape.
    /// </summary>
    Polygon,

    /// <summary>
    /// Convex hull shape.
    /// </summary>
    ConvexHull
}

/// <summary>
/// Component that adds collision detection to an entity by managing an ICollider.
/// Requires a RigidbodyComponent on the same entity to create the collider on.
/// </summary>
public class ColliderComponent : EntityComponent, ISerializableComponent
{
    private ICollider? _collider;

    /// <summary>
    /// Gets the type of collider shape.
    /// </summary>
    public ColliderShapeType ShapeType { get; }

    /// <summary>
    /// Gets the underlying collider. Returns null until the collider is created.
    /// </summary>
    public ICollider? Collider => _collider;

    /// <summary>
    /// Gets whether the collider has been created.
    /// </summary>
    public bool IsColliderCreated => _collider != null;

    /// <summary>
    /// Gets or sets the friction coefficient (0 = slippery, 1 = sticky).
    /// </summary>
    public float Friction { get; set; }

    /// <summary>
    /// Gets or sets the restitution/bounciness (0 = no bounce, 1 = full bounce).
    /// </summary>
    public float Restitution { get; set; }

    /// <summary>
    /// Gets or sets the collision categories this collider belongs to.
    /// Defaults to <see cref="CollisionCategory.Cat1"/>. Applied to the underlying collider when it is created.
    /// </summary>
    public CollisionCategory Categories { get; set; } = CollisionCategory.Cat1;

    /// <summary>
    /// Gets or sets the mask of categories this collider is willing to collide with.
    /// Defaults to <see cref="CollisionCategory.All"/>. Applied to the underlying collider when it is created.
    /// </summary>
    public CollisionCategory CollidesWith { get; set; } = CollisionCategory.All;

    /// <summary>
    /// Gets or sets the local offset from the body's center.
    /// </summary>
    public Vector2 Offset { get; set; }

    /// <summary>
    /// Gets or sets the radius for circle colliders.
    /// </summary>
    public float Radius { get; set; }

    /// <summary>
    /// Gets or sets the size (half-width, half-height) for rectangle colliders.
    /// </summary>
    public Vector2 Size { get; set; }

    /// <summary>
    /// Gets or sets the vertices for polygon colliders (counter-clockwise order).
    /// </summary>
    public Vector2[]? Vertices { get; set; }

    /// <summary>
    /// Gets or sets the points for convex hull colliders.
    /// </summary>
    public Vector2[]? ConvexHullPoints { get; set; }

    /// <summary>
    /// Fired when this collider starts colliding with another collider.
    /// </summary>
    public event Func<ColliderCollisionEventArgs, bool>? OnCollision
    {
        add
        {
            var collider = _collider;
            if (collider != null)
                collider.OnCollision += value;
        }
        remove
        {
            var collider = _collider;
            if (collider != null)
                collider.OnCollision -= value;
        }
    }

    /// <summary>
    /// Fired when this collider stops colliding with another collider.
    /// </summary>
    public event Action<ColliderSeparationEventArgs>? OnSeparation
    {
        add
        {
            var collider = _collider;
            if (collider != null)
                collider.OnSeparation += value;
        }
        remove
        {
            var collider = _collider;
            if (collider != null)
                collider.OnSeparation -= value;
        }
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColliderComponent"/> class with a circle shape.
    /// </summary>
    /// <param name="radius">Radius of the circle.</param>
    /// <param name="offset">Local offset from the body's center.</param>
    public ColliderComponent(float radius, Vector2? offset = null)
    {
        ShapeType = ColliderShapeType.Circle;
        Radius = radius;
        Offset = offset ?? Vector2.Zero;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColliderComponent"/> class with a rectangle shape.
    /// </summary>
    /// <param name="size">Width and height of the rectangle.</param>
    /// <param name="offset">Local offset from the body's center.</param>
    public ColliderComponent(Vector2 size, Vector2? offset = null)
    {
        ShapeType = ColliderShapeType.Rectangle;
        Size = size;
        Offset = offset ?? Vector2.Zero;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColliderComponent"/> class with a polygon shape.
    /// </summary>
    /// <param name="vertices">Array of vertices in local space (counter-clockwise order).</param>
    public ColliderComponent(params Vector2[] vertices)
    {
        ShapeType = ColliderShapeType.Polygon;
        Vertices = vertices;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ColliderComponent"/> class with a convex hull shape.
    /// </summary>
    /// <param name="shapeType">Must be <see cref="ColliderShapeType.ConvexHull"/>.</param>
    /// <param name="points">Points to compute the convex hull from.</param>
    public ColliderComponent(ColliderShapeType shapeType, Vector2[] points)
    {
        if (shapeType != ColliderShapeType.ConvexHull)
            throw new ArgumentException("Use the params constructor for other shape types.", nameof(shapeType));

        ShapeType = ColliderShapeType.ConvexHull;
        ConvexHullPoints = points;
    }

    /// <inheritdoc/>
    public override void OnAttach()
    {
        CreateCollider();
    }

    /// <inheritdoc/>
    public override void OnDetach()
    {
        DestroyCollider();
    }

    /// <inheritdoc/>
    public override void Update(GameTime gameTime)
    {
        // Auto-size circle colliders to hug the owner's rendered sprite. The radius is
        // derived from the sprite size scaled by the entity transform, so the collider
        // always matches what is drawn. Aether fixtures are immutable, so we only
        // destroy + recreate the collider when the computed radius actually changes —
        // the dirty-check below keeps this a cheap no-op in steady state.
        if (ShapeType != ColliderShapeType.Circle || _collider == null || Owner == null)
            return;

        if (!Owner.TryGetComponent<SpriteComponent>(out var spriteComponent) || spriteComponent?.Sprite == null)
            return;

        Vector2 spriteSize;
        try
        {
            spriteSize = spriteComponent.Sprite.GetSize();
        }
        catch (InvalidOperationException)
        {
            return; // Sprite metadata not loaded yet.
        }

        float targetRadius = spriteSize.X / 2f * Owner.Scale.X;
        float currentRadius = _collider.Shape?.Radius ?? Radius;
        if (Math.Abs(targetRadius - currentRadius) > 0.0001f)
            UpdateCircleRadius(targetRadius);
    }

    /// <summary>
    /// Creates the collider on the RigidbodyComponent's body.
    /// Must be called after the RigidbodyComponent has created its body.
    /// </summary>
    public void CreateCollider()
    {
        if (_collider != null)
            return;

        var rigidbody = Owner.GetComponent<RigidbodyComponent>();
        if (rigidbody == null)
            throw new InvalidOperationException("Entity must have a RigidbodyComponent before adding a ColliderComponent.");

        var body = rigidbody.Body;
        if (body == null)
        {
            rigidbody.CreateBody();
            body = rigidbody.Body;
        }

        if (body == null)
            throw new InvalidOperationException("RigidbodyComponent body is not created.");

        switch (ShapeType)
        {
            case ColliderShapeType.Circle:
                _collider = body.CreateCircleCollider(Radius, Offset);
                break;

            case ColliderShapeType.Rectangle:
                _collider = body.CreateRectangleCollider(Size, Offset);
                break;

            case ColliderShapeType.Polygon:
                if (Vertices == null || Vertices.Length == 0)
                    throw new InvalidOperationException("Polygon collider requires vertices.");
                _collider = body.CreatePolygonCollider(Vertices);
                break;

            case ColliderShapeType.ConvexHull:
                if (ConvexHullPoints == null || ConvexHullPoints.Length == 0)
                    throw new InvalidOperationException("Convex hull collider requires points.");
                _collider = body.CreateConvexHullCollider(ConvexHullPoints);
                break;

            default:
                throw new InvalidOperationException($"Unknown collider shape type: {ShapeType}");
        }

        _collider.Friction = Friction;
        _collider.Restitution = Restitution;
        _collider.Categories = Categories;
        _collider.CollidesWith = CollidesWith;
    }

    /// <summary>
    /// Destroys the collider by removing it from the physics body.
    /// </summary>
    public void DestroyCollider()
    {
        if (_collider == null)
            return;

        var rigidbody = Owner.GetComponent<RigidbodyComponent>();
        rigidbody?.RawBody?.RemoveCollider(_collider);
        _collider.Dispose();
        _collider = null;
    }

    /// <summary>
    /// Updates the collider radius and recreates it.
    /// </summary>
    /// <param name="newRadius">The new radius.</param>
    public void UpdateCircleRadius(float newRadius)
    {
        if (ShapeType != ColliderShapeType.Circle)
            throw new InvalidOperationException("This collider is not a circle.");

        Radius = newRadius;
        DestroyCollider();
        CreateCollider();
    }

    /// <summary>
    /// Updates the collider size and recreates it.
    /// </summary>
    /// <param name="newSize">The new size.</param>
    public void UpdateRectangleSize(Vector2 newSize)
    {
        if (ShapeType != ColliderShapeType.Rectangle)
            throw new InvalidOperationException("This collider is not a rectangle.");

        Size = newSize;
        DestroyCollider();
        CreateCollider();
    }

    /// <summary>
    /// Serializes the collider component's state to an XML element.
    /// </summary>
    /// <returns>An XML element containing the component's serialized state.</returns>
    public XElement SerializeToXml()
    {
        return new XElement("ColliderState",
            new XAttribute("ShapeType", ShapeType.ToString()),
            new XAttribute("Friction", Friction),
            new XAttribute("Restitution", Restitution),
            new XAttribute("Categories", Categories.ToString()),
            new XAttribute("CollidesWith", CollidesWith.ToString()),
            new XAttribute("OffsetX", Offset.X),
            new XAttribute("OffsetY", Offset.Y),
            ShapeType == ColliderShapeType.Circle ? new XAttribute("Radius", Radius) : null,
            ShapeType == ColliderShapeType.Rectangle ? new XAttribute("SizeX", Size.X) : null,
            ShapeType == ColliderShapeType.Rectangle ? new XAttribute("SizeY", Size.Y) : null
        );
    }

    /// <summary>
    /// Deserializes the collider component's state from an XML element.
    /// </summary>
    /// <param name="element">The XML element containing the component's state.</param>
    public void DeserializeFromXml(XElement element)
    {
        Friction = float.Parse(element.Attribute("Friction")?.Value ?? "0.5");
        Restitution = float.Parse(element.Attribute("Restitution")?.Value ?? "0.5");
        Categories = ParseCategory(element.Attribute("Categories")?.Value, CollisionCategory.Cat1);
        CollidesWith = ParseCategory(element.Attribute("CollidesWith")?.Value, CollisionCategory.All);

        Offset = new Vector2(
            float.Parse(element.Attribute("OffsetX")?.Value ?? "0"),
            float.Parse(element.Attribute("OffsetY")?.Value ?? "0")
        );

        if (ShapeType == ColliderShapeType.Circle)
        {
            Radius = float.Parse(element.Attribute("Radius")?.Value ?? "1");
        }
        else if (ShapeType == ColliderShapeType.Rectangle)
        {
            Size = new Vector2(
                float.Parse(element.Attribute("SizeX")?.Value ?? "1"),
                float.Parse(element.Attribute("SizeY")?.Value ?? "1")
            );
        }
    }

    /// <summary>
    /// Parses a flags-enum category value from a string, falling back to <paramref name="fallback"/> on any parse failure.
    /// </summary>
    /// <param name="value">The category string (e.g. "Cat1" or "Cat1, Cat2").</param>
    /// <param name="fallback">The value to return when <paramref name="value"/> is null, empty, or unparseable.</param>
    private static CollisionCategory ParseCategory(string? value, CollisionCategory fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
            return fallback;

        return Enum.TryParse<CollisionCategory>(value, ignoreCase: true, out var parsed) ? parsed : fallback;
    }
}
