// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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
using System;
using System.ComponentModel;

namespace BlueControls.Classes_Editor;

[DefaultEvent("Changed")]
internal partial class AbstractClassEditor<T> : Controls.GroupBox where T : IParseable {

    #region Fields

    private T? _item;

    private string _lastState = string.Empty;

    private bool _visibleChangedDone;

    #endregion

    #region Constructors

    public AbstractClassEditor() : base() => InitializeComponent();

    #endregion

    #region Events

    public event EventHandler Changed;

    #endregion

    #region Properties

    public bool Inited { get; private set; }

    public bool IsFilling { get; private set; }

    /// <summary>
    /// Das Objekt, das im Original bearbeitet wird.
    /// </summary>
    /// <returns></returns>
    [DefaultValue(null)]
    internal T? Item {
        get => _item;
        set {
            _item = value;
            if (_item != null) {
                _lastState = _item.ToString();
                if (!Inited) {
                    Inited = true;
                    PrepaireFormula();
                }
                IsFilling = true;
                EnabledAndFillFormula();
                IsFilling = false;
            } else {
                _lastState = string.Empty;
                IsFilling = true;
                DisableAndClearFormula();
                IsFilling = false;
            }
        }
    }

    #endregion

    #region Methods

    /// <summary>
    /// Sperrt die komplette Bearbeitung des Formulars und löscht alle Einträge.
    /// Typischerweiße, wenn das zu bearbeitende Objekt 'null' ist oder beim erstmaligen Initialiseren des Steuerelementes.
    /// </summary>
    protected virtual void DisableAndClearFormula() => Develop.DebugPrint_RoutineMussUeberschriebenWerden();

    /// <summary>
    /// Erlaubt die Bearbeitung des Objektes und füllt den aktuellen Zustand in das Formular.
    /// </summary>
    protected virtual void EnabledAndFillFormula() => Develop.DebugPrint_RoutineMussUeberschriebenWerden();

    protected void OnChanged(T obj) {
        if (IsFilling) { return; }
        var newstatse = obj.ToString();
        if (newstatse == _lastState) { return; }
        _lastState = newstatse;
        Changed?.Invoke(this, System.EventArgs.Empty);
    }

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);
        // Damit das Formular nach der Anzeige erstmal deaktiviert ist.
        if (_visibleChangedDone) { return; }
        _visibleChangedDone = true;
        if (_item == null) {
            IsFilling = true;
            DisableAndClearFormula();
            IsFilling = false;
        }
    }

    /// <summary>
    /// Bereitet das Formular vor. Z.B. werden in den Auswahldialog-Boxen die voreingestellten Werte hineingeschrieben.
    /// Diese Routine wird aufgerufen, wenn das Item zum ersten Mal empfangen wurde.
    /// </summary>
    protected virtual void PrepaireFormula() => Develop.DebugPrint_RoutineMussUeberschriebenWerden();

    #endregion
}