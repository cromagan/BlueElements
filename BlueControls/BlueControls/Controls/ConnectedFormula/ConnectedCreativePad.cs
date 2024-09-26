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
using BlueControls.BlueDatabaseDialogs;
using BlueControls.Designer_Support;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionPad.FunktionsItems_Formular;
using BlueDatabase;
using System.ComponentModel;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public partial class ConnectedCreativePad : GenericControlReciver, IOpenScriptEditor {

    #region Fields

    private RowItem? _lastRow;

    #endregion

    #region Constructors

    public ConnectedCreativePad(ItemCollectionPad.ItemCollectionPad? itemCollectionPad) : base(false, false) {
        // Dieser Aufruf ist für den Windows Form-Designer erforderlich.
        InitializeComponent();
        // Initialisierungen nach dem Aufruf InitializeComponent() hinzufügen
        SetNotFocusable();
        pad.Items = itemCollectionPad;
        pad.Unselect();
        pad.ShowInPrintMode = true;
        pad.EditAllowed = false;
        MouseHighlight = false;
    }

    public ConnectedCreativePad() : this(null) { }

    #endregion

    #region Properties

    public string DefaultDesign { get; set; }
    public float DefaultScale { get; set; }
    public string ExecuteScriptAtRowChange { get; internal set; }

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public RowItem? LastRow {
        get => _lastRow;

        set {
            if (value?.Database == null || value.IsDisposed) { value = null; }

            if (_lastRow == value) { return; }

            _lastRow = value;

            pad.Items = null;

            if (!string.IsNullOrEmpty(LoadAtRowChange)) {
                if (LoadAtRowChange.FileType() is FileFormat.BlueCreativeFile) {
                    pad.Items = new ItemCollectionPad.ItemCollectionPad(LoadAtRowChange);
                    pad.Items.ResetVariables();
                    pad.Items.ReplaceVariables(_lastRow);
                }
            } else if (!string.IsNullOrEmpty(ExecuteScriptAtRowChange)) {
                pad.Items = new ItemCollectionPad.ItemCollectionPad();
                pad.Items.SheetStyleScale = DefaultScale;

                if (Skin.StyleDb?.Row != null) {
                    pad.Items.SheetStyle = Skin.StyleDb.Row[DefaultDesign];
                }

                if (_lastRow != null) {
                    pad.Items.ExecuteScript(ExecuteScriptAtRowChange, Mode, _lastRow);
                }
                //if (_lastRow != null) {
                //    var script = _lastRow.ExecuteScript(null, ExecuteAtRowChange, true, 1, null, true, true);
                //    if (script.AllOk) {
                //        if (script.Variables.Get("PAD") is VariableItemCollectionPad icp && icp.ValueItemCollection is { } item) {
                //            pad.Items = item;
                //        }
                //    }
                //}
            }

            pad.ZoomFit();
        }
    }

    public string LoadAtRowChange { get; internal set; }

    #endregion

    #region Methods

    public void OpenScriptEditor() {
        if (IsDisposed || GeneratedFrom is not CreativePadItem { IsDisposed: false } it) { return; }

        var se = IUniqueWindowExtension.ShowOrCreate<CreativePadScriptEditor>(it);

        se.Row = LastRow;
    }

    /// <summary>
    /// Verwendete Ressourcen bereinigen.
    /// </summary>
    /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
    protected override void Dispose(bool disposing) {
        if (disposing) {
            pad.Items?.Clear();
            pad.Dispose();
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);
        DoRows();

        LastRow = RowSingleOrNull();
    }

    #endregion

    // // TODO: Finalizer nur überschreiben, wenn "Dispose(bool disposing)" Code für die Freigabe nicht verwalteter Ressourcen enthält
    // ~ConnectedFormulaView()
    // {
    //     // Ändern Sie diesen Code nicht. Fügen Sie Bereinigungscode in der Methode "Dispose(bool disposing)" ein.
    //     Dispose(disposing: false);
    // }
}