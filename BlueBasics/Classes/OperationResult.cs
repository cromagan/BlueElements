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

    public readonly string FailedReason;
    public readonly bool IsRetryable;
    public readonly object? Value;

    #endregion

    #region Constructors

    /// <summary>
    /// Alles Ok! Mit eigenen Rückgabewert
    /// </summary>
    /// <param name="returnValue"></param>
    public OperationResult(object? returnValue) : this(returnValue, false, string.Empty) { }

    public OperationResult(bool retry, string failedReason) : this(null, retry, failedReason) {
        if (string.IsNullOrEmpty(FailedReason)) {
            Develop.DebugPrint_NichtImplementiert(true);
        }
    }

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

    public bool IsFailed => !string.IsNullOrEmpty(FailedReason);

    public bool IsSuccessful => string.IsNullOrEmpty(FailedReason);

    #endregion

    #region Methods

    /// <summary>
    /// Signalisiert, dass die Operation fehlgeschlagen ist, mit keiner Aussicht auf Erfolg
    /// </summary>
    public static OperationResult Failed(string failedReason) {
        var t = new OperationResult(null, false, failedReason);

        if (string.IsNullOrEmpty(t.FailedReason)) {
            Develop.DebugPrint_NichtImplementiert(true);
        }

        return t;
    }

    /// <summary>
    /// Signalisiert, dass die Operation fehlgeschlagen ist, mit keiner Aussicht auf Erfolg
    /// </summary>
    public static OperationResult Failed(Exception ex) => new(null, false, ex.Message);

    /// <summary>
    /// Signalisiert, dass die Operation wiederholt werden kann
    /// </summary>
    public static OperationResult FailedRetryable(string failedReason) {
        if (string.IsNullOrEmpty(failedReason)) {
            Develop.DebugPrint_NichtImplementiert(true);
        }

        return new(null, true, failedReason);
    }

    /// <summary>
    /// Signalisiert, dass die Operation wiederholt werden kann
    /// </summary>
    public static OperationResult FailedRetryable(Exception ex) => new(null, true, ex.Message);

    public static OperationResult SuccessValue(object returnValue) => new(returnValue, false, string.Empty);

    #endregion
}