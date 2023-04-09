﻿// Authors:
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

using System.Windows.Forms;
using BlueBasics.Interfaces;
using BlueControls.Controls;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das PadItem zu einem ConnectedFormula-Control übersetzt werden kann.
/// </summary>
public interface IItemToControl : IHasKeyName, IHasVersion {

    #region Methods

    public Control? CreateControl(ConnectedFormulaView parent);

    public bool IsVisibleOnPage(string page);

    #endregion
}