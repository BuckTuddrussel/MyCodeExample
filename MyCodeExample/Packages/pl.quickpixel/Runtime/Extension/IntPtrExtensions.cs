using System;
using System.Runtime.CompilerServices;

namespace QuickPixel
{
    public static unsafe class IntPtrExtensions
    {
        private static readonly int PtrSize = IntPtr.Size;


        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe IntPtr Add(this IntPtr ptr, int offset)
        {
            long ptrValue;

            // ReSharper disable once ConvertIfStatementToSwitchExpression
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (PtrSize == 8)
            {
                ptrValue = ptr.ToInt64();
            }
            else if (PtrSize == 4)
            {
                ptrValue = ptr.ToInt32();
            }
            else
            {
                throw new NotSupportedException("Unsupported platform architecture.");
            }

            return new IntPtr(unchecked(ptrValue + offset));
        }
        
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static unsafe IntPtr Move(this IntPtr ptr, int offset)
        {
            long ptrValue;

            // ReSharper disable once ConvertIfStatementToSwitchExpression
            // ReSharper disable once ConvertIfStatementToSwitchStatement
            if (PtrSize == 8)
            {
                ptrValue = ptr.ToInt64();
            }
            else if (PtrSize == 4)
            {
                ptrValue = ptr.ToInt32();
            }
            else
            {
                throw new NotSupportedException("Unsupported platform architecture.");
            }

            return new IntPtr(unchecked(ptrValue + offset * IntPtr.Size));
        }
    }
}