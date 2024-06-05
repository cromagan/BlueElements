﻿// Authors:
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
using BlueBasics.Interfaces;
using BlueControls.Forms;
using System;
using System.Windows.Forms;
using static BlueBasics.Interfaces.SimpleEditorExtension;

namespace BlueControls.Editoren;

#nullable enable

public partial class EditorAbstract : UserControl {

    #region Fields

    private IEditable? _toEdit = null;

    #endregion

    #region Constructors

    public EditorAbstract() {
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

    public static void Edit(IEditable toEdit) {
        if (toEdit.Editor == null) { return; }

        try {
            object myObject = Activator.CreateInstance(toEdit.Editor);

            if (myObject is EditorAbstract ea) {
                ea.ToEdit = toEdit;

                var l = new DialogWithOkAndCancel(true, true);

                l.Controls.Add(ea);

                l.Setup(string.Empty, ea, ea.Width);
                l.ShowDialog();
            }
        } catch { }
    }

    /// <summary>
    /// Reseted Formulare. Löscht z.B. Texte, Tabellen-Einträge, etc
    /// </summary>
    public virtual void Clear() {
        if (_toEdit is ISimpleEditor) { return; }
        Develop.DebugPrint_RoutineMussUeberschriebenWerden(false);
    }

    /// <summary>
    /// Schreibt die Werte des Objekts in die Steuerelemente
    /// </summary>
    /// <param name="toEdit"></param>
    /// <returns></returns>
    protected virtual bool Init(IEditable? toEdit) {
        if (_toEdit is ISimpleEditor ise) {
            DoForm(ise, this.Controls, this.Width);
            return true;
        }

        Develop.DebugPrint_RoutineMussUeberschriebenWerden(false);
        return false;
    }

    /// <summary>
    /// Bereitet das Formular vor. ZB. Dropdown Boxen
    /// </summary>
    protected virtual void InitializeComponentDefaultValues() {
        if (_toEdit is ISimpleEditor ise) { return; }

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