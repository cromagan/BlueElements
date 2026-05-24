// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Extended_Text.MarkRendering;

public sealed class Zone {

    #region Constructors

    public Zone(MarkRenderer renderer, int startPos, int endPos) {
        Renderer = renderer;
        StartPos = startPos;
        EndPos = endPos;
    }

    #endregion

    #region Properties

    public int EndPos { get; internal set; }
    public MarkRenderer Renderer { get; }
    public int StartPos { get; internal set; }

    #endregion
}
