// Authors:
// Christian Peter
//
// Copyright © 2026 Christian Peter
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

using BlueBasics.ClassesStatic;
using System;

namespace BlueBasics.Classes;

/// <summary>
/// Repräsentiert das Ergebnis einer Operation mit optionalem Rückgabewert und Fehlerinformationen.
/// </summary>
public readonly struct OperationResult {

    #region Fields

    /// <summary>
    /// Fehlgeschlagen, mit Meldung: Interner Programmfehler, Admin verständigen
    /// </summary>
    public static readonly OperationResult FailedInternalError = new(null, false, "Interner Programmfehler, Admin verständigen");

    /// <summary>
    /// Erfolgreiche Operation ohne Rückgabewert
    /// </summary>
    public static readonly OperationResult Success = new(null, false, string.Empty);

    /// <summary>
    /// Erfolgreiche Operation mit Rückgabewert 'false'
    /// </summary>
    public static readonly OperationResult SuccessFalse = new(false, false, string.Empty);

    /// <summary>
    /// Erfolgreiche Operation mit Rückgabewert 'true'
    /// </summary>
    public static readonly OperationResult SuccessTrue = new(true, false, string.Empty);

    /// <summary>
    /// Die Fehlermeldung, falls die Operation fehlgeschlagen ist.
    /// </summary>
    public readonly string FailedReason;

    /// <summary>
    /// Gibt an, ob die fehlgeschlagene Operation wiederholt werden kann.
    /// </summary>
    public readonly bool IsRetryable;

    /// <summary>
    /// Der optionale Rückgabewert der Operation.
    /// </summary>
    public readonly object? Value;

    #endregion

    #region Constructors

    /// <summary>
    /// Erstellt eine erfolgreiche Operation mit eigenem Rückgabewert.
    /// </summary>
    /// <param name="returnValue">Der Rückgabewert der erfolgreichen Operation.</param>
    public OperationResult(object? returnValue) : this(returnValue, false, string.Empty) { }

    /// <summary>
    /// Erstellt eine fehlgeschlagene Operation mit Fehlerinformationen.
    /// </summary>
    /// <param name="retry">Gibt an, ob die Operation wiederholt werden kann.</param>
    /// <param name="failedReason">Die Fehlermeldung oder ein leerer String.</param>
    public OperationResult(bool retry, string failedReason) : this(null, retry, failedReason) { }

    /// <summary>
    /// Privater Konstruktor für interne Initialisierung.
    /// </summary>
    /// <param name="returnValue">Der optionale Rückgabewert.</param>
    /// <param name="retry">Gibt an, ob die Operation wiederholt werden kann.</param>
    /// <param name="failedReason">Die Fehlermeldung oder ein leerer String.</param>
    private OperationResult(object? returnValue, bool retry, string failedReason) {
        Value = returnValue;
        IsRetryable = retry;
        FailedReason = failedReason;

        if (retry && string.IsNullOrEmpty(FailedReason)) {
            Develop.DebugPrint_NichtImplementiert(true);
        }
    }

    #endregion

    #region Properties

    /// <summary>
    /// Gibt an, ob die Operation fehlgeschlagen ist.
    /// </summary>
    public bool IsFailed => !string.IsNullOrEmpty(FailedReason);

    /// <summary>
    /// Gibt an, ob die Operation erfolgreich war.
    /// </summary>
    public bool IsSuccessful => string.IsNullOrEmpty(FailedReason);

    #endregion

    #region Methods

    /// <summary>
    /// Erstellt eine fehlgeschlagene Operation, die nicht wiederholt werden kann.
    /// </summary>
    /// <param name="failedReason">Die Fehlermeldung.</param>
    /// <returns>Eine fehlgeschlagene OperationResult.</returns>
    public static OperationResult Failed(string failedReason) {
        var t = new OperationResult(null, false, failedReason);

        if (string.IsNullOrEmpty(t.FailedReason)) {
            Develop.DebugPrint_NichtImplementiert(true);
        }

        return t;
    }

    /// <summary>
    /// Erstellt eine fehlgeschlagene Operation aus einer Exception.
    /// </summary>
    /// <param name="ex">Die Exception mit der Fehlermeldung.</param>
    /// <returns>Eine fehlgeschlagene OperationResult.</returns>
    public static OperationResult Failed(Exception ex) => new(null, false, ex.Message);

    /// <summary>
    /// Erstellt eine fehlgeschlagene Operation, die wiederholt werden kann.
    /// </summary>
    /// <param name="failedReason">Die Fehlermeldung.</param>
    /// <returns>Eine wiederholbare fehlgeschlagene OperationResult.</returns>
    public static OperationResult FailedRetryable(string failedReason) {
        if (string.IsNullOrEmpty(failedReason)) {
            Develop.DebugPrint_NichtImplementiert(true);
        }

        return new(null, true, failedReason);
    }

    /// <summary>
    /// Erstellt eine wiederholbare fehlgeschlagene Operation aus einer Exception.
    /// </summary>
    /// <param name="ex">Die Exception mit der Fehlermeldung.</param>
    /// <returns>Eine wiederholbare fehlgeschlagene OperationResult.</returns>
    public static OperationResult FailedRetryable(Exception ex) => new(null, true, ex.Message);

    /// <summary>
    /// Erstellt eine erfolgreiche Operation mit einem benutzerdefinierten Rückgabewert.
    /// </summary>
    /// <param name="returnValue">Der Rückgabewert der erfolgreichen Operation.</param>
    /// <returns>Eine erfolgreiche OperationResult mit dem angegebenen Wert.</returns>
    public static OperationResult SuccessValue(object returnValue) => new(returnValue, false, string.Empty);

    #endregion
}