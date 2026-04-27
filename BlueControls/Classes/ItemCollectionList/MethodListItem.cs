using BlueScript.Methods;
using System;

namespace BlueControls.Classes.ItemCollectionList;

public class MethodListItem : TextListItem {

    #region Fields

    private readonly Type _methodType;

    #endregion

    #region Constructors

    public MethodListItem(Type methodType, bool enabled) : base(Method.GetSyntax(methodType), Method.GetPropertyValue<string>(methodType, nameof(Method.Command), string.Empty), null, false, enabled, Method.GetHintText(methodType), string.Empty) => _methodType = methodType;

    #endregion

    #region Properties

    public Type MethodType => _methodType;

    #endregion

    #region Methods

    public string HintText() => Method.GetHintText(_methodType);

    #endregion
}