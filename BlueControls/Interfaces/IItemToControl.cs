// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueBasics.Interfaces;
using BlueControls.Controls;
using System;
using System.Windows.Forms;

namespace BlueControls.Interfaces;

/// <summary>
/// Wird verwendet, wenn das PadItem zu einem ConnectedFormula-Control übersetzt werden kann.
/// </summary>
public interface IItemToControl : IHasVersion, ICloneable, IReadableTextWithKey {

    #region Methods

    Control? CreateControl(ConnectedFormulaView parent, string mode);

    #endregion
}