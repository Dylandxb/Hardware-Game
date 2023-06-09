using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.ComponentModel;
using System.Diagnostics.Eventing.Reader;
using System.IO.Ports;
using System.Security.Cryptography;
using TMPro;
using UnityEditor;
using UnityEngine.Rendering.VirtualTexturing;
using UnityEngine.UI;
using UnityEngineInternal;

public class GAWController : MonoBehaviour
{
    /// <summary>
    /// The number of LEDs in the controller's screen in the x-dimension.
    /// </summary>
    public static readonly int ScreenWidth = 8;

    /// <summary>
    /// The number of LEDs in the controller's screen in the y-dimension.
    /// </summary>
    public static readonly int ScreenHeight = 8;

    /// <summary>
    /// The total number of LEDs in the controller's screen.
    /// </summary>
    public static readonly int PixelCount = ScreenWidth * ScreenHeight;

    [Tooltip(
        "The port to communicate with the controller. If you only have one serial device connected, the controller will be automatically detected. If you have multiple serial devices connected, you should set this manually. In most cases, it will be COM3 or COM4.")]
    public string Com_Port = "COM3";

    private int Baud_Rate = 115200;

    private static Color[] storedPixels = new Color[64];
    private static float lastUpdate = 0;

    private static Texture2D font;
    
    public enum Button
    {
        ButtonOne,
        ButtonTwo
    }

    private static Dictionary<Button, bool> buttonPressed = new Dictionary<Button, bool>();
    private static Dictionary<Button, bool> buttonDown = new Dictionary<Button, bool>();
    private static Dictionary<Button, bool> buttonUp = new Dictionary<Button, bool>();

    private static float frequency = 0.2f;
    private static SerialPort sp = new SerialPort();
    private static string deviceName = "GAWController";

    private static Color clearColour = new Color(0, 0, 0);

// Start is called before the first frame update
    void Start()
    {
        DontDestroyOnLoad(this);
        frequency = Math.Max(0.15f, frequency);
        font = Resources.Load<Texture2D>("GAWController_Font");
        try
        {
            if (SerialPort.GetPortNames().Length == 0)
            {
                Debug.LogError(deviceName + " Error: No serial devices detected");
            }
            else if (SerialPort.GetPortNames().Length == 1)
            {
                Debug.Log(deviceName + " detected, connecting automatically");
                sp.PortName = SerialPort.GetPortNames()[0];
            }
            else
            {
                sp.PortName = Com_Port;
            }

            sp.BaudRate = Baud_Rate;
            sp.ReadTimeout = 100;
            sp.WriteTimeout = 500;
            sp.DtrEnable = true;
            sp.Open();
            SetAllPixels(new Color(0, 0, 0));
            Debug.Log(deviceName + ": Connected to " + deviceName + ".");
        }
        catch
        {
            string errorMessage = deviceName + " Error: Could not connect to " + deviceName +
                                  ":\n1) Check it is connected to a USB port\n2) Check that you have assigned the correct COM port in the inspector. Valid ports are: ";
            string[] portNames = SerialPort.GetPortNames();
            for (int i = 0; i < portNames.Length; i++)
            {
                errorMessage += portNames[i] + ", ";
            }

            Debug.LogError(errorMessage);
        }
    }

