using BlueControls;
using BlueControls.ItemCollection;
public struct strPolygonCollisionResult {
    /// <summary>
    /// Are the polygons going to intersect forward in time?
    /// </summary>
    public bool WillIntersect;
    /// <summary>
    /// Are the polygons currently intersecting
    /// </summary>
    public bool Intersect;
    /// <summary>
    /// The translation to apply to polygon A to push the polygons appart. 
    /// </summary>
    public PointM MinimumTranslationVector;
    public clsAbstractPhysicPadItem CheckedObjectA;
    public clsAbstractPhysicPadItem CheckedObjectB;
}