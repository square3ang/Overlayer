using System;
using System.Reflection.Emit;

namespace Overlayer.Core.Scripting.JSNet.Utils;

public static class Type<T> {
    private delegate IntPtr AddrGetter(ref T obj);

    private delegate int SizeGetter();

    private static readonly AddrGetter addrGetter;

    private static readonly SizeGetter sizeGetter;

    public static readonly Type Base;

    public static int Size => sizeGetter();

    static Type() {
        Base = typeof(T);
        addrGetter = CreateAddrGetter();
        sizeGetter = CreateSizeGetter();
    }

    private static AddrGetter CreateAddrGetter() {
        DynamicMethod dynamicMethod = new(typeof(T).FullName + "_Address", typeof(IntPtr), [typeof(T).MakeByRefType()]);
        ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
        iLGenerator.Emit(OpCodes.Ldarg_0);
        iLGenerator.Emit(OpCodes.Conv_U);
        iLGenerator.Emit(OpCodes.Ret);
        return (AddrGetter)dynamicMethod.CreateDelegate(typeof(AddrGetter));
    }

    private static SizeGetter CreateSizeGetter() {
        DynamicMethod dynamicMethod = new(typeof(T).FullName + "_Size", typeof(int), Type.EmptyTypes);
        ILGenerator iLGenerator = dynamicMethod.GetILGenerator();
        iLGenerator.Emit(OpCodes.Sizeof, typeof(T));
        iLGenerator.Emit(OpCodes.Ret);
        return (SizeGetter)dynamicMethod.CreateDelegate(typeof(SizeGetter));
    }

    public static IntPtr GetAddress(ref T obj) => addrGetter(ref obj);
}
