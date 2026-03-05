using NCalc;
using Overlayer.Models;
using RapidGUI;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Overlayer.Core;

public class NeoDrawer {
    public static NeoDrawer StaticInstance = new();
    public class NeoField {
        public enum StateType {
            OK = 0,
            ERROR = 1,
            WARNING = 2,
            COMPUTE = 3,
        }

        public bool StrInitialized = false;
        public string Str;
        public StateType State;
        public object ComputedValue;

    }

    private string LastFocused;

    private uint id = 0;
    private Dictionary<string, NeoField> fields = new();

    public bool StrInitialize(ref NeoField field, string str) {
        if(!field.StrInitialized) {
            field.Str = str;
            field.StrInitialized = true;
            return true;
        }
        return false;
    }

    public uint FieldGetId() => id;
    public void FieldSetId(uint value) => id = value;
    public void FieldIncId() => id++;

    public void FieldResetId() => id = 0;
    public void FieldResetDictById() {
        var keysToRemove = fields.Keys
            .Where(k => uint.TryParse(k, out _))
            .ToList();

        foreach(var key in keysToRemove) {
            fields.Remove(key);
        }
    }
    public void FieldClear() {
        id = 0;
        fields.Clear();
    }

    public NeoField FieldGet(string uniqueID = null) {
        string key = uniqueID ?? id++.ToString();
        if(!fields.TryGetValue(key, out NeoField field)) {
            field = new NeoField();
            fields[key] = field;
        }
        return field;
    }

    public string FieldGetName(string uniqueID = null) => $"Field_{uniqueID ?? (id - 1).ToString()}";

    public void FieldsRemove(params string[] keys) {
        foreach(var key in keys) {
            fields.Remove(key);
        }
    }

    public void UpdateFocused() => LastFocused = GUI.GetNameOfFocusedControl();

    public object Calc(string exprStr) {
        var expr = new Expression(exprStr);

        expr.EvaluateParameter += (name, args) => {
            switch(name.ToUpperInvariant()) {
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
        } catch {
            return null;
        }
    }

    private bool ApplyFieldValueOnEvent(ref NeoField field, string fieldName, ref object value, Type type) {
        string focused = GUI.GetNameOfFocusedControl();

        bool shouldApply =
            ((focused == fieldName && Event.current.type == EventType.KeyUp && Event.current.keyCode == KeyCode.Return)
            || (LastFocused != fieldName))
            && (field.State == NeoField.StateType.COMPUTE || field.State == NeoField.StateType.WARNING);

        if(shouldApply) {
            try {
                switch(Type.GetTypeCode(type)) {
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
                        if(float.TryParse(Convert.ToString(field.ComputedValue), out float parsed)) {
                            value = parsed;
                        } else {
                            field.State = NeoField.StateType.ERROR;
                        }

                        break;
                    default:
                        field.State = NeoField.StateType.ERROR;
                        return false;
                }

                field.Str = value.ToString();
                field.State = NeoField.StateType.OK;
                return true;
            } catch {
                field.State = NeoField.StateType.ERROR;
                return false;
            }
        }

        return false;
    }

    public void ColorbyState(NeoField.StateType state) {
        GUI.color = state switch {
            NeoField.StateType.ERROR => new Color(1f, 0.5f, 0.5f),
            NeoField.StateType.WARNING => new Color(1f, 1f, 0.5f),
            NeoField.StateType.COMPUTE => new Color(0.5f, 1f, 0.5f),
            _ => Color.white,
        };
    }

    public string StatebyState(NeoField.StateType state) {
        return state switch {
            NeoField.StateType.ERROR => "<color=#FF8888>!!</color>",
            NeoField.StateType.WARNING => "<color=#FFFF88>!</color>",
            NeoField.StateType.COMPUTE => "<color=#88FF88>✓</color>",
            _ => "",
        };
    }

    public bool DrawVector3(string label, ref Vector3 vec3, float lValue, float rValue, string uniqueID = null) {
        bool changed = false;
        GUILayout.Label($"<b>{label}</b>");
        if(uniqueID == null) {
            changed |= DrawSingleWithSlider("X", ref vec3.x, lValue, rValue, 300f);
            changed |= DrawSingleWithSlider("Y", ref vec3.y, lValue, rValue, 300f);
            changed |= DrawSingleWithSlider("Z", ref vec3.z, lValue, rValue, 300f);
        } else {
            changed |= DrawSingleWithSlider("X", ref vec3.x, lValue, rValue, 300f, uniqueID + "_0");
            changed |= DrawSingleWithSlider("Y", ref vec3.y, lValue, rValue, 300f, uniqueID + "_1");
            changed |= DrawSingleWithSlider("Z", ref vec3.z, lValue, rValue, 300f, uniqueID + "_2");
        }
        return changed;
    }

