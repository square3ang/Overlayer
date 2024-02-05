using Overlayer.Core.TextReplacing;
using Overlayer.Tags.Attributes;
using Overlayer.Utils;
using System;
using System.Reflection;
using System.Reflection.Emit;

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
        public OverlayerTag(MethodInfo method, TagAttribute attr, object target = null)
        {
            Tag = new Tag(Name = attr.Name ?? method.Name);
            Tag.SetGetter(method, target);
            Attributes = attr;
            NotPlaying = attr.NotPlaying;
        }
        public OverlayerTag(FieldInfo field, TagAttribute attr)
        {
            Tag = new Tag(Name = attr.Name ?? field.Name);
            Tag.SetGetter(WrapField(field, attr.Flags));
            Attributes = attr;
            NotPlaying = attr.NotPlaying;
        }
        public OverlayerTag(string name, Delegate del, bool notPlaying, AdvancedFlags flags = AdvancedFlags.None)
        {
            var attr = new TagAttribute(Name = name);
            attr.Flags = flags;
            Tag = new Tag(name);
            Tag.SetGetter(del);
            Attributes = attr;
            NotPlaying = notPlaying;
        }
        private static MethodInfo WrapField(FieldInfo field, AdvancedFlags flags)
        {
            TypeBuilder t = mod.DefineType($"FieldTagWrapper${uniqueNum++}", TypeAttributes.Public);
            MethodBuilder m = t.DefineMethod("Getter", MethodAttributes.Public | MethodAttributes.Static);
            ILGenerator il = m.GetILGenerator();
            il.Emit(OpCodes.Ldsfld, field);
            if ((flags & AdvancedFlags.Round) != 0)
            {
                if (field.FieldType != typeof(double))
                    il.Emit(OpCodes.Conv_R8);
                il.Emit(OpCodes.Ldarg_0);
                il.Emit(OpCodes.Call, round);
                if (field.FieldType != typeof(double))
                    il.Convert(field.FieldType);
                m.SetParameters(typeof(int));
                var param = m.DefineParameter(1, ParameterAttributes.None, "digits");
                param.SetConstant(-1);
            }
            il.Emit(OpCodes.Ret);
            m.SetReturnType(field.FieldType);
            return t.CreateType().GetMethod("Getter", (BindingFlags)15420);
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
        private static int uniqueNum = 0;
        private static readonly MethodInfo round = typeof(Extensions).GetMethod("Round", new[] { typeof(double), typeof(int) });
        private static AssemblyBuilder ass;
        private static ModuleBuilder mod;
    }
}
