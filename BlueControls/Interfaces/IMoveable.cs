// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Interfaces;

public interface IMoveable {

    #region Properties

    bool MoveXByMouse { get; }
    bool MoveYByMouse { get; }

    #endregion

    #region Methods

    void Move(float x, float y, bool isMouse);

    #endregion
}