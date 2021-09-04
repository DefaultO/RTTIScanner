using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace RTTIScanner
{
    public static class Memory
    {
        [DllImport("kernel32.dll")]
        private static extern IntPtr OpenProcess(int dwDesiredAccess, bool bInheritHandle, int dwProcessId);

        [DllImport("kernel32.dll")]
        private static extern bool CloseHandle(IntPtr hObject);

        [DllImport("kernel32.dll")]
        private static extern bool ReadProcessMemory(int hProcess, long lpBaseAddress, byte[] lpBuffer, int dwSize, int lpNumberOfBytesRead);

        [DllImport("dbghelp.dll", CharSet = CharSet.Unicode)]
        private static extern int UnDecorateSymbolName(string DecoratedName, StringBuilder UnDecoratedName, int UndecoratedLength, int Flags);

        private static string nullTerminate(this string str)
        {
            if (str.Contains('\0'))
                return str.Substring(0, str.IndexOf('\0'));
            return str;
        }

        /* We can just use Sytem.Diagnostics.Process.Handle...
        
        public static IntPtr OpenProcess(this Process process) =>
            OpenProcess(0x0010, false, process.Id);
        public static bool Close(this IntPtr handle) =>
            CloseHandle(handle);
        */

        public static bool ReadInt32(this IntPtr handle, long address, out int result)
        {
            byte[] buffer = new byte[sizeof(int)];
            if (handle == IntPtr.Zero || !ReadProcessMemory((int)handle, address, buffer, buffer.Length, 0))
            {
                result = 0;
                return false;
            }
            result = BitConverter.ToInt32(buffer, 0);
            return true;
        }

        public static bool ReadInt64(this IntPtr handle, long address, out long result)
        {
            byte[] buffer = new byte[sizeof(long)];
            if (handle == IntPtr.Zero || !ReadProcessMemory((int)handle, address, buffer, buffer.Length, 0))
            {
                result = 0;
                return false;
            }
            result = BitConverter.ToInt64(buffer, 0);
            return true;
        }

        public static bool ReadString(this IntPtr handle, long address, int maxLength, out string result)
        {
            byte[] buffer = new byte[maxLength];
            if (handle == IntPtr.Zero || !ReadProcessMemory((int)handle, address, buffer, buffer.Length, 0))
            {
                result = null;
                return false;
            }
            result = Encoding.UTF8.GetString(buffer).nullTerminate();
            return true;
        }

        public static bool ReadFloat(this IntPtr handle, long address, out float result)
        {
            byte[] buffer = new byte[sizeof(float)];
            if (handle == IntPtr.Zero || !ReadProcessMemory((int)handle, address, buffer, buffer.Length, 0))
            {
                result = 0;
                return false;
            }
            result = BitConverter.ToSingle(buffer, 0);
            return true;
        }

        public static bool ReadDouble(this IntPtr handle, long address, out double result)
        {
            byte[] buffer = new byte[sizeof(double)];
            if (handle == IntPtr.Zero || !ReadProcessMemory((int)handle, address, buffer, buffer.Length, 0))
            {
                result = 0;
                return false;
            }
            result = BitConverter.ToDouble(buffer, 0);
            return true;
        }

        public static bool ReadByte(this IntPtr handle, long address, out byte result)
        {
            byte[] buffer = new byte[1];
            if (handle == IntPtr.Zero || !ReadProcessMemory((int)handle, address, buffer, 1, 0))
            {
                result = 0;
                return false;
            }
            result = buffer[0];
            return true;
        }

        public static bool ReadBytes(this IntPtr handle, long address, int count, byte[] result) =>
            handle != IntPtr.Zero && ReadProcessMemory((int)handle, address, result, count, 0);

        /// <summary>
        /// Retrieves an Int64 address found through a given multi-level pointer, to be used with process reading functions.
        /// </summary>
        public static bool ReadOffsets(this IntPtr handle, out long result, long baseAddress, params long[] offsets)
        {
            handle.ReadInt64(baseAddress, out long value);
            for (int i = 0; i < offsets.Length; i++)
                if (!handle.ReadInt64(value + offsets[i], out value))
                {
                    result = 0;
                    return false;
                }
            result = value;
            return true;
        }

        /// <summary>
        /// Retrieves an Int32 address found through a given multi-level pointer, to be used with process reading functions.
        /// </summary>
        public static bool ReadOffsets32(this IntPtr handle, out long result, long baseAddress, params long[] offsets)
        {
            handle.ReadInt32(baseAddress, out int value);
            for (int i = 0; i < offsets.Length; i++)
                if (!handle.ReadInt32(value + offsets[i], out value))
                {
                    result = 0;
                    return false;
                }
            result = value;
            return true;
        }

        public static string[] GetRTTIClassNames(this IntPtr handle, long address)
        {
            // Transcribed from ReadRemoteRuntimeTypeInformation64() from ReClass.NET
            if (!handle.ReadInt64(address, out long struct_addr)) return null;
            if (!handle.ReadInt64(struct_addr - IntPtr.Size, out long object_locator_ptr)) return null;
            if (!handle.ReadInt64(object_locator_ptr + 0x14, out long base_offset)) return null;
            long base_address = object_locator_ptr - base_offset;
            if (!handle.ReadInt32(object_locator_ptr + 0x10, out int class_hierarchy_descriptor_offset)) return null;
            long class_hierarchy_descriptor_ptr = base_address + class_hierarchy_descriptor_offset;
            if (!handle.ReadInt32(class_hierarchy_descriptor_ptr + 0x08, out int base_class_count)) return null;
            if (base_class_count == 0 || base_class_count > 24) return null;
            if (!handle.ReadInt32(class_hierarchy_descriptor_ptr + 0x0C, out int base_class_array_offset)) return null;
            long base_class_array_ptr = base_address + base_class_array_offset;
            string[] names = new string[base_class_count];
            for (int i = 0; i < base_class_count; i++)
            {
                if (!handle.ReadInt32(base_class_array_ptr + 4 * i, out int base_class_descriptor_offset)) continue;
                long base_class_descriptor_ptr = base_address + base_class_descriptor_offset;
                if (!handle.ReadInt32(base_class_descriptor_ptr, out int type_descriptor_offset)) continue;
                long type_descriptor_ptr = base_address + type_descriptor_offset;
                if (!handle.ReadString(type_descriptor_ptr + 0x14, 32, out names[i])) continue;
                if (names[i].EndsWith("@@"))
                {
                    var sb = new StringBuilder(255);
                    UnDecorateSymbolName("?" + names[i], sb, sb.Capacity, 0x1000);
                    names[i] = sb.ToString();
                }
            }
            return names;
        }

        /// <summary>
        /// Scans given address range for pointers to structures with given name.
        /// </summary>
        /// <returns>Global address to found pointer, or 0 if none found.</returns>
        public static long ScanClassName(this IntPtr handle, long baseAddress, string className, int maxOffset, byte step = 0x08)
        {
            for (long i = baseAddress; i < baseAddress + maxOffset; i += step)
            {
                if (!handle.ReadInt64(i, out long addr))
                    continue;
                string[] classes = handle.GetRTTIClassNames(addr);
                if (classes == null) continue;
                if (classes.Contains(className))
                    return i;
            }
            return 0;
        }
    }
}