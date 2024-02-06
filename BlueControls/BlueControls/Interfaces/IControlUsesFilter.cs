// Authors:
// Christian Peter
//
// Copyright (c) 2024 Christian Peter
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

using BlueBasics.Interfaces;
using BlueDatabase;

namespace BlueControls.Interfaces;

public interface IControlUsesFilter : IDisposableExtendedWithEvent, IControlAcceptFilter {

    #region Methods

    public void HandleFilterInputNow();

    /// <summary>
    /// Wird ausgelöst, wenn eine relevante Änderung der eingehenen Filter(Daten) erfolgt ist.
    /// Hier können die neuen temporären Filter(Daten) (FilterInput) berechnet werden und sollten auch angezeigt werden und ein Invalidate gesetzt werden
    /// Events können gekoppelt werden
    /// </summary>
    public void ParentFilterOutput_Changed();

    #endregion
}

public static class IControlUsesFilterExtension {

    #region Methods

    /// <summary>
    /// Verwirft den aktuellen InputFilter und erstellt einen neuen von allen Parents
    /// </summary>
    /// <param name="item"></param>
    /// <param name="mustbeDatabase"></param>
    /// <param name="doEmptyFilterToo"></param>
    public static void DoInputFilter(this IControlAcceptFilter item, Database? mustbeDatabase, bool doEmptyFilterToo) {
        if (item.IsDisposed) { return; }
        //if (item.FilterManualSeted) { return; }

        item.Invalidate_FilterInput();

        item.FilterInput.ChangeTo(item.GetInputFilter(mustbeDatabase, doEmptyFilterToo));
    }

    #endregion
}