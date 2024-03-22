// TODO: Investigate what's causing crash in coroutine manager when using pointers
//       for some reason its still crashing, even after list initialization

// Note: IOS does not allow to generate code 

#if !DONT_USE_FUNCTION_POINTERS && CSHARP_7_3_OR_NEWER && !UNITY_IOS
#define USE_FUNCTION_POINTERS
#endif

using System;
using System.Collections.Generic;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.InteropServices;
using UnityEngine.Assertions;

namespace QuickPixel
{
    // ReSharper disable RedundantUnsafeContext
    public static class ListExtensions
    {
        private static readonly int UnmanagedArrayFirstElementOffset;

        static ListExtensions()
        {
            // Note: Below is array layout in memory
            // Source: https://mycodingplace.wordpress.com/2018/01/10/object-header-get-complicated/
            // Managed array header:
            // - SyncBlock
            // - MethodTable (type reference)
            // - Size of Array
            // - Element type pointer (reference type arrays are object[] under the hood)
            // - First element
            // Unmanaged array header:
            // - SyncBlock
            // - MethodTable (type reference)
            // - Size of Array
            // - First element

            UnmanagedArrayFirstElementOffset = IntPtr.Size * 4;
        }

        public static void RemoveAtFast<T>(this List<T> list, int index)
        {
            var index1 = list.Count - 1;
            list[index] = list[index1];
            list.RemoveAt(index1);
        }

        public static void RemoveFast<T>(this List<T> list, T value)
        {
            var index = list.IndexOf(value);
            var index1 = list.Count - 1;
            list[index] = list[index1];
            list.RemoveAt(index1);
        }

        public static T[] GetInternalArray<T>(this IReadOnlyList<T> readOnlyList)
        {
            // Note: If you do this in runtime - app will crash instantly like never before :)
            //       This happen mostly when list initialization is parameterless or you set capacity to 0,
            //       so in simple words 0 means no object = calling ptr.zero :) ( or something worse if old data left after shrink)
            if (readOnlyList is not List<T> list) throw new InvalidCastException();

            Assert.IsTrue(list.Capacity > 0,
                "The internal array of the list is null or empty. Please check the list initialization or capacity.");

            unsafe
            {
                return ListInternalAccessor<T>.GetArray(list);
            }
        }

        public static T[] GetInternalArray<T>(this List<T> list)
        {
            // Note: If you do this in runtime - app will crash instantly like never before :)
            //       This happen mostly when list initialization is parameterless or you set capacity to 0,
            //       so in simple words 0 means no object = calling ptr.zero :) ( or something worse if old data left after shrink)
            Assert.IsTrue(list.Capacity > 0,
                "The internal array of the list is null or empty. Please check the list initialization or capacity.");

            unsafe
            {
                return ListInternalAccessor<T>.GetArray(list);
            }
        }

        public static void AddArrayFast<T>(this List<T> list, T[] itemsToAdd)
        {
            list.AddArrayFast(itemsToAdd, itemsToAdd.Length);
        }


        public static void AddRangeFast<T>(this List<T> list, T[] itemsToAdd, int itemCount)
        {
            var newCapacity = list.Count + itemCount;
            if (list.Capacity < newCapacity)
            {
                list.Capacity = newCapacity;
                unsafe
                {
                    ListInternalAccessor<T>.SetSize(list, newCapacity);
                }
            }

            Array.Copy(itemsToAdd, 0, list.GetInternalArray(), list.Count, itemCount);
        }

        public static void AddArrayFast<T>(this List<T> list, T[] itemsToAdd, int itemCount)
        {
            var newCapacity = list.Count + itemCount;
            if (list.Capacity < newCapacity) list.Capacity = newCapacity;

            Array.Copy(itemsToAdd, 0, list.GetInternalArray(), list.Count, itemCount);

            unsafe
            {
                ListInternalAccessor<T>.SetSize(list, newCapacity);
            }
        }

        public static void SetInternalSize<T>(this List<T> list, int newSize)
        {
            unsafe
            {
                ListInternalAccessor<T>.SetSize(list, newSize);
            }
        }

        public static void AddListFast<T>(this List<T> list, List<T> itemsToAdd)
        {
            list.AddArrayFast(itemsToAdd.GetInternalArray(), itemsToAdd.Count);
        }

        public static void MoveFast<T>(this List<T> list, int originalIndex, int targetIndex)
        {
            if (list.Capacity < targetIndex) throw new IndexOutOfRangeException();

            var indexDifference = Math.Sign(targetIndex - originalIndex);
            if (indexDifference == 0) return;

            var internalArray = list.GetInternalArray();
            var movedObject = internalArray[originalIndex];

            for (var index = originalIndex; index != targetIndex; index += indexDifference)
                internalArray[index] = internalArray[index + indexDifference];

            internalArray[targetIndex] = movedObject;
        }


