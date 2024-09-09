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

using BlueBasics.Enums;
using BlueBasics.Interfaces;
using System;

namespace BlueBasics.EventArgs;

public class MessageEventArgs : System.EventArgs, IReadableTextWithKey {

    #region Constructors

    public MessageEventArgs(FehlerArt type, string message) {
        Message = message;
        Type = type;
        WrittenToLogifile = false;
        Shown = false;
        Time = DateTime.Now;
    }

    #endregion

    #region Properties
    public DateTime Time { get; }
    public string Message { get; }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public bool Shown { get; set; }

    public FehlerArt Type { get; }

    // ReSharper disable once AutoPropertyCanBeMadeGetOnly.Global
    public bool WrittenToLogifile { get; set; }

    public string QuickInfo => Message;

    public string KeyName => Generic.GetUniqueKey();

    public string ReadableText() => $"[{Time.ToString4()}]  {Message}";

    public QuickImage? SymbolForReadableText() {
        
        switch (Type) {

            case FehlerArt.Warnung: return QuickImage.Get(ImageCode.Warnung, 16);
            case FehlerArt.Fehler: return QuickImage.Get(ImageCode.Kreis, 16);
            default: return null;
        }



    }

    #endregion
}