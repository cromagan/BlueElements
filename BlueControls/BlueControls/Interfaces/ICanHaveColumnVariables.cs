using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.Interfaces
{
    public interface ICanHaveColumnVariables
    {
        bool ParseVariable(string VariableName, enValueType ValueType, string Value);
        bool ParseSpecialCodes();
        bool ResetVariables();
        bool RenameColumn(string oldName, ColumnItem cColumnItem);
    }
}