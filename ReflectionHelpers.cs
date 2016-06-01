using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System
{
    internal static class ReflectionHelpers
    {
        public static void GetEventMethods(this Type @this, string eventName, out MethodInfo addMethod, out MethodInfo removeMethod)
        {
            EventInfo eventInfo = @this.GetEvent(eventName, BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (eventInfo == null)
            {
                addMethod = null;
                removeMethod = null;
                return;
            }
            addMethod = eventInfo.AddMethod;
            removeMethod = eventInfo.RemoveMethod;
        }

        public static Delegate CreateEventDelegate<TEventArgs, TReturn>(this object @this, string onMethodName, Type realEventArgsType, Type realHandlerType) where TEventArgs : EventArgs
        {
            ParameterExpression eParameter = Expression.Parameter(typeof(TEventArgs));
            ParameterExpression argsParameter = Expression.Parameter(realEventArgsType);
            return Expression.Lambda(
                realHandlerType,
                Expression.Invoke(
                    Expression.Lambda<Func<TEventArgs, TReturn>>(
                        Expression.Call(
                            Expression.Constant(
                                @this, typeof(AppDomain)),
                            typeof(AppDomain).GetMethod(
                                onMethodName,
                                BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic),
                            Enumerable.Repeat(eParameter, 1)),
                        Enumerable.Repeat(
                            eParameter,
                            1)),
                    Enumerable.Repeat(
                        Expression.New(
                            eParameter.Type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First(),
                            Enumerable.Repeat(
                                argsParameter,
                                1)),
                        1)),
                false,
#pragma warning disable HeapAnalyzerExplicitNewArrayRule // Explicit new array type allocation
                (new ParameterExpression[]
#pragma warning restore HeapAnalyzerExplicitNewArrayRule // Explicit new array type allocation
                {
                    Expression.Parameter(typeof(object)),
                    argsParameter
                }).AsEnumerable()).Compile();
        }

        public static Delegate CreateEventDelegate<TEventArgs>(this object @this, string onMethodName, Type realEventArgsType, Type realHandlerType) where TEventArgs : EventArgs
        {
            ParameterExpression eParameter = Expression.Parameter(typeof(TEventArgs));
            ParameterExpression argsParameter = Expression.Parameter(realEventArgsType);
            return Expression.Lambda(
                realHandlerType,
                Expression.Invoke(
                    Expression.Lambda<Action<TEventArgs>>(
                        Expression.Call(
                            Expression.Constant(
                                @this, typeof(AppDomain)),
                            typeof(AppDomain).GetMethod(
                                onMethodName,
                                BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic),
                            Enumerable.Repeat(eParameter, 1)),
                        Enumerable.Repeat(
                            eParameter,
                            1)),
                    Enumerable.Repeat(
                        Expression.New(
                            eParameter.Type.GetConstructors(BindingFlags.NonPublic | BindingFlags.Instance).First(),
                            Enumerable.Repeat(
                                argsParameter,
                                1)),
                        1)),
                false,
#pragma warning disable HeapAnalyzerExplicitNewArrayRule // Explicit new array type allocation
                (new ParameterExpression[]
#pragma warning restore HeapAnalyzerExplicitNewArrayRule // Explicit new array type allocation
                {
                    Expression.Parameter(typeof(object)),
                    argsParameter
                }).AsEnumerable()).Compile();
        }

        public static void AttachOrDetachEvent(this object @this, MulticastDelegate @delegate, Delegate realDelegate, MethodInfo manipulationInfo)
        {
            if (@delegate == null || @delegate.GetInvocationList().Length == 0)
            {
#pragma warning disable HeapAnalyzerImplicitNewArrayCreationRule // Implicit new array creation allocation
                manipulationInfo.Invoke(@this, new[] { realDelegate });
#pragma warning restore HeapAnalyzerImplicitNewArrayCreationRule // Implicit new array creation allocation
            }
        }

        public static Func<object> GetStaticPropertyFunction(this Type @this, string propertyName) => Expression.Lambda<Func<object>>(Expression.Property(null, @this.GetProperty(propertyName, BindingFlags.DeclaredOnly | BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic).GetMethod), Enumerable.Repeat<ParameterExpression>(null, 0)).Compile();

        public static Func<object, T> GetInstancePropertyFunction<T>(this Type @this, string propertyName)
        {
            PropertyInfo propertyInfo = @this.GetProperty(propertyName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (propertyInfo == null)
            {
                return null;
            }
            ParameterExpression parameter = Expression.Parameter(typeof(object));
            return Expression.Lambda<Func<object, T>>(Expression.Property(Expression.Convert(parameter, @this), propertyInfo.GetMethod), true, Enumerable.Repeat(parameter, 1)).Compile();
        }

        public static Func<object, T> GetInstanceFieldFunction<T>(this Type @this, string fieldName)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(object));
            return Expression.Lambda<Func<object, T>>(Expression.Field(Expression.Convert(parameter, @this), @this.GetField(fieldName, BindingFlags.DeclaredOnly | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)), true, Enumerable.Repeat(parameter, 1)).Compile();
        }

        public static Func<object, T> GetInstanceFunctionFunction<T>(this Type @this, string functionName)
        {
            ParameterExpression parameter = Expression.Parameter(typeof(object));
            return Expression.Lambda<Func<object, T>>(Expression.Call(Expression.Convert(parameter, @this), @this.GetMethod(functionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)), true, Enumerable.Repeat(parameter, 1)).Compile();
        }

        public static Func<object, TArg, TResult> GetInstanceFunctionFunction<TArg, TResult>(this Type @this, string functionName)
        {
            ParameterExpression thisParameter = Expression.Parameter(typeof(object));
            ParameterExpression argParameter = Expression.Parameter(typeof(TArg));
            MethodInfo[] methodInfos = @this.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo methodInfo = null;
            for(int i = 0; i < methodInfos.Length; i++, methodInfo = null)
            {
                methodInfo = methodInfos[i];
                if (methodInfo.Name == functionName)
                {
                    ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType.FullName == typeof(TArg).FullName)
                    {
                        break;
                    }
                }
            }
            if (methodInfo == null)
            {
                return null;
            }
            return Expression.Lambda<Func<object, TArg, TResult>>(Expression.Call(Expression.Convert(thisParameter, @this), methodInfo, Enumerable.Repeat(argParameter, 1)), true, new[] { thisParameter, argParameter }).Compile();
        }

        public static Type RealType(this Type @this) => typeof(string).GetTypeInfo().Assembly.GetType(@this.FullName);
    }
}
