// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.Windows.Forms;

namespace BlueControls.Controls;

public class FlexiControlForDelegate : FlexiControl {

    #region Fields

    private DoThis? _doThis;

    #endregion

    #region Constructors

    public FlexiControlForDelegate() : this(null, string.Empty, BlueBasics.Enums.ImageCode.Kreuz) { }

    public FlexiControlForDelegate(DoThis? doThis, string text, BlueBasics.Enums.ImageCode image) : base() {
        Init(doThis, text, QuickImage.Get(image, 22));
    }

    /// <summary>
    /// Öffnet den Dialog zum DIREKTEN Bearbeiten.
    /// Soll es überschrieben werden  (z.B. Weil es readonly ist)
    /// FlexiControlForProperty benutzen.
    /// </summary>
    /// <param name="editable"></param>
    public FlexiControlForDelegate(IEditable editable) {
        if (editable is IReadableTextWithKey irt) {
            Init(editable.Edit, $"{editable.CaptionForEditor} '{irt.KeyName}' bearbeiten", irt.SymbolForReadableText());
        } else if (editable is IReadableText ir) {
            Init(editable.Edit, $"{editable.CaptionForEditor} bearbeiten", ir.SymbolForReadableText());
        } else {
            Init(editable.Edit, $"{editable.CaptionForEditor} bearbeiten", QuickImage.Get(BlueBasics.Enums.ImageCode.Smiley, 22));
        }
    }

    #endregion

    #region Delegates

    public delegate void DoThis();

    #endregion

    #region Methods

    protected override void OnControlAdded(ControlEventArgs e) {
        CheckEnabledState();
        base.OnControlAdded(e);
    }

    protected override void OnExecuteComand() {
        base.OnExecuteComand();
        _doThis?.Invoke();
    }

    private void CheckEnabledState() {
        if (DesignMode) {
            DisabledReason = string.Empty;
            return;
        }

        if (_doThis is null) {
            DisabledReason = "Kein zugehöriges Objekt definiert.";
            return;
        }

        DisabledReason = string.Empty;
    }

    private void GenFehlerText() => InfoText = string.Empty;

    private void Init(DoThis? doThis, string text, QuickImage? image) {
        _doThis = doThis;

        Size = new Size(200, 24);

        EditType = EditTypeFormula.Button;
        CaptionPosition = CaptionPosition.ohne;
        Caption = text;
        if (image is { } im) {
            ImageCode = im.KeyName;
        }
        var s0 = BlueControls.Controls.Caption.RequiredTextSize(text, Design.Caption, Translate, -1);

        Size = new Size(s0.Width + 50 + 22, 30);

        GenFehlerText();

        CheckEnabledState();
    }

    #endregion
}