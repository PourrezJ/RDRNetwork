using System;
using System.Runtime.InteropServices;

namespace RDRNetwork.Utils
{
    internal static class Memory
    {
        internal static unsafe IntPtr FindPattern(string bytes, string mask)
        {
            var patternPtr = Marshal.StringToHGlobalAnsi(bytes);
            var maskPtr = Marshal.StringToHGlobalAnsi(bytes);

            IntPtr output;

            try
            {
                output =
                    new IntPtr(
                        unchecked(
                            (long)
                                FindPattern(
                                    patternPtr.ToString(),
                                    maskPtr.ToString()
                                    )));
            }
            finally
            {
                Marshal.FreeHGlobal(patternPtr);
                Marshal.FreeHGlobal(maskPtr);
            }

            return output;
        }

        public static void WriteMemory(IntPtr pointer, byte value, int length)
        {
            for (int i = 0; i < length; i++)
            {
               RDR2DN.NativeMemory.WriteByte(pointer + i, value);
            }
        }

        public static void WriteMemory(IntPtr pointer, byte[] value)
        {
            for (int i = 0; i < value.Length; i++)
            {
                RDR2DN.NativeMemory.WriteByte(pointer + i, value[i]);
            }
        }

        public static byte[] ReadMemory(IntPtr pointer, int length)
        {
            byte[] memory = new byte[length];
            for (int i = 0; i < length; i++)
            {
                memory[i] = RDR2DN.NativeMemory.ReadByte(pointer + i);
            }
            return memory;
        }
    }
}
