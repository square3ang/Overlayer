using UnityEngine;
using System;
using System.Linq;
using System.Drawing;
using System.IO;
using Overlayer;
using Overlayer.Core;
using Color = UnityEngine.Color;

namespace RapidGUI
{
    

    /// <summary>
    /// IMGUI ColorPicker based on
    /// https://github.com/mattatz/unity-immediate-color-picker
    /// </summary>
    public class IMColorPicker : IDoGUIWindow
    {
        public enum SliderMode
        {
            HSV,
            RGB
        };

        
        #region static

        static readonly Texture2D circle, rightArrow, leftArrow, button, buttonHighlighted;

        static readonly GUIStyle previewStyle;
        static readonly GUIStyle labelStyle;
        static readonly GUIStyle hueStyle;
        static readonly GUIStyle presetStyle, presetHighlightedStyle;

        static readonly GUIContent previewContents;

        static IMColorPreset defaultPreset;

        static IMColorPicker()
        {
            circle = new Texture2D(2, 2);
            circle.LoadImage(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAADAAAAAwCAQAAAD9CzEMAAACRElEQVRYCe3Bzy5jARiH4Z9qK9qV6JyIGW5ARmwqdSGSsbAQjV6A+HsBzNgSaWJmS2hiIxJXMTFig5IQNuhMBoN2887iy0mbo9X2nJXE80jv3gBCDDHPNocUKFGiwCHbzJEipKDoZYkrarnkKz3yC4c1StRTJEtCzWOU3zSqwIiaQYQfVLpjnTRJOokSJUGSNBvcUSlLWI0hxh5lR4zRriqIMc4JZbvEVB8R9nA9MklYryDCFE+4dgmrHr7jOqZPDaCfPK6sXscorp98UINw2Mf1RbXh8AdzzAc1AYc85pZO1cIa5h99ahL9PGFWVR29lDCT8oFpTJGPqoYlzBFh+UCEPGZRLxHiCjMmn0hjLmiRF0OYv7TLJ+LcYwblxTxmXQGwiZmVF9uYtAIgg8nJi0NMUgGQwhzIiwKmUwHgYG7kRQkTVQC0YYryooSJKgDaMEV5UcAkFAAO5kZeHGKSCoAU5kBebGPSCoAMJicv5jAbCoAtzIy8SGHuiMkn4jxgkvIixCVmXD4xgTmnRS/xDXNCRD4Q5RSzoGrooYSZkg/MYZ7pVnVkMY98VpMY4BmzrFpIUMDkcdQEujjDXNOh2hjBtY+jBtHFL1zDeh1ZXHn61QAGOMO1onoIs4vriWkiegVR5njGtUOr6iPGLmV50sRVBXEmOKVsh3Y1hjBZKt2zSYYUDm204ZAiwxYPVFqhVc1ghFsadc2wmkcnqxSp55llOuQXn1jkglrOWaBbQRFikFlyHHBDkSI3HJBjhiQtevcG/AfcchAwrAwT4wAAAABJRU5ErkJggg=="));
            rightArrow = new Texture2D(2, 2);
            rightArrow.LoadImage(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAABmJLR0QA/wD/AP+gvaeTAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAB3RJTUUH4wcYChUv6no+dwAAALBJREFUeNrt2FkOwjAQBFGLk8/NOz9IIEiIWSJgqt4NuqQs9hiSJEmSdIAkRQ8QdIRcFD0AM0LuFT0AK0K2FT0AI0L2FT1A7wiZV/QAPSPkeUUP0CtCXlf0AD0i5H1FD/DfEfI5RQ9wSITTkI+AL0E/g/4I+SvsYcjjsBciXol9M0DP8ZMB+o6fCNB7/E6A/uMfBGCM3wjAGb8SgDX+JgBv/FUA5vhzAO54SZIkST9rASnVJsSet2LdAAAAAElFTkSuQmCC"));
            leftArrow = new Texture2D(2, 2);
            leftArrow.LoadImage(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAEAAAABACAYAAACqaXHeAAAABmJLR0QA/wD/AP+gvaeTAAAACXBIWXMAAAsTAAALEwEAmpwYAAAAB3RJTUUH4wcYChQ1DgP2TAAAALRJREFUeNrt2FkOwkAMBNERJ+fm5hchlqwo4351gyppEttjAAAAAMDFqKp7tHxVVbR8ZIBn+bgAr/JRAd7JxwT4JB8R4Jt8+wC/5FsHWCLfNsBS+ZYB1si3C7BWvlWALfJtAmyVbxFgj/z0AfbKTx3gCPlpAxwlP2WAI+X/EeA24An4CPoNGoSMwpYh67CDiJPYJSPET4zxY3P87hC/QMVvkfGrdPw9If6oEn9ZGgAAAABwAg8YTCbEq77wEwAAAABJRU5ErkJggg=="));
            button = new Texture2D(2, 2);
            button.LoadImage(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAIAAACQkWg2AAAAF0lEQVR42mNgIAP8JxqMahjVMNAaSAIAEz9J0wH4O8QAAAAASUVORK5CYII="));
            buttonHighlighted = new Texture2D(2, 2);
            buttonHighlighted.LoadImage(Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAABAAAAAQCAIAAACQkWg2AAAAF0lEQVR42mNgoBP4TwQY1TCqgRgNNAQAFC2uYAIOr4cAAAAASUVORK5CYII="));

            previewStyle = new GUIStyle();
            var previewSize = new Vector2Int(kPreviewBarWidth, kPreviewBarHeight);
            var checkerBoard = CreateChekcerBoardTexture(previewSize, 4, Color.white, Color.HSVToRGB(0f, 0f, 0.8f));
            previewStyle.normal.background = checkerBoard;

            labelStyle = new GUIStyle("Label");
            labelStyle.fontSize = 12;

            hueStyle = new GUIStyle();
            var hueTexture = CreateHueTexture(20, kHSVPickerSize);
            hueStyle.normal.background = hueTexture;

            presetStyle = new GUIStyle();
            presetStyle.normal.background = button;

            presetHighlightedStyle = new GUIStyle();
            presetHighlightedStyle.normal.background = buttonHighlighted;

            var whiteTex = new Texture2D(previewSize.x, previewSize.y);
            whiteTex.SetPixels(whiteTex.GetPixels().Select(_ => Color.white).ToArray());
            whiteTex.Apply();
            previewContents = new GUIContent(whiteTex);

            defaultPreset = ScriptableObject.CreateInstance<IMColorPreset>();
        }

        static Texture2D CreateHueTexture(int width, int height)
        {
            var tex = new Texture2D(width, height);
            for (int y = 0; y < height; y++)
            {
                var h = 1f * y / height;
                var color = Color.HSVToRGB(h, 1f, 1f);
                for (int x = 0; x < width; x++)
                {
                    tex.SetPixel(x, y, color);
                }
            }

            tex.Apply();
            return tex;
        }

        static Texture2D CreateChekcerBoardTexture(Vector2Int size, int gridSize, Color col0, Color col1)
        {
            var tex = new Texture2D(size.x, size.y);
            for (var y = 0; y < size.y; y++)
            {
                var flagY = ((y / gridSize) % 2 == 0);
                for (var x = 0; x < size.x; x++)
                {
                    var flagX = ((x / gridSize) % 2 == 0);
                    tex.SetPixel(x, y, (flagX ^ flagY) ? col0 : col1);
                }
            }

            tex.Apply();
            return tex;
        }

        #endregion

        const int kHSVPickerSize = 280, kHuePickerWidth = 24;
        const int kPreviewBarWidth = 160, kPreviewBarHeight = 24;


        public Color color
        {
            get
            {
                return _color;
            }
            set
            {
                if (value != _color)
                {
                    _color = value;
                    Color.RGBToHSV(_color, out _hsv.x, out _hsv.y, out _hsv.z);
                    UpdateSVTexture();
                    ClearPresetSelection();
                }
            }
        }

        public Vector3 hsv
        {
            get { return _hsv; }
            set
            {
                if (value != _hsv)
                {
                    _hsv = value;
                    var a = _color.a;
                    _color = Color.HSVToRGB(hsv.x, hsv.y, hsv.z);
                    _color.a = a;
                    
                    UpdateSVTexture();
                    ClearPresetSelection();
                }
            }
        }

        public float H => _hsv.x;
        public float S => _hsv.y;                       
        public float V => _hsv.z;

        SliderMode sliderMode = SliderMode.RGB;

        Color _colorPrev;
        Color _color;
        IMColorPreset preset;

        Vector3 _hsv = new Vector3(0f, 0f, 0f);

        public Rect windowRect = new Rect(20, 20, 350, 500);
        public bool destroy { get; protected set; }

        GUIStyle svStyle;
        Texture2D svTexture;
        int selectedPreset = -1;



        public IMColorPicker() : this(Color.red, defaultPreset) { }
        public IMColorPicker(Color c) : this(c, defaultPreset) { }
        public IMColorPicker(IMColorPreset pr) : this(Color.red, pr) { }
        public IMColorPicker(Color c, IMColorPreset pr)
        {
            Setup();

            _colorPrev = c;
            color = c;
            UpdateSVTexture();

            preset = pr;
        }

        void Setup()
        {
            svTexture = new Texture2D(kHSVPickerSize, kHSVPickerSize);
            svStyle = new GUIStyle();
            svStyle.normal.background = svTexture;
        }


        public void SetWindowPosition(Vector2 pos)
        {
            windowRect.position = pos;
        }


        #region IDoGUIWindow

        public void DoGUIWindow()
        {
            DrawWindow();
        }

        public void CloseWindow() { }

        #endregion

        public bool DrawWindow(string title = "")
        {
            windowRect = GUI.ModalWindow(GetHashCode(), windowRect, DrawColorPickerWindow, title, RGUIStyle.darkWindow);
            return destroy;
        }

        void DrawColorPickerWindow(int windowID)
        {
            DrawColorPicker();
            GUI.DragWindow();

            var ev = Event.current;
       
            destroy |= (ev.button == 0 && ev.rawType == EventType.MouseDown && !windowRect.Contains(GUIUtility.GUIToScreenPoint(ev.mousePosition)));
        }

        public void DrawColorPicker()
        {
            using (new GUILayout.VerticalScope())
            {
                GUILayout.Space(5f);
                DrawPreviews();

                GUILayout.Space(10f);
                DrawHSVPicker();

                GUILayout.Space(10f);
                DrawSliderMode();

                GUILayout.Space(10f);
                switch (sliderMode)
                {
                    case SliderMode.HSV: DrawSlidersHSV(); break;
                    case SliderMode.RGB: DrawSlidersRBG(); break;
                }


                /*if (preset != null)
                {
                    GUILayout.Space(10f);
                    DrawPresets();
                }*/
            }
        }

        void DrawPreviews()
        {
            using (new GUILayout.HorizontalScope())
            {
                DrawPreview(_colorPrev, () => color = _colorPrev);
                DrawPreview(_color, null);
                GUILayout.FlexibleSpace();
            }
        }

        void DrawPreview(Color c, Action onButton)
        {
            using (new GUILayout.VerticalScope())
            {
                var tmpBC = GUI.backgroundColor;
                var tmpCC = GUI.contentColor;
                {

                    GUI.backgroundColor = Color.white;
                    GUI.contentColor = c;

                    if (GUILayout.Button(previewContents, previewStyle, GUILayout.Width(kPreviewBarWidth), GUILayout.Height(kPreviewBarHeight)))
                    {
                        onButton?.Invoke();
                    }
                }
                GUI.backgroundColor = tmpBC;
                GUI.contentColor = tmpCC;
            }
        }

        void DrawHSVPicker()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label("", svStyle, GUILayout.Width(kHSVPickerSize), GUILayout.Height(kHSVPickerSize));
                DrawSVHandler(GUILayoutUtility.GetLastRect());

                GUILayout.Space(16f);

                GUILayout.Label("", hueStyle, GUILayout.Width(kHuePickerWidth), GUILayout.Height(kHSVPickerSize));
                DrawHueHandler(GUILayoutUtility.GetLastRect());
            }
        }

        void DrawSliderMode()
        {
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.FlexibleSpace();
                var style = new GUIStyle(Drawer.myButton);
                style.fontSize = 12;
                if (GUILayout.Button(sliderMode.ToString(), style, GUILayout.Width(50f)))
                {
                    sliderMode = (SliderMode)(((int)sliderMode + 1) % Enum.GetValues(typeof(SliderMode)).Length);
                }
            }
        }

        void DrawSlidersHSV()
        {
            var newHSV = hsv;
            newHSV.x = DrawSlide(hsv.x, "H");
            newHSV.y = DrawSlide(hsv.y, "S");
            newHSV.z = DrawSlide(hsv.z, "V");
            _color.a = DrawSlide(_color.a, "A");

            hsv = newHSV;
        }
        void DrawSlidersRBG()
        {
            var col = color;
            col.r = DrawSlide(col.r, "R");
            col.g = DrawSlide(col.g, "G");
            col.b = DrawSlide(col.b, "B");
            col.a = DrawSlide(col.a, "A");

            color = col;
        }


        float DrawSlide(float v, string label)
        {
            var tmp = RGUI.PrefixLabelSetting.width;
            RGUI.PrefixLabelSetting.width = 16f;
            
            v =  RGUI.Slider(v, label);

            RGUI.PrefixLabelSetting.width = tmp;

            return v;
        
            /*
            using (new GUILayout.HorizontalScope())
            {
                GUILayout.Label(label, GUILayout.Width(16f));
                v = GUILayout.HorizontalSlider(v, 0f, 1f);
                float.TryParse(GUILayout.TextField(v.ToString("0.000"), GUILayout.Width(40f)), out v); ;
            }

            return v;
            */
        }


        void DrawPresets()
        {
            const int presetSize = 16;

            GUILayout.Label("Presets", labelStyle);
            GUILayout.Space(2f);

            var tmp = GUI.backgroundColor;
            {
                var width = GUILayout.Width(presetSize);
                var height = GUILayout.Height(presetSize);

                var num = preset.Colors.Count;

                const int colNum = 10;
                var rowNum = num / colNum;
                for (int row = 0; row <= rowNum; row++)
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Space(1f);
                        var limit = Mathf.Min(num, (row + 1) * colNum);
                        var i = row * colNum;
                        for (; i < limit; i++)
                        {
                            var c = preset.Colors[i];
                            GUI.backgroundColor = c;
                            if (GUILayout.Button(" ", (i == selectedPreset) ? presetHighlightedStyle : presetStyle, width, height))
                            {
                                switch (Event.current.button)
                                {
                                    case 0:
                                        {
                                            selectedPreset = i;
                                            color = c;
                                        }
                                        break;
                                    case 1:
                                        {
                                            preset.Colors.RemoveAt(i);
                                            ClearPresetSelection();
                                        }
                                        return;
                                }
                            }
                            GUILayout.Space(1f);
                        }
                    }
                }
            }
            GUI.backgroundColor = tmp;
            const int buttonWidth = 67, buttonHeight = 20;
            using (new GUILayout.HorizontalScope())
            {
                if (Drawer.Button("Save", GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
                {
                    preset.Save(color);
                    selectedPreset = preset.Colors.Count - 1;
                }
                if (selectedPreset >= 0 && Drawer.Button("Remove", GUILayout.Width(buttonWidth), GUILayout.Height(buttonHeight)))
                {
                    preset.Colors.RemoveAt(selectedPreset);
                    ClearPresetSelection();
                }
            }
        }

        void ClearPresetSelection()
        {
            selectedPreset = -1;
        }

        void DrawSVHandler(Rect rect)
        {
            const float size = 10f;
            const float offset = 5f;
            var s = hsv.y;
            var v = hsv.z;
            GUI.DrawTexture(new Rect(rect.x + s * rect.width - offset, rect.y + (1f - v) * rect.height - offset, size, size), circle);

            DraggableRectHandler(rect, () =>
            {
                var p = Event.current.mousePosition;

                var newHSV = hsv;
                newHSV.y = Mathf.Clamp01((p.x - rect.x) / rect.width);
                newHSV.z = Mathf.Clamp01(1f - (p.y - rect.y) / rect.height);
                hsv = newHSV;
            });
        }


        void DrawHueHandler(Rect rect)
        {
            const float size = 15f;
            var h = hsv.x;
            GUI.DrawTexture(new Rect(rect.x - size * 0.75f, rect.y + (1f - h) * rect.height - size * 0.5f, size, size), rightArrow);
            GUI.DrawTexture(new Rect(rect.x + rect.width - size * 0.25f, rect.y + (1f - h) * rect.height - size * 0.5f, size, size), leftArrow);


            DraggableRectHandler(rect, () =>
            {
                var p = Event.current.mousePosition;

                var newHSV = hsv;
                newHSV.x = Mathf.Clamp01(1f - (p.y - rect.y) / rect.height);
                hsv = newHSV;
            });
        }

        void DraggableRectHandler(Rect rect, Action action)
        {
            var e = Event.current;
            var controlID = GUIUtility.GetControlID(FocusType.Passive);
            var etype = e.GetTypeForControl(controlID);
            var p = e.mousePosition;

            switch (etype)
            {
                case EventType.MouseDown:
                    {
                        if ((e.button == 0) && rect.Contains(p))
                        {
                            GUIUtility.hotControl = controlID;
                        }
                    }
                    break;

                case EventType.MouseUp:
                    {
                        if (GUIUtility.hotControl == controlID) GUIUtility.hotControl = 0;
                    }
                    break;
            }

            if (e.isMouse && (GUIUtility.hotControl == controlID))
            {
                action();
                e.Use();
            }
        }


        void UpdateSVTexture()
        {
            var size = svTexture.width;
            for (int y = 0; y < size; y++)
            {
                var v = 1f * y / size;
                for (int x = 0; x < size; x++)
                {
                    var s = 1f * x / size;
                    var c = Color.HSVToRGB(_hsv.x, s, v);
                    svTexture.SetPixel(x, y, c);
                }
            }

            svTexture.Apply();
        }
    }
}

