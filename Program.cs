using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using ClickableTransparentOverlay;
using ImGuiNET;

namespace IMGUI;

public class Program : Overlay
{
    [DllImport("kernel32.dll")]
    private static extern IntPtr GetConsoleWindow();

    [DllImport("user32.dll")]
    private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);

    // Window
    private bool _isOpen = true;
    private Vector4 _windowColor = new(0.135f, 0.135f, 0.135f, 0.865f);
    private const int SwHide = 0; // Console Hide
    private const int SwShow = 5; // Console Show

    // Progress Bar
    private float _progress;
    private const float Duration = 5f;
    private readonly DateTime _startTime;
    private bool _isLoadingComplete;

    // Funny rainbow
    private const float RainbowCycleSpeed = .5f; // Speed of the rainbow cycling, the smaller the value, the slower
    private bool _useRainbowColor;

    //Fonts
    private const string ArialFontPath = "Fonts\\arial.ttf";

    // The program variables
    private int _points;
    private int _clickMultiplier = 1;
    private string _result = "";
    private int _upgradePrice = 100;

    private Program()
    {
        _startTime = DateTime.Now;
    }

    private Vector4 CalculateContrastingColor(Vector4 bgColor)
    {
        var luminance = 0.299f * bgColor.X + 0.587f * bgColor.Y + 0.114f * bgColor.Z;
        return luminance > 0.5f
            ? new Vector4(0f, 0f, 0f, 1f) // Dark text for bright backgrounds
            : new Vector4(1f, 1f, 1f, 1f); // Light text for dark backgrounds
    }

    private Vector4 GetRainbowColor(float timeOffset = 0f)
    {
        var time = (float)(DateTime.Now - _startTime).TotalSeconds;
        var hue = (time * RainbowCycleSpeed + timeOffset) % 1.0f;

        return HsVtoRgb(hue, 1f, 1f); // Full saturation and value
    }

    // Some magic sh*t
    private Vector4 HsVtoRgb(float h, float s, float v)
    {
        var i = (int)(h * 6);
        var f = h * 6 - i;
        var p = v * (1 - s);
        var q = v * (1 - f * s);
        var t = v * (1 - (1 - f) * s);

        i = i % 6;
        return i switch
        {
            0 => new Vector4(v, t, p, 1f),
            1 => new Vector4(q, v, p, 1f),
            2 => new Vector4(p, v, t, 1f),
            3 => new Vector4(p, q, v, 1f),
            4 => new Vector4(t, p, v, 1f),
            5 => new Vector4(v, p, q, 1f),
            _ => new Vector4(0, 0, 0, 1f)
        };
    }

    protected override void Render()
    {
        if (!_isOpen) Close();

        SetupStyling();

        // Finally starting the window
        ImGui.Begin("IMGUI", ref _isOpen, ImGuiWindowFlags.NoCollapse);
        Size = new Size(3840, 2160); // This sets up ClickableTransparentOverlay size
        // ReSharper disable once BitwiseOperatorOnEnumWithoutFlags
        ReplaceFont(ArialFontPath, 16, FontGlyphRangeType.English | FontGlyphRangeType.Cyrillic);

        // Fake loading for funnies
        if (!_isLoadingComplete)
        {
            var windowSize = ImGui.GetContentRegionAvail();

            // Center that bitch
            var progressBarHeight = 40f;
            var yOffset = (windowSize.Y - progressBarHeight) / 2;

            ImGui.Dummy(new Vector2(0, yOffset));

            var elapsed = (float)(DateTime.Now - _startTime).TotalSeconds;
            _progress = Math.Clamp(elapsed / Duration, 0f, 1f);

            var easedProgress = (float)(0.5 - 0.5 * Math.Cos(_progress * Math.PI));
            ImGui.ProgressBar(easedProgress, new Vector2(windowSize.X, progressBarHeight));

            if (easedProgress >= 1f) _isLoadingComplete = true;
        }

        if (_isLoadingComplete && ImGui.BeginTabBar("Tabs"))
        {
            if (ImGui.BeginTabItem("Main"))
            {
                ImGui.SeparatorText("Клікер!");
                ImGui.Text($"У вас {_points} балів");
                if (ImGui.Button("Клік!")) _points+=_clickMultiplier;

                ImGui.Dummy(new Vector2(0, 20));

                if (ImGui.Button($"Збільшити клік (Ціна: {_upgradePrice})"))
                {
                    if (_points >= _upgradePrice)
                    {
                        _clickMultiplier += 1;
                        _points -= _upgradePrice;
                        _upgradePrice = Convert.ToInt32(_upgradePrice * 1.5);
                        _result = "Апгрейд куплено!";
                    }
                    else
                    {
                        _result = "Недостатньо балів";
                    }
                }
                
                ImGui.Dummy(new Vector2(0, 10));
                ImGui.Text(_result);

                ImGui.EndTabItem();
            }

            if (ImGui.BeginTabItem("Settings"))
            {
                ImGui.Checkbox("ВКЛЮЧИТЬ РАДУГУ!!!", ref _useRainbowColor); // funny
                if (!_useRainbowColor) ImGui.ColorEdit4("Основной Цвет", ref _windowColor);
                
                ImGui.EndTabItem();
            }

            ImGui.EndTabBar();
        }

        ImGui.End();
        ImGui.PopStyleColor();
    }

    private void SetupStyling()
    {
        var currentColor = _useRainbowColor ? GetRainbowColor() : _windowColor;
        var textColor = CalculateContrastingColor(currentColor);

        ImGui.PushStyleColor(ImGuiCol.WindowBg, currentColor);
        ImGui.PushStyleColor(ImGuiCol.TitleBg, currentColor + new Vector4(-0.05f, -0.05f, -0.05f, 1f));
        ImGui.PushStyleColor(ImGuiCol.TitleBgActive, currentColor + new Vector4(0.1f, 0.1f, 0.1f, 1f));
        ImGui.PushStyleColor(ImGuiCol.FrameBg, currentColor + new Vector4(0.15f, 0.15f, 0.15f, 1f));
        ImGui.PushStyleColor(ImGuiCol.Button, currentColor + new Vector4(0.15f, 0.15f, 0.15f, 1f));
        ImGui.PushStyleColor(ImGuiCol.TabSelected, currentColor + new Vector4(0.25f, 0.25f, 0.25f, 1f));
        ImGui.PushStyleColor(ImGuiCol.TabHovered, currentColor + new Vector4(0.1f, 0.1f, 0.1f, 1f));
        ImGui.PushStyleColor(ImGuiCol.Tab, currentColor + new Vector4(0.15f, 0.15f, 0.15f, 1f));
        ImGui.PushStyleColor(ImGuiCol.Text, textColor);

        ImGui.PushStyleVar(ImGuiStyleVar.WindowRounding, 5);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 2);
        ImGui.PushStyleVar(ImGuiStyleVar.GrabRounding, 2);
        ImGui.PushStyleVar(ImGuiStyleVar.FrameRounding, 1);
        
        
    }

    public static void Main(string[] args)
    {
        var handle = GetConsoleWindow();
        ShowWindow(handle, SwHide);

        Console.WriteLine("Starting...");
        var program = new Program();

        program.Start().Wait();
    }
    
    
}