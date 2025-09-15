﻿using BlueBasics;
using BlueBasics.Enums;
using BlueControls.CellRenderer;
using BlueControls.Controls;
using BlueControls.Forms;
using BlueTable;
using System;
using System.Threading;
using static BlueBasics.Extensions;

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
        GenerateLogTable(tblLog);

        Develop.Message += Message;
    }

    #endregion

    #region Methods

    public static void GenerateLogTable(Controls.TableView tblLog) {
        //    public void Message(string category, string symbol, string message, int indent) {
        BlueTable.Table db = new(Table.UniqueKeyValue()) {
            LogUndo = false,
            DropMessages = false
        };
        var dbi = db.Column.GenerateAndAdd("ID", "ID", ColumnFormatHolder.Text);
        dbi.IsFirst = true;
        _ = db.Column.GenerateAndAdd("Symbol", "Symbol", ColumnFormatHolder.BildCode);
        var az = db.Column.GenerateAndAdd("Zeit", "Zeit", ColumnFormatHolder.DateTime);
        _ = db.Column.GenerateAndAdd("category", "Kategorie", ColumnFormatHolder.Text);
        _ = db.Column.GenerateAndAdd("Message", "Message", ColumnFormatHolder.Text);
        _ = db.Column.GenerateAndAdd("Indent", "Stufe", ColumnFormatHolder.Long);

        foreach (var thisColumn in db.Column) {
            if (!thisColumn.IsSystemColumn()) {
                thisColumn.MultiLine = true;
                thisColumn.EditableWithTextInput = false;
                thisColumn.EditableWithDropdown = false;
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
        tcvc[1].ShowColumns("Symbol", "Zeit", "category", "indent", "Message");

        db.ColumnArrangements = tcvc.ToString(false);

        tblLog.TableSet(db, string.Empty);
        tblLog.Arrangement = string.Empty;
        tblLog.SortDefinitionTemporary = new RowSortDefinition(db, az, true);
    }

    public static void Start() {
        // Prüfe, ob Thread und Monitor bereits funktionieren
        if (_monitorThread != null && _monitorThread.IsAlive && Monitor != null && !Monitor.IsDisposed) {
            // Thread läuft und Fenster existiert, bringe es in den Vordergrund
            try {
                _ = Monitor.BeginInvoke(new Action(() => {
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

        _monitorThread = new Thread(() => StartMonitorInThread(_cancellationTokenSource.Token)) {
            IsBackground = true
        };
        _monitorThread.SetApartmentState(ApartmentState.STA);
        _monitorThread.Start();

        // Warte kurz, bis das Fenster erstellt wurde
        var attempts = 0;
        while (Monitor == null && attempts < 50) {
            Thread.Sleep(10);
            attempts++;
        }
    }

    public void Message(ErrorType type, object? reference, string category, ImageCode symbol, string message, int indent) {
        if (Disposing || IsDisposed) { return; }

        if (string.IsNullOrEmpty(category)) { return; }

        if (InvokeRequired) {
            try {
                _ = Invoke(new Action(() => Message(type, reference, category, symbol, message, indent)));
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

        var r = tblLog.Table?.Row.GenerateAndAdd(_n.ToString(), "New Undo Item");
        if (r == null) { return; }

        r.CellSet("symbol", symbol.ToString() + "|16", string.Empty);

        r.CellSet("Zeit", DateTime.Now.ToString7(), string.Empty);
        r.CellSet("category", category, string.Empty);
        r.CellSet("message", message, string.Empty);
        r.CellSet("indent", indent, string.Empty);
        //tblLog.Refresh();
    }
    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        Develop.Message -= Message;
        base.OnFormClosing(e);
    }


    protected override void OnShown(System.EventArgs e) {
        base.OnShown(e);
        Develop.Message?.Invoke(ErrorType.Info, this, "Global", ImageCode.Information, "Monitoring gestartet", 0);
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
            _ = cancellationToken.Register(() => {
                try {
                    if (Monitor != null && !Monitor.IsDisposed) {
                        _ = Monitor.BeginInvoke(new Action(() => Monitor.Close()));
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
        if (tblLog.Table is { IsDisposed: false } db) {
            _ = db.Row.Clear("Monitoring-Log geleert");
        }

        Develop.Message?.Invoke(ErrorType.Info, this, "Global", ImageCode.Information, "Monitoring-Log geleert", 0);
    }

    #endregion
}