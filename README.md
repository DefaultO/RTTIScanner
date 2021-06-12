[![CodeFactor](https://www.codefactor.io/repository/github/defaulto/theleftexit.memory/badge)](https://www.codefactor.io/repository/github/defaulto/theleftexit.memory)
### If this Project/Class helped you in your own Project, don't mind starring the original repository, found here: https://github.com/TheLeftExit/TheLeftExit.Memory

Using this class you don't have to Pointer Scan anymore for values sitting in named structures. Given you found your Values once, and noticed that the Structure they were in got a name, let's say, using the Cheat Engine's Feature **dissect data/structures** found in Memory Viewer > Tools, this will return you working and up-to-date pointers.

This comes with it's own rpm capabilities, so you technically don't have to rely on other Memory Libs. If you want to improve upon on this project, fork it from here: https://github.com/TheLeftExit/TheLeftExit.Memory

## Keep in mind that RTTI information only generates for classes that contain virtual function tables, and the method implemented here is only guaranteed for MSVC-built applications. Admin rights and a 64-bit system required.

The main purpose of this project was to implement some useful RTTI-based functionality I hadn't been able to find outside of larger projects, if at all.

![image](https://user-images.githubusercontent.com/42414542/121788422-749b4880-cbcd-11eb-915f-46eaf45c2b18.png)

This allows you to probe for base class names of a structure, similar to what Cheat Engine does when displaying Pointer to intance of ClassName in its Structure Viewer.
```csharp
string[] GetRTTIClassNames(this IntPtr handle, long address)
```
This allows you to scan a given address range for a pointer to a structure with a specific name.
```csharp
long ScanClassName(...)
```
This function generates a multi-level pointer based on names of structures along its way.
```csharp
List<int> ScanClassNameOffsets(...)
```

With the right applications, you can ditch your old multi-level pointers in favor of lists of class names which, unlike static offsets, won't change with version updates.
