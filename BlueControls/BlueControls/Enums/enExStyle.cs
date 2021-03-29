namespace BlueControls.Enums {
    public enum enExStyle {

        // 3x  &H0? Kann nicht sein! Sicherheitshalber auskommentuert!

        EX_ACCEPTFILES = 0x10,
        EX_APPWINDOW = 0x40000,
        EX_CLIENTEDGE = 0x200,
        EX_CONTEXTHELP = 0x400,
        EX_CONTROLPARENT = 0x10000,
        EX_DLGMODALFRAME = 0x1,
        // EX_LEFT = &H0
        EX_LEFTSCROLLBAR = 0x4000,
        //  EX_LTRREADING = &H0
        EX_MDICHILD = 0x40,
        EX_NOACTIVATE = 0x8000000,
        EX_NOPARENTNOTIFY = 0x4,
        EX_OVERLAPPEDWINDOW = 0x300,
        EX_PALETTEWINDOW = 0x188,
        EX_RIGHT = 0x1000,
        // EX_RIGHTSCROLLBAR = &H0
        EX_RTLREADING = 0x2000,
        EX_STATICEDGE = 0x20000,
        EX_TOOLWINDOW = 0x80,
        EX_TOPMOST = 0x8,
        EX_TRANSPARENT = 0x20,
        EX_WINDOWEDGE = 0x100
    }
}