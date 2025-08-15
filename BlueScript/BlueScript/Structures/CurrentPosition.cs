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

using static BlueBasics.Extensions;

#nullable enable

namespace BlueScript.Structures;

public class CurrentPosition {

    #region Constructors

    public CurrentPosition() : this("Main", 0, string.Empty, string.Empty, string.Empty, false) { }

    public CurrentPosition(CurrentPosition cp, int position) : this(cp.Subname, position, cp.Protocol, cp.Chain, cp.FailedReason, cp.NeedsScriptFix) { }

    public CurrentPosition(CurrentPosition cp, string subname, int position) : this(subname, position, cp.Protocol, cp.Chain, cp.FailedReason, cp.NeedsScriptFix) { }

    public CurrentPosition(CurrentPosition? cp) {
        Subname = cp?.Subname ?? "Main";
        Position = cp?.Position ?? 0;
        Protocol = cp?.Protocol ?? string.Empty;
        Chain = cp?.Chain ?? string.Empty;
        FailedReason = cp?.FailedReason ?? string.Empty;
        NeedsScriptFix = cp?.NeedsScriptFix ?? false;
    }

    public CurrentPosition(string subname, int position, string protocol, string chain, string failedReason, bool needsScriptFix) {
        Subname = subname;
        Position = position;
        Protocol = protocol;
        Chain = chain;
        FailedReason = failedReason;
        NeedsScriptFix = needsScriptFix;
    }

    public CurrentPosition(CurrentPosition? cp, string failedreason, bool needsScriptFix) : this(cp) {
        FailedReason = failedreason;
        NeedsScriptFix = needsScriptFix;

        if (!string.IsNullOrEmpty(failedreason)) {
            Protocol = failedreason + "\r\n" + Protocol;
        }
    }

    #endregion

    #region Properties

    public string Chain { get; } = string.Empty;

    public bool Failed => NeedsScriptFix || !string.IsNullOrWhiteSpace(FailedReason);

    /// <summary>
    /// Gibt empty zurück, wenn der Befehl ausgeführt werden kann.
    /// Ansonsten den Grund, warum er nicht ausgeführt werden kann.
    /// Nur in Zusammenhang mit NeedsScriptFix zu benutzen, weil hier auch einfach die Meldung sein kann, dass der Befehl nicht erkannt wurde - was an sich kein Fehler ist.
    /// </summary>
    public string FailedReason { get; }

    /// <summary>
    /// TRUE, wenn der Befehl erkannt wurde, aber nicht ausgeführt werden kann.
    /// </summary>
    public bool NeedsScriptFix { get; }

    public int Position { get; } = -1;
    public string Protocol { get; } = string.Empty;
    public int Stufe => Chain.CountChar('\\', null);
    public string Subname { get; } = string.Empty;

    #endregion

    #region Methods

    public int Line(string normalizedScriptText) => normalizedScriptText.CountChar('¶', Position) + 1;

    #endregion
}