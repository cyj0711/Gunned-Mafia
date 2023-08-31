using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum E_UIColor
{
    Red, Green, Blue, Gray, Yellow, White, TextSkyBlue, TextGreen, TextYellow, TextGray, TextRed, Black, TextBlue
}

public class UIColor
{
    private static List<Color> colors = new List<Color>()
    {
        new Color(0.8f, 0f, 0f),        // Red
        new Color(0f, 0.8f, 0f),        // Green
        new Color(0f, 0f, 0.8f),        // Blue
        new Color(0.3f, 0.3f, 0.3f),    // Gray
        new Color(0.9f,1f,0f),          // Yellow
        new Color(1f,1f,1f),            // White
        new Color(0.5f,0.7f,1f),        // TextSkyBlue
        new Color(0f,0.8f,0f),          // TextGreen
        new Color(0.8f,0.8f,0f),        // TextYellow
        new Color(0.6f,0.6f,0.6f),      // TextGray
        new Color(1f,0f,0f),            // TextRed
        new Color(0f,0f,0f),            // Black
        new Color(0.2f,0.2f,1f)         // TextBlue
    };

    public static Color GetColor(E_UIColor uiColor) { return colors[(int)uiColor]; }
    public static Color Red { get { return colors[(int)E_UIColor.Red]; } }
    public static Color Green { get { return colors[(int)E_UIColor.Green]; } }
    public static Color Blue { get { return colors[(int)E_UIColor.Blue]; } }
    public static Color Gray { get { return colors[(int)E_UIColor.Gray]; } }
    public static Color Yellow { get { return colors[(int)E_UIColor.Yellow]; } }
    public static Color White { get { return colors[(int)E_UIColor.White]; } }
    public static Color TextSkyBlue { get { return colors[(int)E_UIColor.TextSkyBlue]; } }
    public static Color TextGreen { get { return colors[(int)E_UIColor.TextGreen]; } }
    public static Color TextYellow { get { return colors[(int)E_UIColor.TextYellow]; } }
    public static Color TextGray { get { return colors[(int)E_UIColor.TextGray]; } }
    public static Color TextRed { get { return colors[(int)E_UIColor.TextRed]; } }
    public static Color Black { get { return colors[(int)E_UIColor.Black]; } }
    public static Color TextBlue { get { return colors[(int)E_UIColor.TextBlue]; } }
}
