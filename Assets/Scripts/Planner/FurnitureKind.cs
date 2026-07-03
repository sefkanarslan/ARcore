namespace ArSpacePlanner.Planner
{
    /// <summary>Shape family used by <see cref="FurnitureMeshBuilder"/> to build a mesh.</summary>
    public enum FurnitureKind
    {
        Table,
        Chair,
        Sofa,
        Bed,
        Wardrobe,
        Desk,
        Tv,
        Painting,
        Shelf
    }

    /// <summary>Which surface an item attaches to.</summary>
    public enum MountType
    {
        Floor,
        Wall
    }
}
