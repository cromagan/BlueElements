namespace BlueBasics.Interfaces
{
    public interface ICheckable
    {
        bool IsOk();
        string ErrorReason();
    }
}