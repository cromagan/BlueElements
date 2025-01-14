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

using System;

namespace BlueControls.Enums;

[Flags]
public enum MarkState {
    None = 0,

    /// <summary>
    /// Bei Rechtschreibfehlern
    /// </summary>
    Ringelchen = 1,

    /// <summary>
    /// Felder im Creativepad
    /// </summary>
    Field = 2,

    /// <summary>
    /// Verknüpfungen, der eigene Name
    /// </summary>
    MyOwn = 4,

    /// <summary>
    /// Verknüpfungen, ein erkannter Link
    /// </summary>
    Other = 8
}