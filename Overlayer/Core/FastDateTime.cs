﻿using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Overlayer.Core
{
    public static class FastDateTime
    {
        public delegate TimeSpan GDTNUOFU(DateTime utcNow, out bool ald);
        public static long ticks;
        public static bool ald;
        public static readonly ConstructorInfo dtConstructor = typeof(DateTime).GetConstructor((BindingFlags)15420, null, new[] { typeof(long), typeof(DateTimeKind), typeof(bool) }, null);
        public static readonly MethodInfo utc = typeof(DateTime).GetProperty("UtcNow").GetGetMethod();
        public static readonly MethodInfo dtTicks = typeof(DateTime).GetProperty("Ticks").GetGetMethod();
        public static readonly FieldInfo ticksFld = typeof(FastDateTime).GetField("ticks", (BindingFlags)15420);
        public static readonly FieldInfo aldFld = typeof(FastDateTime).GetField("ald", (BindingFlags)15420);
        public static readonly GDTNUOFU getOffset;
        public static readonly Func<DateTime> GetNow;
        static FastDateTime()
        {
            getOffset = (GDTNUOFU)typeof(TimeZoneInfo).GetMethod("GetDateTimeNowUtcOffsetFromUtc", (BindingFlags)15420).CreateDelegate(typeof(GDTNUOFU));
            ticks = getOffset(DateTime.UtcNow, out ald).Ticks;
            DynamicMethod nowGetter = new DynamicMethod(string.Empty, typeof(DateTime), Type.EmptyTypes, true);
            ILGenerator il = nowGetter.GetILGenerator();
            LocalBuilder dtLoc = il.DeclareLocal(typeof(DateTime));
            LocalBuilder tLoc = il.DeclareLocal(typeof(long));
            il.Emit(OpCodes.Call, utc);
            il.Emit(OpCodes.Stloc, dtLoc);
            il.Emit(OpCodes.Ldloca, dtLoc);
            il.Emit(OpCodes.Call, dtTicks);
            il.Emit(OpCodes.Ldsfld, ticksFld);
            il.Emit(OpCodes.Add);
            il.Emit(OpCodes.Stloc, tLoc);
            il.Emit(OpCodes.Ldloc, tLoc);
            il.Emit(OpCodes.Ldc_I4_2);
            il.Emit(OpCodes.Ldsfld, aldFld);
            il.Emit(OpCodes.Newobj, dtConstructor);
            il.Emit(OpCodes.Ret);
            GetNow = (Func<DateTime>)nowGetter.CreateDelegate(typeof(Func<DateTime>));
        }
        public static DateTime Now => GetNow();
    }
}
