using BlueControls.ItemCollectionPad.Abstract;

namespace BlueControls.ItemCollectionPad.Temporär;

public struct StrPolygonCollisionResult {

    #region Fields

    public AbstractPhysicPadItem CheckedObjectA;

    public AbstractPhysicPadItem CheckedObjectB;

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