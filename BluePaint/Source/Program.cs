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

#nullable enable

using BlueBasics;
using BlueBasics.Enums;
using System;
using System.Windows.Forms;

namespace BluePaint;

internal static class Program {

    #region Methods

    private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e) {
        try {
            var ex = (Exception)e.ExceptionObject;
            Develop.DebugPrint(ErrorType.Error, "Allgemeiner unbehandelter Fehler unbekannter Herkunft", ex);
        } catch { }
        Develop.AbortExe(true);
    }

    /// <summary>
    /// Der Haupteinstiegspunkt für die Anwendung.
    /// </summary>
    [STAThread]
    private static void Main() {
        Develop.StartService();

        var currentDomain = AppDomain.CurrentDomain;
        currentDomain.UnhandledException += CurrentDomain_UnhandledException;

        // ProcessExit Handler - wird beim Beenden aufgerufen
        currentDomain.ProcessExit += (sender, e) => {
            System.Diagnostics.Debugger.Break(); // Breakpoint hier
            System.Diagnostics.Debug.WriteLine("ProcessExit wurde aufgerufen!");
            System.Diagnostics.Debug.WriteLine(Environment.StackTrace);
        };

        //CultureInfo culture = new("de-DE");
        //CultureInfo.DefaultThreadCurrentCulture = culture;
        //CultureInfo.DefaultThreadCurrentUICulture = culture;
        //System.Windows.Forms.Application.EnableVisualStyles();
        //System.Windows.Forms.Application.SetCompatibleTextRenderingDefault(false);

        Application.Run(new MainWindow(true));
    }

    #endregion
}