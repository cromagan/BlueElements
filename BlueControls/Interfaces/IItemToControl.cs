// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using BlueControls.Controls;
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