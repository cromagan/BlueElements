// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
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

namespace BlueControls.Enums {

    public enum AsciiKey {
        Undefined = -1,
        LineFeed = 10,

        // VerticalTab = 11
        //FormFeed = 12
        //CR = 13
        //ESC = 27
        //FileSeparator = 28
        //GroupSeparator = 29
        //RecordSeparator = 30
        //UnitSeparator = 31
        ENTER = 13,

        ESC = 27,
        DEL = 127, // [ENTF]
        StrgF = 6,
        BackSpace = 8,
        TAB = 9,
        StrgC = 3,
        StrgV = 22,
        StrgX = 24,
        StrgA = 1,
        Space = 32,

        //Chr255 = 255,
        Euro = 8364

        //ImageStart = 20000
    }
}