// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using System.ComponentModel.Design;

namespace BlueControls.Designer_Support;

internal sealed class ButtonDesigner : BasicDesigner {

    #region Fields

    private DesignerActionListCollection? _aList;

    #endregion

    #region Properties

    public override DesignerActionListCollection ActionLists {
        get {
            _aList ??=
                [
                    new ButtonActionList(Component)
                ];
            return _aList;
        }
    }

    #endregion
}