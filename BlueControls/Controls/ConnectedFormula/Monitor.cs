// Licensed under AGPL-3.0; see License.md for disclaimer and details.

using BlueControls.Controls.ConnectedFormula;
using BlueControls.Designer_Support;
using System.Globalization;
using static BlueControls.Classes.ItemCollectionList.AbstractListItemExtension;

namespace BlueControls.Controls;

[Designer(typeof(BasicDesigner))]
public sealed partial class Monitor : GenericControlReciver //UserControl
    {
    #region Fields

    private int _n = 99999;

    #endregion

    #region Constructors

    public Monitor() : base(false, false, false) {
        // Dieser Aufruf ist für den Designer erforderlich.
        InitializeComponent();
        // Fügen Sie Initialisierungen nach dem InitializeComponent()-Aufruf hinzu.
        IsSelectable = false;

        // Handler für Develop.Message registrieren
        Develop.MessageDG += OnDevelopMessage;
    }

    #endregion

    #region Properties

    public RowItem? LastRow {
        get;

        set {
            if (value?.Table is null || value.IsDisposed) { value = null; }

            if (field == value) { return; }

            _n = 99999;
            field = value;
            capInfo.Text = string.Empty;
            lstDone.ItemClear();

            if (field is not null) {
                capInfo.Text = "Überwache: " + field.CellFirstString();
                // Simuliere eine Start-Meldung
                _lastRow_DropMessage(ErrorType.Info, ImageCode.Monitor, "Überwachung gestartet");
            }
        }
    }

    #endregion

    #region Methods

    protected override void Dispose(bool disposing) {
        if (disposing) {
            // Handler wieder entfernen
            Develop.MessageDG -= OnDevelopMessage;
        }
        base.Dispose(disposing);
    }

    protected override void HandleChangesNow() {
        base.HandleChangesNow();
        if (IsDisposed) { return; }
        if (RowsInputChangedHandled && FilterInputChangedHandled) { return; }

        DoInputFilter(null, false);
        RowsInputChangedHandled = true;

        LastRow = RowSingleOrNull();
    }

    /// <summary>
    /// Ursprüngliche Methode, angepasst für das neue Message-System
    /// </summary>
    private void _lastRow_DropMessage(ErrorType type, ImageCode symbol, string message) {
        if (Disposing || IsDisposed) { return; }

        if (InvokeRequired) {
            try {
                Invoke(new Action(() => _lastRow_DropMessage(type, symbol, message)));
            } catch { }
            return;
        }

        _n--;
        if (_n < 0) { _n = 99999; }

        lstDone.ItemAdd(ItemOf(message, Generic.GetUniqueKey(), symbol, false, _n.ToString7()));

        lstDone.Refresh();
        //capInfo.Text = message;
    }

    /// <summary>
    /// Handler für Develop.Message - prüft ob die Referenz der überwachten Row entspricht
    /// </summary>
    private void OnDevelopMessage(ErrorType type, object? reference, string category, ImageCode symbol, string message, int indent) {
        message = "[" + DateTime.Now.ToString("HH:mm:ss", CultureInfo.InvariantCulture) + "] " + message;

        // Nur Meldungen verarbeiten, die sich auf die überwachte Row beziehen
        if (reference == LastRow && LastRow is not null) {
            _lastRow_DropMessage(type, symbol, message);
            return;
        }

        if (reference is InvalidatedRowsManager) {
            _lastRow_DropMessage(type, symbol, message);
            return;
        }

        if (category == Develop.MonitorMessage) {
            _lastRow_DropMessage(type, symbol, message);
            return;
        }
    }

    #endregion
}