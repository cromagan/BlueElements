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
using BlueControls.Controls;
using BlueControls.Enums;
using BlueControls.Forms;
using BlueControls.Interfaces;
using BlueControls.ItemCollectionList;
using BlueControls.ItemCollectionPad.Abstract;
using BlueDatabase;
using BlueDatabase.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using static BlueBasics.Converter;
using static BlueBasics.Generic;
using static BlueBasics.Polygons;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

#nullable enable

namespace BlueControls.ItemCollectionPad.FunktionsItems_Formular.Abstract;

/// <summary>
/// Standard für Objekte, die einen Datenbank/Zeilenbezug haben.
/// Stellt auch alle Methode breit, zum Einrichten der Breite und Benutzer-Sichtbarkeiten.
/// Nur Tabs, die ein solches Objekt haben, werden als anzeigewürdig gewertet.
/// </summary>

public abstract class ReciverSenderControlPadItem : ReciverControlPadItem {

    #region Constructors

    protected ReciverSenderControlPadItem(string keyName, ConnectedFormula.ConnectedFormula? parentFormula) : base(keyName, parentFormula) {
    }

    #endregion
}