        public static Span<T> ToSpanFast<T>(this List<T> list) where T : unmanaged
        {
#if USE_FUNCTION_POINTERS
            unsafe
            {
                var arrayIntPtr = ListInternalAccessor<T>.GetArrayIntPtr(list);
                var firstElementIntPtr = arrayIntPtr.Add(UnmanagedArrayFirstElementOffset);
                UnityEngine.Debug.Assert(firstElementIntPtr != IntPtr.Zero);
                return new Span<T>(firstElementIntPtr.ToPointer(), list.Count);
            }
#else
            return new Span<T>(list.GetInternalArray());
#endif
        }


        // ReSharper disable AssignNullToNotNullAttribute
        // ReSharper disable StaticMemberInGenericType
        // ReSharper disable RedundantExplicitArrayCreation
#if USE_FUNCTION_POINTERS
        private abstract unsafe class ListInternalAccessor<T> : ListInternalAccessorBase<T>
        {
            public static readonly delegate* <List<T>, IntPtr> GetArrayIntPtr;
            public static readonly delegate* <List<T>, T[]> GetArray;
            public static readonly delegate* <List<T>, int, void> SetSize;

            private static GCHandle arrayGCHandle;
            private static GCHandle sizeGCHandle;

            static ListInternalAccessor()
            {
                var getArrayDynamicMethod = GenerateGetListInternalArrayDynamicMethod();
                var setSizeDynamicMethod = GenerateSetListInternalSizeDynamicMethod();

                var getArray = getArrayDynamicMethod.CreateDelegate(typeof(Func<List<T>, T[]>));
                var getSize = setSizeDynamicMethod.CreateDelegate(typeof(Action<List<T>, int>));

                arrayGCHandle = GCHandle.Alloc(getArray, GCHandleType.Pinned);
                sizeGCHandle = GCHandle.Alloc(getSize, GCHandleType.Pinned);

                GetArray = (delegate*<List<T>, T[]>)getArrayDynamicMethod.MethodHandle.GetFunctionPointer();
                SetSize = (delegate* <List<T>, int, void>)setSizeDynamicMethod.MethodHandle.GetFunctionPointer();
                GetArrayIntPtr = (delegate*<List<T>, IntPtr>)getArrayDynamicMethod.MethodHandle.GetFunctionPointer();
            }

            ~ListInternalAccessor()
            {
                arrayGCHandle.Free();
                sizeGCHandle.Free();
            }
        }
#else
        private abstract unsafe class ListInternalAccessor<T> : ListInternalAccessorBase<T>
        {
            public static readonly Func<List<T>, T[]> GetArray;
            public static readonly Action<List<T>, int> SetSize;

            static ListInternalAccessor()
            {
                var getArrayDynamicMethod = GenerateGetListInternalArrayDynamicMethod();
                var setSizeDynamicMethod = GenerateSetListInternalSizeDynamicMethod();

                GetArray = (Func<List<T>, T[]>)getArrayDynamicMethod.CreateDelegate(typeof(Func<List<T>, T[]>));
                SetSize = (Action<List<T>, int>)setSizeDynamicMethod.CreateDelegate(typeof(Action<List<T>, int>));
            }
        }
#endif

        private abstract unsafe class ListInternalAccessorBase<T>
        {
            protected static DynamicMethod GenerateGetListInternalArrayDynamicMethod()
            {
                var dynamicMethod = new DynamicMethod("QuickPixel_GetListInternalArray",
                    MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, typeof(T[]),
                    new Type[]
                    {
                        typeof(List<T>)
                    }, typeof(ListInternalAccessor<T>), true);

                var itemsField = typeof(List<T>).GetField("_items", BindingFlags.Instance | BindingFlags.NonPublic);
                var ilGenerator = dynamicMethod.GetILGenerator();
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldfld, itemsField);
                ilGenerator.Emit(OpCodes.Ret);

                return dynamicMethod;
            }

            protected static DynamicMethod GenerateSetListInternalSizeDynamicMethod()
            {
                var dynamicMethod = new DynamicMethod("QuickPixel_SetListInternalSize",
                    MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, null,
                    new Type[]
                    {
                        typeof(List<T>),
                        typeof(int)
                    }, typeof(ListInternalAccessor<T>), true);

                var sizeField = typeof(List<T>).GetField("_size", BindingFlags.Instance | BindingFlags.NonPublic);
                var versionField = typeof(List<T>).GetField("_version", BindingFlags.Instance | BindingFlags.NonPublic);

                var ilGenerator = dynamicMethod.GetILGenerator();
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Ldarg_1);
                ilGenerator.Emit(OpCodes.Stfld, sizeField);
                ilGenerator.Emit(OpCodes.Ldarg_0);
                ilGenerator.Emit(OpCodes.Dup);
                ilGenerator.Emit(OpCodes.Ldfld, versionField);
                ilGenerator.Emit(OpCodes.Ldc_I4_1);
                ilGenerator.Emit(OpCodes.Add);
                ilGenerator.Emit(OpCodes.Stfld, versionField);
                ilGenerator.Emit(OpCodes.Ret);

                return dynamicMethod;
            }
        }
    }
}