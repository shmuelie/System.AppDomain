using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace System
{
    internal static class ReflectionHelpers
    {
        public static void GetEventMethods(this Type @this, string eventName, out Action<object, Delegate> addMethod, out Action<object, Delegate> removeMethod)
        {
            EventInfo eventInfo = @this.GetEvent(eventName, BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            if (eventInfo == null)
            {
                FieldInfo fieldInfo = @this.GetField($"_{char.ToLowerInvariant(eventName[0])}{eventName.Substring(1)}", BindingFlags.FlattenHierarchy | BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
                if (fieldInfo == null)
                {
                    addMethod = null;
                    removeMethod = null;
                    return;
                }
                EventFieldClosure efc = new EventFieldClosure(fieldInfo);
                addMethod = efc.Add;
                removeMethod = efc.Remove;
            }
            else
            {
                addMethod = CreateEventMethod(eventInfo.AddMethod);
                removeMethod = CreateEventMethod(eventInfo.RemoveMethod);
            }
        }

        private sealed class EventFieldClosure
        {
            private readonly FieldInfo fieldInfo;

            public EventFieldClosure(FieldInfo fieldInfo)
            {
                this.fieldInfo = fieldInfo;
                Add = new Action<object, Delegate>(AddMethod);
                Remove = new Action<object, Delegate>(RemoveMethod);
            }

            private void AddMethod(object @this, Delegate value)
            {
                fieldInfo.SetValue(@this, Delegate.Combine((Delegate)fieldInfo.GetValue(@this), value));
            }

            private void RemoveMethod(object @this, Delegate value)
            {
                fieldInfo.SetValue(@this, Delegate.Remove((Delegate)fieldInfo.GetValue(@this), value));
            }

            public Action<object, Delegate> Add
            {
                get;
            }

            public Action<object, Delegate> Remove
            {
                get;
            }
        }

        private static Action<object, Delegate> CreateEventMethod(MethodInfo methodInfo)
        {
            ParameterExpression thisParameter = Expression.Parameter(typeof(object));
            UnaryExpression convertThis = Expression.Convert(thisParameter, methodInfo.DeclaringType);
            ParameterExpression valueParamater = Expression.Parameter(typeof(Delegate));
            UnaryExpression convertValue = Expression.Convert(valueParamater, methodInfo.GetParameters()[0].ParameterType);
            return Expression.Lambda<Action<object, Delegate>>(Expression.Call(convertThis, methodInfo, Enumerable.Repeat(convertValue, 1)), false, new ParameterExpression[] { thisParameter, valueParamater }).Compile();
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
                            eParameter.Type.GetConstructor(typeof(object)),
                            Enumerable.Repeat(
                                argsParameter,
                                1)),
                        1)),
                false,
                (new ParameterExpression[]
                {
                    Expression.Parameter(typeof(object)),
                    argsParameter
                }).AsEnumerable()).Compile();
        }

        public static Delegate CreateEventDelegate<TEventArgs>(this object @this, string onMethodName, Type realEventArgsType, Type realHandlerType) where TEventArgs : EventArgs
        {
            ParameterExpression eParameter = Expression.Parameter(typeof(TEventArgs));
            ParameterExpression argsParameter = Expression.Parameter(realEventArgsType);
            ConstructorInfo constructorInfo = eParameter.Type.GetConstructor(typeof(object)) ?? eParameter.Type.GetDefaultConstructor();
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
                            constructorInfo,
                            Enumerable.Repeat(
                                argsParameter,
                                constructorInfo.GetParameters().Length)),
                        1)),
                false,
                (new ParameterExpression[]
                {
                    Expression.Parameter(typeof(object)),
                    argsParameter
                }).AsEnumerable()).Compile();
        }

        public static void AttachOrDetachEvent(this object @this, MulticastDelegate @delegate, Delegate realDelegate, Action<object, Delegate> realAction)
        {
            if (@delegate == null || @delegate.GetInvocationList().Length == 0)
            {
                realAction?.Invoke(@this, realDelegate);
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
            MethodInfo methodInfo = @this.GetMethod(functionName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            if (methodInfo == null)
            {
                return null;
            }
            return Expression.Lambda<Func<object, T>>(Expression.Call(Expression.Convert(parameter, @this), methodInfo), true, Enumerable.Repeat(parameter, 1)).Compile();
        }

        public static Func<object, TArg, TResult> GetInstanceFunctionFunction<TArg, TResult>(this Type @this, string functionName)
        {
            ParameterExpression thisParameter = Expression.Parameter(typeof(object));
            ParameterExpression argParameter = Expression.Parameter(typeof(TArg));
            MethodInfo[] methodInfos = @this.GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
            MethodInfo methodInfo = null;
            for (int i = 0; i < methodInfos.Length; i++, methodInfo = null)
            {
                methodInfo = methodInfos[i];
                if (methodInfo.Name == functionName)
                {
                    ParameterInfo[] parameterInfos = methodInfo.GetParameters();
                    if (parameterInfos.Length == 1 && parameterInfos[0].ParameterType.IsSameAs(typeof(TArg)))
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

        public static ConstructorInfo GetDefaultConstructor(this Type @this) => @this.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(ci => ci.GetParameters().Length == 0);

        public static ConstructorInfo GetConstructor(this Type @this, Type argumentType) => @this.GetConstructors(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance).FirstOrDefault(new ConstructorArgumentFilter(argumentType).Predicate);

        public static bool IsSameAs(this Type @this, Type other) => @this.Equals(other) || (@this.IsAssignableFrom(other) && other.IsAssignableFrom(@this));

        private sealed class ConstructorArgumentFilter
        {
            private readonly Type argumentType;

            public ConstructorArgumentFilter(Type argumentType)
            {
                this.argumentType = argumentType;
                Predicate = new Func<ConstructorInfo, bool>(Filter);
            }

            private bool Filter(ConstructorInfo constructorInfo)
            {
                ParameterInfo[] parameterInfos = constructorInfo.GetParameters();
                if (parameterInfos.Length != 1)
                {
                    return false;
                }
                return argumentType.IsSameAs(parameterInfos[0].ParameterType);
            }

            public Func<ConstructorInfo, bool> Predicate
            {
                get;
            }
        }

        public static Type RealType(this Type @this) => typeof(string).GetTypeInfo().Assembly.GetType(@this.FullName);
    }
}
