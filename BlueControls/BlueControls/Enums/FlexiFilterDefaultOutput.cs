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

using BlueDatabase.Enums;

namespace BlueControls.Enums;

public enum FlexiFilterDefaultOutput {

    /// <summary>
    /// Wir nichts eingegeben, wird kein Filter zurück gegeben.
    /// Sozusagen ist das das ein AlwaysTrue Filter
    /// </summary>
    Alles_Anzeigen = 0,

    /// <summary>
    /// Wird nichts eingegeben wird keine Zeile gezeigt. Es muss also was eingegeben werden
    /// </summary>
    Nichts_Anzeigen = FilterType.AlwaysFalse
}