    public bool DrawRotate3(string label, ref Vector3 vec3, float lValue, float rValue, string uniqueID = null) {
        bool changed = false;
        GUILayout.Label($"<b>{label}</b>");
        Color old = GUI.color;
        if(uniqueID == null) {
            GUI.color = new Color(1.0f, 0.68f, 0.68f);
            changed |= DrawSingleWithSlider(Drawer.icon_XRotate, "X", ref vec3.x, lValue, rValue, 300f);
            GUI.color = new Color(0.68f, 1.0f, 0.68f);
            changed |= DrawSingleWithSlider(Drawer.icon_YRotate, "Y", ref vec3.y, lValue, rValue, 300f);
            GUI.color = new Color(0.68f, 0.68f, 1.0f);
            changed |= DrawSingleWithSlider(Drawer.icon_ZRotate, "Z", ref vec3.z, lValue, rValue, 300f);
        } else {
            GUI.color = new Color(1.0f, 0.68f, 0.68f);
            changed |= DrawSingleWithSlider(Drawer.icon_XRotate, "X", ref vec3.x, lValue, rValue, 300f, uniqueID + "_0");
            GUI.color = new Color(0.68f, 1.0f, 0.68f);
            changed |= DrawSingleWithSlider(Drawer.icon_YRotate, "Y", ref vec3.y, lValue, rValue, 300f, uniqueID + "_1");
            GUI.color = new Color(0.68f, 0.68f, 1.0f);
            changed |= DrawSingleWithSlider(Drawer.icon_ZRotate, "Z", ref vec3.z, lValue, rValue, 300f, uniqueID + "_2");
        }
        GUI.color = old;
        return changed;
    }

    public bool DrawVector2(string label, ref Vector2 vec2, float lValue, float rValue, string uniqueID = null) {
        bool changed = false;
        GUILayout.Label($"<b>{label}</b>");
        if(uniqueID == null) {
            changed |= DrawSingleWithSlider("X", ref vec2.x, lValue, rValue, 300f);
            changed |= DrawSingleWithSlider("Y", ref vec2.y, lValue, rValue, 300f);
        } else {
            changed |= DrawSingleWithSlider("X", ref vec2.x, lValue, rValue, 300f, uniqueID + "_0");
            changed |= DrawSingleWithSlider("Y", ref vec2.y, lValue, rValue, 300f, uniqueID + "_1");
        }
        return changed;
    }

    public bool DrawSize2(string label, ref Vector2 vec2, float lValue, float rValue, string uniqueID = null) {
        bool changed = false;
        GUILayout.Label($"<b>{label}</b>");
        Color old = GUI.color;
        if(uniqueID == null) {
            GUI.color = new Color(1.0f, 0.68f, 0.68f);
            changed |= DrawSingleWithSlider(Drawer.icon_LeftRight, "X", ref vec2.x, lValue, rValue, 300f);
            GUI.color = new Color(0.68f, 1.0f, 0.68f);
            changed |= DrawSingleWithSlider(Drawer.icon_UpDown, "Y", ref vec2.y, lValue, rValue, 300f);
        } else {
            GUI.color = new Color(1.0f, 0.68f, 0.68f);
            changed |= DrawSingleWithSlider(Drawer.icon_LeftRight, "X", ref vec2.x, lValue, rValue, 300f, uniqueID + "_0");
            GUI.color = new Color(0.68f, 1.0f, 0.68f);
            changed |= DrawSingleWithSlider(Drawer.icon_UpDown, "Y", ref vec2.y, lValue, rValue, 300f, uniqueID + "_1");
        }
        GUI.color = old;
        return changed;
    }

