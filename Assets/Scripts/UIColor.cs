using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_UIColor
{
    Red, Green, Blue, Gray, Yellow, White
}

public class UIColor
{
    private static List<Color> colors = new List<Color>()
    {
        new Color(0.8f, 0f, 0f),
        new Color(0f, 0.8f, 0f),
        new Color(0f, 0f, 0.8f),
        new Color(0.3f, 0.3f, 0.3f),
        new Color(0.9f,1f,0f),
        new Color(1f,1f,1f)
    };

    public static Color GetColor(E_UIColor uiColor) { return colors[(int)uiColor]; }
    public static Color Red { get { return colors[(int)E_UIColor.Red]; } }
    public static Color Green { get { return colors[(int)E_UIColor.Green]; } }
    public static Color Blue { get { return colors[(int)E_UIColor.Blue]; } }
    public static Color Gray { get { return colors[(int)E_UIColor.Gray]; } }
    public static Color Yellow { get { return colors[(int)E_UIColor.Yellow]; } }
    public static Color White { get { return colors[(int)E_UIColor.White]; } }
}
