// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueControls.MarkRenderer;

public abstract class MarkRenderer : IComparable<MarkRenderer>, IHasKeyName {

    #region Fields

    public static readonly AssemblyAwareCache<MarkRenderer> AllRenderers = new();

    #endregion

    #region Constructors

    protected MarkRenderer() { }

    #endregion

    #region Properties

    public abstract string KeyName { get; }
    public abstract int Priority { get; }

    #endregion

    #region Methods

    public int CompareTo(MarkRenderer? other) {
        if (other is null) { return 1; }
        return Priority.CompareTo(other.Priority);
    }

    public abstract void Render(Graphics gr, float zoom, float startX, float startY, float endX, float endY, float height);

    #endregion
}