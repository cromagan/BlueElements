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


// http://openbook.galileo-press.de/visualbasic_2008/vb2008_03_klassendesign_015.htm#mjb06b32f7141ae42e9e38c96b77a2b713

// http://www.activevb.de/tutorials/tut_interface/interface.html

// Schnittstellen ansprechen
// http://www.dotnetperls.com/interface-vbnet




// INumerable -> For Each ermöglichen
// http://www.dotnetperls.com/ienumerable-vbnet

// IComparable - Vergleich zweier Objecte
// http://www.dotnetperls.com/icomparable-vbnet

// ISort, ISortable 
// http://www.activevb.de/tutorials/tut_interface/interface.html

//IFormattable


// IConvertible
// -> ToInt, etc


//Public Interface ICompareAble_Extended
//    Inherits IComparable
//    Function CompareKey() As String
//End Interface

namespace BlueControls.Interfaces
{


    public interface IQuickInfo
    {

        string QuickInfo { get; set; }

    }
}