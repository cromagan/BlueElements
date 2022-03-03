using BlueControls;
using BlueControls.ItemCollection;

public struct StrPolygonCollisionResult {

    #region Fields

    public clsAbstractPhysicPadItem CheckedObjectA;

    public clsAbstractPhysicPadItem CheckedObjectB;

    /// <summary>
    /// Are the polygons currently intersecting
    /// </summary>
    public bool Intersect;

    /// <summary>
    /// The translation to apply to polygon A to push the polygons appart.
    /// </summary>
    public PointM MinimumTranslationVector;

    /// <summary>
    /// Are the polygons going to intersect forward in time?
    /// </summary>
    public bool WillIntersect;

    #endregion
}