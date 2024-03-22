using System;
using Unity.Collections.LowLevel.Unsafe;

namespace QuickPixel
{
    public static class EnumExtensions
    {
        public static bool HasFlagFast<T>(this T target, T flag) where T : struct, Enum
        {
            var targetValue = UnsafeUtility.As<T, ulong>(ref target);
            var flagValue = UnsafeUtility.As<T, ulong>(ref flag);

            return (targetValue & flagValue) == flagValue;
        }

        public static void AddFlag<T>(this ref T target, T flag) where T : struct, Enum
        {
            var targetValue = UnsafeUtility.As<T, ulong>(ref target);
            var flagValue = UnsafeUtility.As<T, ulong>(ref flag);

            if ((targetValue & flagValue) != flagValue)
            {
                var newFlagValue = targetValue | flagValue;
                target = UnsafeUtility.As<ulong, T>(ref newFlagValue);
            }
        }

        public static void RemoveFlag<T>(this ref T target, T flag) where T : struct, Enum
        {
            var targetValue = UnsafeUtility.As<T, ulong>(ref target);
            var flagValue = UnsafeUtility.As<T, ulong>(ref flag);

            if ((targetValue & flagValue) != 0)
            {
                var newFlagValue = targetValue & ~flagValue;
                target = UnsafeUtility.As<ulong, T>(ref newFlagValue);
            }
        }

        public static bool HasAnyFlag<T>(this T target, T flag) where T : struct, Enum
        {
            var targetValue = UnsafeUtility.As<T, ulong>(ref target);
            var flagValue = UnsafeUtility.As<T, ulong>(ref flag);

            return (targetValue & flagValue) != 0;
        }
    }
}