![RTTIScanner-Banner](https://user-images.githubusercontent.com/42414542/132094787-86636a02-8757-4f61-bd74-bcb20b463350.png)
# ***RTTI*SCANNER**
All Credits go towards **[@TheLeftExit](https://github.com/TheLeftExit)**. I believe knowing several technicues like this one as a hacker, reverser or just a programmer is important as they make our lifes easier. RTTI-Scanning is the yet unknown sister next to Signature/AoB scanning and Pointer Hunting, and all it needs is the right environment and the structure/class name to be the perfect option out of all these options.

The main purpose of this project was to implement some useful RTTI-based functionality I hadn't been able to find outside of larger projects, if at all. With the right applications, you can ditch your old multi-level pointers in favor of lists of class names which, unlike static offsets, won't change with version updates.
## Keep in mind that RTTI information only generates for classes that contain virtual function tables, and the method implemented here is only guaranteed for MSVC-built applications. Admin rights and a 64-bit system is required.
