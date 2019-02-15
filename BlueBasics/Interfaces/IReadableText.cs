namespace BlueBasics.Interfaces
{
    public interface IReadableText : IChangedFeedback
    {
        string ReadableText();
        QuickImage SymbolForReadableText();



    }
}