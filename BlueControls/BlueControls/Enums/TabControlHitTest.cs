namespace BlueControls.Enums {
    public enum TabControlHitTest {
        TCHT_NOWHERE = 1,
        TCHT_ONITEMICON = 2,
        TCHT_ONITEMLABEL = 4,
        TCHT_ONITEM = TCHT_ONITEMICON | TCHT_ONITEMLABEL
    }
}