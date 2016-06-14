using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

// General Information about an assembly is controlled through the following
// set of attributes. Change these attribute values to modify the information
// associated with an assembly.
[assembly: AssemblyTitle("Exposes the hidden System.AppDomain in .NET Core")]
[assembly: AssemblyDescription("Exposes the hidden System.AppDomain in .NET Core. Required supporting classes are also exposed.")]
[assembly: AssemblyConfiguration(
#if DEBUG
    "Debug"
#elif RELEASE
    "Release"
#endif
    )]
[assembly: AssemblyCompany("Shmueli Englard")]
[assembly: AssemblyProduct("System.AppDomain")]
[assembly: AssemblyCopyright("Copyright © 2016 Shmueli Englard")]
[assembly: AssemblyTrademark("")]
[assembly: AssemblyCulture("neutral")]

// Setting ComVisible to false makes the types in this assembly not visible
// to COM components.  If you need to access a type in this assembly from
// COM, set the ComVisible attribute to true on that type.
[assembly: ComVisible(false)]

// The following GUID is for the ID of the typelib if this project is exposed to COM
[assembly: Guid("c418225c-309e-46fe-a25b-5d484f75e671")]
[assembly: CLSCompliant(true)]
