namespace Mvvm.Utils.Services {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;

    public static class DynamicServiceSource {
        public static readonly object[] EmptyParameters = new object[] { };
        public static T Create<T>(Type serviceType) {
            return CreateCore<T>(new Type[] { serviceType });
        }
        public static T Create<T, P1>(Type serviceType, P1 parameter1) {
            return CreateCore<T, P1>(new Type[] { serviceType }, parameter1);
        }
        public static T Create<T, P1, P2>(Type serviceType, P1 parameter1, P2 parameter2) {
            return CreateCore<T, P1, P2>(new Type[] { serviceType }, parameter1, parameter2);
        }
        public static T Create<T, P1, P2, P3>(Type serviceType, P1 parameter1, P2 parameter2, P3 parameter3) {
            return CreateCore<T, P1, P2, P3>(new Type[] { serviceType }, parameter1, parameter2, parameter3);
        }
        public static T Create<T>(Type[] serviceTypes) {
            return CreateCore<T>(serviceTypes);
        }
        public static T Create<T, P1>(Type[] serviceTypes, P1 parameter1) {
            return CreateCore<T, P1>(serviceTypes, parameter1);
        }
        public static T Create<T, P1, P2>(Type[] serviceTypes, P1 parameter1, P2 parameter2) {
            return CreateCore<T, P1, P2>(serviceTypes, parameter1, parameter2);
        }
        public static T Create<T, P1, P2, P3>(Type[] serviceTypes, P1 parameter1, P2 parameter2, P3 parameter3) {
            return CreateCore<T, P1, P2, P3>(serviceTypes, parameter1, parameter2, parameter3);
        }
        static IDictionary<Type, Delegate> createCache0 = new Dictionary<Type, Delegate>();
        static T CreateCore<T>(Type[] interfaces) {
            Delegate create = null;
            if(!createCache0.TryGetValue(typeof(T), out create)) {
                Type type = CreateType(typeof(T), interfaces);
                var ctorInfo = type.GetConstructor(Type.EmptyTypes);
                var ctorExpr = Expression.New(ctorInfo);
                create = Expression.Lambda(ctorExpr).Compile();
                createCache0.Add(typeof(T), create);
            }
            return ((Func<T>)create)();
        }
        static IDictionary<Type, Delegate> createCache1 = new Dictionary<Type, Delegate>();
        static T CreateCore<T, P1>(Type[] interfaces, P1 parameter1) {
            Delegate create = null;
            if(!createCache1.TryGetValue(typeof(T), out create)) {
                Type type = CreateType(typeof(T), interfaces);
                var ctorInfo = type.GetConstructor(new Type[] { typeof(P1) });
                ParameterExpression p1 = Expression.Parameter(typeof(P1));
                var ctorExpr = Expression.New(ctorInfo, p1);
                create = Expression.Lambda(ctorExpr, p1).Compile();
                createCache1.Add(typeof(T), create);
            }
            return ((Func<P1, T>)create)(parameter1);
        }
        static IDictionary<Type, Delegate> createCache2 = new Dictionary<Type, Delegate>();
        static T CreateCore<T, P1, P2>(Type[] interfaces, P1 parameter1, P2 parameter2) {
            Delegate create = null;
            if(!createCache2.TryGetValue(typeof(T), out create)) {
                Type type = CreateType(typeof(T), interfaces);
                var ctorInfo = type.GetConstructor(new Type[] { typeof(P1), typeof(P2) });
                ParameterExpression p1 = Expression.Parameter(typeof(P1));
                ParameterExpression p2 = Expression.Parameter(typeof(P2));
                var ctorExpr = Expression.New(ctorInfo, p1, p2);
                create = Expression.Lambda(ctorExpr, p1, p2).Compile();
                createCache2.Add(typeof(T), create);
            }
            return ((Func<P1, P2, T>)create)(parameter1, parameter2);
        }
        static IDictionary<Type, Delegate> createCache3 = new Dictionary<Type, Delegate>();
        static T CreateCore<T, P1, P2, P3>(Type[] interfaces, P1 parameter1, P2 parameter2, P3 parameter3) {
            Delegate create = null;
            if(!createCache3.TryGetValue(typeof(T), out create)) {
                Type type = CreateType(typeof(T), interfaces);
                var ctorInfo = type.GetConstructor(new Type[] { typeof(P1), typeof(P2), typeof(P3) });
                ParameterExpression p1 = Expression.Parameter(typeof(P1));
                ParameterExpression p2 = Expression.Parameter(typeof(P2));
                ParameterExpression p3 = Expression.Parameter(typeof(P3));
                var ctorExpr = Expression.New(ctorInfo, p1, p2, p3);
                create = Expression.Lambda(ctorExpr, p1, p2, p3).Compile();
                createCache3.Add(typeof(T), create);
            }
            return ((Func<P1, P2, P3, T>)create)(parameter1, parameter2, parameter3);
        }
        static IDictionary<Type, Type> typesCache = new Dictionary<Type, Type>();
        static Type CreateType(Type sourceType, Type[] interfaces) {
            Type result;
            if(!typesCache.TryGetValue(sourceType, out result)) {
                var typeBuilder = DynamicServiceHelper.GetTypeBuilder(interfaces[0], sourceType);
                var cInfos = sourceType.GetConstructors(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
                for(int i = 0; i < cInfos.Length; i++)
                    CreateConstructor(cInfos[i], typeBuilder, sourceType);
                for(int i = 0; i < interfaces.Length; i++)
                    CreateInterfaceImplementation(sourceType, interfaces[i], typeBuilder);
                result = typeBuilder.CreateType();
                typesCache.Add(sourceType, result);
            }
            return result;
        }
        static void CreateInterfaceImplementation(Type sourceType, Type interfaceType, TypeBuilder typeBuilder) {
            IDictionary<MethodInfo, MethodBuilder> methodsCache = new Dictionary<MethodInfo, MethodBuilder>();
            var methods = interfaceType.GetMethods();
            for(int i = 0; i < methods.Length; i++) {
                var builder = CreateMethod(methods[i], typeBuilder, sourceType);
                if(builder != null) {
                    methodsCache.Add(methods[i], builder);
                    typeBuilder.DefineMethodOverride(builder, methods[i]);
                }
            }
            var properties = interfaceType.GetProperties();
            for(int i = 0; i < properties.Length; i++)
                CreateProperty(properties[i], typeBuilder, methodsCache);
            var events = interfaceType.GetEvents();
            for(int i = 0; i < events.Length; i++)
                CreateEvent(events[i], typeBuilder, methodsCache);
            typeBuilder.AddInterfaceImplementation(interfaceType);
        }
        static void CreateConstructor(ConstructorInfo cInfo, TypeBuilder typeBuilder, Type sourceType) {
            var parameterTypes = GetParameterTypes(cInfo);
            var ctorBuilder = typeBuilder.DefineConstructor(MethodAttributes.Public, CallingConventions.Standard, parameterTypes);
            var ctorGenerator = ctorBuilder.GetILGenerator();
            ctorGenerator.Emit(OpCodes.Ldarg_0);
            EmitLdargs(parameterTypes, ctorGenerator);
            ctorGenerator.Emit(OpCodes.Call, cInfo);
            ctorGenerator.Emit(OpCodes.Ret);
        }
        const MethodAttributes methodAttributes = MethodAttributes.Private | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual | MethodAttributes.Final;
        static MethodBuilder CreateMethod(MethodInfo mInfo, TypeBuilder typeBuilder, Type sourceType) {
            var parameterTypes = GetParameterTypes(mInfo);
            var customAttributes = (mInfo.IsSpecialName) ? MethodAttributes.SpecialName : MethodAttributes.Private;
            var methodBuilder = typeBuilder.DefineMethod(DefineName(mInfo), customAttributes | methodAttributes, mInfo.ReturnType, parameterTypes);
            var methodGenerator = methodBuilder.GetILGenerator();
            var sourceMethod = sourceType.GetMethod(mInfo.Name, CheckParameterTypes(parameterTypes, mInfo.DeclaringType));
            if(sourceMethod != null) {
                methodGenerator.Emit(OpCodes.Ldarg_0); // @this
                EmitLdargs(parameterTypes, methodGenerator);
                methodGenerator.Emit(OpCodes.Call, sourceMethod);
                if(sourceMethod.ReturnType != CheckEnumType(mInfo.ReturnType, mInfo.DeclaringType))
                    methodGenerator.Emit(OpCodes.Castclass, mInfo.ReturnType);
            }
            else {
                if(mInfo.ReturnType != typeof(void)) {
                    if(!mInfo.ReturnType.IsValueType)
                        methodGenerator.Emit(OpCodes.Ldnull);
                    else {
                        Action<ILGenerator> generate;
                        if(defaultValuesGenerator.TryGetValue(CheckEnumType(mInfo.ReturnType), out generate))
                            generate(methodGenerator);
                    }
                }
            }
            methodGenerator.Emit(OpCodes.Ret);
            return methodBuilder;
        }
        static PropertyBuilder CreateProperty(PropertyInfo pInfo, TypeBuilder typeBuilder, IDictionary<MethodInfo, MethodBuilder> methodsCache) {
            var propertyBuilder = typeBuilder.DefineProperty(DefineName(pInfo), PropertyAttributes.None, CallingConventions.HasThis, pInfo.PropertyType, null);
            if(pInfo.CanRead)
                propertyBuilder.SetGetMethod(methodsCache[pInfo.GetGetMethod()]);
            if(pInfo.CanWrite)
                propertyBuilder.SetSetMethod(methodsCache[pInfo.GetSetMethod()]);
            return propertyBuilder;
        }
        static EventBuilder CreateEvent(EventInfo eInfo, TypeBuilder typeBuilder, IDictionary<MethodInfo, MethodBuilder> methodsCache) {
            var eventBuilder = typeBuilder.DefineEvent(DefineName(eInfo), EventAttributes.None, eInfo.EventHandlerType);
            eventBuilder.SetAddOnMethod(methodsCache[eInfo.GetAddMethod()]);
            eventBuilder.SetRemoveOnMethod(methodsCache[eInfo.GetRemoveMethod()]);
            return eventBuilder;
        }
        static string DefineName(MemberInfo memberInfo) {
            return memberInfo.DeclaringType.FullName + "." + memberInfo.Name;
        }
        static Type CheckEnumType(Type type) {
            return type.IsEnum ? type.GetEnumUnderlyingType() : type;
        }
        static Type CheckEnumType(Type type, Type interfaceType) {
            return (interfaceType.Assembly == type.Assembly) ? CheckEnumType(type) : type;
        }
        static IDictionary<Type, Action<ILGenerator>> defaultValuesGenerator = new Dictionary<Type, Action<ILGenerator>> { 
            { typeof(int), (g) => g.Emit(OpCodes.Ldc_I4_0) },
            { typeof(byte), (g) => g.Emit(OpCodes.Ldc_I4_0) },
            { typeof(short), (g) => g.Emit(OpCodes.Ldc_I4_0) },
            { typeof(long), (g) => g.Emit(OpCodes.Ldc_I8, (long)0) },
            { typeof(float), (g) => g.Emit(OpCodes.Ldc_R4, 0f) },
            { typeof(double), (g) => g.Emit(OpCodes.Ldc_R8, 0.0) },
        };
        static Type[] GetParameterTypes(MethodBase method) {
            return method.GetParameters().Select(p => p.ParameterType).ToArray();
        }
        static Type[] CheckParameterTypes(Type[] parameterTypes, Type interfaceType) {
            return parameterTypes.Select(t => CheckEnumType(t, interfaceType)).ToArray();
        }
        static OpCode[] args = new OpCode[] { OpCodes.Ldarg_1, OpCodes.Ldarg_2, OpCodes.Ldarg_3 };
        static void EmitLdargs(Array parameters, ILGenerator generator) {
            for(int i = 0; i < parameters.Length; i++) {
                if(i < 3)
                    generator.Emit(args[i]);
                else
                    generator.Emit(OpCodes.Ldarg_S, i + 1);
            }
        }
        public static void Reset() {
            createCache0.Clear();
            createCache1.Clear();
            createCache2.Clear();
            createCache3.Clear();
            typesCache.Clear();
        }
    }
    //
    static class DynamicServiceHelper {
        static readonly string dynamicSuffix = ".Dynamic." + Guid.NewGuid().ToString();
#if DEBUGTEST
        static IDictionary<string, AssemblyBuilder> aCache = new Dictionary<string, AssemblyBuilder>();
#endif
        static IDictionary<string, ModuleBuilder> mCache = new Dictionary<string, ModuleBuilder>();
        internal static TypeBuilder GetTypeBuilder(Type serviceType) {
            var moduleBuilder = GetModuleBuilder(serviceType.Assembly);
            return moduleBuilder.DefineType(serviceType.Name + dynamicSuffix);
        }
        internal static TypeBuilder GetTypeBuilder(Type serviceType, Type sourceType) {
            var moduleBuilder = GetModuleBuilder(serviceType.Assembly);
            return moduleBuilder.DefineType(sourceType.Name + dynamicSuffix, TypeAttributes.NotPublic, sourceType);
        }
        internal static ModuleBuilder GetModuleBuilder(Assembly assembly) {
            string strAssemblyName = assembly.GetName().Name;
            ModuleBuilder moduleBuilder;
            if(!mCache.TryGetValue(strAssemblyName, out moduleBuilder)) {
                var assemblyName = new AssemblyName(strAssemblyName + dynamicSuffix);
#if DEBUGTEST
                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.RunAndSave);
                aCache.Add(strAssemblyName, assemblyBuilder);
#else
                var assemblyBuilder = AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);
#endif
                moduleBuilder = assemblyBuilder.DefineDynamicModule(strAssemblyName + dynamicSuffix + ".dll");
                mCache.Add(strAssemblyName, moduleBuilder);
            }
            return moduleBuilder;
        }
#if DEBUGTEST
        internal static void Save() {
            foreach(var item in aCache)
                item.Value.Save(item.Key + dynamicSuffix + ".dll");
        }
#endif
        internal static Func<System.Collections.IEnumerable, object> GetEnumerableCast(
            ref Func<System.Collections.IEnumerable, object> enumerableCastConverter, Func<Type> getType) {
            if(enumerableCastConverter == null) {
                var castMethod = typeof(Enumerable).GetMethod("Cast").MakeGenericMethod(getType());
                var source = Expression.Parameter(typeof(System.Collections.IEnumerable), "source");
                enumerableCastConverter = Expression.Lambda<Func<System.Collections.IEnumerable, object>>(
                    Expression.Call(castMethod, source), source).Compile();
            }
            return enumerableCastConverter;
        }
    }
}