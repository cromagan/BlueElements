// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

using System.ComponentModel.Design;

namespace BlueControls.Designer_Support;

internal sealed class TextBoxDesigner : BasicDesigner {

    #region Fields

    private DesignerActionListCollection? _aList;

    #endregion

    #region Properties

    public override DesignerActionListCollection ActionLists {
        get {
            _aList ??=
                [
                    new TextBoxActionList(Component)
                ];
            return _aList;
        }
    }

    #endregion
}