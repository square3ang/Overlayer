﻿using Overlayer.Core.TextReplacing;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Text;

namespace Overlayer.Tags
{
    public class OverlayerTag
    {
        public static bool Initialized { get; private set; }
        public string Name { get; }
        public bool NotPlaying { get; }
        public bool Referenced => Tag.Referenced;
        public Tag Tag { get; }
        public TagAttribute Attributes { get; }
        public Type DeclaringType { get; }
        public OverlayerTag(MethodInfo method, TagAttribute attr, object target = null)
        {
            Tag = new Tag(Name = attr.Name ?? method.Name);
            Tag.SetGetter(WrapProcessor(method, target, attr.ProcessingFlags, attr.ProcessingFlagsArg), target);
            Attributes = attr;
            NotPlaying = attr.NotPlaying;
            DeclaringType = method.DeclaringType;
        }
        public OverlayerTag(FieldInfo field, TagAttribute attr, object target = null)
        {
            Tag = new Tag(Name = attr.Name ?? field.Name);
            Tag.SetGetter(WrapProcessor(field, target, attr.ProcessingFlags, attr.ProcessingFlagsArg));
            Attributes = attr;
            NotPlaying = attr.NotPlaying;
            DeclaringType = field.DeclaringType;
        }
        public OverlayerTag(PropertyInfo prop, TagAttribute attr, object target = null)
        {
            Tag = new Tag(Name = attr.Name ?? prop.Name);
            Tag.SetGetter(WrapProcessor(prop, target, attr.ProcessingFlags, attr.ProcessingFlagsArg));
            Attributes = attr;
            NotPlaying = attr.NotPlaying;
            DeclaringType = prop.DeclaringType;
        }
        public OverlayerTag(string name, Delegate del, bool notPlaying, ValueProcessing flags = ValueProcessing.None)
        {
            var attr = new TagAttribute(Name = name);
            attr.ProcessingFlags = flags;
            Tag = new Tag(name);
            Tag.SetGetter(del);
            Attributes = attr;
            NotPlaying = notPlaying;
            DeclaringType = del.Method.DeclaringType;
        }
        private static MethodInfo WrapProcessor(MemberInfo fieldPropMethod, object target, ValueProcessing flags, object flagsArg)
        {
            if (fieldPropMethod == null) throw new NullReferenceException(nameof(fieldPropMethod));
            if (fieldPropMethod is MethodInfo meth && flags == ValueProcessing.None) return meth;
            if (!IsValid(flags)) throw new InvalidOperationException($"Invalid FieldFlags! ({flags})");
            TypeBuilder t = mod.DefineType($"ValueProcessor_{fieldPropMethod?.Name}${uniqueNum++}", TypeAttributes.Public);
            MethodBuilder m = t.DefineMethod("Getter", MethodAttributes.Public | MethodAttributes.Static);
            FieldBuilder targetField = t.DefineField("target", target?.GetType() ?? typeof(object), FieldAttributes.Public | FieldAttributes.Static);
            ILGenerator il = m.GetILGenerator();
            Type rt = null;
            List<(Type, string, object)> parameters = new List<(Type, string, object)>();
            if (fieldPropMethod is FieldInfo field)
            {
                if (!field.IsStatic && target == null)
                    throw new InvalidOperationException($"Field '{field.Name}' Cannot Get Instance Member Without Target!!");
                if (!field.IsStatic && target != null)
                    il.Emit(OpCodes.Ldsfld, targetField);
                il.Emit(OpCodes.Ldsfld, field);
                rt = field.FieldType;
            }
            if (fieldPropMethod is PropertyInfo property)
            {
                MethodInfo getter = property.GetGetMethod();
                if (getter == null)
                    throw new InvalidOperationException($"Property '{property.Name}' Getter Is Not Exist Or Not Public!");
                fieldPropMethod = getter;
            }
            if (fieldPropMethod is MethodInfo method)
            {
                if (method.GetParameters().Length > 0)
                    throw new InvalidOperationException($"Method '{method.Name}' Cannot Call Instance Member Without Target!!");
                if (!method.IsStatic && target == null)
                    throw new InvalidOperationException($"Method '{method.Name}' Has Parameter!!");
                if (!method.IsStatic && target != null)
                    il.Emit(OpCodes.Ldsfld, targetField);
                il.Emit(OpCodes.Call, method);
                rt = method.ReturnType;
            }
            if ((flags & ValueProcessing.AccessMember) != 0)
            {
                il.Emit(OpCodes.Ldarg, parameters.Count);
                il.Emit(OpCodes.Call, runtimeAccessor);
                parameters.Add((typeof(string), "accessor", null));
            }
            if ((flags & ValueProcessing.RoundNumber) != 0)
            {
                if (rt != typeof(double))
                    il.Emit(OpCodes.Conv_R8);
                il.Emit(OpCodes.Ldarg, parameters.Count);
                il.Emit(OpCodes.Call, round);
                if (rt != typeof(double))
                    il.Convert(rt);
                parameters.Add((typeof(int), "digits", -1));
            }
            else if ((flags & ValueProcessing.TrimString) != 0)
            {
                il.Emit(OpCodes.Ldarg, parameters.Count);
                il.Emit(OpCodes.Ldarg, parameters.Count + 1);
                il.Emit(OpCodes.Call, trim);
                parameters.Add((typeof(int), "maxLength", -1));
                parameters.Add((typeof(string), "afterTrimStr", Extensions.DefaultTrimStr));
            }
            il.Emit(OpCodes.Ret);
            m.SetParameters(parameters.Select(t => t.Item1).ToArray());
            int offset = 0;
            foreach (var (_, name, constant) in parameters)
            {
                var paramBuilder = m.DefineParameter(1 + offset++, ParameterAttributes.None, name);
                if (constant != null) paramBuilder.SetConstant(constant);
            }
            m.SetReturnType(rt);
            var createdType = t.CreateType();
            if (target != null) createdType.GetField("target").SetValue(null, target);
            return createdType.GetMethod("Getter", (BindingFlags)15420);
        }
        public static void Initialize()
        {
            if (Initialized) return;
            uniqueNum = 0;
            ass = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Overlayer.Tags.FieldTagWrappers"), AssemblyBuilderAccess.RunAndCollect);
            mod = ass.DefineDynamicModule("Overlayer.Tags.FieldTagWrappers");
            Initialized = true;
        }
        public static void Release()
        {
            if (!Initialized) return;
            ass = null;
            mod = null;
            Initialized = false;
        }
        private static bool IsValid(ValueProcessing flags)
        {
            if (flags.HasFlag(ValueProcessing.RoundNumber) &&
                flags.HasFlag(ValueProcessing.TrimString))
                return false;
            return true;
        }
        [Obsolete("Internal Only!!", true)]
        public static object RuntimeAccessor(object obj, string accessor)
        {
            if (obj == null) return null;
            Type objType = obj.GetType();
            accessor = accessor.TrimEnd('.');
            if (accessorCache.TryGetValue($"{objType}_Accessor_{accessor}", out var del)) return del(obj);
            object result = obj;
            string[] accessors = accessor.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (accessors.Length < 1) return obj;
            for (int i = 0; i < accessors.Length; i++)
            {
                Type type = result?.GetType();
                MemberInfo[] members = type.GetMembers((BindingFlags)15420);
                if (accessors[i].Equals("ListMembers", StringComparison.OrdinalIgnoreCase))
                {
                    StringBuilder sb = new StringBuilder();
                    for (int j = 0; j < members.Length; j++)
                    {
                        MemberInfo m = members[j];
                        if (m is FieldInfo field && !field.IsStatic)
                            sb.AppendLine($"{(field.IsPublic ? "Public" : "Private")} Field {field.FieldType} '{field.Name}'");
                        else if (m is PropertyInfo prop && prop.GetGetMethod(true) is MethodInfo getter && !getter.IsStatic)
                            sb.AppendLine($"{(getter.IsPublic ? "Public" : "Private")} Property {prop.PropertyType} '{prop.Name}'");
                    }
                    return sb.ToString();
                }
                var ignoreCase = type.GetCustomAttribute<IgnoreCaseAttribute>() != null;
                var foundMembers = ignoreCase ? members.Where(m => m.Name.Equals(accessors[i], StringComparison.OrdinalIgnoreCase)) : members.Where(m => m.Name == accessors[i]);
                var member = foundMembers.Where(m => m.MemberType == MemberTypes.Field || m.MemberType == MemberTypes.Property).FirstOrDefault();
                if (member is FieldInfo f && !f.IsStatic) result = f.GetValue(result);
                else if (member is PropertyInfo p && !(p.GetGetMethod(true)?.IsStatic ?? true)) result = p.GetValue(result);
                else result = null;
                if (result == null) return null;
            }
            accessorCache[$"{objType}_Accessor_{accessor}"] = (Func<object, object>)CreateMemberAccessor(objType, accessor).CreateDelegate(typeof(Func<object, object>));
            return result;
        }
        public static DynamicMethod CreateMemberAccessor(Type type, string accessor)
        {
            if (accessorCacheDM.TryGetValue($"{type}_Accessor_{accessor}", out var dm)) return dm;
            string[] accessors = accessor.Split(new[] { '.' }, StringSplitOptions.RemoveEmptyEntries);
            if (accessors.Length < 1) return null;
            Type t = type;
            MemberInfo result;
            List<MemberInfo> toEmitMembers = new List<MemberInfo>();
            for (int i = 0; i < accessors.Length; i++)
            {
                var ignoreCase = t.GetCustomAttribute<IgnoreCaseAttribute>() != null;
                if (ignoreCase)
                    result = t?.GetMembers((BindingFlags)15420).Where(m => m.Name.Equals(accessors[i], StringComparison.OrdinalIgnoreCase)).FirstOrDefault();
                else result = t?.GetMember(accessors[i], MemberTypes.Field | MemberTypes.Property, (BindingFlags)15420).FirstOrDefault();
                if (result is FieldInfo f && !f.IsStatic) t = f.FieldType;
                else if (result is PropertyInfo p && !(p.GetGetMethod(true)?.IsStatic ?? true)) t = p.PropertyType;
                else return null;
                toEmitMembers.Add(result);
            }
            MemberInfo last = toEmitMembers.Last();
            Type rt = last is FieldInfo ff ? ff.FieldType : last is PropertyInfo pp ? pp.PropertyType : typeof(object);
            accessorCacheDM[$"{type}_Accessor_{accessor}"] = dm = new DynamicMethod($"{type}_Accessor_{accessor}", typeof(object), new[] { typeof(object) }, typeof(OverlayerTag), true);
            ILGenerator il = dm.GetILGenerator();
            il.Emit(OpCodes.Ldarg_0);
            foreach (MemberInfo m in toEmitMembers)
            {
                if (m is FieldInfo f)
                    il.Emit(OpCodes.Ldfld, f);
                else if (m is PropertyInfo p)
                    il.Emit(OpCodes.Call, p.GetGetMethod(true));
            }
            if (typeof(object) != rt)
                il.Emit(OpCodes.Box, rt);
            il.Emit(OpCodes.Ret);
            Main.Logger.Log($"Accessor Method '{$"{type}_Accessor_{accessor}"}' Generated!");
            return dm;
        }
        private static int uniqueNum = 0;
        private static readonly MethodInfo round = typeof(Extensions).GetMethod("Round", new[] { typeof(double), typeof(int) });
        private static readonly MethodInfo trim = typeof(Extensions).GetMethod("Trim", new[] { typeof(string), typeof(int), typeof(string) });
        private static readonly MethodInfo runtimeAccessor = typeof(OverlayerTag).GetMethod("RuntimeAccessor", new[] { typeof(object), typeof(string) });
        private static AssemblyBuilder ass;
        private static ModuleBuilder mod;
        private static Dictionary<string, Func<object, object>> accessorCache = new Dictionary<string, Func<object, object>>();
        private static Dictionary<string, DynamicMethod> accessorCacheDM = new Dictionary<string, DynamicMethod>();
    }
}
