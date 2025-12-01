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

using BlueControls.EventArgs;
using System;

namespace BlueControls.Interfaces;

/// <summary>
/// Interface, das zur Generierung von Kontextmenüs benötigt wird.
/// Die ganze Erstellung und Handling übernimmt dabei FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
/// Dabei werden die hier angegebenen Routinen und Events abgefragt und ausgelöst.
/// </summary>
public interface IContextMenu {

    #region Events

    /// <summary>
    /// Dieses Event wird vom Steuerelement benutzt, um von außen zusätzliche Items zu erhalten.
    /// </summary>
    event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    #endregion

    #region Methods

    /// <summary>
    /// Diese Routine wird als erstes aufgerufen und holt sich alle relevanten Daten.
    /// Hier werden auch die Kontextmenü-Einträge erstellt, die intern abgehandelt werden müssen.
    /// Und/Oder OnContextMenuInit wird aufgerufen - damit auch außerhalb es Steuerelementes
    /// Einträge hinzugefügt werden können
    /// </summary>
    /// <param name="e"></param>
    void GetContextMenuItems(ContextMenuInitEventArgs e);

    #endregion
}