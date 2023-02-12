// Authors:
// Christian Peter
//
// Copyright (c) 2023 Christian Peter
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

using System;
using System.ComponentModel;
using BlueBasics;
using BlueBasics.Interfaces;
using BlueControls.Controls;

namespace BlueControls.Classes_Editor;

[DefaultEvent("Changed")]
internal partial class AbstractClassEditor<T> : GroupBox, IChangedFeedback, IDisposableExtended where T : IParseable {

    #region Fields

    private T? _item;

    private string _lastState = string.Empty;

    private bool _visibleChangedDone;

    #endregion

    #region Constructors

    public AbstractClassEditor() : base() => InitializeComponent();

    #endregion

    #region Events

    public event EventHandler? Changed;

    #endregion

    #region Properties

    public bool Inited { get; private set; }

    /// <summary>
    /// Das Objekt, das im Original bearbeitet wird.
    /// </summary>
    /// <returns></returns>
    [DefaultValue(null)]
    internal T? Item {
        get => _item;
        set {
            if (_item != null) {
                _item.Changed -= _item_Changed;
            }

            _item = default;

            if (value != null) {
                _lastState = value.ToString();
                if (!Inited) {
                    Inited = true;
                    PrepaireFormula(value);
                }

                EnabledAndFillFormula(value);
                _item = value;
                _item.Changed += _item_Changed;
            } else {
                _lastState = string.Empty;
                DisableAndClearFormula();
            }
        }
    }

    #endregion

    #region Methods

    public void OnChanged() {
        Changed?.Invoke(this, System.EventArgs.Empty);
    }

    /// <summary>
    /// Sperrt die komplette Bearbeitung des Formulars und löscht alle Einträge.
    /// Typischerweiße, wenn das zu bearbeitende Objekt 'null' ist oder beim erstmaligen Initialiseren des Steuerelementes.
    /// </summary>
    protected virtual void DisableAndClearFormula() => Develop.DebugPrint_RoutineMussUeberschriebenWerden();

    /// <summary>
    /// Erlaubt die Bearbeitung des Objektes und füllt den aktuellen Zustand in das Formular.
    /// </summary>
    protected virtual void EnabledAndFillFormula(T data) => Develop.DebugPrint_RoutineMussUeberschriebenWerden();

    protected override void OnVisibleChanged(System.EventArgs e) {
        base.OnVisibleChanged(e);
        // Damit das Formular nach der Anzeige erstmal deaktiviert ist.
        if (_visibleChangedDone) { return; }
        _visibleChangedDone = true;

        if (_item == null) {
            DisableAndClearFormula();
        }
    }

    /// <summary>
    /// Bereitet das Formular vor. Z.B. werden in den Auswahldialog-Boxen die voreingestellten Werte hineingeschrieben.
    /// Diese Routine wird aufgerufen, wenn das Item zum ersten Mal empfangen wurde.
    /// </summary>
    protected virtual void PrepaireFormula(T data) => Develop.DebugPrint_RoutineMussUeberschriebenWerden();

    private void _item_Changed(object sender, System.EventArgs e) {
        if (_item == null) { return; }

        if (_item is IDisposableExtended d && d.IsDisposed) { return; }

        var newstatse = _item.ToString();
        if (newstatse == _lastState) { return; }

        _lastState = newstatse;
        OnChanged();
    }

    #endregion
}