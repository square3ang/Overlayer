using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NCalc;
using Overlayer.Models;
using RapidGUI;
using UnityEngine;

namespace Overlayer.Core;

public class NeoDrawer {
    public static NeoDrawer StaticInstance = new();

    public class NeoField {
        public enum StateType {
            OK = 0,
            ERROR = 1,
            WARNING = 2,
            COMPUTE = 3
        }

        public bool StrInitialized;
        public string Str;
        public StateType State;
        public object ComputedValue;
    }

    private string LastFocused;

    private uint id;
    private readonly Dictionary<string, NeoField> fields = [];

    public bool StrInitialize(ref NeoField field, string str) {
        if (!field.StrInitialized) {
            field.Str = str;
            field.StrInitialized = true;
            return true;
        }

        return false;
    }

    public uint FieldGetId() {
        return id;
    }

    public void FieldSetId(uint value) {
        id = value;
    }

    public void FieldIncId() {
        id++;
    }

    public void FieldResetId() {
        id = 0;
    }

    public void FieldResetDictById() {
        var keysToRemove = fields.Keys
            .Where(k => uint.TryParse(k, out _))
            .ToList();

        foreach (var key in keysToRemove) fields.Remove(key);
    }

    public void FieldClear() {
        id = 0;
        fields.Clear();
    }

    public NeoField FieldGet(string uniqueID = null) {
        var key = uniqueID ?? id++.ToString();
        if (!fields.TryGetValue(key, out var field)) {
            field = new NeoField();
            fields[key] = field;
        }

        return field;
    }

    public string FieldGetName(string uniqueID = null) {
        return $"Field_{uniqueID ?? (id - 1).ToString()}";
    }

    public void FieldsRemove(params string[] keys) {
        foreach (var key in keys) fields.Remove(key);
    }

    public void UpdateFocused() {
        LastFocused = GUI.GetNameOfFocusedControl();
    }

    public object Calc(string exprStr) {
        var expr = new Expression(exprStr);

        expr.EvaluateParameter += (name, args) => {
            switch (name.ToUpperInvariant()) {
                case "PI":
                    args.Result = Math.PI;
                    break;
                case "E":
                    args.Result = Math.E;
                    break;
            }
        };

        try {
            return expr.Evaluate();
        }
        catch {
            return null;
        }
    }

