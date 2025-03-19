// Authors:
// Christian Peter
//
// Copyright (c) 2025 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

using BlueBasics;
using BlueControls.Forms;
using System;
using System.Threading;
using static BlueBasics.Extensions;
using static BlueControls.ItemCollectionList.AbstractListItemExtension;

#nullable enable

namespace BlueControls;

public partial class GlobalMonitor : Form {

    #region Fields

    private static Thread? _monitorThread;
    private static GlobalMonitor? Monitor;
    private int _n = 0;

    #endregion

    #region Constructors

    public GlobalMonitor() {
        InitializeComponent();

        Develop.MonitorMessage = Message;
    }

    #endregion

    #region Methods

    public void Message(string category, string symbol, string message, int indent) {
        if (Disposing || IsDisposed) { return; }

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

        var e = $"[{DateTime.Now.ToString7()}] [Ebene {indent + 1}] {category}: {new string(' ', indent * 6)} {message}";

        lstLog.ItemAdd(ItemOf(e, _n.ToStringInt7()));

        lstLog.Refresh();
    }

    internal static void Start() {
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
        _monitorThread = new Thread(new ThreadStart(StartMonitorInThread));
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

    protected override void OnFormClosing(System.Windows.Forms.FormClosingEventArgs e) {
        Develop.MonitorMessage = null;
        base.OnFormClosing(e);
    }

    protected override void OnShown(System.EventArgs e) {
        base.OnShown(e);
        Develop.MonitorMessage?.Invoke("Global", "Information", "Monitoring gestartet", 0);
    }

    private static void DisposeMonitor() {
        if (Monitor != null) {
            Develop.MonitorMessage = null; // Delegat zurücksetzen
            Monitor.Dispose();
            Monitor = null;
        }
    }

    private static void StartMonitorInThread() {
        try {
            // STA-Modus für den Thread festlegen (wichtig für Windows Forms)
            System.Windows.Forms.Application.EnableVisualStyles();
            System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

            // Erstelle eine neue Instanz des Monitors
            Monitor = new GlobalMonitor();

            // Starte den MessageLoop für das Formular
            System.Windows.Forms.Application.Run(Monitor);
        } catch (Exception ex) {
            // Fehlerbehandlung
            Develop.DebugPrint(ex.Message);
            DisposeMonitor();            // Monitor zurücksetzen, damit ein Neustart möglich ist
        }
    }

    private void btnFilterDel_Click(object sender, System.EventArgs e) => txbFilter.Text = string.Empty;

    private void btnLeeren_Click(object sender, System.EventArgs e) {
        lstLog.ItemClear();
        Develop.MonitorMessage?.Invoke("Global", "Information", "Monitoring-Log geleert", 0);
    }

    private void txbFilter_TextChanged(object sender, System.EventArgs e) {
        lstLog.FilterText = txbFilter.Text;
        btnFilterDel.Enabled = Enabled && !string.IsNullOrEmpty(txbFilter.Text);
    }

    #endregion
}