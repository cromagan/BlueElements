using BlueBasics;
using BlueControls.CellRenderer;
using BlueControls.Forms;
using BlueDatabase;
using System;
using System.Threading;
using static BlueBasics.Extensions;
using BlueControls.Controls;

#nullable enable

namespace BlueControls;

public partial class GlobalMonitor : Form {

    #region Fields

    private static CancellationTokenSource? _cancellationTokenSource;
    private static Thread? _monitorThread;
    private static GlobalMonitor? Monitor;
    private int _n = 0;

    #endregion

    #region Constructors

    public GlobalMonitor() {
        InitializeComponent();
        GenerateUndoTabelle(tblLog);

        Develop.MonitorMessage = Message;
    }

    #endregion

    #region Methods

    public static void GenerateUndoTabelle(Table tblLog) {
        //    public void Message(string category, string symbol, string message, int indent) {
        Database db = new(Database.UniqueKeyValue());
        db.LogUndo = false;
        _ = db.Column.GenerateAndAdd("ID", "ID", ColumnFormatHolder.Text);
        _ = db.Column.GenerateAndAdd("Symbol", "Symbol", ColumnFormatHolder.BildCode);
        var az = db.Column.GenerateAndAdd("Time", "Zeit", ColumnFormatHolder.DateTime);
        _ = db.Column.GenerateAndAdd("category", "Kategorie", ColumnFormatHolder.Text);
        _ = db.Column.GenerateAndAdd("Message", "Message", ColumnFormatHolder.Text);
        _ = db.Column.GenerateAndAdd("Indent", "Stufe", ColumnFormatHolder.Long);

        foreach (var thisColumn in db.Column) {
            if (!thisColumn.IsSystemColumn()) {
                thisColumn.MultiLine = true;
                thisColumn.TextBearbeitungErlaubt = false;
                thisColumn.DropdownBearbeitungErlaubt = false;
                thisColumn.DefaultRenderer = Renderer_TextOneLine.ClassId;
            }
        }

        if (db.Column["Symbol"] is { IsDisposed: false } c) {
            var o = new Renderer_ImageAndText {
                Text_anzeigen = false,
                Bild_anzeigen = true
            };
            c.DefaultRenderer = o.MyClassId;
            c.RendererSettings = o.ParseableItems().FinishParseable();
        }

        db.RepairAfterParse();

        var tcvc = ColumnViewCollection.ParseAll(db);
        tcvc[1].ShowColumns("Symbol", "Time", "category", "indent", "Message");

        db.ColumnArrangements = tcvc.ToString(false);

        tblLog.DatabaseSet(db, string.Empty);
        tblLog.Arrangement = string.Empty;
        tblLog.SortDefinitionTemporary = new RowSortDefinition(db, az, true);
    }

    public static void Start() {
        // Prüfe, ob Thread und Monitor bereits funktionieren
        if (_monitorThread != null && _monitorThread.IsAlive && Monitor != null && !Monitor.IsDisposed) {
            // Thread läuft und Fenster existiert, bringe es in den Vordergrund
            try {
                Monitor.BeginInvoke(new Action(() => {
                    Monitor.BringToFront();
                    if (Monitor.WindowState == System.Windows.Forms.FormWindowState.Minimized) {
                        Monitor.WindowState = System.Windows.Forms.FormWindowState.Normal;
                    }
                }));
                return; // Alles in Ordnung, früher beenden
            } catch {
                // Formular bereits entsorgt, wird im nächsten Block neu erstellt
                DisposeMonitor();
            }
        }

        // Neustart des Threads und/oder Monitors erforderlich
        DisposeMonitor();

        // Neuen CancellationTokenSource erstellen
        _cancellationTokenSource = new CancellationTokenSource();

        _monitorThread = new Thread(() => StartMonitorInThread(_cancellationTokenSource.Token));
        _monitorThread.IsBackground = true;
        _monitorThread.SetApartmentState(ApartmentState.STA);
        _monitorThread.Start();

        // Warte kurz, bis das Fenster erstellt wurde
        int attempts = 0;
        while (Monitor == null && attempts < 50) {
            Thread.Sleep(10);
            attempts++;
        }
    }

