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

using BlueBasics.Enums;
using System;

namespace BlueControls;

[AttributeUsage(AttributeTargets.Class)]
public class StandaloneInfo : Attribute {

    #region Constructors

    public StandaloneInfo(string name, ImageCode symbol, string kategorie, int sort) {
        Name = name;
        Symbol = symbol;
        Kategorie = kategorie;
        Sort = sort;
    }

    #endregion

    #region Properties

    public string Kategorie { get; }
    public string Name { get; }
    public int Sort { get; }
    public ImageCode Symbol { get; }

    #endregion
}