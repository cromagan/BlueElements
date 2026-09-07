// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

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