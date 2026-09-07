// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueBasics.Classes;

/// <summary>
/// Weist einem Enum-Member ein Symbol und optional einen Anzeigetext zu.
/// Wird beim automatischen Erzeugen von ListItems aus Enums verwendet.
/// </summary>
[AttributeUsage(AttributeTargets.Field | AttributeTargets.Enum)]
public sealed class ImageAttribute : Attribute {

    #region Constructors

    public ImageAttribute(ImageCode symbol) => Symbol = symbol;

    public ImageAttribute(ImageCode symbol, string text) : this(symbol) => Text = text;

    public ImageAttribute(string image) => ImageName = image;

    public ImageAttribute(string image, string text) : this(image) => Text = text;

    #endregion

    #region Properties

    public ImageCode? Symbol { get; }

    /// <summary>
    /// Bildname für QuickImage, z. B. <c>BildmodusVerzerren|32</c>.
    /// </summary>
    public string ImageName { get; } = string.Empty;

    /// <summary>
    /// Optionaler Anzeigetext. Bleibt leer, um den Enum-Membernamen anzuzeigen.
    /// </summary>
    public string Text { get; } = string.Empty;

    #endregion
}
