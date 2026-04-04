using System;
using System.Linq;
using System.Reflection;

namespace Overlayer.Core.Patches;

public static class SafePatch {
    public static MethodBase GetMethodSafe(string typeName, string methodName, Type[] args = null, bool allowStatic = false) {
        var type = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(a => a.GetTypes())
            .FirstOrDefault(t => t.Name == typeName) ?? throw new Exception($"[{nameof(SafePatch)}] Type not found: {typeName}");

        BindingFlags bf = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        if(allowStatic) {
            bf |= BindingFlags.Static;
        }

        if(methodName == ".ctor") {
            return args != null
                ? type.GetConstructor(bf, null, args, null)
                    ?? throw new Exception($"[{nameof(SafePatch)}] Constructor with specified args not found in {typeName}")
                : type.GetConstructors(bf).FirstOrDefault()
                ?? throw new Exception($"[{nameof(SafePatch)}] No constructors found in {typeName}");
        }

        if(methodName == ".cctor") {
            return type.TypeInitializer
                ?? throw new Exception($"[{nameof(SafePatch)}] Static constructor not found in {typeName}");
        }

        if(args != null) {
            return type.GetMethod(methodName, bf, null, args, null)
                ?? throw new Exception($"[{nameof(SafePatch)}] Method {methodName} with specified args not found in {typeName}");
        }

        var method = type.GetMethods(bf).FirstOrDefault(m => m.Name == methodName)
            ?? throw new Exception($"[{nameof(SafePatch)}] Method {methodName} not found in {typeName}");
        return method;
    }

    public static ConstructorInfo GetConstructorSafe(string typeName, Type[] args = null, bool allowStatic = false)
        => (ConstructorInfo)GetMethodSafe(typeName, ".ctor", args, allowStatic);
}