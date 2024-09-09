using Overlayer.Utils;
using System;
using System.Linq;
using System.Reflection;

namespace Overlayer.Core.Patches
{
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = true)]
    internal class LazyPatchAttribute : Attribute
    {
        public static readonly int CurrentVersion = (int)typeof(GCNS).GetField("releaseNumber").GetValue(null);
        public string Id { get; }
        public string TargetType { get; }
        public string TargetMethod { get; }
        public string[] TargetMethodArgs { get; }
        public string[] Triggers { get; set; } = new string[] { LazyPatch.InternalTrigger };
        public int MinVersion { get; set; } = -1;
        public int MaxVersion { get; set; } = -1;
        public BindingFlags ORFlags { get; set; } = BindingFlags.DeclaredOnly;
        public BindingFlags ANDNOTFlags { get; set; } = BindingFlags.Default;
        public LazyPatchAttribute(string id, string targetType, string targetMethod)
        {
            Id = id!;
            TargetType = targetType!;
            TargetMethod = targetMethod!;
            TargetMethodArgs = null;
        }
        public LazyPatchAttribute(string id, string targetType, string targetMethod, string[] targetMethodArgs)
        {
            Id = id!;
            TargetType = targetType!;
            TargetMethod = targetMethod!;
            TargetMethodArgs = targetMethodArgs;
        }
        public bool IsCompatible => (CurrentVersion >= MinVersion || MinVersion < 0) && (MaxVersion >= CurrentVersion || MaxVersion < 0);
        public MethodBase Resolve()
        {
            if (!IsCompatible)
            {
                Main.Logger.Log($"{Id} Patch Is Not Compatible! (Min:{MinVersion}, Max:{MaxVersion}, Current:{CurrentVersion})");
                return null;
            }
            var bf = ((BindingFlags)15420 | ORFlags) & ~ANDNOTFlags;
            try
            {
                var tt = MiscUtils.TypeByName(TargetType);
                var tma = TargetMethodArgs?.Select(MiscUtils.TypeByName).ToArray();
                if (TargetMethod == ".ctor")
                    return tma != null ?
                        tt?.GetConstructor(bf, null, tma, null) :
                        tt?.GetConstructors(bf).FirstOrDefault();
                else if (TargetMethod == ".cctor") return tt.TypeInitializer;
                else return tma != null ?
                        tt?.GetMethod(TargetMethod, bf, null, tma, null) :
                        tt?.GetMethod(TargetMethod, bf);
            }
            catch (AmbiguousMatchException)
            {
                //foreach (var amMethod in MiscUtils.TypeByName(TargetType).GetMethods(bf).Where(t => t.Name == TargetMethod))
                //    Main.Logger.Log(amMethod.ToString());
                Main.Logger.Log($"{Id} Patch Is Ambiguous Match! (Min:{MinVersion}, Max:{MaxVersion}, Current:{CurrentVersion})");
                return null;
            }
        }
    }
}