    public bool DrawColor(string label, ref Color color, float cWidth = 460f, string uniqueID = null) {
        bool changed = false;

        NeoField field = FieldGet(uniqueID);
        StrInitialize(ref field, ColorUtility.ToHtmlStringRGBA(color));

        GUILayout.BeginHorizontal();
        if(!string.IsNullOrEmpty(label)) {
            GUILayout.Label(label);
            GUILayout.Space(4f);
        }

        Color old = GUI.color;
        GUI.color = field.State == NeoField.StateType.ERROR ? new Color(1f, 0.5f, 0.5f) : Color.white;

        GUI.SetNextControlName(FieldGetName(uniqueID));
        string newHex = GUILayout.TextField(field.Str, 8, Drawer.myTextFieldNoPad, GUILayout.Width(80f));

        if(newHex != field.Str) {
            field.Str = newHex;
            changed = true;

            if(ColorUtility.TryParseHtmlString("#" + field.Str, out Color parsed)) {
                color = parsed;
                field.State = NeoField.StateType.OK;
            } else {
                field.State = NeoField.StateType.ERROR;
            }
        }

        GUI.color = old;
        GUILayout.Space(2f);

        GUILayout.Label(StatebyState(field.State), GUILayout.Width(10));

        Color newColor = RGUI.Field(color, "", GUILayout.Width(cWidth));

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        if(newColor != color) {
            color = newColor;
            changed = true;
            field.Str = ColorUtility.ToHtmlStringRGBA(color);
            field.State = NeoField.StateType.OK;
        }

        return changed;
    }