    private bool ApplyFieldValueOnEvent(ref NeoField field, string fieldName, ref object value, Type type) {
        var focused = GUI.GetNameOfFocusedControl();

        var shouldApply =
            ((focused == fieldName && Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
             || LastFocused != fieldName)
            && (field.State == NeoField.StateType.COMPUTE || field.State == NeoField.StateType.WARNING);

        if (shouldApply)
            try {
                switch (Type.GetTypeCode(type)) {
                    case TypeCode.Boolean:
                        value = Convert.ToBoolean(field.ComputedValue);
                        break;
                    case TypeCode.Char:
                        value = Convert.ToChar(field.ComputedValue);
                        break;
                    case TypeCode.SByte:
                        value = Convert.ToSByte(field.ComputedValue);
                        break;
                    case TypeCode.Byte:
                        value = Convert.ToByte(field.ComputedValue);
                        break;
                    case TypeCode.Int16:
                        value = Convert.ToInt16(field.ComputedValue);
                        break;
                    case TypeCode.UInt16:
                        value = Convert.ToUInt16(field.ComputedValue);
                        break;
                    case TypeCode.Int32:
                        value = Convert.ToInt32(field.ComputedValue);
                        break;
                    case TypeCode.UInt32:
                        value = Convert.ToUInt32(field.ComputedValue);
                        break;
                    case TypeCode.Int64:
                        value = Convert.ToInt64(field.ComputedValue);
                        break;
                    case TypeCode.UInt64:
                        value = Convert.ToUInt64(field.ComputedValue);
                        break;
                    case TypeCode.Single:
                        value = Convert.ToSingle(field.ComputedValue);
                        break;
                    case TypeCode.Double:
                        value = Convert.ToDouble(field.ComputedValue);
                        break;
                    case TypeCode.Decimal:
                        value = Convert.ToDecimal(field.ComputedValue);
                        break;
                    case TypeCode.String:
                        value = Convert.ToString(field.ComputedValue);
                        break;
                    default:
                        field.State = NeoField.StateType.ERROR;
                        return false;
                }

                field.Str = value.ToString();
                field.State = NeoField.StateType.OK;
                return true;
            }
            catch {
                field.State = NeoField.StateType.ERROR;
                return false;
            }

        return false;
    }

    public void ColorbyState(NeoField.StateType state) {
        GUI.color = state switch {
            NeoField.StateType.ERROR => new Color(1f, 0.5f, 0.5f),
            NeoField.StateType.WARNING => new Color(1f, 1f, 0.5f),
            NeoField.StateType.COMPUTE => new Color(0.5f, 1f, 0.5f),
            _ => Color.white
        };
    }

    public string StatebyState(NeoField.StateType state) {
        return state switch {
            NeoField.StateType.ERROR => "<color=#FF8888>!!</color>",
            NeoField.StateType.WARNING => "<color=#FFFF88>!</color>",
            NeoField.StateType.COMPUTE => "<color=#88FF88>✓</color>",
            _ => ""
        };
    }

    public bool DrawVector3(string label, ref Vector3 vec3, float lValue, float rValue, string uniqueID = null) {
        GUILayout.Label(label);
        return DrawVector3(ref vec3, lValue, rValue, uniqueID);
    }

    public bool DrawVector3(ref Vector3 vec3, float lValue, float rValue, string uniqueID = null) {
        var changed = false;
        if (uniqueID == null) {
            changed |= DrawSingleWithSlider("X", ref vec3.x, lValue, rValue, 300f);
            changed |= DrawSingleWithSlider("Y", ref vec3.y, lValue, rValue, 300f);
            changed |= DrawSingleWithSlider("Z", ref vec3.z, lValue, rValue, 300f);
        }
        else {
            changed |= DrawSingleWithSlider("X", ref vec3.x, lValue, rValue, 300f, uniqueID + "_0");
            changed |= DrawSingleWithSlider("Y", ref vec3.y, lValue, rValue, 300f, uniqueID + "_1");
            changed |= DrawSingleWithSlider("Z", ref vec3.z, lValue, rValue, 300f, uniqueID + "_2");
        }

        return changed;
    }

    public bool DrawRotate3(string label, ref Vector3 vec3, float lValue, float rValue, string uniqueID = null) {
        GUILayout.Label(label);
        return DrawRotate3(ref vec3, lValue, rValue, uniqueID);
    }

    public bool DrawRotate3(ref Vector3 vec3, float lValue, float rValue, string uniqueID = null) {
        var changed = false;
        var old = GUI.color;
        if (uniqueID == null) {
            GUI.color = new Color(1.0f, 0.68f, 0.68f);
            changed |= DrawSingleWithSlider(Drawer.Icon_XRotate, "X", ref vec3.x, lValue, rValue, 300f);
            GUI.color = new Color(0.68f, 1.0f, 0.68f);
            changed |= DrawSingleWithSlider(Drawer.Icon_YRotate, "Y", ref vec3.y, lValue, rValue, 300f);
            GUI.color = new Color(0.68f, 0.68f, 1.0f);
            changed |= DrawSingleWithSlider(Drawer.Icon_ZRotate, "Z", ref vec3.z, lValue, rValue, 300f);
        }
        else {
            GUI.color = new Color(1.0f, 0.68f, 0.68f);
            changed |= DrawSingleWithSlider(Drawer.Icon_XRotate, "X", ref vec3.x, lValue, rValue, 300f,
                uniqueID + "_0");
            GUI.color = new Color(0.68f, 1.0f, 0.68f);
            changed |= DrawSingleWithSlider(Drawer.Icon_YRotate, "Y", ref vec3.y, lValue, rValue, 300f,
                uniqueID + "_1");
            GUI.color = new Color(0.68f, 0.68f, 1.0f);
            changed |= DrawSingleWithSlider(Drawer.Icon_ZRotate, "Z", ref vec3.z, lValue, rValue, 300f,
                uniqueID + "_2");
        }

        GUI.color = old;
        return changed;
    }

    public bool DrawVector2(string label, ref Vector2 vec2, float lValue, float rValue, string uniqueID = null) {
        GUILayout.Label(label);
        return DrawVector2(ref vec2, lValue, rValue, uniqueID);
    }

    public bool DrawVector2(ref Vector2 vec2, float lValue, float rValue, string uniqueID = null) {
        var changed = false;
        if (uniqueID == null) {
            changed |= DrawSingleWithSlider("X", ref vec2.x, lValue, rValue, 300f);
            changed |= DrawSingleWithSlider("Y", ref vec2.y, lValue, rValue, 300f);
        }
        else {
            changed |= DrawSingleWithSlider("X", ref vec2.x, lValue, rValue, 300f, uniqueID + "_0");
            changed |= DrawSingleWithSlider("Y", ref vec2.y, lValue, rValue, 300f, uniqueID + "_1");
        }

        return changed;
    }

    public bool DrawSize2(string label, ref Vector2 vec2, float lValue, float rValue, string uniqueID = null) {
        GUILayout.Label(label);
        return DrawSize2(ref vec2, lValue, rValue, uniqueID);
    }

    public bool DrawSize2(ref Vector2 vec2, float lValue, float rValue, string uniqueID = null) {
        var changed = false;
        var old = GUI.color;
        if (uniqueID == null) {
            GUI.color = new Color(1.0f, 0.68f, 0.68f);
            changed |= DrawSingleWithSlider(Drawer.Icon_LeftRight, "X", ref vec2.x, lValue, rValue, 300f);
            GUI.color = new Color(0.68f, 1.0f, 0.68f);
            changed |= DrawSingleWithSlider(Drawer.Icon_UpDown, "Y", ref vec2.y, lValue, rValue, 300f);
        }
        else {
            GUI.color = new Color(1.0f, 0.68f, 0.68f);
            changed |= DrawSingleWithSlider(Drawer.Icon_LeftRight, "X", ref vec2.x, lValue, rValue, 300f,
                uniqueID + "_0");
            GUI.color = new Color(0.68f, 1.0f, 0.68f);
            changed |= DrawSingleWithSlider(Drawer.Icon_UpDown, "Y", ref vec2.y, lValue, rValue, 300f, uniqueID + "_1");
        }

        GUI.color = old;
        return changed;
    }

    public bool DrawColor(string label, ref Color color, float cWidth = 460f, string uniqueID = null) {
        bool changed;
        GUILayout.BeginHorizontal();
        GUILayout.Label(label);
        GUILayout.Space(4f);
        changed = DrawColor(ref color, cWidth, uniqueID);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        return changed;
    }

    public bool DrawColor(ref Color color, float cWidth = 460f, string uniqueID = null) {
        var changed = false;

        var field = FieldGet(uniqueID);
        StrInitialize(ref field, ColorUtility.ToHtmlStringRGBA(color));

        var old = GUI.color;
        GUI.color = field.State == NeoField.StateType.ERROR ? new Color(1f, 0.5f, 0.5f) : Color.white;

        GUI.SetNextControlName(FieldGetName(uniqueID));
        var newHex = GUILayout.TextField(field.Str, 8, Drawer.myTextFieldNoPad, GUILayout.Width(80f));

        if (newHex != field.Str) {
            field.Str = newHex;
            changed = true;

            if (ColorUtility.TryParseHtmlString("#" + field.Str, out var parsed)) {
                color = parsed;
                field.State = NeoField.StateType.OK;
            }
            else {
                field.State = NeoField.StateType.ERROR;
            }
        }

        GUI.color = old;
        GUILayout.Space(2f);

        GUILayout.Label(StatebyState(field.State), GUILayout.Width(10));

        var newColor = RGUI.Field(color, "", GUILayout.Width(cWidth));

        if (newColor != color) {
            color = newColor;
            changed = true;
            field.Str = ColorUtility.ToHtmlStringRGBA(color);
            field.State = NeoField.StateType.OK;
        }

        return changed;
    }

    public bool DrawGColor(ref GColor color, float cWidth = 460f, string uniqueID = null) {
        var prevGe = color.gradientEnabled;
        var ge = prevGe;

        Drawer.BeginTab();
        if (Drawer.DrawBool(Drawer.Icon_Gradation, Main.Lang.Get("MISC_ENABLE_GRADIENT", "Enable Gradient"), ref ge))
            color = color with { gradientEnabled = ge };
        Drawer.EndTab();

        color = color with { gradientEnabled = color.gradientEnabled };

        var changed = ge != prevGe;

        if (color.gradientEnabled) {
            var fieldTL = FieldGet(uniqueID);
            NeoField fieldTR;
            NeoField fieldBL;
            NeoField fieldBR;

            if (string.IsNullOrEmpty(uniqueID)) {
                fieldTR = FieldGet();
                fieldBL = FieldGet();
                fieldBR = FieldGet();
            }
            else {
                fieldTR = FieldGet(uniqueID + "_1");
                fieldBL = FieldGet(uniqueID + "_2");
                fieldBR = FieldGet(uniqueID + "_3");
            }

            StrInitialize(ref fieldTL, color.topLeftHex);
            StrInitialize(ref fieldTR, color.topRightHex);
            StrInitialize(ref fieldBL, color.bottomLeftHex);
            StrInitialize(ref fieldBR, color.bottomRightHex);

            if (changed && ge) {
                fieldTL.Str = color.topLeftHex;
                fieldTR.Str = color.topRightHex;
                fieldBL.Str = color.bottomLeftHex;
                fieldBR.Str = color.bottomRightHex;

                fieldTL.State = NeoField.StateType.OK;
                fieldTR.State = NeoField.StateType.OK;
                fieldBL.State = NeoField.StateType.OK;
                fieldBR.State = NeoField.StateType.OK;
            }

            /* ! TOP ! */

            GUILayout.BeginHorizontal();

            // TL
            var newColorTL = RGUI.Field(color.topLeft, "", GUILayout.Width(cWidth / 2.6f));
            GUILayout.Space(2f);

            var old = GUI.color;
            if (fieldTL.State == NeoField.StateType.ERROR) GUI.color = new Color(1f, 0.5f, 0.5f);

            GUI.SetNextControlName(FieldGetName(uniqueID));
            var newHexTL = GUILayout.TextField(fieldTL.Str, 8, Drawer.myTextFieldNoPad, GUILayout.Width(80f));
            GUI.color = old;

            if (newHexTL != fieldTL.Str) {
                fieldTL.Str = newHexTL;
                changed = true;

                if (ColorUtility.TryParseHtmlString("#" + fieldTL.Str, out var parsed)) {
                    color.topLeft = parsed;
                    fieldTL.State = NeoField.StateType.OK;
                }
                else {
                    fieldTL.State = NeoField.StateType.ERROR;
                }
            }

            if (newColorTL != color.topLeft) {
                color.topLeft = newColorTL;
                changed = true;

                fieldTL.Str = color.topLeftHex;
                fieldTL.State = NeoField.StateType.OK;
            }

            if (fieldTL.State == NeoField.StateType.ERROR) GUI.color = new Color(1f, 0.5f, 0.5f);

            GUILayout.Space(4f);
            GUILayout.Label("↖", GUILayout.Width(16));
            GUI.color = old;

            // TR
            if (fieldTR.State == NeoField.StateType.ERROR) GUI.color = new Color(1f, 0.5f, 0.5f);

            GUILayout.Label("↗", GUILayout.Width(16));
            GUI.SetNextControlName(FieldGetName(uniqueID + "_1"));
            var newHexTR = GUILayout.TextField(fieldTR.Str, 8, Drawer.myTextFieldNoPad, GUILayout.Width(80f));
            GUI.color = old;

            if (newHexTR != fieldTR.Str) {
                fieldTR.Str = newHexTR;
                changed = true;

                if (ColorUtility.TryParseHtmlString("#" + fieldTR.Str, out var parsed)) {
                    color.topRight = parsed;
                    fieldTR.State = NeoField.StateType.OK;
                }
                else {
                    fieldTR.State = NeoField.StateType.ERROR;
                }
            }

            var newColorTR = RGUI.Field(color.topRight, "", GUILayout.Width(cWidth / 2.6f));
            if (newColorTR != color.topRight) {
                color.topRight = newColorTR;
                changed = true;

                fieldTR.Str = color.topLeftHex;
                fieldTR.State = NeoField.StateType.OK;
            }

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();

            /* ! BOTTOM ! */

            GUILayout.BeginHorizontal();

            // BL
            var newColorBL = RGUI.Field(color.bottomLeft, "", GUILayout.Width(cWidth / 2.6f));
            GUILayout.Space(2f);

            if (fieldBL.State == NeoField.StateType.ERROR) GUI.color = new Color(1f, 0.5f, 0.5f);

            GUI.SetNextControlName(FieldGetName(uniqueID + "_2"));
            var newHexBL = GUILayout.TextField(fieldBL.Str, 8, Drawer.myTextFieldNoPad, GUILayout.Width(80f));
            GUI.color = old;

            if (newHexBL != fieldBL.Str) {
                fieldBL.Str = newHexBL;
                changed = true;

                if (ColorUtility.TryParseHtmlString("#" + fieldBL.Str, out var parsed)) {
                    color.bottomLeft = parsed;
                    fieldBL.State = NeoField.StateType.OK;
                }
                else {
                    fieldBL.State = NeoField.StateType.ERROR;
                }
            }

            if (newColorBL != color.bottomLeft) {
                color.bottomLeft = newColorBL;
                changed = true;

                fieldBL.Str = color.topLeftHex;
                fieldBL.State = NeoField.StateType.OK;
            }

            if (fieldBL.State == NeoField.StateType.ERROR) GUI.color = new Color(1f, 0.5f, 0.5f);

            GUILayout.Space(4f);
            GUILayout.Label("↙", GUILayout.Width(16));
            GUI.color = old;

            // BR
            if (fieldBR.State == NeoField.StateType.ERROR) GUI.color = new Color(1f, 0.5f, 0.5f);

            GUILayout.Label("↘", GUILayout.Width(16));
            GUI.SetNextControlName(FieldGetName(uniqueID + "_3"));
            var newHexBR = GUILayout.TextField(fieldBR.Str, 8, Drawer.myTextFieldNoPad, GUILayout.Width(80f));
            GUI.color = old;

            if (newHexBR != fieldBR.Str) {
                fieldBR.Str = newHexBR;
                changed = true;

                if (ColorUtility.TryParseHtmlString("#" + fieldBR.Str, out var parsed)) {
                    color.bottomRight = parsed;
                    fieldBR.State = NeoField.StateType.OK;
                }
                else {
                    fieldBR.State = NeoField.StateType.ERROR;
                }
            }

            var newColorBR = RGUI.Field(color.bottomRight, "", GUILayout.Width(cWidth / 2.6f));
            if (newColorBR != color.bottomRight) {
                color.bottomRight = newColorBR;
                changed = true;

                fieldBR.Str = color.topLeftHex;
                fieldBR.State = NeoField.StateType.OK;
            }

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        }
        else {
            var all = color.topLeft;
            GUILayout.BeginHorizontal();
            if (changed = DrawColor(ref all, cWidth, uniqueID)) color = all;
            GUILayout.FlexibleSpace();
            GUILayout.EndHorizontal();
        }

        return changed;
    }

    public bool DrawSingle(string label, ref float value, string uniqueID = null) {
        bool changed;
        GUILayout.BeginHorizontal();
        GUILayout.Label(label);
        GUILayout.Space(4f);
        changed = DrawSingle(ref value, uniqueID);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        return changed;
    }

    public bool DrawSingle(ref float value, string uniqueID = null) {
        var field = FieldGet(uniqueID);
        StrInitialize(ref field, value.ToString());
        var changed = false;

        var old = GUI.color;
        ColorbyState(field.State);

        var fieldName = FieldGetName(uniqueID);
        GUI.SetNextControlName(fieldName);
        var newField = GUILayout.TextField(field.Str, Drawer.myTextField);
        GUI.color = old;

        if (newField != field.Str) {
            field.Str = newField;
            if (string.IsNullOrEmpty(field.Str)) {
                field.State = NeoField.StateType.ERROR;
            }
            else {
                if (float.TryParse(newField, out var parsed)) {
                    value = parsed;
                    field.ComputedValue = parsed;
                    field.State = NeoField.StateType.OK;
                    changed = true;
                }
                else {
                    var result = Calc(field.Str);
                    if (result == null) {
                        field.State = NeoField.StateType.ERROR;
                    }
                    else {
                        var computed = Convert.ToSingle(result);
                        field.ComputedValue = computed;
                        field.State = float.IsNaN(computed) || float.IsInfinity(computed)
                            ? NeoField.StateType.WARNING
                            : NeoField.StateType.COMPUTE;
                    }
                }
            }
        }

        object objValue = value;
        if (ApplyFieldValueOnEvent(ref field, fieldName, ref objValue, typeof(float))) {
            value = (float)objValue;
            changed = true;
        }

        GUILayout.Space(2f);
        GUILayout.Label(StatebyState(field.State), GUILayout.Width(12));
        return changed;
    }

    public bool DrawSingleWithSlider(Texture2D icon, string label, ref float value, float lValue, float rValue,
        float width, string uniqueID = null) {
        bool changed;
        GUILayout.BeginHorizontal();
        GUILayout.Label(icon);
        GUILayout.Label(label);
        GUILayout.Space(4f);
        changed = DrawSingleWithSlider(ref value, lValue, rValue, width, uniqueID);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        return changed;
    }

    public bool DrawSingleWithSlider(string label, ref float value, float lValue, float rValue, float width,
        string uniqueID = null) {
        bool changed;
        GUILayout.BeginHorizontal();
        GUILayout.Label(label);
        GUILayout.Space(4f);
        changed = DrawSingleWithSlider(ref value, lValue, rValue, width, uniqueID);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        return changed;
    }

    public bool DrawSingleWithSlider(ref float value, float lValue, float rValue, float width, string uniqueID = null) {
        var field = FieldGet(uniqueID);
        StrInitialize(ref field, value.ToString());
        var changed = false;

        var sliderValue = GUILayout.HorizontalSlider(value, lValue, rValue, Drawer.mySlider, Drawer.myThumb,
            GUILayout.Width(width));
        if (sliderValue != value) {
            value = sliderValue;
            field.Str = value.ToString();
            field.State = NeoField.StateType.OK;
            changed = true;
        }

        GUILayout.Space(8f);

        var old = GUI.color;
        ColorbyState(field.State);

        var fieldName = FieldGetName(uniqueID);
        GUI.SetNextControlName(fieldName);
        var newField = GUILayout.TextField(field.Str, Drawer.myTextField);
        GUI.color = old;

        if (newField != field.Str) {
            field.Str = newField;
            if (string.IsNullOrEmpty(field.Str)) {
                field.State = NeoField.StateType.ERROR;
            }
            else {
                if (float.TryParse(newField, out var parsed)) {
                    value = parsed;
                    field.ComputedValue = parsed;
                    field.State = NeoField.StateType.OK;
                    changed = true;
                }
                else {
                    var result = Calc(field.Str);
                    if (result == null) {
                        field.State = NeoField.StateType.ERROR;
                    }
                    else {
                        var computed = Convert.ToSingle(result);
                        field.ComputedValue = computed;
                        field.State = float.IsNaN(computed) || float.IsInfinity(computed)
                            ? NeoField.StateType.WARNING
                            : NeoField.StateType.COMPUTE;
                    }
                }
            }
        }

        object objValue = value;
        if (ApplyFieldValueOnEvent(ref field, fieldName, ref objValue, typeof(float))) {
            value = (float)objValue;
            changed = true;
        }

        GUILayout.Space(2f);
        GUILayout.Label(StatebyState(field.State), GUILayout.Width(12));

        return changed;
    }

    public bool DrawDouble(string label, ref double value, string uniqueID = null) {
        bool changed;
        GUILayout.BeginHorizontal();
        GUILayout.Label(label);
        GUILayout.Space(4f);
        changed = DrawDouble(ref value, uniqueID);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        return changed;
    }

    public bool DrawDouble(ref double value, string uniqueID = null) {
        var field = FieldGet(uniqueID);
        StrInitialize(ref field, value.ToString());
        var changed = false;

        var old = GUI.color;
        ColorbyState(field.State);

        var fieldName = FieldGetName(uniqueID);
        GUI.SetNextControlName(fieldName);
        var newField = GUILayout.TextField(field.Str, Drawer.myTextField);
        GUI.color = old;

        if (newField != field.Str) {
            field.Str = newField;
            if (string.IsNullOrEmpty(field.Str)) {
                field.State = NeoField.StateType.ERROR;
            }
            else {
                if (double.TryParse(newField, out var parsed)) {
                    value = parsed;
                    field.ComputedValue = parsed;
                    field.State = NeoField.StateType.OK;
                    changed = true;
                }
                else {
                    var result = Calc(field.Str);
                    if (result == null) {
                        field.State = NeoField.StateType.ERROR;
                    }
                    else {
                        double computed = Convert.ToSingle(result);
                        field.ComputedValue = computed;
                        field.State = double.IsNaN(computed) || double.IsInfinity(computed)
                            ? NeoField.StateType.WARNING
                            : NeoField.StateType.COMPUTE;
                    }
                }
            }
        }

        object objValue = value;
        if (ApplyFieldValueOnEvent(ref field, fieldName, ref objValue, typeof(double))) {
            value = (double)objValue;
            changed = true;
        }

        GUILayout.Space(2f);
        GUILayout.Label(StatebyState(field.State), GUILayout.Width(12));

        return changed;
    }

    public bool DrawInt32(string label, ref int value, string uniqueID = null) {
        bool changed;
        GUILayout.BeginHorizontal();
        GUILayout.Label(label);
        GUILayout.Space(4f);
        changed = DrawInt32(ref value, uniqueID);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        return changed;
    }

    public bool DrawInt32(ref int value, string uniqueID = null) {
        var field = FieldGet(uniqueID);
        StrInitialize(ref field, value.ToString());
        var changed = false;

        var old = GUI.color;
        ColorbyState(field.State);

        var fieldName = FieldGetName(uniqueID);
        GUI.SetNextControlName(fieldName);
        var newField = GUILayout.TextField(field.Str, Drawer.myTextField);
        GUI.color = old;

        if (newField != field.Str) {
            field.Str = newField;
            if (string.IsNullOrEmpty(field.Str)) {
                field.State = NeoField.StateType.ERROR;
            }
            else {
                if (int.TryParse(newField, out var parsed)) {
                    value = parsed;
                    field.ComputedValue = parsed;
                    field.State = NeoField.StateType.OK;
                    changed = true;
                }
                else {
                    var result = Calc(field.Str);
                    if (result == null) {
                        field.State = NeoField.StateType.ERROR;
                    }
                    else {
                        var computed = Convert.ToDouble(result);
                        if (double.IsNaN(computed) || double.IsInfinity(computed)) {
                            field.State = NeoField.StateType.ERROR;
                        }
                        else if (computed > int.MaxValue) {
                            field.State = NeoField.StateType.WARNING;
                            field.ComputedValue = int.MaxValue;
                        }
                        else if (computed < int.MinValue) {
                            field.State = NeoField.StateType.WARNING;
                            field.ComputedValue = int.MinValue;
                        }
                        else {
                            var computedInt = (int)Math.Round(computed);
                            field.ComputedValue = computedInt;
                            field.State = NeoField.StateType.COMPUTE;
                        }
                    }
                }
            }
        }

        object objValue = value;
        if (ApplyFieldValueOnEvent(ref field, fieldName, ref objValue, typeof(int))) {
            value = (int)objValue;
            changed = true;
        }

        GUILayout.Space(2f);
        GUILayout.Label(StatebyState(field.State), GUILayout.Width(12));

        return changed;
    }

    public bool DrawPath(Texture2D icon, string label, ref string name, string path = null, string extension = null,
        string uniqueID = null) {
        bool changed;
        GUILayout.BeginHorizontal();
        GUILayout.Label(icon);
        GUILayout.Label(label);
        GUILayout.Space(4f);
        changed = DrawPath(ref name, path, extension, uniqueID);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        return changed;
    }

    public bool DrawPath(string label, ref string name, string path = null, string extension = null,
        string uniqueID = null) {
        bool changed;
        GUILayout.BeginHorizontal();
        GUILayout.Label(label);
        GUILayout.Space(4f);
        changed = DrawPath(ref name, path, extension, uniqueID);
        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();
        return changed;
    }

    public bool DrawPath(ref string name, string path = null, string extension = null, string uniqueID = null) {
        var field = FieldGet(uniqueID);
        StrInitialize(ref field, name);
        var changed = false;

        var old = GUI.color;
        ColorbyState(field.State);

        var fieldName = FieldGetName(uniqueID);
        GUI.SetNextControlName(fieldName);
        var newField = GUILayout.TextField(field.Str, Drawer.myTextField);
        GUI.color = old;

        if (newField != field.Str) {
            field.Str = newField;

            if (field.Str == name) {
                field.State = NeoField.StateType.OK;
            }
            else {
                var invalidChars = Path.GetInvalidFileNameChars();
                if (string.IsNullOrEmpty(field.Str) || field.Str.IndexOfAny(invalidChars) >= 0) {
                    field.State = NeoField.StateType.ERROR;
                }
                else if (!string.IsNullOrEmpty(path)) {
                    var ext = string.IsNullOrEmpty(extension) ? "" :
                        extension.StartsWith(".") ? extension : "." + extension;
                    var fullPath = Path.Combine(path, field.Str + ext);

                    if (File.Exists(fullPath) || Directory.Exists(fullPath)) {
                        field.State = NeoField.StateType.ERROR;
                    }
                    else {
                        field.ComputedValue = field.Str;
                        field.State = NeoField.StateType.COMPUTE;
                    }
                }
                else {
                    field.ComputedValue = field.Str;
                    field.State = NeoField.StateType.COMPUTE;
                }
            }
        }

        object objValue = name;
        if (ApplyFieldValueOnEvent(ref field, fieldName, ref objValue, typeof(string))) {
            name = (string)objValue;
            changed = true;
        }

        GUILayout.Space(2f);
        GUILayout.Label(StatebyState(field.State), GUILayout.Width(12));

        return changed;
    }
}