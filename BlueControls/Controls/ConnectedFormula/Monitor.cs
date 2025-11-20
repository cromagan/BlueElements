// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
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
using BlueControls.Designer_Support;
using BlueTable;
using System;
using System.ComponentModel;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public sealed partial class Monitor : GenericControlReciver //UserControl
    {
    #region Fields

    private int _n = 99999;

    #endregion

    #region Constructors

    public Monitor() : base(false, false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        SetNotFocusable();

        // Handler für Develop.Message registrieren
        Develop.Message += OnDevelopMessage;
    }

    #endregion

    #region Properties

    public RowItem? LastRow {
        get;

        set {
            if (value?.Table == null || value.IsDisposed) { value = null; }

            if (field == value) { return; }

            _n = 99999;
            field = value;
            capInfo.Text = string.Empty;
            lstDone.ItemClear();

            if (field != null) {
                capInfo.Text = "Überwache: " + field.CellFirstString();
                // Simuliere eine Start-Meldung
                _lastRow_DropMessage(ErrorType.Info, ImageCode.Monitor, "Überwachung gestartet");
            }
        }
    }

    #endregion

    #region Methods

    protected override void Dispose(bool disposing) {
        if (disposing) {
            // Handler wieder entfernen
            Develop.Message -= OnDevelopMessage;
        }
        base.Dispose(disposing);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);
        RowsInputChangedHandled = true;

        LastRow = RowSingleOrNull();
    }

    /// <summary>
    /// Ursprüngliche Methode, angepasst für das neue Message-System
    /// </summary>
    private void _lastRow_DropMessage(ErrorType type, ImageCode symbol, string message) {
        if (Disposing || IsDisposed) { return; }

        if (InvokeRequired) {
            try {
                Invoke(new Action(() => _lastRow_DropMessage(type, symbol, message)));
            } catch { }
            return;
        }

        _n--;
        if (_n < 0) { _n = 99999; }

        lstDone.ItemAdd(ItemOf(message, Generic.GetUniqueKey(), symbol, false, _n.ToStringInt7()));

        lstDone.Refresh();
        //capInfo.Text = message;
    }

    /// <summary>
    /// Handler für Develop.Message - prüft ob die Referenz der überwachten Row entspricht
    /// </summary>
    private void OnDevelopMessage(ErrorType type, object? reference, string category, ImageCode symbol, string message, int indent) {
        message = "[" + DateTime.Now.ToString("HH:mm:ss") + "] " + message;

        // Nur Meldungen verarbeiten, die sich auf die überwachte Row beziehen
        if (reference == LastRow && LastRow != null) {
            _lastRow_DropMessage(type, symbol, message);
            return;
        }

        if (reference is InvalidatedRowsManager) {
            _lastRow_DropMessage(type, symbol, message);
            return;
        }

        if (category == Develop.MonitorMessage) {
            _lastRow_DropMessage(type, symbol, message);
            return;
        }
    }

    #endregion
}