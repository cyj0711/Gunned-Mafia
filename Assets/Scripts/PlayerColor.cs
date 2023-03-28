using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_PlayerColor
{
    Red, Blue, Green,
    Pink, Orange, Yellow,
    Black, White, Purple,
    Brown, Cyan, Lime
}

public class PlayerColor
{
    private static List<Color> colors = new List<Color>()
    {
        new Color(1f, 0f, 0f),
        new Color(0.1f, 0.1f, 1f),
        new Color(0f, 0.6f, 0f),
        new Color(1f, 0.3f, 0.9f),
        new Color(1f, 0.4f, 0f),
        new Color(1f, 0.9f, 0.1f),
        new Color(0.2f, 0.2f, 0.2f),
        new Color(0.9f, 1f, 1f),
        new Color(0.6f, 0f, 0.6f),
        new Color(0.7f, 0.2f, 0f),
        new Color(0f, 1f, 1f),
        new Color(0.1f, 1f, 0.1f)
    };

    public static Color GetColor(E_PlayerColor playerColor) { return colors[(int)playerColor]; }
    public static Color Red { get { return colors[(int)E_PlayerColor.Red]; } }
    public static Color Blue { get { return colors[(int)E_PlayerColor.Blue]; } }
    public static Color Green { get { return colors[(int)E_PlayerColor.Green]; } }
    public static Color Pink { get { return colors[(int)E_PlayerColor.Pink]; } }
    public static Color Orange { get { return colors[(int)E_PlayerColor.Orange]; } }
    public static Color Yellow { get { return colors[(int)E_PlayerColor.Yellow]; } }
    public static Color Black { get { return colors[(int)E_PlayerColor.Black]; } }
    public static Color White { get { return colors[(int)E_PlayerColor.White]; } }
    public static Color Purple { get { return colors[(int)E_PlayerColor.Purple]; } }
    public static Color Brown { get { return colors[(int)E_PlayerColor.Brown]; } }
    public static Color Cyan { get { return colors[(int)E_PlayerColor.Cyan]; } }
    public static Color Lime { get { return colors[(int)E_PlayerColor.Lime]; } }

}
