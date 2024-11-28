using System.Drawing;
using System.Numerics;
using System.Runtime.InteropServices;
using ClickableTransparentOverlay;
using ImGuiNET;


namespace IMGUI
{
    public class Program : Overlay
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr GetConsoleWindow();
        
        [DllImport("user32.dll")]
        private static extern bool ShowWindow(IntPtr hWnd, int nCmdShow);
        
        // Window
        private bool _isOpen = true;
        private Vector4 _windowColor = new Vector4(0.135f, 0.135f, 0.135f, 0.865f);
        private const int SW_HIDE = 0; // Console Hide
        private const int SW_SHOW = 5; // Console Show
        
        // Progress Bar
        private float _progress;
        private readonly float _duration = 5f;
        private readonly DateTime _startTime;
        private bool _isLoadingComplete;

        // Funny rainbow
        private readonly float _rainbowCycleSpeed = .5f; // Speed of the rainbow cycling, the smaller the value, the slower
        private bool _useRainbowColor;

        //Fonts
        private string _arialFontPath = "Fonts\\arial.ttf";

        // The program variables
        private int _bet = 1;
        private int _points = 100;
        private int _onWhat = 1;

        string result = "Тут буде результат";
        
        public Program()
        {
            _startTime = DateTime.Now;
        }

        private Vector4 CalculateContrastingColor(Vector4 bgColor)
        {
            float luminance = 0.299f * bgColor.X + 0.587f * bgColor.Y + 0.114f * bgColor.Z;
            return luminance > 0.5f 
                ? new Vector4(0f, 0f, 0f, 1f) // Dark text for bright backgrounds
                : new Vector4(1f, 1f, 1f, 1f); // Light text for dark backgrounds
        }

        private Vector4 GetRainbowColor(float timeOffset = 0f)
        {
            float time = (float)(DateTime.Now - _startTime).TotalSeconds;
            float hue = (time * _rainbowCycleSpeed + timeOffset) % 1.0f;

            return HSVtoRGB(hue, 1f, 1f); // Full saturation and value
        }

        
        // Some magic shit
        private Vector4 HSVtoRGB(float h, float s, float v)
        {
            int i = (int)(h * 6);
            float f = h * 6 - i;
            float p = v * (1 - s);
            float q = v * (1 - f * s);
            float t = v * (1 - (1 - f) * s);

            i = i % 6;
            return i switch
            {
                0 => new Vector4(v, t, p, 1f),
                1 => new Vector4(q, v, p, 1f),
                2 => new Vector4(p, v, t, 1f),
                3 => new Vector4(p, q, v, 1f),
                4 => new Vector4(t, p, v, 1f),
                5 => new Vector4(v, p, q, 1f),
                _ => new Vector4(0, 0, 0, 1f),
            };
        }
        
        protected override void Render()
        {
            if (!_isOpen)
            {
                Close();
            }

            SetupStyling();
            
            // Finally starting the window
            ImGui.Begin("IMGUI", ref _isOpen, ImGuiWindowFlags.NoCollapse );
            // Get the screen resolution dynamically (SkiaSharp Example)

            Size = new Size(3840,2160); // This sets up ClickableTransparentOverlay size, which *should be the same as your resolution* (TODO)
            ReplaceFont(_arialFontPath, 16, FontGlyphRangeType.Cyrillic | FontGlyphRangeType.English);
            
            // Fake loading for funnies
            if (!_isLoadingComplete)
            {
                Vector2 windowSize = ImGui.GetContentRegionAvail();
                
                // Center that bitch
                float progressBarHeight = 40f; 
                float yOffset = (windowSize.Y - progressBarHeight) / 2;
                
                ImGui.Dummy(new Vector2(0, yOffset));
                
                var elapsed = (float)(DateTime.Now - _startTime).TotalSeconds;
                _progress = Math.Clamp(elapsed / _duration, 0f, 1f);
                
                float easedProgress = (float)(0.5 - 0.5 * Math.Cos(_progress * Math.PI));
                ImGui.ProgressBar(easedProgress, new Vector2(windowSize.X, progressBarHeight));

                if (easedProgress >= 1f)
                {
                    _isLoadingComplete = true;
                }
            }

            if (_isLoadingComplete && ImGui.BeginTabBar("Tabs"))
            {
                if (ImGui.BeginTabItem("Main"))
                {
                    
                    ImGui.SeparatorText("Рулетка!");
                    ImGui.Text($"У вас {_points} балів");
                    ImGui.InputInt("Ставка", ref _bet);
                    ImGui.InputInt("На яке число ставимо (1-6)", ref _onWhat);
                    if (ImGui.Button("Зробити ставку"))
                    {
                        if (_onWhat < 1 || _onWhat > 6)
                        {
                            result = "Число на яке ми ставемо неможливе";
                        }
                        else if (_bet > _points)
                        {
                            result = "Недостатньо балів";
                        }
                        else if (_bet < 1)
                        {
                            result = "Не можна ставити нічого/негативне число!";
                        }
                        else
                        {
                            int win = Random.Shared.Next(1, 6);
                            _points -= _bet;
                            if (win == _onWhat)
                            {
                                _points += _bet * 3;
                                result = "Ти виграв!";
                            }
                            else
                            {
                                result = "Ти програв!";
                            }
                        }
                    }
                    ImGui.Dummy(new Vector2(0, 40));
                    ImGui.Text(result);

                    if (_points == 0)
                    {
                        _isOpen = false;
                    }
                    
                    ImGui.EndTabItem();
                }

                if (ImGui.BeginTabItem("Settings"))
                {
                    ImGui.Checkbox("ВКЛЮЧИТЬ РАДУГУ!!!", ref _useRainbowColor); // funny
                    if (!_useRainbowColor)
                    {
                        ImGui.ColorEdit4("Основной Цвет", ref _windowColor);
                    }
                    ImGui.EndTabItem();
                    
                }

                ImGui.EndTabBar();
            }

            ImGui.End();
            ImGui.PopStyleColor();

        }

        public void SetupStyling()
        {
            Vector4 currentColor = _useRainbowColor ? GetRainbowColor() : _windowColor;
            Vector4 textColor = CalculateContrastingColor(currentColor);
            
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
            IntPtr handle = GetConsoleWindow();
            ShowWindow(handle, SW_HIDE);
            
            Console.WriteLine("Starting...");
            Program program = new Program();
            
            program.Start().Wait();
        }
    }
}