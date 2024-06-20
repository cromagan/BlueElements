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

using BlueBasics;
using BlueBasics.Enums;
using BlueBasics.Interfaces;
using BlueControls.Forms;
using BlueControls.Interfaces;
using System;
using System.Windows.Forms;
using static BlueBasics.Interfaces.SimpleEditorExtension;

namespace BlueControls.Editoren;

#nullable enable

public partial class EditorEasy : UserControl, IIsEditor {

    #region Fields

    private IEditable? _toEdit = null;

    #endregion

    #region Constructors

    public EditorEasy() {
        InitializeComponent();
    }

    #endregion

    #region Properties

    public bool Editabe { get; set; }

    public string Error { get; private set; } = "Nicht Initialisiert.";

    public IEditable? ToEdit {
        protected get => _toEdit;

        set {
            if (_toEdit == value) { return; }
            _toEdit = value;
            if (!DefaultGenerated) { return; }

            Clear();

            Error = string.Empty;
            if (!Init(_toEdit)) {
                Error = "Objekt konnte nicht initialisiert werden.";
            }
        }
    }

    /// <summary>
    /// Ob die Standardwerte der Elemente erstell wurden. Z.B. Komboboxen befüllt
    /// </summary>
    protected bool DefaultGenerated { get; private set; }

    #endregion

    #region Methods

    /// <summary>
    /// Reseted Formulare. Löscht z.B. Texte, Tabellen-Einträge, etc
    /// </summary>
    public virtual void Clear() {
        Develop.DebugPrint_RoutineMussUeberschriebenWerden(false);
    }

    public virtual IEditable? GetCloneOfCurrent() {
        if (!string.IsNullOrEmpty(Error)) { return null; }

        Develop.DebugPrint_RoutineMussUeberschriebenWerden(false);
        return null;
    }

    /// <summary>
    /// Schreibt die Werte des Objekts in die Steuerelemente
    /// </summary>
    /// <param name="toEdit"></param>
    /// <returns></returns>
    public virtual bool Init(IEditable? toEdit) {
        Develop.DebugPrint_RoutineMussUeberschriebenWerden(false);
        return false;
    }

    /// <summary>
    /// Bereitet das Formular vor. ZB. Dropdown Boxen
    /// </summary>
    protected virtual void InitializeComponentDefaultValues() {
        Develop.DebugPrint_RoutineMussUeberschriebenWerden(false);
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);

        if (DefaultGenerated || !Visible || Disposing) { return; }

        DefaultGenerated = true;

        InitializeComponentDefaultValues();

        Clear();

        Error = string.Empty;
        if (!Init(_toEdit)) {
            Error = "Objekt konnte nicht initialisiert werden.";
        }
    }

    #endregion
}