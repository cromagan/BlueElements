// Licensed under AGPL-3.0; see License.md for disclaimer and details.

namespace BlueControls.Forms;

public sealed partial class Test : Form {

    #region Constructors

    public Test() : base() =>
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();

    #endregion

    #region Methods

    [StandaloneInfo("Test", ImageCode.Puzzle, "Admin", "Test", 900)]
    public static System.Windows.Forms.Form Start() => new Test();

    #endregion
}