    public bool DrawGColor(ref GColor color, bool canEnableGradient, float cWidth = 220f, string uniqueID = null) {
        bool prevGe = color.gradientEnabled;
        bool ge = prevGe;

        if(canEnableGradient && Drawer.DrawBool(Drawer.icon_Gradation, Main.Lang.Get("MISC_ENABLE_GRADIENT", "Enable Gradient"), ref ge)) {
            color = color with { gradientEnabled = ge };
        }

        color = color with { gradientEnabled = color.gradientEnabled && canEnableGradient };

        bool changed = ge != prevGe;

        if(color.gradientEnabled && canEnableGradient) {

            NeoField fieldTL = FieldGet(uniqueID);
            NeoField fieldTR;
            NeoField fieldBL;
            NeoField fieldBR;

            if(string.IsNullOrEmpty(uniqueID)) {
                fieldTR = FieldGet();
                fieldBL = FieldGet();
                fieldBR = FieldGet();
            } else {
                fieldTR = FieldGet(uniqueID + "_1");
                fieldBL = FieldGet(uniqueID + "_2");
                fieldBR = FieldGet(uniqueID + "_3");
            }

            StrInitialize(ref fieldTL, color.topLeftHex);
            StrInitialize(ref fieldTR, color.topRightHex);
            StrInitialize(ref fieldBL, color.bottomLeftHex);
            StrInitialize(ref fieldBR, color.bottomRightHex);

            if(changed && ge) {
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
            Color newColorTL = RGUI.Field(color.topLeft, "", GUILayout.Width(cWidth));
            GUILayout.Space(2f);

            Color old = GUI.color;
            if(fieldTL.State == NeoField.StateType.ERROR) {
                GUI.color = new Color(1f, 0.5f, 0.5f);
            }

            GUI.SetNextControlName(FieldGetName(uniqueID));
            string newHexTL = GUILayout.TextField(fieldTL.Str, 8, Drawer.myTextFieldNoPad, GUILayout.Width(80f));
            GUI.color = old;

            if(newHexTL != fieldTL.Str) {
                fieldTL.Str = newHexTL;
                changed = true;

                if(ColorUtility.TryParseHtmlString("#" + fieldTL.Str, out Color parsed)) {
                    color.topLeft = parsed;
                    fieldTL.State = NeoField.StateType.OK;
                } else {
                    fieldTL.State = NeoField.StateType.ERROR;
                }
            }

            if(newColorTL != color.topLeft) {
                color.topLeft = newColorTL;
                changed = true;

                fieldTL.Str = color.topLeftHex;
                fieldTL.State = NeoField.StateType.OK;
            }

            GUILayout.Space(4f);
            GUILayout.Label("↖", GUILayout.Width(16));

            // TR
            if(fieldTR.State == NeoField.StateType.ERROR) {
                GUI.color = new Color(1f, 0.5f, 0.5f);
            }

            GUILayout.Label("↗", GUILayout.Width(16));
            GUI.SetNextControlName(FieldGetName(uniqueID + "_1"));
            string newHexTR = GUILayout.TextField(fieldTR.Str, 8, Drawer.myTextFieldNoPad, GUILayout.Width(80f));
            GUI.color = old;

            if(newHexTR != fieldTR.Str) {
                fieldTR.Str = newHexTR;
                changed = true;

                if(ColorUtility.TryParseHtmlString("#" + fieldTR.Str, out Color parsed)) {
                    color.topRight = parsed;
                    fieldTR.State = NeoField.StateType.OK;
                } else {
                    fieldTR.State = NeoField.StateType.ERROR;
                }
            }

            Color newColorTR = RGUI.Field(color.topRight, "", GUILayout.Width(cWidth));
            if(newColorTR != color.topRight) {
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
            Color newColorBL = RGUI.Field(color.bottomLeft, "", GUILayout.Width(cWidth));
            GUILayout.Space(2f);

            if(fieldBL.State == NeoField.StateType.ERROR) {
                GUI.color = new Color(1f, 0.5f, 0.5f);
            }

            GUI.SetNextControlName(FieldGetName(uniqueID + "_2"));
            string newHexBL = GUILayout.TextField(fieldBL.Str, 8, Drawer.myTextFieldNoPad, GUILayout.Width(80f));
            GUI.color = old;

            if(newHexBL != fieldBL.Str) {
                fieldBL.Str = newHexBL;
                changed = true;

                if(ColorUtility.TryParseHtmlString("#" + fieldBL.Str, out Color parsed)) {
                    color.bottomLeft = parsed;
                    fieldBL.State = NeoField.StateType.OK;
                } else {
                    fieldBL.State = NeoField.StateType.ERROR;
                }
            }

            if(newColorBL != color.bottomLeft) {
                color.bottomLeft = newColorBL;
                changed = true;

                fieldBL.Str = color.topLeftHex;
                fieldBL.State = NeoField.StateType.OK;
            }

            GUILayout.Space(4f);
            GUILayout.Label("↙", GUILayout.Width(16));

            // BR
            if(fieldBR.State == NeoField.StateType.ERROR) {
                GUI.color = new Color(1f, 0.5f, 0.5f);
            }

            GUILayout.Label("↘", GUILayout.Width(16));
            GUI.SetNextControlName(FieldGetName(uniqueID + "_3"));
            string newHexBR = GUILayout.TextField(fieldBR.Str, 8, Drawer.myTextFieldNoPad, GUILayout.Width(80f));
            GUI.color = old;

            if(newHexBR != fieldBR.Str) {
                fieldBR.Str = newHexBR;
                changed = true;

                if(ColorUtility.TryParseHtmlString("#" + fieldBR.Str, out Color parsed)) {
                    color.bottomRight = parsed;
                    fieldBR.State = NeoField.StateType.OK;
                } else {
                    fieldBR.State = NeoField.StateType.ERROR;
                }
            }

            Color newColorBR = RGUI.Field(color.bottomRight, "", GUILayout.Width(cWidth));
            if(newColorBR != color.bottomRight) {
                color.bottomRight = newColorBR;
                changed = true;

                fieldBR.Str = color.topLeftHex;
                fieldBR.State = NeoField.StateType.OK;
            }

            GUILayout.EndHorizontal();
            GUILayout.FlexibleSpace();
        } else {
            Color all = color.topLeft;
            if(changed = DrawColor("", ref all, cWidth, uniqueID)) {
                color = all;
            }
        }
        return changed;
    }

    public bool DrawSingle(string label, ref float value, string uniqueID = null) {
        NeoField field = FieldGet(uniqueID);
        StrInitialize(ref field, value.ToString());

        GUILayout.BeginHorizontal();
        GUILayout.Label(label);
        GUILayout.Space(4f);

        bool changed = false;

        Color old = GUI.color;
        ColorbyState(field.State);

        string fieldName = FieldGetName(uniqueID);
        GUI.SetNextControlName(fieldName);
        string newField = GUILayout.TextField(field.Str, Drawer.myTextField);
        GUI.color = old;

        if(newField != field.Str) {
            field.Str = newField;
            if(string.IsNullOrEmpty(field.Str)) {
                field.State = NeoField.StateType.ERROR;
            } else {
                if(float.TryParse(newField, out float parsed)) {
                    value = parsed;
                    field.ComputedValue = parsed;
                    field.State = NeoField.StateType.OK;
                    changed = true;
                } else {
                    var result = Calc(field.Str);
                    if(result == null) {
                        field.State = NeoField.StateType.ERROR;
                    } else {
                        float computed = Convert.ToSingle(result);
                        field.ComputedValue = computed;
                        field.State = (float.IsNaN(computed) || float.IsInfinity(computed))
                            ? NeoField.StateType.WARNING
                            : NeoField.StateType.COMPUTE;
                    }
                }
            }
        }

        object objValue = value;
        if(ApplyFieldValueOnEvent(ref field, fieldName, ref objValue, typeof(float))) {
            value = (float)objValue;
            changed = true;
        }

        GUILayout.Space(2f);
        GUILayout.Label(StatebyState(field.State), GUILayout.Width(12));

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        return changed;
    }

    public bool DrawSingleWithSlider(string label, ref float value, float lValue, float rValue, float width, string uniqueID = null) {
        NeoField field = FieldGet(uniqueID);
        StrInitialize(ref field, value.ToString());

        GUILayout.BeginHorizontal();
        GUILayout.Label(label);
        GUILayout.Space(4f);

        bool changed = false;

        float sliderValue = GUILayout.HorizontalSlider(value, lValue, rValue, Drawer.mySlider, Drawer.myThumb, GUILayout.Width(width));
        if(sliderValue != value) {
            value = sliderValue;
            field.Str = value.ToString();
            field.State = NeoField.StateType.OK;
            changed = true;
        }

        GUILayout.Space(8f);

        Color old = GUI.color;
        ColorbyState(field.State);

        string fieldName = FieldGetName(uniqueID);
        GUI.SetNextControlName(fieldName);
        string newField = GUILayout.TextField(field.Str, Drawer.myTextField);
        GUI.color = old;

        if(newField != field.Str) {
            field.Str = newField;
            if(string.IsNullOrEmpty(field.Str)) {
                field.State = NeoField.StateType.ERROR;
            } else {
                if(float.TryParse(newField, out float parsed)) {
                    value = parsed;
                    field.ComputedValue = parsed;
                    field.State = NeoField.StateType.OK;
                    changed = true;
                } else {
                    var result = Calc(field.Str);
                    if(result == null) {
                        field.State = NeoField.StateType.ERROR;
                    } else {
                        float computed = Convert.ToSingle(result);
                        field.ComputedValue = computed;
                        field.State = (float.IsNaN(computed) || float.IsInfinity(computed))
                            ? NeoField.StateType.WARNING
                            : NeoField.StateType.COMPUTE;
                    }
                }
            }
        }

        object objValue = value;
        if(ApplyFieldValueOnEvent(ref field, fieldName, ref objValue, typeof(float))) {
            value = (float)objValue;
            changed = true;
        }

        GUILayout.Space(2f);
        GUILayout.Label(StatebyState(field.State), GUILayout.Width(12));

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        return changed;
    }

    public bool DrawSingleWithSlider(Texture2D icon, string label, ref float value, float lValue, float rValue, float width, string uniqueID = null) {
        NeoField field = FieldGet(uniqueID);
        StrInitialize(ref field, value.ToString());

        GUILayout.BeginHorizontal();
        GUILayout.Label(icon);
        GUILayout.Label(label);
        GUILayout.Space(4f);

        bool changed = false;

        float sliderValue = GUILayout.HorizontalSlider(value, lValue, rValue, Drawer.mySlider, Drawer.myThumb, GUILayout.Width(width));
        if(sliderValue != value) {
            value = sliderValue;
            field.Str = value.ToString();
            field.State = NeoField.StateType.OK;
            changed = true;
        }

        GUILayout.Space(8f);

        Color old = GUI.color;
        ColorbyState(field.State);

        string fieldName = FieldGetName(uniqueID);
        GUI.SetNextControlName(fieldName);
        string newField = GUILayout.TextField(field.Str, Drawer.myTextField);
        GUI.color = old;

        if(newField != field.Str) {
            field.Str = newField;
            if(string.IsNullOrEmpty(field.Str)) {
                field.State = NeoField.StateType.ERROR;
            } else {
                if(float.TryParse(newField, out float parsed)) {
                    value = parsed;
                    field.ComputedValue = parsed;
                    field.State = NeoField.StateType.OK;
                    changed = true;
                } else {
                    var result = Calc(field.Str);
                    if(result == null) {
                        field.State = NeoField.StateType.ERROR;
                    } else {
                        float computed = Convert.ToSingle(result);
                        field.ComputedValue = computed;
                        field.State = (float.IsNaN(computed) || float.IsInfinity(computed))
                            ? NeoField.StateType.WARNING
                            : NeoField.StateType.COMPUTE;
                    }
                }
            }
        }

        object objValue = value;
        if(ApplyFieldValueOnEvent(ref field, fieldName, ref objValue, typeof(float))) {
            value = (float)objValue;
            changed = true;
        }

        GUILayout.Space(2f);
        GUILayout.Label(StatebyState(field.State), GUILayout.Width(12));

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        return changed;
    }

    public bool DrawDouble(string label, ref double value, string uniqueID = null) {
        NeoField field = FieldGet(uniqueID);
        StrInitialize(ref field, value.ToString());

        GUILayout.BeginHorizontal();
        GUILayout.Label(label);
        GUILayout.Space(4f);

        bool changed = false;

        Color old = GUI.color;
        ColorbyState(field.State);

        string fieldName = FieldGetName(uniqueID);
        GUI.SetNextControlName(fieldName);
        string newField = GUILayout.TextField(field.Str, Drawer.myTextField);
        GUI.color = old;

        if(newField != field.Str) {
            field.Str = newField;
            if(string.IsNullOrEmpty(field.Str)) {
                field.State = NeoField.StateType.ERROR;
            } else {
                if(double.TryParse(newField, out double parsed)) {
                    value = parsed;
                    field.ComputedValue = parsed;
                    field.State = NeoField.StateType.OK;
                    changed = true;
                } else {
                    var result = Calc(field.Str);
                    if(result == null) {
                        field.State = NeoField.StateType.ERROR;
                    } else {
                        double computed = Convert.ToSingle(result);
                        field.ComputedValue = computed;
                        field.State = (double.IsNaN(computed) || double.IsInfinity(computed))
                            ? NeoField.StateType.WARNING
                            : NeoField.StateType.COMPUTE;
                    }
                }
            }
        }

        object objValue = value;
        if(ApplyFieldValueOnEvent(ref field, fieldName, ref objValue, typeof(double))) {
            value = (double)objValue;
            changed = true;
        }

        GUILayout.Space(2f);
        GUILayout.Label(StatebyState(field.State), GUILayout.Width(12));

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        return changed;
    }

    public bool DrawInt32(string label, ref int value, string uniqueID = null) {
        NeoField field = FieldGet(uniqueID);
        StrInitialize(ref field, value.ToString());

        GUILayout.BeginHorizontal();
        GUILayout.Label(label);
        GUILayout.Space(4f);

        bool changed = false;

        Color old = GUI.color;
        ColorbyState(field.State);

        string fieldName = FieldGetName(uniqueID);
        GUI.SetNextControlName(fieldName);
        string newField = GUILayout.TextField(field.Str, Drawer.myTextField);
        GUI.color = old;

        if(newField != field.Str) {
            field.Str = newField;
            if(string.IsNullOrEmpty(field.Str)) {
                field.State = NeoField.StateType.ERROR;
            } else {
                if(int.TryParse(newField, out int parsed)) {
                    value = parsed;
                    field.ComputedValue = parsed;
                    field.State = NeoField.StateType.OK;
                    changed = true;
                } else {
                    var result = Calc(field.Str);
                    if(result == null) {
                        field.State = NeoField.StateType.ERROR;
                    } else {
                        double computed = Convert.ToDouble(result);
                        if(double.IsNaN(computed) || double.IsInfinity(computed)) {
                            field.State = NeoField.StateType.ERROR;
                        } else if(computed > int.MaxValue) {
                            field.State = NeoField.StateType.WARNING;
                            field.ComputedValue = int.MaxValue;
                        } else if(computed < int.MinValue) {
                            field.State = NeoField.StateType.WARNING;
                            field.ComputedValue = int.MinValue;
                        } else {
                            int computedInt = (int)Math.Round(computed);
                            field.ComputedValue = computedInt;
                            field.State = NeoField.StateType.COMPUTE;
                        }
                    }
                }
            }
        }

        object objValue = value;
        if(ApplyFieldValueOnEvent(ref field, fieldName, ref objValue, typeof(int))) {
            value = (int)objValue;
            changed = true;
        }

        GUILayout.Space(2f);
        GUILayout.Label(StatebyState(field.State), GUILayout.Width(12));

        GUILayout.FlexibleSpace();
        GUILayout.EndHorizontal();

        return changed;
    }
}