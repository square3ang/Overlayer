using Newtonsoft.Json.Linq;
using Overlayer.Core.Interfaces;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace Overlayer.Models;

public static class ModelUtils {
    public static readonly Type model_t = typeof(IModel);
    public static readonly Type vec2_t = typeof(Vector2);
    public static readonly Type vec3_t = typeof(Vector3);
    public static readonly Type vec4_t = typeof(Vector4);
    public static readonly Type col_t = typeof(Color);
    public static readonly Type col32_t = typeof(Color32);
    public static readonly Type quat_t = typeof(Quaternion);
    public static readonly Type rect_t = typeof(Rect);
    public static readonly Type ro_t = typeof(RectOffset);
    public static readonly Type m4x4_t = typeof(Matrix4x4);
    public static JToken ToNode<T>(object obj) {
        if(obj == null) {
            return JValue.CreateNull();
        }

        Type t = typeof(T);
        switch(Type.GetTypeCode(t)) {
            case TypeCode.Object:
                if(obj is IModel model) {
                    return model.Serialize();
                } else if(obj is Vector2 vec2) {
                    return JToken.FromObject(vec2);
                } else if(obj is Vector3 vec3) {
                    return JToken.FromObject(vec3);
                } else if(obj is Vector4 vec4) {
                    return JToken.FromObject(vec4);
                } else if(obj is Color col) {
                    return JToken.FromObject(col);
                } else if(obj is Color32 col32) {
                    return JToken.FromObject(col32);
                } else if(obj is Quaternion quat) {
                    return JToken.FromObject(quat);
                } else if(obj is Rect r) {
                    return JToken.FromObject(r);
                } else if(obj is RectOffset ro) {
                    return JToken.FromObject(ro);
                } else if(obj is Matrix4x4 m4x4) {
                    return JToken.FromObject(m4x4);
                }

                goto default;
            case TypeCode.Boolean:
                return new JValue((bool)obj);
            case TypeCode.Char:
                return new JValue((char)obj);
            case TypeCode.SByte:
                return new JValue((sbyte)obj);
            case TypeCode.Byte:
                return new JValue((byte)obj);
            case TypeCode.Int16:
                return new JValue((short)obj);
            case TypeCode.UInt16:
                return new JValue((ushort)obj);
            case TypeCode.Int32:
                return new JValue((int)obj);
            case TypeCode.UInt32:
                return new JValue((uint)obj);
            case TypeCode.Int64:
                return new JValue((long)obj);
            case TypeCode.UInt64:
                return new JValue((ulong)obj);
            case TypeCode.Single:
                return new JValue((float)obj);
            case TypeCode.Double:
                return new JValue((double)obj);
            case TypeCode.Decimal:
                return new JValue((decimal)obj);
            case TypeCode.DateTime:
                return new JValue((DateTime)obj);
            case TypeCode.String:
                return new JValue(obj.ToString());
            default:
                return JToken.FromObject(obj);
        }
    }

    public static object ToObject<T>(JToken token) {
        if(token == null) {
            return null;
        }

        Type t = typeof(T);
        switch(Type.GetTypeCode(t)) {
            case TypeCode.Object:
                if(typeof(IModel).IsAssignableFrom(t)) {
                    IModel model = (IModel)Activator.CreateInstance(t);
                    model.Deserialize(token);
                    return model;
                }
                return token.ToObject<T>();
            case TypeCode.Boolean:
                return token.Value<bool>();
            case TypeCode.Char:
                return token.Value<char>();
            case TypeCode.SByte:
                return token.Value<sbyte>();
            case TypeCode.Byte:
                return token.Value<byte>();
            case TypeCode.Int16:
                return token.Value<short>();
            case TypeCode.UInt16:
                return token.Value<ushort>();
            case TypeCode.Int32:
                return token.Value<int>();
            case TypeCode.UInt32:
                return token.Value<uint>();
            case TypeCode.Int64:
                return token.Value<long>();
            case TypeCode.UInt64:
                return token.Value<ulong>();
            case TypeCode.Single:
                return token.Value<float>();
            case TypeCode.Double:
                return token.Value<double>();
            case TypeCode.Decimal:
                return token.Value<decimal>();
            case TypeCode.DateTime:
                return token.Value<DateTime>();
            case TypeCode.String:
                return token.Value<string>();
            default:
                return token.ToObject<T>();
        }
    }

    public static T Unbox<T>(JToken token) where T : IModel, new() {
        if(token == null) {
            return default;
        }
        T t = new();
        if(token.Type == JTokenType.Object) {
            t.Deserialize(token);
        }
        return t;
    }

    public static JArray WrapList<T>(List<T> list) where T : IModel, new() {
        var array = new JArray();
        list.ForEach(i => array.Add(i.Serialize()));
        return array;
    }

    public static List<T> UnwrapList<T>(JArray array) where T : IModel, new() {
        var list = new List<T>();
        foreach(var v in array) {
            var t = new T();
            t.Deserialize(v);
            list.Add(t);
        }
        return list;
    }

    public static JToken ToNode(Vector4 v) => new JArray { v.x, v.y, v.z, v.w };
    public static Vector4 ToVector4(JToken token) {
        if(token.Type == JTokenType.Array) {
            var arr = (JArray)token;
            return new Vector4(
                (float)arr[0],
                (float)arr[1],
                (float)arr[2],
                (float)arr[3]
            );
        }
        return default;
    }

    public static JToken ToNode(Vector3 v) => new JArray { v.x, v.y, v.z };
    public static Vector3 ToVector3(JToken token) {
        if(token.Type == JTokenType.Array) {
            var arr = (JArray)token;
            return new Vector3(
                (float)arr[0],
                (float)arr[1],
                (float)arr[2]
            );
        }
        return default;
    }

    public static JToken ToNode(Vector2 v) => new JArray { v.x, v.y };
    public static Vector2 ToVector2(JToken token) {
        if(token.Type == JTokenType.Array) {
            var arr = (JArray)token;
            return new Vector2(
                (float)arr[0],
                (float)arr[1]
            );
        }
        return default;
    }

    public static JToken ToNode(Color c) => new JArray { c.r, c.g, c.b, c.a };
    public static Color ToColor(JToken token) {
        if(token.Type == JTokenType.Array) {
            var arr = (JArray)token;
            return new Color(
                arr.Count > 0 ? (float)arr[0] : 0f,
                arr.Count > 1 ? (float)arr[1] : 0f,
                arr.Count > 2 ? (float)arr[2] : 0f,
                arr.Count > 3 ? (float)arr[3] : 1f
            );
        }
        return default;
    }
}
