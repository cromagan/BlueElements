using BlueDatabase;
using BlueDatabase.Enums;

namespace BlueControls.Interfaces
{
    public interface ICanHaveColumnVariables
    {
        bool ReplaceVariable(string VariableName, enValueType ValueType, string Value);
        bool DoSpecialCodes();
        bool ResetVariables();
        bool RenameColumn(string oldName, ColumnItem cColumnItem);
    }
}