namespace BlueBasics.Interfaces
{
    public interface IParseable : IChangedFeedback
    {
        void Parse(string StringToParse);
        string ToString();

        bool IsParsing { get; }
    }
}