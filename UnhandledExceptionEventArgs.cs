using System.Reflection;

namespace System
{
    public class UnhandledExceptionEventArgs : EventArgs
    {
        internal static readonly Type RealType = typeof(string).GetTypeInfo().Assembly.GetType(typeof(UnhandledExceptionEventArgs).FullName);
        private static readonly Func<object, object> getExceptionObject;
        private static readonly Func<object, bool> getIsTerminating;

        static UnhandledExceptionEventArgs()
        {
            getExceptionObject = RealType.GetInstancePropertyFunction<object>(nameof(ExceptionObject));
            getIsTerminating = RealType.GetInstancePropertyFunction<bool>(nameof(IsTerminating));
        }

        private readonly object unhandledExceptionEventArgs;

        public object ExceptionObject
        {
            get
            {
                return getExceptionObject(unhandledExceptionEventArgs);
            }
        }

        public bool IsTerminating
        {
            get
            {
                return getIsTerminating(unhandledExceptionEventArgs);
            }
        }

        internal UnhandledExceptionEventArgs(object unhandledExceptionEventArgs)
        {
            unhandledExceptionEventArgs.NotNull(nameof(unhandledExceptionEventArgs));
            unhandledExceptionEventArgs.InstanceOf(nameof(unhandledExceptionEventArgs), RealType);
            this.unhandledExceptionEventArgs = unhandledExceptionEventArgs;
        }
    }
}
