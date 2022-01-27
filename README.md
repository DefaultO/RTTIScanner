![RTTIScanner-Banner](https://user-images.githubusercontent.com/42414542/132094787-86636a02-8757-4f61-bd74-bcb20b463350.png)
# ***RTTI*SCANNER**

## <img src="./img/info.png" align="left" width="130px"/> Quick Reminder: Further (and better) Updates can be found in TLE's memory library project: <br/>https://github.com/TheLeftExit/TheLeftExit.Memory/blob/master/TheLeftExit.Memory/Rtti/RttiMethods.cs <br clear="left"/>

All Credits go towards **[@TheLeftExit](https://github.com/TheLeftExit)**. I believe knowing several technicues like this one as a hacker, reverser or just a programmer is important as they make our lifes easier. RTTI-Scanning is the yet unknown sister next to Signature/AoB scanning and Pointer Hunting, and all it needs is the right environment and the structure/class name to be the perfect option out of all these options.

The main purpose of this project was to implement some useful RTTI-based functionality I hadn't been able to find outside of larger projects, if at all. With the right applications, you can ditch your old multi-level pointers in favor of lists of class names which, unlike static offsets, won't change with version updates.
## Keep in mind that RTTI information only generates for classes that contain virtual function tables, and the method implemented here is only guaranteed for MSVC-built applications. Admin rights and a 64-bit system is required.

# Usage
Have you ever seen that [Cheat Engine](https://www.cheatengine.org/), when you dissect data/structures, gives some structures - names? Well, that's because Cheat Engine in fact got RTTI-functionality. Say you want to get the address of such a structure, all you need to pass to the RTTIScanner would be the range you want it to look for it, and the name that you got from reversing.

![RTTIScanner-Screen](https://user-images.githubusercontent.com/42414542/132096314-90fa7c47-b821-46bf-8517-854f3c2e5052.png)

This Code Snippet got taken out of **TDEM** and shows how easy it is to dynamically get the offset to your structure using this project/class:
```csharp
        public static long offsetToApp;

        static void Main(string[] args)
        {
            Color badMessage    = System.Drawing.ColorTranslator.FromHtml("#556cab");
            Color goodMessage   = System.Drawing.ColorTranslator.FromHtml("#80a1ff");
            Color comment       = System.Drawing.ColorTranslator.FromHtml("#2a3554");

            Process gt = Process.GetProcessesByName("Growtopia").First();
            long baseAddress = (long)gt.MainModule.BaseAddress;

            Console.WriteLine("/// Initializing..", comment);
            Int32 appBase = 0x400000;
            Int32 appBaseIncrement = 0x100000;
            long pointerToApp = gt.Handle.ScanClassName(baseAddress + appBase, "App", 0x600000);
            
            while (pointerToApp == 0)
            {
                Console.WriteLine($"[!] Couldn't find the pointer to app", badMessage);
                Console.WriteLine($"Trying to increment appBase by (HEX){appBaseIncrement.ToString("X")}", comment);
                appBase += appBaseIncrement;
                pointerToApp = gt.Handle.ScanClassName(baseAddress + appBase, "App", 0x600000);
            }
            Console.WriteLine($"[>] Found pointer to app at (HEX){pointerToApp.ToString("X")}", goodMessage);
            offsetToApp = pointerToApp - baseAddress;
            Console.WriteLine($"App Offset: (HEX){offsetToApp.ToString("X")}", comment);
        }
```
Looking at it you see this function call here which is all you need next to an open handle to the process:
```csharp
long pointerToApp = gt.Handle.ScanClassName(
    baseAddress + appBase,  // Where to start from
    "App",                  // Class/Struct Name
    0x600000                // Range to search in
);
```
