using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace RTTIScanner
{
    class PointerHunter
    {
        private IntPtr handle;
        private long mainModuleAddress;

        public Dictionary<string, long> AddressCache;
        public long getAddress(long baseAddress, string className, int maxOffset, byte step = 0x08)
        {
            if (!AddressCache.ContainsKey(className))
            {
                long res = handle.ScanClassName(baseAddress, className, maxOffset, step);
                if (res == 0 || !handle.ReadInt64(res, out long ret))
                    throw new Exception($"Couldn't find address for {className}.");
                AddressCache.Add(className, ret);
            }
            return AddressCache[className];
        }

        // only so i can write GetWorldData as a ladder as well
        public long getAddress(long address)
        {
            if (!handle.ReadInt64(address, out long res))
                throw new Exception($"Couldn't read at {address}.");
            return res;
        }

        public PointerHunter(Process process, Dictionary<string, long> cache = null)
        {
            handle = process.Handle;
            mainModuleAddress = (long)process.MainModule.BaseAddress;
            AddressCache = cache ?? new Dictionary<string, long>();
        }

        /* Examples:

        public long GetPlayerX() =>
            getAddress(
                getAddress(
                    getAddress(
                        mainModuleAddress + 0x400000,
                        "App", 0x400000),
                    "GameLogicComponent", 0x1000),
                "NetAvatar", 0x1000) + 0x8;

        public long GetPlayerY() =>
            getAddress(
                getAddress(
                    getAddress(
                        mainModuleAddress + 0x400000,
                        "App", 0x400000),
                    "GameLogicComponent", 0x1000),
                "NetAvatar", 0x1000) + 0xC;

        public long GetPlayerDir() =>
            getAddress(
                getAddress(
                    getAddress(
                        mainModuleAddress + 0x400000,
                        "App", 0x400000),
                    "GameLogicComponent", 0x1000),
                "NetAvatar", 0x1000) + 0x61;

        public long GetWorldData() =>
            getAddress(
                getAddress(
                    getAddress(
                        getAddress(
                            mainModuleAddress + 0x400000,
                            "App", 0x400000),
                        "GameLogicComponent", 0x1000),
                    "World", 0x1000) + 0x28);

        */
    }
}