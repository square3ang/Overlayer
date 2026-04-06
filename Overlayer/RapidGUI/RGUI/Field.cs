using System;
using System.Collections.Generic;
using UnityEngine;
using FieldFunc = System.Func<object, System.Type, object>;
using LabelRightFunc = System.Func<object, System.Type, object>;

namespace RapidGUI;

public static partial class RGUI {
    // dummy GUIStyle.none.
    // unity is optimized to GUIStyle.none.
    // it seems to occur indent mismatch for complex Vertical/Horizontal Scope.
    private static readonly GUIStyle styleNone = new(GUIStyle.none);

    public static T Field<T>(T v, string label = null, params GUILayoutOption[] options) {
        return Field(v, label, styleNone, options);
    }

    public static T Field<T>(T v, string label, GUIStyle style, params GUILayoutOption[] options) {
        var type = typeof(T);
        var obj = Field(v, type, label, style, options);
        return (T)Convert.ChangeType(obj, type);
    }

    public static object Field(object obj, Type type, string label = null, params GUILayoutOption[] options) {
        return Field(obj, type, label, GUIStyle.none, options);
    }

    public static object Field(object obj, Type type, string label, GUIStyle style, params GUILayoutOption[] options) {
        return DoField(obj, type, label, style, DispatchFieldFunc(type), DispatchLabelRightFunc(type), options);
    }

    private static object DoField(object obj, Type type, string label, GUIStyle style, FieldFunc fieldFunc,
        LabelRightFunc labelRightFunc, GUILayoutOption[] options) {
        using (new GUILayout.VerticalScope(style, options)) {
            GUILayout.BeginHorizontal();

            obj = PrefixLabelDraggable(label, obj, type, out var isLong);

            if (isLong || labelRightFunc != null) {
                if (labelRightFunc != null) obj = labelRightFunc(obj, type);
                GUILayout.EndHorizontal();
                GUILayout.BeginHorizontal();
                GUILayout.Space(PrefixLabelSetting.width + GUI.skin.label.margin.horizontal);
            }

            obj = fieldFunc(obj, type);

            GUILayout.EndHorizontal();
        }

        return obj;
    }

    private static readonly Dictionary<Type, FieldFunc> fieldFuncTable = new() {
        { typeof(bool), (obj, t) => BoolField(obj) },
        { typeof(Color), (obj, t) => ColorField(obj) }
    };

    private static FieldFunc DispatchFieldFunc(Type type) {
        if (!fieldFuncTable.TryGetValue(type, out var func)) {
            func = type.IsEnum
                ? (obj, t) => EnumField(obj)
                : TypeUtility.IsList(type)
                    ? ListField
                    : TypeUtility.IsRecursive(type)
                        ? new FieldFunc((obj, t) => RecursiveField(obj))
                        : StandardField;

            fieldFuncTable[type] = func;
        }

        return func;
    }

    private static LabelRightFunc DispatchLabelRightFunc(Type type) {
        LabelRightFunc ret = null;
        if (TypeUtility.IsList(type)) ret = ListLabelRightFunc;

        return ret;
    }
}