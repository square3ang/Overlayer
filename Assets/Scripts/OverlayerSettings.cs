using System;
using System.Collections.Generic;
using System.IO;
using ImGuiNET;
using UImGui;
#if !UIMGUI_REMOVE_IMNODES
using imnodesNET;
#endif
#if !UIMGUI_REMOVE_IMPLOT
using ImPlotNET;
using System.Linq;
#endif
#if !UIMGUI_REMOVE_IMGUIZMO
using ImGuizmoNET;
#endif
using UnityEngine;

namespace Overlayer
{
    [DefaultExecutionOrder(-10000)]
    public class OverlayerSettings : MonoBehaviour
    {
        /*
#if !UIMGUI_REMOVE_IMPLOT
        [SerializeField]
        float[] _barValues = Enumerable.Range(1, 10).Select(x => (x * x) * 1.0f).ToArray();
        [SerializeField]
        float[] _xValues = Enumerable.Range(1, 10).Select(x => (x * x) * 1.0f).ToArray();
        [SerializeField]
        float[] _yValues = Enumerable.Range(1, 10).Select(x => (x * x) * 1.0f).ToArray();
#endif
*/
        public GameObject TextTemplate;
        public static bool open = false;

        public static OverlayerSettings Instance;

        public static List<OverlayerObject> objects = new();

        private void OnEnable()
        {
            UImGuiUtility.Layout += OnLayout;
        }

        private void OnDisable()
        {
            UImGuiUtility.Layout -= OnLayout;
        }


        public static void FontInitializer(ImGuiIOPtr io)
        {
            var fontPath = Path.Combine(Application.isEditor ? Application.streamingAssetsPath : Main.ModEntry.Path,
                "Pretendard-Bold.otf");
            io.Fonts.AddFontFromFileTTF(fontPath, 18, null, io.Fonts.GetGlyphRangesKorean());
        }

        private bool prevOpen = false;

        private string alertString = "";
        private DateTime alertTime = new(0);

        void Awake()
        {
            Instance = this;
        }

        void Update()
        {
            if (Input.GetKeyDown(KeyCode.F3))
            {
                if (Main.IsPlaying) open = false;
                else open = !open;
            }

            if (open)
            {
                if (Input.GetKey(KeyCode.LeftControl) && Input.GetKeyDown(KeyCode.S))
                {
                    Main.Save();
                    alertString = "Saved!";
                    alertTime = DateTime.Now;
                }
            }

            if (open != prevOpen)
            {
                Input.imeCompositionMode = open ? IMECompositionMode.On : IMECompositionMode.Auto;
                prevOpen = open;

                foreach (var obj in objects)
                {
                    if (obj is not OverlayerText txt) continue;
                    txt.PlayingText.ParseTags();
                    txt.NotPlayingText.ParseTags();
                }

                if (!open)
                {
                    Main.Save();
                    alertString = "Saved!";
                    alertTime = DateTime.Now;
                }
            }
        }

        public static bool EnumCombo<T>(ref T enumValue, string label) where T : Enum
        {
            var enumType = typeof(T);
            var enumNames = Enum.GetNames(enumType);
            var enumValues = Enum.GetValues(enumType);
            int currentIndex = Array.IndexOf(enumValues, enumValue);

            if (ImGui.BeginCombo(label, enumNames[currentIndex]))
            {
                for (int i = 0; i < enumNames.Length; i++)
                {
                    bool isSelected = (i == currentIndex);
                    if (ImGui.Selectable(enumNames[i], isSelected))
                    {
                        currentIndex = i;
                        enumValue = (T)enumValues.GetValue(i);
                    }

                    if (isSelected)
                    {
                        ImGui.SetItemDefaultFocus();
                    }
                }

                ImGui.EndCombo();
            }

            return currentIndex != Array.IndexOf(enumValues, enumValue);
        }

        static Queue<OverlayerObject> toRemove = new();

        public static bool sim_play = false;

        private void OnLayout(UImGui.UImGui uImGui)
        {
            if (alertString != "" && DateTime.Now - alertTime <= new TimeSpan(0, 0, 1))
            {
                var TxtSiz = ImGui.CalcTextSize(alertString);
                ImGui.SetNextWindowSize(new Vector2(TxtSiz.x + 15, TxtSiz.y + 15), ImGuiCond.Always);
                ImGui.SetNextWindowPos(new Vector2(10, 10), ImGuiCond.Always);
                if (ImGui.Begin("Alert",
                        ImGuiWindowFlags.NoDecoration | ImGuiWindowFlags.NoMove | ImGuiWindowFlags.NoResize |
                        ImGuiWindowFlags.NoSavedSettings | ImGuiWindowFlags.NoFocusOnAppearing |
                        ImGuiWindowFlags.NoNav))
                {
                    ImGui.Text(alertString);
                    ImGui.End();
                }
            }
            else alertString = "";

            if (!open) return;
            ImGui.SetNextWindowSize(new Vector2(325, 300), ImGuiCond.Appearing);
            if (ImGui.Begin("Overlayer v" + (Application.isEditor ? "Test" : Main.ModEntry.Info.Version), ref open,
                    ImGuiWindowFlags.MenuBar))
            {
                if (ImGui.Button("Add Text##ADD_TEXT"))
                {
                    var txt_ = Instantiate(TextTemplate, SettingGUIManager.Instance.rimg.transform.parent);
                    txt_.transform.SetAsFirstSibling();
                    var txt = txt_.GetComponent<OverlayerText>();
                    txt.Name = "New Text";
                    objects.Add(txt);
                }

                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    ImGui.SetTooltip("Adds the text");
                }

                ImGui.Checkbox("Simulate Playing##SIM_PLAYING", ref sim_play);
                if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                {
                    ImGui.SetTooltip("Simulates the playing state");
                }

                foreach (var obj in objects)
                {
                    bool active = obj.gameObject.activeSelf;
                    ImGui.Checkbox("##ACTIVE_" + obj.Id, ref active);
                    obj.gameObject.SetActive(active);
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Activates or deactivates the object");
                    }

                    ImGui.SameLine();
                    ImGui.Checkbox("##DRAG_LOCK_" + obj.Id, ref obj.DragLock);
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Locks the object from being dragged");
                    }

