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

namespace BlueBasics.Interfaces;

public static class ParseableExtension {

    #region Methods

    public static void Parse(this IParseable parsable, string toParse) {
        foreach (var pair in toParse.GetAllTags()) {
            var i = parsable.ParseThis(pair.Key.ToLowerInvariant(), pair.Value);

            if (!i) {
                Develop.DebugPrint(FehlerArt.Warnung, "Kann nicht geparsed werden: " + pair.Key + "/" + pair.Value + "/" + toParse);
            }
        }
        parsable.ParseFinished(toParse);
    }

    #endregion
}

public interface IParseable : IStringable {

    #region Methods

    public void ParseFinished(string parsed);

    bool ParseThis(string key, string value);

    #endregion
}