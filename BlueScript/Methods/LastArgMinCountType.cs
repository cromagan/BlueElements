// Ersetzungen:
//
// if (lastArgMinCount < 0 && attributes.Count > types.Count)
//   → if (lastArgMinCount == LastArgMinCountType.ExactlyOnce && attributes.Count > types.Count)
//
// if (attributes.Count < types.Count && lastArgMinCount != 0)
//   → if (attributes.Count < types.Count && lastArgMinCount != LastArgMinCountType.Optional)
//
// if (lastArgMinCount >= 1 && attributes.Count < types.Count + lastArgMinCount - 1)
//   → if (lastArgMinCount == LastArgMinCountType.MinOnce && attributes.Count < types.Count)
//   oder bei >= 2: if (lastArgMinCount == LastArgMinCountType.MinTwice && attributes.Count < types.Count + 1)
//
// public virtual int LastArgMinCount => -1;
//   → public virtual LastArgMinCountType LastArgMinCount => LastArgMinCountType.ExactlyOnce;
//
// public override int LastArgMinCount => 0;
//   → public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.Optional;
//
// public override int LastArgMinCount => 1;
//   → public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinOnce;
//
// public override int LastArgMinCount => 2;
//   → public override LastArgMinCountType LastArgMinCount => LastArgMinCountType.MinTwice;
//
// public static SplittedAttributesFeedback SplitAttributeToVars(..., int lastArgMinCount, ...)
//   → public static SplittedAttributesFeedback SplitAttributeToVars(..., LastArgMinCountType lastArgMinCount, ...)
//
// case -1: → case LastArgMinCountType.ExactlyOnce:
// case 0:  → case LastArgMinCountType.Optional:
// case 1:  → case LastArgMinCountType.MinOnce:
// default:  → case LastArgMinCountType.MinTwice:

namespace BlueScript.Methods;

public enum LastArgMinCountType {
    /// <summary>
    /// Das letzte Argument muss genau 1x vorhanden sein (Standard).
    /// </summary>
    ExactlyOnce = -1,

    /// <summary>
    /// Das letzte Argument darf fehlen oder öfters vorhanden sein.
    /// </summary>
    Optional = 0,

    /// <summary>
    /// Das letzte Argument muss angegeben werden; darf mehrfach wiederholt werden.
    /// </summary>
    MinOnce = 1,

    /// <summary>
    /// Das letzte Argument muss mindestens 2x wiederholt werden.
    /// </summary>
    MinTwice = 2
}
