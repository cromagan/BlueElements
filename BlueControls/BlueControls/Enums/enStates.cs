﻿using System;

namespace BlueControls.Enums
{
    [Flags]
    public enum enStates
    {
        Undefiniert = -1,

        Standard = 0,


        Standard_Disabled = 1,
        Standard_MouseOver = 2,
        Standard_HasFocus = 4,
        Standard_MousePressed = 8,

        Checked = 16,

        Standard_HasFocus_MousePressed = Standard_MousePressed | Standard_HasFocus,
        Standard_MouseOver_HasFocus_MousePressed = Standard_MouseOver | Standard_MousePressed | Standard_HasFocus,
        Standard_MouseOver_HasFocus = Standard_MouseOver | Standard_HasFocus,
        Standard_MouseOver_MousePressed = Standard_MousePressed | Standard_MouseOver,


        Checked_Disabled = Standard_Disabled | Checked,
        Checked_MouseOver = Standard_MouseOver | Checked,
        Checked_HasFocus = Standard_HasFocus | Checked,
        Checked_MousePressed = Standard_MousePressed | Checked,

        Checked_HasFocus_MousePressed = Standard_HasFocus_MousePressed | Checked,
        Checked_MouseOver_HasFocus_MousePressed = Standard_MouseOver_HasFocus_MousePressed | Checked,
        Checked_MouseOver_HasFocus = Standard_MouseOver_HasFocus | Checked,
        Checked_MouseOver_MousePressed = Standard_MouseOver_MousePressed | Checked


        //Checked_Disabled = Checked Or Standard_Disabled
        //Checked_MouseOver = Checked Or Standard_MouseOverx
        //Checked_HasFocus = Checked Or Standard_HasFocus
        //Checked_MousePressed = Checked Or Standard_MousePressed
        //Checked_HasFocus_MousePressed = Standard_HasFocus Or Standard_MousePressed Or Checked


        //Checked_MouseOver_HasFocus = Checked Or Standard_HasFocus Or Standard_MouseOver

        //Checked_MouseOver_HasFocus_MousePressed = Checked Or Standard_MousePressed Or Standard_HasFocus Or Standard_MouseOver
    }
}