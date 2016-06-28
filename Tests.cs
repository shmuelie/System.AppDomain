#if TEST
using System.Reflection;

namespace System
{
    public static class Tests
    {
        public static void Main()
        {
            AppDomain appDomain = AppDomain.CurrentDomain;
            appDomain.AssemblyResolve += AppDomain_AssemblyResolve;
            appDomain.ProcessExit += AppDomain_ProcessExit;
            appDomain.ResourceResolve += AppDomain_ResourceResolve;
            appDomain.TypeResolve += AppDomain_TypeResolve;
            appDomain.UnhandledException += AppDomain_UnhandledException;
            string baseDirectory = appDomain.BaseDirectory;
            string friendlyName = appDomain.FriendlyName;
            Assembly[] assemblies = appDomain.GetAssemblies();
            object data = appDomain.GetData(string.Empty);
            bool isFullyTrusted = appDomain.IsFullyTrusted;
            bool isHomogenous = appDomain.IsHomogenous;
        }

        private static void AppDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
        }

        private static Assembly AppDomain_TypeResolve(object sender, ResolveEventArgs args)
        {
            return null;
        }

        private static Assembly AppDomain_ResourceResolve(object sender, ResolveEventArgs args)
        {
            return null;
        }

        private static void AppDomain_ProcessExit(object sender, EventArgs e)
        {
        }

        private static Assembly AppDomain_AssemblyResolve(object sender, ResolveEventArgs args)
        {
            return null;
        }
    }
}
#endif