    public void Message(string category, string symbol, string message, int indent) {
        if (Disposing || IsDisposed) { return; }

        if (string.IsNullOrEmpty(category)) { return; }

        if (InvokeRequired) {
            try {
                _ = Invoke(new Action(() => Message(category, symbol, message, indent)));
                return;
            } catch {
                return;
            }
        }

        _n--;
        if (_n < 0) { _n = 99999; }

        //var e = $"[{DateTime.Now.ToString7()}] [Ebene {indent + 1}] {category}: {new string(' ', indent * 6)} {message}";

        //lstLog.ItemAdd(ItemOf(e, _n.ToStringInt7()));

        //lstLog.Refresh();

        var r = tblLog.Database?.Row.GenerateAndAdd(_n.ToString(), "New Undo Item");
        if (r == null) { return; }

        if (!string.IsNullOrEmpty(symbol)) { r.CellSet("symbol", symbol + "|16", string.Empty); }
        r.CellSet("Time", DateTime.Now.ToString7(), string.Empty);
        r.CellSet("category", category, string.Empty);
        r.CellSet("message", message, string.Empty);
        r.CellSet("indent", indent, string.Empty);
        //tblLog.Refresh();
    }

    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        Develop.MonitorMessage = null;
        base.OnFormClosing(e);
    }

    protected override void OnShown(System.EventArgs e) {
        base.OnShown(e);
        Develop.MonitorMessage?.Invoke("Global", "Information", "Monitoring gestartet", 0);
    }

    private static void DisposeMonitor() {
        // Breche den aktuellen Thread ab
        try {
            if (_cancellationTokenSource != null) {
                if (!_cancellationTokenSource.IsCancellationRequested) {
                    _cancellationTokenSource.Cancel();
                }
                _cancellationTokenSource.Dispose();
                _cancellationTokenSource = null;
            }
        } catch {
            // Ignoriere Fehler beim Abbrechen
        }

        // Warte eine kurze Zeit, damit der Thread auf die Abbruchanforderung reagieren kann
        if (_monitorThread != null && _monitorThread.IsAlive) {
            try {
                // Kurze Wartezeit für geordnetes Beenden
                if (!_monitorThread.Join(100)) {
                    // Falls der Thread nicht innerhalb der Wartezeit beendet wurde
                    // Setze ihn auf null, damit er beim Garbage Collector landen kann
                }
            } catch {
                // Ignoriere Fehler beim Warten
            }
            _monitorThread = null;
        }

        // Setze den Monitor zurück
        if (Monitor != null) {
            Develop.MonitorMessage = null; // Delegat zurücksetzen
            try {
                Monitor.Dispose();
            } catch {
                // Ignoriere Fehler beim Entsorgen
            }
            Monitor = null;
        }
    }

    private static void StartMonitorInThread(CancellationToken cancellationToken) {
        try {
            // STA-Modus für den Thread festlegen (wichtig für Windows Forms)
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            // Erstelle eine neue Instanz des Monitors
            Monitor = new GlobalMonitor();

            // Registriere die Formularschließung beim CancellationToken
            cancellationToken.Register(() => {
                try {
                    if (Monitor != null && !Monitor.IsDisposed) {
                        Monitor.BeginInvoke(new Action(() => Monitor.Close()));
                    }
                } catch {
                    // Ignoriere Fehler beim Schließen
                }
            });

            // Starte den MessageLoop für das Formular, wenn das Token nicht abgebrochen wurde
            if (!cancellationToken.IsCancellationRequested) {
                System.Windows.Forms.Application.Run(Monitor);
            }
        } catch (Exception ex) {
            // Fehlerbehandlung
            Develop.DebugPrint(ex.Message);
            DisposeMonitor();  // Monitor zurücksetzen, damit ein Neustart möglich ist
        }
    }

    private void btnLeeren_Click(object sender, System.EventArgs e) {
        if (tblLog.Database is { } db) {
            db.Row.Clear("Monitoring-Log geleert");
        }

        Develop.MonitorMessage?.Invoke("Global", "Information", "Monitoring-Log geleert", 0);
    }

    #endregion
}