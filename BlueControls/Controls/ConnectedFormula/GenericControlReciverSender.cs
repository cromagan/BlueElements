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
using BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;
using BlueTable;
using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace BlueControls.Controls;

public class GenericControlReciverSender : GenericControlReciver {

    #region Fields

    private const int MaxRecursionDepth = 10;
    private int _recursionDepth = 0;

    #endregion

    #region Constructors

    public GenericControlReciverSender(bool doubleBuffer, bool useBackgroundBitmap, bool mouseHighlight) : base(doubleBuffer, useBackgroundBitmap, mouseHighlight) => FilterOutput = new($"{_outputf} {GetType().Name}");

    #endregion

    #region Events

    public event EventHandler? FilterOutputPropertyChanged;

    #endregion

    #region Properties

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public List<GenericControlReciver> Childs { get; } = [];

    [Browsable(false)]
    [EditorBrowsable(EditorBrowsableState.Never)]
    [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
    public FilterCollection FilterOutput { get; }

    #endregion

    #region Methods

    public void DoDefaultSettings(ConnectedFormulaView? parentFormula, ReciverSenderControlPadItem source, string mode) {
        FilterOutput.Table = source.TableOutput;
        base.DoDefaultSettings(parentFormula, source, mode);
    }

    internal void ChildIsBorn(GenericControlReciver child) {
        if (child.IsDisposed || IsDisposed) { return; }

        child.Parents.AddIfNotExists(this);

        Childs.AddIfNotExists(child);

        child.Invalidate_FilterInput();
    }

    protected override void Dispose(bool disposing) {
        if (disposing) {
            Parent?.Controls.Remove(this);

            foreach (var thisChild in Childs) {
                thisChild.Parents.Remove(this);
            }

            Childs.Clear();
            FilterOutput.Dispose();
        }

        base.Dispose(disposing);
    }

    protected void Invalidate_FilterOutput() => FilterOutput.Clear();

    protected override void OnCreateControl() {
        base.OnCreateControl();
        FilterOutput.PropertyChanged += FilterOutput_PropertyChanged;
        FilterOutput.DisposingEvent += FilterOutput_DisposingEvent;
    }

    private void FilterOutput_DisposingEvent(object sender, System.EventArgs e) {
        if (IsDisposed) { return; }

        FilterOutput.PropertyChanged -= FilterOutput_PropertyChanged;
        FilterOutput.DisposingEvent -= FilterOutput_DisposingEvent;
        FilterOutput.Table = null;
    }

    private void FilterOutput_PropertyChanged(object sender, PropertyChangedEventArgs e) {
        if (IsDisposed) { return; }

        if (_recursionDepth >= MaxRecursionDepth) {
            Develop.DebugPrint(ErrorType.Error, "Maximale Rekursionstiefe erreicht");
            return;
        }

        _recursionDepth++;
        try {
            foreach (var thisChild in Childs) {
                thisChild.Invalidate_FilterInput();
            }
            OnFilterOutputPropertyChanged();
        } catch {
            //Develop.DebugPrint(ErrorType.Error, "Fehler in FilterOutput_PropertyChanged", ex);
            Develop.AbortAppIfStackOverflow();
            FilterOutput_PropertyChanged(sender, e);
        } finally {
            _recursionDepth--;
        }
    }

    private void OnFilterOutputPropertyChanged() => FilterOutputPropertyChanged?.Invoke(this, System.EventArgs.Empty);

    #endregion
}