    /// <summary>
    /// Render a single character to the display. Rendering a character clears the current display before displaying the indicated character. To change the background colour, first set the clear colour with SetClearColour().
    /// </summary>
    /// <param name="character">The character to render. Letters A-Z (uppercase only) and numbers 0-9 are all valid.</param>
    /// <param name="colour">The colour to draw the character.</param>
    public static void DrawCharacter(char character, Color color)
    {
        if (character >= 97 && character <= 122)
        {
            character = (char) (character - 32);
        }

        int charPos = -1;
        if (character >= 48 && character <= 57)
        {
            charPos = (26 + character - 48) * 8;
        }
        else if (character >= 65 && character <= 90)
        {
            charPos = (character - 65) * 8;
        }

        if (charPos >= 0)
        {
            for (int x = 0; x < ScreenWidth; x++)
            {
                for (int y = 0; y < ScreenHeight; y++)
                {
                    if (font.GetPixel(x + charPos, y).a > 0f)
                    {
                        SetPixel(x, y, color);
                    }
                    else
                    {
                        SetPixel(x, y, clearColour);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Renders a Texture2D to the screen. If the texture is larger than 8x8, only the upper left of the texture will be rendered.
    /// </summary>
    /// <param name="image">The image to draw to the screen.</param>
    public static void DrawImage(Texture2D image)
    {
        for (int x = 0; x < ScreenWidth; x++)
        {
            for (int y = 0; y < ScreenHeight; y++)
            {
                if (image.GetPixel(x, y).a > 0f)
                {
                    SetPixel(x, y, image.GetPixel(x, y)); 
                }
                else
                {
                    SetPixel(x, y, clearColour);
                }
            }
        }
    }
    
    /// <summary>
    /// Sets the amount of seconds to wait between each refresh of the controller's display (effectively the frame rate). Default: 0.15
    /// </summary>
    /// <param name="frequency">The minimum time (in seconds) to wait between each refresh of the controller's display.</param>
    public static void SetFrequency(float frequency)
    {
        GAWController.frequency = Math.Min(5f, Math.Max(frequency, 0.15f));
    }

    /// <summary>
    /// Sets the clear colour, used with ClearPixels() (default (0, 0, 0)). 
    /// </summary>
    /// <param name="colour"></param>
    public static void SetClearColour(Color colour)
    {
        clearColour = colour;
    }

    /// <summary>
    /// Clears the screen with the current clear colour (default (0, 0, 0)).
    /// </summary>
    public static void ClearPixels()
    {
        SetAllPixels(clearColour);
    }

    private static void PlotLineLow(int x0, int y0, int x1, int y1, Color colour)
    {
        int dx = x1 - x0;
        int dy = y1 - y0;
        int yi = 1;
        if (dy < 0)
        {
            yi = -1;
            dy = -dy;
        }

        int d = (2 * dy) - dx;
        int y = y0;

        for (int i = x0; i <= x1; i++)
        {
            int pointer = (i + y * 8);
            storedPixels[pointer] = colour;
            if (d > 0)
            {
                y = y + yi;
                d = d + (2 * (dy - dx));
            }
            else
            {
                d = d + 2 * dy;
            }
        }
    }

    private static void PlotLineHigh(int x0, int y0, int x1, int y1, Color colour)
    {
        int dx = x1 - x0;
        int dy = y1 - y0;
        int xi = 1;
        if (dx < 0)
        {
            xi = -1;
            dx = -dx;
        }

        int d = (2 * dx) - dy;
        int x = x0;

        for (int j = y0; j <= y1; j++)
        {
            int pointer = (x + j * 8);
            storedPixels[pointer] = colour;
            if (d > 0)
            {
                x = x + xi;
                d = d + (2 * (dx - dy));
            }
            else
            {
                d = d + 2 * dx;
            }
        }
    }

    /// <summary>
    /// Draws a line of the specified colour from (x0, y0) to (x1, y1).
    /// </summary>
    /// <param name="x0">The x-coordinate of the starting point of the line.</param>
    /// <param name="y0">The y-coordinate of the starting point of the line.</param>
    /// <param name="x1">The x-coordinate of the end point of the line.</param>
    /// <param name="y1">The y-coordinate of the end point of the line.</param>
    /// <param name="colour">The colour to draw the line.</param>
    public static void DrawLine(int x0, int y0, int x1, int y1, Color colour)
    {
        if ((x0 < 0 || x0 >= ScreenWidth)
            || (x1 < 0 || x1 >= ScreenWidth)
            || (y0 < 0 || y0 >= ScreenHeight)
            || (y1 < 0 || y1 >= ScreenHeight))
        {
            Debug.LogWarning(deviceName + " Error: Attempted to draw a line outside accepted bounds: (" + x0 + "," + y0 +
                           ") to (" + x1 + "," + y1 + ").");
            return;
        }

        if (Math.Abs(y1 - y0) < Math.Abs(x1 - x0))
        {
            if (x0 > x1)
            {
                PlotLineLow(x1, y1, x0, y0, colour);
            }
            else
            {
                PlotLineLow(x0, y0, x1, y1, colour);
            }
        }
        else
        {
            if (y0 > y1)
            {
                PlotLineHigh(x1, y1, x0, y0, colour);
            }
            else
            {
                PlotLineHigh(x0, y0, x1, y1, colour);
            }
        }
    }

    /// <summary>
    /// Set the colour of an individual LED.
    /// </summary>
    /// <param name="id">The id of the pixel which you want to set the colour of (0-63).</param>
    /// <param name="colour">The colour to set the LED</param>
    public static void SetPixel(int id, Color colour)
    {
        if (id >= 0 && id < PixelCount)
        {
            storedPixels[id] = colour;
        }
        else
        {
            Debug.LogWarning(deviceName + " Error: Tried to set an invalid pixel id (" + id +
                           "), must be between 0 and 63");
        }
    }

    /// <summary>
    /// Set the colour of an individual LED.
    /// </summary>
    /// <param name="x">The x-position of the LED</param>
    /// <param name="y">The y-position of the LED</param>
    /// <param name="colour">The colour to set the LED</param>
    public static void SetPixel(int x, int y, Color colour)
    {
        if ((x <= 0 && x >= ScreenWidth) || (y <= 0 && y >= ScreenHeight))
        {
            Debug.LogWarning(deviceName + " Error: Tried to set an invalid pixel position (" + x + "," + y +
                           "). Both x and y must be between 0 and 7");
        }
        else
        {
            int pointer = (x + y * 8);
            storedPixels[pointer] = colour;
        }
    }

    /// <summary>
    /// Draws a square of the specified colour.
    /// </summary>
    /// <param name="x">The x-position of the top-left of the square.</param>
    /// <param name="y">The y-position of the top-left of the square.</param>
    /// <param name="size">The width and height of the square.</param>
    /// <param name="colour">The colour to set the LEDs.</param>
    public static void DrawSquare(int x, int y, int size, Color colour)
    {
        DrawRectangle(x, y, size, size, colour);
    }

    /// <summary>
    /// Draws a rectangle of the specified colour.
    /// </summary>
    /// <param name="x">The x-position of the top-left of the rectangle.</param>
    /// <param name="y">The y-position of the top-left of the rectangle.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    /// <param name="colour">The colour to set the LEDs.</param>
    public static void DrawRectangle(int x, int y, int width, int height, Color colour)
    {
        width = width - 1;
        height = height - 1;
        if (width >= 0 && height >= 0)
        {
            for (int i = x; i < x + width; i++)
            {
                if (i >= 0 && i < ScreenWidth)
                {
                    if (y >= 0 && y < ScreenHeight)
                    {
                        SetPixel(i, y, colour);
                    }

                    if (y + height >= 0 && y + height < ScreenHeight)
                    {
                        SetPixel(i, y + height, colour);
                    }
                }
            }

            for (int j = y; j < y + height; j++)
            {
                if (j >= 0 && j < ScreenHeight)
                {
                    if (x >= 0 && x < ScreenWidth)
                    {
                        SetPixel(x, j, colour);
                    }

                    if (x + width >= 0 && x + width < ScreenWidth)
                    {
                        SetPixel(x + width, j, colour);
                    }
                }
            }

            if ((x + width >= 0 && x + width < ScreenWidth) && (y + height >= 0 && y + height < ScreenHeight))
            {
                SetPixel(x + width, y + height, colour);
            }
        }
    }

    /// <summary>
    /// Draws and fills a square of the specified colour.
    /// </summary>
    /// <param name="x">The x-position of the top-left of the square.</param>
    /// <param name="y">The y-position of the top-left of the square.</param>
    /// <param name="size">The width and height of the square.</param>
    /// <param name="colour">The colour to set the LEDs.</param>
    public static void FillSquare(int x, int y, int size, Color colour)
    {
        FillRectangle(x, y, size, size, colour);
    }

    /// <summary>
    /// Draws and fills a rectangle of the specified colour.
    /// </summary>
    /// <param name="x">The x-position of the top-left of the rectangle.</param>
    /// <param name="y">The y-position of the top-left of the rectangle.</param>
    /// <param name="width">The width of the rectangle.</param>
    /// <param name="height">The height of the rectangle.</param>
    /// <param name="colour">The colour to set the LEDs.</param>
    public static void FillRectangle(int x, int y, int width, int height, Color colour)
    {
        for (int i = x; i < x + width; i++)
        {
            if (i >= 0 && i < 8)
            {
                for (int j = y; j < y + height; j++)
                {
                    if (j >= 0 && j < 8)
                    {
                        SetPixel(i, j, colour);
                    }
                }
            }
        }
    }

    /// <summary>
    /// Sets all the controller's LEDs to the same colour.
    /// </summary>
    /// <param name="colour">The colour to set the LEDs.</param>
    public static void SetAllPixels(Color colour)
    {
        for (int i = 0; i < 64; i++)
        {
            storedPixels[i] = colour;
        }
    }

    /// <summary>
    /// Sets all the controller's LEDs to a specific colour.
    /// </summary>
    /// <param name="colours">A 64-element 1D array of type Color. Each element corresponds to one LED.</param>
    public static void SetAllPixels(Color[] colours)
    {
        for (int i = 0; i < 64; i++)
        {
            storedPixels[i] = colours[i];
        }
    }

    /// <summary>
    /// Sets all the controller's LEDs to a specific colour.
    /// </summary>
    /// <param name="colours">An 8x8 2D array of type Color. Each element corresponds to one LED.</param>
    public static void SetAllPixels(Color[][] colours)
    {
        for (int x = 0; x < 8; x++)
        {
            for (int y = 0; y < 8; y++)
            {
                int pointer = (x + y * 8);
                storedPixels[pointer] = colours[x][y];
            }
        }
    }

    /// <summary>
    /// Check whether a button on the controller was pressed down on this frame.
    /// </summary>
    /// <param name="button">Specifies which button to check.</param>
    /// <returns>True if the specified button was pressed down on this frame, false if it was not.</returns>
    public static bool GetButtonDown(Button button)
    {
        bool pressed = false;
        buttonDown.TryGetValue(button, out pressed);
        return pressed;
    }

    /// <summary>
    /// Check whether a button on the controller was released on this frame.
    /// </summary>
    /// <param name="button">Specifies which button to check.</param>
    /// <returns>True if the specified button was released on this frame, false if it was not.</returns>
    public static bool GetButtonUp(Button button)
    {
        bool pressed = false;
        buttonUp.TryGetValue(button, out pressed);
        return pressed;
    }

    /// <summary>
    /// Check whether a button on the controller is currently pressed.
    /// </summary>
    /// <param name="button">Specifies which button to check.</param>
    /// <returns>True if the specified button is currently pressed, false if it is not.</returns>
    public static bool IsDown(Button button)
    {
        bool pressed = false;
        buttonPressed.TryGetValue(button, out pressed);
        return pressed;
    }

    private static void UpdatePixels()
    {
        if (lastUpdate > frequency)
        {
            if (sp.IsOpen)
            {
                byte[] bytes = new byte[192];

                for (int x = 0; x < 8; x++)
                {
                    for (int y = 0; y < 8; y++)
                    {
                        int pixelPointer = (x * 8) + y;
                        int pointer = 3*(x + y*8);
                        bytes[pointer] = (byte) Mathf.RoundToInt(255 * storedPixels[pixelPointer].r);
                        bytes[pointer + 1] = (byte) Mathf.RoundToInt(255 * storedPixels[pixelPointer].g);
                        bytes[pointer + 2] = (byte) Mathf.RoundToInt(255 * storedPixels[pixelPointer].b);
                    }
                }

                sp.Write(bytes, 0, 192);
                lastUpdate = 0;
            }
        }
    }

// Update is called once per frame
    private int lastReadValue = 0;

    void Update()
    {
        lastUpdate += Time.deltaTime;
        UpdatePixels();

        buttonDown[Button.ButtonOne] = false;
        buttonDown[Button.ButtonTwo] = false;
        buttonUp[Button.ButtonOne] = false;
        buttonUp[Button.ButtonTwo] = false;

        try
        {
            string readInput = sp.ReadLine();
            readInput = readInput.Replace("\r", "");

            int readVal = Convert.ToInt32(readInput);
            switch (readVal)
            {
                case 0:
                    buttonPressed[Button.ButtonOne] = false;
                    buttonPressed[Button.ButtonTwo] = false;
                    if (lastReadValue == 1 || lastReadValue == 3)
                    {
                        buttonUp[Button.ButtonTwo] = true;
                    }

                    if (lastReadValue == 2 || lastReadValue == 3)
                    {
                        buttonUp[Button.ButtonOne] = true;
                    }

                    break;
                case 1:
                    buttonPressed[Button.ButtonOne] = false;
                    buttonPressed[Button.ButtonTwo] = true;
                    if (lastReadValue != 1 && lastReadValue != 3)
                    {
                        buttonDown[Button.ButtonTwo] = true;
                    }

                    if (lastReadValue == 2 || lastReadValue == 3)
                    {
                        buttonUp[Button.ButtonOne] = true;
                    }

                    break;
                case 2:
                    buttonPressed[Button.ButtonOne] = true;
                    buttonPressed[Button.ButtonTwo] = false;
                    if (lastReadValue != 2 && lastReadValue != 3)
                    {
                        buttonDown[Button.ButtonOne] = true;
                    }

                    if (lastReadValue == 1 || lastReadValue == 3)
                    {
                        buttonUp[Button.ButtonTwo] = true;
                    }

                    break;
                case 3:
                    buttonPressed[Button.ButtonOne] = true;
                    buttonPressed[Button.ButtonTwo] = true;
                    if (lastReadValue != 1 && lastReadValue != 3)
                    {
                        buttonDown[Button.ButtonTwo] = true;
                    }

                    if (lastReadValue != 2 && lastReadValue != 3)
                    {
                        buttonDown[Button.ButtonOne] = true;
                    }

                    break;
            }

            lastReadValue = readVal;
        }
        catch
        {
        }
    }

    void OnApplicationQuit()
    {
        if (sp.IsOpen)
        {
            byte[] bytes = new byte[192];
            for (int i = 0; i < 64; i++)
            {
                int pointer = (63-i) * 3;
                bytes[pointer] = 0;
                bytes[pointer + 1] = 0;
                bytes[pointer + 2] = 0;
            }

            sp.Write(bytes, 0, 192);
        }
        sp.Close();
    }
}