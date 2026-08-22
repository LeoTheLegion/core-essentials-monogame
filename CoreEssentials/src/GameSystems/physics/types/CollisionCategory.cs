namespace CoreEssentials.GameSystems.Physics.Types;

/// <summary>
/// Engine-agnostic collision category bits used for collision filtering.
/// Mirrors the underlying physics engine's category flags bit-for-bit so the
/// adapter can cast directly without translation. Combine bits with <c>|</c>
/// to assign a collider to multiple categories, e.g. <c>CollisionCategory.Cat1 | CollisionCategory.Cat2</c>.
/// </summary>
[Flags]
public enum CollisionCategory
{
    /// <summary>No category. The collider belongs to no category and collides with nothing by mask.</summary>
    None = 0x0,

    /// <summary>Category 1 (default category for new fixtures).</summary>
    Cat1 = 0x1,

    /// <summary>Category 2.</summary>
    Cat2 = 0x2,

    /// <summary>Category 3.</summary>
    Cat3 = 0x4,

    /// <summary>Category 4.</summary>
    Cat4 = 0x8,

    /// <summary>Category 5.</summary>
    Cat5 = 0x10,

    /// <summary>Category 6.</summary>
    Cat6 = 0x20,

    /// <summary>Category 7.</summary>
    Cat7 = 0x40,

    /// <summary>Category 8.</summary>
    Cat8 = 0x80,

    /// <summary>Category 9.</summary>
    Cat9 = 0x100,

    /// <summary>Category 10.</summary>
    Cat10 = 0x200,

    /// <summary>Category 11.</summary>
    Cat11 = 0x400,

    /// <summary>Category 12.</summary>
    Cat12 = 0x800,

    /// <summary>Category 13.</summary>
    Cat13 = 0x1000,

    /// <summary>Category 14.</summary>
    Cat14 = 0x2000,

    /// <summary>Category 15.</summary>
    Cat15 = 0x4000,

    /// <summary>Category 16.</summary>
    Cat16 = 0x8000,

    /// <summary>Category 17.</summary>
    Cat17 = 0x10000,

    /// <summary>Category 18.</summary>
    Cat18 = 0x20000,

    /// <summary>Category 19.</summary>
    Cat19 = 0x40000,

    /// <summary>Category 20.</summary>
    Cat20 = 0x80000,

    /// <summary>Category 21.</summary>
    Cat21 = 0x100000,

    /// <summary>Category 22.</summary>
    Cat22 = 0x200000,

    /// <summary>Category 23.</summary>
    Cat23 = 0x400000,

    /// <summary>Category 24.</summary>
    Cat24 = 0x800000,

    /// <summary>Category 25.</summary>
    Cat25 = 0x1000000,

    /// <summary>Category 26.</summary>
    Cat26 = 0x2000000,

    /// <summary>Category 27.</summary>
    Cat27 = 0x4000000,

    /// <summary>Category 28.</summary>
    Cat28 = 0x8000000,

    /// <summary>Category 29.</summary>
    Cat29 = 0x10000000,

    /// <summary>Category 30.</summary>
    Cat30 = 0x20000000,

    /// <summary>Category 31.</summary>
    Cat31 = 0x40000000,

    /// <summary>All categories. Use as a <see cref="CoreEssentials.GameSystems.Physics.Types.ICollider.CollidesWith"/> mask to accept every category.</summary>
    All = int.MaxValue
}
