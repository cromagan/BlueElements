﻿// Authors:
// Christian Peter
//
// Copyright (c) 2021 Christian Peter
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

public struct strDoItWithEndedPosFeedback {

    #region Fields

    public string ErrorMessage;

    public int Position;

    public string Value;

    #endregion

    #region Constructors

    public strDoItWithEndedPosFeedback(string errormessage, string value, int endpos) {
        ErrorMessage = errormessage;
        Value = value;
        Position = endpos;
    }

    public strDoItWithEndedPosFeedback(string errormessage) {
        Position = -1;
        ErrorMessage = errormessage;
        Value = null;
    }

    #endregion
}