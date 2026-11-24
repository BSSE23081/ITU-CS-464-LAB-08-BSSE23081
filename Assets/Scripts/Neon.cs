using UnityEngine;

// Runtime copy of the palette so gameplay scripts use the same colours as the menu.
public static class Neon
{
    public static readonly Color BG = Hex("05050F"), Cyan = Hex("19E6FF"), Pink = Hex("FF2D8A"),
        Gold = Hex("FFC933"), Green = Hex("2EE89A"), Red = Hex("FF3358"), White = Hex("EDEDF7"),
        Grey = Hex("9A9AB0"), Dim = Hex("3A3A52");

    static Color Hex(string h) { Color c; ColorUtility.TryParseHtmlString("#" + h, out c); return c; }
}
