// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueDeskSnap.QualityPlugins;

/// <summary>
/// Bewegt den Mauszeiger jede Minute um ein Pixel, abwechselnd nach links und nach rechts - aber nur, solange der Benutzer die Maus nicht selbst bewegt.
/// </summary>
public sealed class MouseJigglerPlugin : QualityPlugin {

    #region Fields

    private bool _moveRight;

    #endregion

    #region Properties

    public override string KeyName => "MouseJiggler";

    public override string Name => "Maus-Jiggler";

    public override string QuickInfo => "Bewegt die Maus jede Minute um ein Pixel, solange sie eine Minute lang nicht bewegt wurde.";

    public override ImageCode Symbol => ImageCode.Mauspfeil;

    #endregion

    #region Methods

    protected override void ExecuteTick() {
        // Der eigene Pixel-Versatz setzt die Idle-Zeit ebenfalls zurück - dadurch kommt automatisch genau ein Versatz pro Minute heraus.
        if (Develop.GetUserIdleSeconds() < 60) { return; }

        var pos = Animator.GetCursorPos();
        System.Windows.Forms.Cursor.Position = new Point(pos.X + (_moveRight ? 1 : -1), pos.Y);
        _moveRight = !_moveRight;
    }

    #endregion
}
