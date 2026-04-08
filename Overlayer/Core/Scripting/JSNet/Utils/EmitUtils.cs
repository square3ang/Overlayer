using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using System.Runtime.CompilerServices;

namespace Overlayer.Core.Scripting.JSNet.Utils;

public static class EmitUtils {
    private static readonly AssemblyBuilder ass;

    private static readonly ModuleBuilder mod;

    private static int TypeCount;

    private static readonly HashSet<string> accessIgnored;

    private static readonly ConstructorInfo iact;

    static EmitUtils() {
        accessIgnored = [];
        iact = typeof(IgnoresAccessChecksToAttribute).GetConstructor([typeof(string)]);
        AssemblyName assemblyName = new("JSNet.Utils.RuntimeAssembly");
        ass = AssemblyBuilder.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
        mod = ass.DefineDynamicModule(assemblyName.Name);
    }

    public static void Convert(this ILGenerator il, Type to) {
        switch(Type.GetTypeCode(to)) {
            case TypeCode.Object:
                il.Emit(OpCodes.Box);
                break;
            case TypeCode.Char:
            case TypeCode.Int16:
                il.Emit(OpCodes.Conv_I2);
                break;
            case TypeCode.SByte:
                il.Emit(OpCodes.Conv_I1);
                break;
            case TypeCode.Byte:
                il.Emit(OpCodes.Conv_U1);
                break;
            case TypeCode.UInt16:
                il.Emit(OpCodes.Conv_U2);
                break;
            case TypeCode.Boolean:
            case TypeCode.Int32:
                il.Emit(OpCodes.Conv_I4);
                break;
            case TypeCode.UInt32:
                il.Emit(OpCodes.Conv_U4);
                break;
            case TypeCode.Int64:
                il.Emit(OpCodes.Conv_I8);
                break;
            case TypeCode.UInt64:
                il.Emit(OpCodes.Conv_U8);
                break;
            case TypeCode.Single:
                il.Emit(OpCodes.Conv_R4);
                break;
            case TypeCode.Double:
                il.Emit(OpCodes.Conv_R8);
                break;
            case TypeCode.String:
                il.Emit(OpCodes.Call, typeof(Convert).GetMethod("ToString", [to]));
                break;
            case TypeCode.DBNull:
            case TypeCode.Decimal:
            case TypeCode.DateTime:
            case (TypeCode)17:
                break;
        }
    }

    public static IntPtr EmitObject<T>(this ILGenerator il, ref T obj) {
        IntPtr address = Type<T>.GetAddress(ref obj);
        if(IntPtr.Size == 4) {
            il.Emit(OpCodes.Ldc_I4, address.ToInt32());
        } else {
            il.Emit(OpCodes.Ldc_I8, address.ToInt64());
        }
        il.Emit(OpCodes.Ldobj, obj.GetType());
        return address;
    }

    public static GCHandle EmitObjectGC(this ILGenerator il, object obj) {
        GCHandle gCHandle = GCHandle.Alloc(il);
        IntPtr intPtr = GCHandle.ToIntPtr(gCHandle);
        if(IntPtr.Size == 4) {
            il.Emit(OpCodes.Ldc_I4, intPtr.ToInt32());
        } else {
            il.Emit(OpCodes.Ldc_I8, intPtr.ToInt64());
        }
        il.Emit(OpCodes.Ldobj, obj.GetType());
        return gCHandle;
    }

    public static LocalBuilder MakeArray<T>(this ILGenerator il, int length) {
        LocalBuilder localBuilder = il.DeclareLocal(typeof(T[]));
        il.Emit(OpCodes.Ldc_I4, length);
        il.Emit(OpCodes.Newarr, typeof(T));
        il.Emit(OpCodes.Stloc, localBuilder);
        return localBuilder;
    }

    public static void IgnoreAccessCheck(Type type) {
        AssemblyName name = type.Assembly.GetName();
        if(!name.Name.StartsWith("System") && accessIgnored.Add(name.Name)) {
            ass.SetCustomAttribute(GetIACT(name.Name));
        }
    }

    private static CustomAttributeBuilder GetIACT(string name) {
        ConstructorInfo con = iact;
        object[] constructorArgs = [name];
        return new CustomAttributeBuilder(con, constructorArgs);
    }

    public static MethodInfo Wrap<T>(this T del) where T : Delegate {
        Type type = del.GetType();
        IgnoreAccessCheck(type);
        MethodInfo method = type.GetMethod("Invoke");
        MethodInfo method2 = del.Method;
        TypeBuilder typeBuilder = mod.DefineType(TypeCount++.ToString(), TypeAttributes.Public);
        ParameterInfo[] parameters = method2.GetParameters();
        MethodBuilder methodBuilder = typeBuilder.DefineMethod(parameterTypes: parameters.Select((ParameterInfo p) => p.ParameterType).ToArray(), name: "Wrapper", attributes: MethodAttributes.Public | MethodAttributes.Static, returnType: method.ReturnType);
        FieldBuilder field = typeBuilder.DefineField("function", type, FieldAttributes.Public | FieldAttributes.Static);
        IgnoreAccessCheck(method.ReturnType);
        ILGenerator iLGenerator = methodBuilder.GetILGenerator();
        iLGenerator.Emit(OpCodes.Ldsfld, field);
        int num = 1;
        ParameterInfo[] array = parameters;
        foreach(ParameterInfo parameterInfo in array) {
            IgnoreAccessCheck(parameterInfo.ParameterType);
            methodBuilder.DefineParameter(num++, ParameterAttributes.None, parameterInfo.Name);
            iLGenerator.Emit(OpCodes.Ldarg, num - 2);
        }
        iLGenerator.Emit(OpCodes.Call, method);
        iLGenerator.Emit(OpCodes.Ret);
        Type type2 = typeBuilder.CreateType();
        type2.GetField("function").SetValue(null, del);
        return type2.GetMethod("Wrapper");
    }

    public static TypeBuilder NewType(string name = null) {
        return string.IsNullOrWhiteSpace(name)
            ? mod.DefineType(TypeCount++.ToString(), TypeAttributes.Public)
            : mod.DefineType(name, TypeAttributes.Public);
    }
}
