﻿#region BlueElements - a collection of useful tools, database and controls
// Authors: 
// Christian Peter
// 
// Copyright (c) 2020 Christian Peter
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
#endregion


namespace BlueControls.Enums {

    public enum enAddType {

        /// <summary>
        /// Add-Button wird nicht angezeigt, und auch niemals das Add-Ereignis ausgelöst.
        /// </summary>
        None = 0,

        /// <summary>
        /// Add-Button wird angezeigt, und auf einen Klick dessen wird das Add-Ereignis ausgelöst und dann ein Item mittels einer Input-Box erstellt.
        /// </summary>
        Text = 1,
        /// <summary>
        /// Add-Button wird angezeigt, und auf einen Klick dessen wird das Add-Ereignis ausgelös und dann ein Item mittels einer File-Selcet-Box erstellt. Die Original-Dateien werden nicht verändert.
        /// </summary>
        BinarysFromFileSystem = 2,

        /// <summary>
        /// Add-Button wird angezeigt, und auf einen Klick dessen wird das Add-Ereignis ausgelöst und dann ein Item mittels einer List-Box erstellt.
        /// </summary>
        OnlySuggests = 3,

        /// <summary>
        /// Add-Button wird angezeigt, und auf einen Klick dessen wird das Add-Ereignis ausgelöst und sonst nichts gemacht.
        /// </summary>
        UserDef = 4

    }
}