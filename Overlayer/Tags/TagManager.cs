using Overlayer.Tags.Attributes;
using System;
using System.Reflection;
using Overlayer.Core.TextReplacing;
using System.Reflection.Emit;

namespace Overlayer.Tags
{
    public static class TagManager
    {
        public static void Load(Type type)
        {
            foreach (var method in type.GetMethods((BindingFlags)15420))
            {
                
            }
            foreach (var field in type.GetFields((BindingFlags)15420))
            {

            }
        }
        public static void Initialize()
        {
            ass = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName("Overlayer.Tags.TagManager.FieldTagWrappers"), AssemblyBuilderAccess.RunAndCollect);
            mod = ass.DefineDynamicModule("Overlayer.Tags.TagManager.FieldTagWrappers");
        }
        public static void Release()
        {
            ass = null;
            mod = null;
        }
        private static AssemblyBuilder ass;
        private static ModuleBuilder mod;
        private static Tag InitializeTag(MemberInfo member)
        {
            var tagAttr = member.GetCustomAttribute<TagAttribute>();
            if (tagAttr == null) return null;
            return new Tag(tagAttr.Name ?? member.Name);
        }
    }
}
