﻿// Authors:
// Christian Peter
//
// Copyright (c) 2022 Christian Peter
// https://github.com/cromagan/BlueElements
//
// License: GNU Affero General Public License v3.0
// https://github.com/cromagan/BlueElements/blob/master/LICENSE
//
// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL
// THE AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING
// FROM, OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER
// DEALINGS IN THE SOFTWARE.

namespace BlueControls.Enums {

    public enum enWndProc {
        WM_NULL = 0x0,
        WM_CREATE = 0x1,
        WM_DESTROY = 0x2,
        WM_MOVE = 0x3,
        WM_SIZE = 0x5,
        WM_ACTIVATE = 0x6,
        WM_SETFOCUS = 0x7,
        WM_KILLFOCUS = 0x8,
        WM_ENABLE = 0xA,
        WM_SETREDRAW = 0xB,
        WM_SETTEXT = 0xC,
        WM_GETTEXT = 0xD,
        WM_GETTEXTLENGTH = 0xE,
        WM_PAINT = 0xF,
        WM_CLOSE = 0x10,
        WM_QUERYENDSESSION = 0x11,
        WM_QUIT = 0x12,
        WM_QUERYOPEN = 0x13,
        WM_ERASEBKGND = 0x14,
        WM_SYSCOLORCHANGE = 0x15,
        WM_ENDSESSION = 0x16,
        WM_SHOWWINDOW = 0x18,
        WM_CTLCOLOR = 0x19,
        WM_WININICHANGE = 0x1A,
        WM_SETTINGCHANGE = 0x1A,
        WM_DEVMODECHANGE = 0x1B,
        WM_ACTIVATEAPP = 0x1C,
        WM_FONTCHANGE = 0x1D,
        WM_TIMECHANGE = 0x1E,
        WM_CANCELMODE = 0x1F,
        WM_SETCURSOR = 0x20,
        WM_MOUSEACTIVATE = 0x21,
        WM_CHILDACTIVATE = 0x22,
        WM_QUEUESYNC = 0x23,
        WM_GETMINMAXINFO = 0x24,
        WM_PAINTICON = 0x26,
        WM_ICONERASEBKGND = 0x27,
        WM_NEXTDLGCTL = 0x28,
        WM_SPOOLERSTATUS = 0x2A,
        WM_DRAWITEM = 0x2B,
        WM_MEASUREITEM = 0x2C,
        WM_DELETEITEM = 0x2D,
        WM_VKEYTOITEM = 0x2E,
        WM_CHARTOITEM = 0x2F,
        WM_SETFONT = 0x30,
        WM_GETFONT = 0x31,
        WM_SETHOTKEY = 0x32,
        WM_GETHOTKEY = 0x33,
        WM_QUERYDRAGICON = 0x37,
        WM_COMPAREITEM = 0x39,
        WM_GETOBJECT = 0x3D,
        WM_COMPACTING = 0x41,
        WM_COMMNOTIFY = 0x44,
        WM_WINDOWPOSCHANGING = 0x46,
        WM_WINDOWPOSCHANGED = 0x47,
        WM_POWER = 0x48,
        WM_COPYDATA = 0x4A,
        WM_CANCELJOURNAL = 0x4B,
        WM_NOTIFY = 0x4E,
        WM_INPUTLANGCHANGEREQUEST = 0x50,
        WM_INPUTLANGCHANGE = 0x51,
        WM_TCARD = 0x52,
        WM_HELP = 0x53,
        WM_USERCHANGED = 0x54,
        WM_NOTIFYFORMAT = 0x55,
        WM_CONTEXTMENU = 0x7B,
        WM_STYLECHANGING = 0x7C,
        WM_STYLECHANGED = 0x7D,
        WM_DISPLAYCHANGE = 0x7E,
        WM_GETICON = 0x7F,
        WM_SETICON = 0x80,
        WM_NCCREATE = 0x81,
        WM_NCDESTROY = 0x82,
        WM_NCCALCSIZE = 0x83,
        WM_NCHITTEST = 0x84,
        WM_NCPAINT = 0x85,
        WM_NCACTIVATE = 0x86,
        WM_GETDLGCODE = 0x87,
        WM_SYNCPAINT = 0x88,
        WM_NCMOUSEMOVE = 0xA0,
        WM_NCLBUTTONDOWN = 0xA1,
        WM_NCLBUTTONUP = 0xA2,
        WM_NCLBUTTONDBLCLK = 0xA3,
        WM_NCRBUTTONDOWN = 0xA4,
        WM_NCRBUTTONUP = 0xA5,
        WM_NCRBUTTONDBLCLK = 0xA6,
        WM_NCMBUTTONDOWN = 0xA7,
        WM_NCMBUTTONUP = 0xA8,
        WM_NCMBUTTONDBLCLK = 0xA9,
        WM_KEYDOWN = 0x100,
        WM_KEYUP = 0x101,
        WM_CHAR = 0x102,
        WM_DEADCHAR = 0x103,
        WM_SYSKEYDOWN = 0x104,
        WM_SYSKEYUP = 0x105,
        WM_SYSCHAR = 0x106,
        WM_SYSDEADCHAR = 0x107,
        WM_KEYLAST = 0x108,
        WM_IME_STARTCOMPOSITION = 0x10D,
        WM_IME_ENDCOMPOSITION = 0x10E,
        WM_IME_COMPOSITION = 0x10F,
        WM_IME_KEYLAST = 0x10F,
        WM_INITDIALOG = 0x110,
        WM_COMMAND = 0x111,
        WM_SYSCOMMAND = 0x112,
        WM_TIMER = 0x113,
        WM_HSCROLL = 0x114,
        WM_VSCROLL = 0x115,
        WM_INITMENU = 0x116,
        WM_INITMENUPOPUP = 0x117,
        WM_MENUSELECT = 0x11F,
        WM_MENUCHAR = 0x120,
        WM_ENTERIDLE = 0x121,
        WM_MENURBUTTONUP = 0x122,
        WM_MENUDRAG = 0x123,
        WM_MENUGETOBJECT = 0x124,
        WM_UNINITMENUPOPUP = 0x125,
        WM_MENUCOMMAND = 0x126,
        WM_CTLCOLORMSGBOX = 0x132,
        WM_CTLCOLOREDIT = 0x133,
        WM_CTLCOLORLISTBOX = 0x134,
        WM_CTLCOLORBTN = 0x135,
        WM_CTLCOLORDLG = 0x136,
        WM_CTLCOLORSCROLLBAR = 0x137,
        WM_CTLCOLORSTATIC = 0x138,
        WM_MOUSEMOVE = 0x200,
        WM_LBUTTONDOWN = 0x201,
        WM_LBUTTONUP = 0x202,
        WM_LBUTTONDBLCLK = 0x203,
        WM_RBUTTONDOWN = 0x204,
        WM_RBUTTONUP = 0x205,
        WM_RBUTTONDBLCLK = 0x206,
        WM_MBUTTONDOWN = 0x207,
        WM_MBUTTONUP = 0x208,
        WM_MBUTTONDBLCLK = 0x209,
        WM_MOUSEWHEEL = 0x20A,
        WM_PARENTNOTIFY = 0x210,
        WM_ENTERMENULOOP = 0x211,
        WM_EXITMENULOOP = 0x212,
        WM_NEXTMENU = 0x213,
        WM_SIZING = 0x214,
        WM_CAPTURECHANGED = 0x215,
        WM_MOVING = 0x216,
        WM_DEVICECHANGE = 0x219,
        WM_MDICREATE = 0x220,
        WM_MDIDESTROY = 0x221,
        WM_MDIACTIVATE = 0x222,
        WM_MDIRESTORE = 0x223,
        WM_MDINEXT = 0x224,
        WM_MDIMAXIMIZE = 0x225,
        WM_MDITILE = 0x226,
        WM_MDICASCADE = 0x227,
        WM_MDIICONARRANGE = 0x228,
        WM_MDIGETACTIVE = 0x229,
        WM_MDISETMENU = 0x230,
        WM_ENTERSIZEMOVE = 0x231,
        WM_EXITSIZEMOVE = 0x232,
        WM_DROPFILES = 0x233,
        WM_MDIREFRESHMENU = 0x234,
        WM_IME_SETCONTEXT = 0x281,
        WM_IME_NOTIFY = 0x282,
        WM_IME_CONTROL = 0x283,
        WM_IME_COMPOSITIONFULL = 0x284,
        WM_IME_SELECT = 0x285,
        WM_IME_CHAR = 0x286,
        WM_IME_REQUEST = 0x288,
        WM_IME_KEYDOWN = 0x290,
        WM_IME_KEYUP = 0x291,
        WM_MOUSEHOVER = 0x2A1,
        WM_MOUSELEAVE = 0x2A3,
        WM_CUT = 0x300,
        WM_COPY = 0x301,
        WM_PASTE = 0x302,
        WM_CLEAR = 0x303,
        WM_UNDO = 0x304,
        WM_RENDERFORMAT = 0x305,
        WM_RENDERALLFORMATS = 0x306,
        WM_DESTROYCLIPBOARD = 0x307,
        WM_DRAWCLIPBOARD = 0x308,
        WM_PAINTCLIPBOARD = 0x309,
        WM_VSCROLLCLIPBOARD = 0x30A,
        WM_SIZECLIPBOARD = 0x30B,
        WM_ASKCBFORMATNAME = 0x30C,
        WM_CHANGECBCHAIN = 0x30D,
        WM_HSCROLLCLIPBOARD = 0x30E,
        WM_QUERYNEWPALETTE = 0x30F,
        WM_PALETTEISCHANGING = 0x310,
        WM_PALETTECHANGED = 0x311,
        WM_HOTKEY = 0x312,
        WM_PRINT = 0x317,
        WM_PRINTCLIENT = 0x318,
        WM_HANDHELDFIRST = 0x358,
        WM_HANDHELDLAST = 0x35F,
        WM_AFXFIRST = 0x360,
        WM_AFXLAST = 0x37F,
        WM_PENWINFIRST = 0x380,
        WM_PENWINLAST = 0x38F,
        WM_APP = 0x8000,
        WM_USER = 0x400,
        WM_REFLECT = WM_USER + 0x1C00,
        TCM_FIRST = 0x1300,
        TCM_HITTEST = TCM_FIRST + 13,
        TCM_SETPADDING = TCM_FIRST + 43
    }
}