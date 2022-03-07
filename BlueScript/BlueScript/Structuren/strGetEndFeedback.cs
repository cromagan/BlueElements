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

namespace BlueScript.Structuren
{
    public struct strGetEndFeedback {

        #region Fields

        public string AttributeText;

        //public strGetEndFeedback(string errormessage, int continuePosition, string attributetext) {
        //    ContinuePosition = continuePosition;
        //    ErrorMessage = errormessage;
        //    AttributeText = attributetext;
        //}
        public int ContinuePosition;

        public string ErrorMessage;

        #endregion

        #region Constructors

        public strGetEndFeedback(string errormessage) {
            ContinuePosition = 0;
            ErrorMessage = errormessage;
            AttributeText = string.Empty;
            //ComandText = string.Empty;
        }

        public strGetEndFeedback(int continuePosition, string attributetext) {
            ContinuePosition = continuePosition;
            ErrorMessage = string.Empty;
            AttributeText = attributetext;
        }

        #endregion
    }
}