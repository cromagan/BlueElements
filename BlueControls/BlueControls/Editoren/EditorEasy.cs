// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

#nullable enable

using System.ComponentModel;
using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Interfaces;

namespace BlueControls.Editoren;

/// <summary>
/// Standard element, dass das Grundgerüst eines Editors Darstellt.
/// Es müssen die Routinen SetValuesToFormula, Clear und InitializeComponentDefaultValues überschrieben werden.
/// </summary>

public partial class EditorEasy : System.Windows.Forms.UserControl, IIsEditor {

    #region Fields

    private GroupBoxStyle _groupBoxStyle = GroupBoxStyle.Nothing;
    private IEditable? _toEdit;

    #endregion

    #region Constructors

    public EditorEasy() => InitializeComponent();

    #endregion

    #region Properties

    public bool Editabe { get; set; }

    public string Error { get; private set; } = "Nicht Initialisiert.";

    [DefaultValue(GroupBoxStyle.Nothing)]
    public GroupBoxStyle GroupBoxStyle {
        get => _groupBoxStyle;
        set {
            if (_groupBoxStyle == value) { return; }
            _groupBoxStyle = value;
            Invalidate();
        }
    }

    public IEditable? ToEdit {
        get => _toEdit;

        set {
            if (_toEdit == value) { return; }

            if (!Visible || Disposing || IsDisposed) {
                _toEdit = value;
                return;
            }

            _toEdit = null; // Keine Steuerelement Änderungen auffangen

            if (!DefaultValuesInitialized) {
                DefaultValuesInitialized = true;
                InitializeComponentDefaultValues();
            }

            Clear();

            Error = string.Empty;
            if (!SetValuesToFormula(value)) {
                Error = "Objekt konnte nicht initialisiert werden.";
            }

            _toEdit = value;
            Invalidate(); // Überschriften aktualisieren
        }
    }

    /// <summary>
    /// Ob die Standardwerte der Elemente erstellt wurden. Z.B. Komboboxen befüllt
    /// </summary>
    protected bool DefaultValuesInitialized { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    /// Reseted Formulare. Löscht z.B. Texte, Tabellen-Einträge, etc
    /// </summary>
    public virtual void Clear() => Develop.DebugPrint_RoutineMussUeberschriebenWerden(false);

    /// <summary>
    /// Bereitet das Formular vor. ZB. Dropdown Boxen
    /// </summary>
    protected virtual void InitializeComponentDefaultValues() => Develop.DebugPrint_RoutineMussUeberschriebenWerden(false);

    protected override void OnPaint(System.Windows.Forms.PaintEventArgs e) {
        if (IsDisposed) { return; }

        base.OnPaint(e);

        if (_groupBoxStyle == GroupBoxStyle.Nothing) { return; }

        var t = "[?]";

        if (_toEdit is { } ea) { t = ea.CaptionForEditor; }

        var s = States.Standard;

        if (!Enabled) { s = States.Standard_Disabled; }
        GroupBox.DrawGroupBox(this, e.Graphics, s, _groupBoxStyle, t);
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);

        if (!Visible || Disposing || IsDisposed) { return; }

        if (DefaultValuesInitialized) { return; }

        var merk = _toEdit;
        _toEdit = null; // Keine Steuerelement Änderungen auffangen
        ToEdit = merk;
    }

    /// <summary>
    /// Schreibt die Werte des Objekts in die Steuerelemente
    /// </summary>
    /// <param name="toEdit"></param>
    /// <returns></returns>
    protected virtual bool SetValuesToFormula(IEditable? toEdit) {
        Develop.DebugPrint_RoutineMussUeberschriebenWerden(false);
        return false;
    }

    #endregion
}