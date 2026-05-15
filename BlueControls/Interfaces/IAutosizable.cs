// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Interfaces;

public interface IAutosizable {

    #region Properties

    bool AutoSizeableHeight { get; }
    RectangleF CanvasUsedArea { get; }

    #endregion

    #region Methods

    bool IsVisibleForMe(string mode, bool nowDrawing);

    void SetCoordinates(RectangleF r);

    #endregion
}

public static class AutosizableExtension {

    #region Fields

    public const float GridSize = 8; // PixelToMm(4f, ItemCollectionPadItem.Dpi);

    public const float MinHeigthCapAndBox = 48;
    public const float MinHeigthCaption = 16;
    public const float MinHeigthTextBox = 24;

    #endregion

    #region Methods

    public static bool CanChangeHeightTo(this IAutosizable item, float heightinPixel) => item.AutoSizeableHeight && heightinPixel > MinHeigthCapAndBox;

    public static bool CanScaleHeightTo(this IAutosizable item, float scale) => CanChangeHeightTo(item, item.CanvasUsedArea.Height.CanvasToControl(scale));

    #endregion
}