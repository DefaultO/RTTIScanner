[![CodeFactor](https://www.codefactor.io/repository/github/defaulto/theleftexit.memory/badge)](https://www.codefactor.io/repository/github/defaulto/theleftexit.memory)
### If this Project/Class helped you in your own Project, don't mind starring the original repository, found here: https://github.com/TheLeftExit/TheLeftExit.Memory

Using this class you don't have to Pointer Scan anymore for values sitting in named structures. Given you found your Values once, and noticed that the Structure they were in got a name, let's say, using the Cheat Engine's Feature **dissect data/structures** found in Memory Viewer > Tools, this will return you working and up-to-date pointers.

This comes with it's own rpm capabilities, so you technically don't have to rely on other Memory Libs. If you want to improve upon on this project, fork it from here: https://github.com/TheLeftExit/TheLeftExit.Memory

## Keep in mind that RTTI information only generates for classes that contain virtual function tables, and the method implemented here is only guaranteed for MSVC-built applications. Admin rights and a 64-bit system required.

The main purpose of this project was to implement some useful RTTI-based functionality I hadn't been able to find outside of larger projects, if at all. With the right applications, you can ditch your old multi-level pointers in favor of lists of class names which, unlike static offsets, won't change with version updates.


![image](https://user-images.githubusercontent.com/42414542/121788422-749b4880-cbcd-11eb-915f-46eaf45c2b18.png)

## Usage
```csharp
// Taken from https://github.com/DefaultO/TheLeftExit.Memory/blob/8bb868205239ccbbb2681aa470865069515fee9a/Memory.cs#L120
public static string[] GetRTTIClassNames(this IntPtr handle, long address)
```
``GetRTTIClassNames`` allows you to probe for base class names of a structure, similar to what Cheat Engine does when displaying **Pointer to intance of *ClassName*** in its Structure Viewer.
```csharp
// Taken from https://github.com/DefaultO/TheLeftExit.Memory/blob/8bb868205239ccbbb2681aa470865069515fee9a/Memory.cs#L155
public static long ScanClassName(this IntPtr handle, long baseAddress, string className, int maxOffset)
```
``ScanClassName`` allows you to scan a given address range for a pointer to a structure with a specific name.
```csharp
// Taken from https://github.com/DefaultO/TheLeftExit.Memory/blob/8bb868205239ccbbb2681aa470865069515fee9a/Memory.cs#L173
public static List<int> ScanClassNameOffsets(this IntPtr handle, long baseAddress, params (string className, int maxOffset)[] searchParameters)
```
``ScanClassNameOffsets`` function generates a multi-level pointer based on names of structures along its way.

## Example
```csharp
Process gt = Process.GetProcessesByName("Growtopia").Single();
IntPtr gthandle = gt.OpenProcess();

long baseAddress = (long)gt.MainModule.BaseAddress;
string res = "Growtopia.exe+";

string[] targetClasses = new string[] { "App", "GameLogicComponent", "NetAvatar" };
string offsets = "";
long address = 0;
long lastAddress = baseAddress;
for (int index = 0; index < targetClasses.Length; index++)
{
    address = gthandle.ScanForPointerToClass(targetClasses[index], lastAddress, index == 0 ? 0x800000 : 0x1000);
    offsets += (address - lastAddress).ToString("X") + Environment.NewLine;
    gthandle.ReadInt64(address, out lastAddress);
}
MessageBox.Show(offsets);
```
Will result into these messagebox contents:

![image](https://user-images.githubusercontent.com/42414542/121859416-f7202700-ccf7-11eb-9c5e-7e73bd50ae7b.png)

The long hex string would stand behind the "res" string (line 5). And the shorter ones would be the offsets.
Final product could look like this "Growtopia.exe+7667F8,AB0,198" (Memory.dll Design) or my / Azukii's Design:
```csharp
public static Pointer NetAvatar = new Pointer
{
    Module = "Growtopia.exe",
    Offset = "0x7667F8",
    Offsets = new string[] { "AB0", "198" }
};
```

