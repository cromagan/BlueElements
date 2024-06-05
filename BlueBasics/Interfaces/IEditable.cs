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

using static BlueBasics.Interfaces.EditableExtension;

namespace BlueBasics.Interfaces;

public interface IEditable {

    #region Properties

    string CaptionForEditor { get; }
    dOpenEditor? OpenEditor { get; }

    #endregion
}

public static class EditableExtension {

    #region Delegates

    public delegate void dOpenEditor(IEditable toEdit);

    #endregion

    #region Methods

    public static void Edit(this IEditable toEdit) {
        if (toEdit == null) { return; }

        toEdit.OpenEditor?.Invoke(toEdit);
    }

    #endregion
}