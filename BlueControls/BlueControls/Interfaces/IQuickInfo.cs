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