                    ImGui.SameLine();
                    ImGui.SetNextItemWidth(100);
                    ImGui.InputText("##NAME_CHANGE_" + obj.Id, ref obj.Name, 32);
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Name of the object");
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("Delete##DELETE_OBJ_" + obj.Id))
                    {
                        toRemove.Enqueue(obj);
                    }

                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Deletes the object");
                    }

                    ImGui.SameLine();
                    if (ImGui.Button("Config##CONFIG_OBJ_" + obj.Id))
                    {
                        obj.Configuring = true;
                    }

                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Opens a menu to change the configure of the object");
                    }
                }

                if (ImGui.BeginMenuBar())
                {
                    if (ImGui.BeginMenu("File"))
                    {
                        ImGui.MenuItem("Import Profile");
                        if (ImGui.BeginMenu("Migration"))
                        {
                            ImGui.MenuItem("Import V3 Profile");
                            ImGui.EndMenu();
                        }

                        ImGui.EndMenu();
                    }

                    ImGui.EndMenuBar();
                }

                while (toRemove.Count > 0)
                {
                    var obj = toRemove.Dequeue();
                    objects.Remove(obj);
                    Destroy(obj.gameObject);
                }

                ImGui.End();
            }

            foreach (var obj in objects)
            {
                if (!obj.Configuring) continue;

                ImGui.SetNextWindowSize(new Vector2(400, 600), ImGuiCond.Appearing);
                ImGui.SetNextWindowPos(new Vector2(Screen.width / 2f - 400f / 2f, Screen.height / 2f - 600f / 2f),
                    ImGuiCond.Appearing);
                if (!ImGui.Begin(obj.Name + " Configuration###CONFIG_" + obj.Id,
                        ref obj.Configuring)) continue;


                if (ImGui.CollapsingHeader("Transform##TransformCompo_" + obj.Id))
                {

                    Vector2 pos = obj.transform.localPosition;
                    ImGui.DragFloat2("Position##POS_" + obj.Id, ref pos, 1);
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Position of the object (you can drag on it)");
                    }

                    obj.transform.localPosition = pos;

                    var rot = obj.transform.eulerAngles.z;
                    ImGui.DragFloat("Rotation##ROT_" + obj.Id, ref rot, 1);
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Rotation of the object (you can drag on it)");
                    }

                    obj.transform.eulerAngles = new Vector3(0, 0, rot);

                    var pivot = obj._rectTransform.pivot;
                    ImGui.DragFloat2("Pivot##PIVOT_" + obj.Id, ref pivot, 0.01f, 0f, 1f);
                    if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                    {
                        ImGui.SetTooltip("Pivot of the object (you can drag on it)");
                    }

                    obj._rectTransform.pivot = pivot;
                }

                if (obj is OverlayerText txt)
                {
                    if (ImGui.CollapsingHeader("Text##TextCompo_" + obj.Id))
                    {

                        var text = txt.PlayingText.text;
                        ImGui.InputTextMultiline("Playing Text##TEXT_" + txt.Id, ref text, 65536, new Vector2(0, 150));
                        txt.PlayingText.text = text;

                        text = txt.NotPlayingText.text;
                        ImGui.InputTextMultiline("Not Playing Text##NP_TEXT_" + txt.Id, ref text, 65536,
                            new Vector2(0, 150));
                        txt.NotPlayingText.text = text;

                        var fntSize = txt.text.fontSize;
                        ImGui.DragFloat("Font Size##FONT_SIZE_" + txt.Id, ref fntSize, 1);
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                        {
                            ImGui.SetTooltip("Font size of the text (you can drag on it)");
                        }

                        txt.text.fontSize = fntSize;

                        var color = (Vector4)txt.text.color;
                        ImGui.ColorEdit4("Color##COLOR_" + txt.Id, ref color);
                        if (ImGui.IsItemHovered(ImGuiHoveredFlags.AllowWhenDisabled))
                        {
                            ImGui.SetTooltip("Color of the text");
                        }

                        txt.text.color = color;

                        var align = txt.text.alignment;
                        EnumCombo(ref align, "Alignment##ALIGN_" + txt.Id);
                        txt.text.alignment = align;
                    }

                }

                ImGui.End();
            }
            //ImGui.ShowDemoWindow();
        }
    }
}