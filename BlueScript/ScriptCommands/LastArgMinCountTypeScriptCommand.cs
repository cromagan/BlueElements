// Licensed under MIT; see License.md for disclaimer, details, and extended user conditions.

namespace BlueScript.ScriptCommands;

public enum LastArgMinCountTypeScriptCommand {

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