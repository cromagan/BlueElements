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

using BlueControls.EventArgs;
using System;
using System.Collections.Generic;
using System.Windows.Forms;
using BlueControls.ItemCollectionList;

namespace BlueControls.Interfaces;

/// <summary>
/// Interface, das zur Generierung von Kontextmenüs benötigt wird.
/// Die ganze Erstellung und Handling übernimmt dabei FloatingInputBoxListBoxStyle.ContextMenuShow(this, e);
/// Dabei werden die hier angegebenen Routinen und Events abgefragt und ausgelöst.
/// </summary>
public interface IContextMenu {

    #region Events

    // ReSharper disable once EventNeverSubscribedTo.Global
    event EventHandler<ContextMenuInitEventArgs>? ContextMenuInit;

    // ReSharper disable once EventNeverSubscribedTo.Global
    event EventHandler<ContextMenuItemClickedEventArgs>? ContextMenuItemClicked;

    #endregion

    #region Methods

    /// <summary>
    /// Hier wird dem Steuerelement die Möglichkeit, deen Kontextmenü-Befehl selbst abzuarbeiten.
    /// Gibt dann zurück, ob der im Steuerelement selbst abgehandelt wurde.
    /// Ansonten wird OnContextMenuItemClicked aufgerufen, um den übergeordneten Steuerelement die Möglichkeit zu geben, den Befehl abzuarbeiten.
    /// Wenn das Kontextmenu ausschließlich von außerhalb des Steuerelemnts generiert wurde, muss immer false zurück gegeben werden.
    /// </summary>
    /// <param name="sender"></param>
    /// <param name="e"></param>
    /// <returns></returns>
    bool ContextMenuItemClickedInternalProcessig(object sender, ContextMenuItemClickedEventArgs e);

    /// <summary>
    /// Diese Routine wird als erstes aufgerufen und holt sich alle relevanten Daten.
    /// Hier werden auch die Kontextmenü-Einträge erstellt, die intern abgehandelt werden müssen.
    /// </summary>
    /// <param name="e"></param>
    /// <param name="items"></param>
    /// <param name="hotItem"></param>
    void GetContextMenuItems(MouseEventArgs? e, List<AbstractListItem> items, out object? hotItem);

    /// <summary>
    /// Hier wird dem übergeordneten Steuerelement die Möglichkeit gegeben, Einträge in das Kontextmenu hinzuzufügen.
    /// </summary>
    /// <param name="e"></param>
    void OnContextMenuInit(ContextMenuInitEventArgs e);

    /// <summary>
    /// Wird der angeklickte Kontextmenü-Befehl nicht innerhalb des Steuerlements abgehandelt
    /// (ContextMenuItemClickedInternalProcessig = FALSE), wird ein Event ausglöst, in dem das
    /// übergeordnete Steuerelement den Kontextmenü-Befehl ausführen kann.
    /// </summary>
    /// <param name="e"></param>
    void OnContextMenuItemClicked(ContextMenuItemClickedEventArgs e);

    #endregion
}