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
using BlueScript.Variables;
using System.Collections.Generic;

namespace BlueScript.Structures;

public class ScriptEndedFeedback {

	#region Constructors

	public ScriptEndedFeedback(VariableCollection variables, List<string> protocol, bool needsScriptFix, bool breakFired, bool endscript, string failedReason) {
		Variables = variables;
		GiveItAnotherTry = false;
		Protocol = protocol;

		ProtocolText = GenNiceProtokoll(protocol);
		BreakFired = breakFired;
		EndScript = endscript;

		FailedReason = failedReason;
		NeedsScriptFix = needsScriptFix;
	}

	/// <summary>
	/// Wird ausschließlich verwendet, wenn eine Vorabprüfung scheitert,
	/// und das Skript erst gar nicht gestartet wird.
	/// </summary>
	/// <param name="failedReason"></param>
	/// <param name="giveitanothertry"></param>
	/// <param name="needsScriptFix"></param>
	/// <param name="scriptname"></param>
	public ScriptEndedFeedback(string failedReason, bool giveitanothertry, bool needsScriptFix, string scriptname) {
		Variables = null;

		GiveItAnotherTry = giveitanothertry;

		Protocol = ["[" + scriptname + ", Start abgebrochen]@" + failedReason];
		ProtocolText = GenNiceProtokoll(Protocol);


		FailedReason = "Start abgebrochen: " + failedReason;
		NeedsScriptFix = needsScriptFix;
	}

	/// <summary>
	/// Wird verwendet, wenn ein Script beendet wird, ohne weitere Vorkommnisse
	/// </summary>
	public ScriptEndedFeedback(VariableCollection variables, string failedReason) {
		GiveItAnotherTry = false;

		Protocol = [];
		ProtocolText = string.Empty;

		NeedsScriptFix = false;
		FailedReason = failedReason;
		Variables = variables;
	}

	#endregion

	#region Properties

	public bool BreakFired { get; }
	public bool EndScript { get; }

	public bool GiveItAnotherTry { get; }
	public string FailedReason { get; }
	public List<string> Protocol { get; }
	public string ProtocolText { get; }
	public bool NeedsScriptFix { get; }

	public bool Failed => NeedsScriptFix || !string.IsNullOrWhiteSpace(FailedReason);
	public VariableCollection? Variables { get; }

	#endregion

	#region Methods

	private static string GenNiceProtokoll(IEnumerable<string> protokoll) => "Skript-Protokoll:\r\n\r\n" + protokoll.JoinWith("\r\n\r\n").Replace("]@", "]\r\n");

	#endregion
}