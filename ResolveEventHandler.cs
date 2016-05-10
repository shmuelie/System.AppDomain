using System.Reflection;

namespace System
{
    /// <summary>
    ///     Represents a method that handles the <see cref="AppDomain.TypeResolve"/>, <see cref="AppDomain.ResourceResolve"/>, or <see cref="AppDomain.AssemblyResolve"/> event of an <see cref="AppDomain"/>.
    /// </summary>
    /// <param name="sender">The source of the event.</param>
    /// <param name="args">The event data.</param>
    /// <returns>The assembly that resolves the type, assembly, or resource; or <see langword="null"/> if the assembly cannot be resolved.</returns>
    /// <remarks>If the runtime class loader cannot resolve a reference to an assembly, type, or resource, the corresponding events are raised to give the callback a chance to tell the runtime which assembly the referenced assembly, type, or resource is in. It is the responsibility of the ResolveEventHandler to return the assembly that resolves the type, assembly, or resource, or to return null if the assembly is not recognized.</remarks>
    /// <seealso cref="AppDomain.ResourceResolve"/>
    /// <seealso cref="AppDomain.ReflectionOnlyAssemblyResolve"/>
    /// <seealso cref="AppDomain.AssemblyResolve"/>
    public delegate Assembly ResolveEventHandler(object sender, ResolveEventArgs args);
}
