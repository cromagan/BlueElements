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
using BlueBasics.Enums;
using BlueBasics.Interfaces;

namespace BlueBasics.EventArgs;

public class MessageEventArgs : System.EventArgs, IReadableTextWithKey {

    #region Fields

    private readonly DateTime _time = DateTime.Now;

    #endregion

    #region Constructors

    public MessageEventArgs(FehlerArt type, string message) {
        Message = message;
        Type = type;
    }

    #endregion

    #region Properties

    public string KeyName => Generic.GetUniqueKey();
    public string Message { get; }
    public string QuickInfo => Message;
    public FehlerArt Type { get; }

    #endregion

    #region Methods

    public string ReadableText() => $"[{_time.ToString4()}]  {Message}";

    public QuickImage? SymbolForReadableText() {
        switch (Type) {
            case FehlerArt.Warnung:
                return QuickImage.Get(ImageCode.Warnung, 16);

            case FehlerArt.Fehler:
                return QuickImage.Get(ImageCode.Kreis, 16);

            default:
                return null;
        }
    }

    #endregion
}