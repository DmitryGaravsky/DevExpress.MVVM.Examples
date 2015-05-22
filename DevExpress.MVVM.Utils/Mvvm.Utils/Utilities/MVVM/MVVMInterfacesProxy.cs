namespace Mvvm.Utils {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using BF = System.Reflection.BindingFlags;

    static class MVVMInterfacesProxy {
        static IDictionary<Type, Func<object, object>> getParentViewModelCache = new Dictionary<Type, Func<object, object>>();
        internal static object GetParentViewModel(Type supportParentViewModelType, object viewModel) {
            return Member<Func<object, object>>(getParentViewModelCache, supportParentViewModelType, "ParentViewModel", MakeAccessor)(viewModel);
        }
        static IDictionary<Type, Action<object, object>> setParentViewModelCache = new Dictionary<Type, Action<object, object>>();
        internal static void SetParentViewModel(Type supportParentViewModelType, object viewModel, object parentViewModel) {
            Member<Action<object, object>>(setParentViewModelCache, supportParentViewModelType, "ParentViewModel", MakeMutator)(viewModel, parentViewModel);
        }
        static IDictionary<Type, Func<object, object>> getServiceContainerCache = new Dictionary<Type, Func<object, object>>();
        internal static object GetServiceContainer(Type supportServicesType, object viewModel) {
            return Member<Func<object, object>>(getServiceContainerCache, supportServicesType, "ServiceContainer", MakeAccessor)(viewModel);
        }
        static IDictionary<Type, Func<object>> getDefaultServiceContainerCache = new Dictionary<Type, Func<object>>();
        internal static object GetDefaultServiceContainer(Type defaultServiceContainerType) {
            return Member<Func<object>>(getDefaultServiceContainerCache, defaultServiceContainerType, "Default", MakeStaticAccessor)();
        }
        static IDictionary<Type, Action<object, object>> setParameterCache = new Dictionary<Type, Action<object, object>>();
        internal static void SetParameter(Type supportParameterType, object viewModel, object parameter) {
            Member<Action<object, object>>(setParameterCache, supportParameterType, "Parameter", MakeMutator)(viewModel, parameter);
        }
        static IDictionary<Type, Action<object, object>> setDocumentOwnerCache = new Dictionary<Type, Action<object, object>>();
        internal static void SetDocumentOwner(Type documentContentType, Type documentOwnerType, object viewModel, object documentOwner) {
            Member<Action<object, object>>(setDocumentOwnerCache, documentContentType, documentOwnerType, "DocumentOwner", MakeMutator)(viewModel, documentOwner);
        }
        static IDictionary<Type, Func<object, object>> getTitleCache = new Dictionary<Type, Func<object, object>>();
        internal static object GetTitle(Type documentContentType, object documentContent) {
            return Member<Func<object, object>>(getTitleCache, documentContentType, "Title", MakeAccessor)(documentContent);
        }
        static IDictionary<Type, Func<object, object>> getTagCache = new Dictionary<Type, Func<object, object>>();
        internal static object GetUICommandTag(Type uiCommandType, object command) {
            return Member<Func<object, object>>(getTagCache, uiCommandType, "Tag", MakeAccessor)(command);
        }
        static IDictionary<Type, Action<bool>> setDefaultUseCommandManagerCache = new Dictionary<Type, Action<bool>>();
        internal static void SetDefaultUseCommandManager(Type commandBaseType, bool value) {
            Member<Action<bool>>(setDefaultUseCommandManagerCache, commandBaseType, "DefaultUseCommandManager", MakeStaticMutator<bool>)(value);
        }
        static IDictionary<Type, Func<object, object>> getCancelCommandCache = new Dictionary<Type, Func<object, object>>();
        internal static object GetCancelCommand(Type asyncCommandType, object command) {
            return Member<Func<object, object>>(getCancelCommandCache, asyncCommandType, "CancelCommand", MakeAccessor)(command);
        }
        #region Attribute Properties
        internal static string GetAttributeName(Type attributeType, object attribute) {
            return (string)GetAttributeProperty(attributeType, attribute, "Name");
        }
        internal static bool GetAttributeIsCommand(Type attributeType, object attribute) {
            return (bool)GetAttributeProperty(attributeType, attribute, "IsCommand");
        }
        internal static bool GetAttributeIsBindable(Type attributeType, object attribute) {
            return (bool)GetAttributeProperty(attributeType, attribute, "IsBindable");
        }
        internal static string GetAttributeCommandParameter(Type attributeType, object attribute) {
            return (string)GetAttributeProperty(attributeType, attribute, "CommandParameter");
        }
        static IDictionary<string, Func<object, object>> getAttributePropertyCache = new Dictionary<string, Func<object, object>>();
        internal static object GetAttributeProperty(Type attributeType, object attribute, string propertyName) {
            return Member<Func<object, object>>(getAttributePropertyCache, attributeType, propertyName, MakeAccessorChecked)(attribute);
        }
        #endregion Attribute Properties
        static IDictionary<string, Func<object, object>> getAccessorsCache = new Dictionary<string, Func<object, object>>();
        internal static Func<object, object> GetAccessor(Type sourceType, string propertyName) {
            return Member<Func<object, object>>(getAccessorsCache, sourceType, propertyName, MakeAccessorChecked);
        }
        #region Member(Accessor & Mutator)
        static TMember Member<TMember>(IDictionary<string, TMember> cache, Type type, string memberName, Func<Type, string, TMember> makeFunc) {
            string key = type.Name + "." + memberName;
            TMember member;
            if(!cache.TryGetValue(key, out member)) {
                member = makeFunc(type, memberName);
                cache.Add(key, member);
            }
            return member;
        }
        static TMember Member<TMember>(IDictionary<Type, TMember> cache, Type type, string memberName, Func<Type, string, TMember> makeFunc) {
            TMember member;
            if(!cache.TryGetValue(type, out member)) {
                member = makeFunc(type, memberName);
                cache.Add(type, member);
            }
            return member;
        }
        static TMember Member<TMember>(IDictionary<Type, TMember> cache, Type type, Type memberType, string memberName, Func<Type, Type, string, TMember> makeFunc) {
            TMember member;
            if(!cache.TryGetValue(type, out member)) {
                member = makeFunc(type, memberType, memberName);
                cache.Add(type, member);
            }
            return member;
        }
        static Type GetMemberType(MemberInfo mInfo) {
            PropertyInfo pInfo = mInfo as PropertyInfo;
            if(pInfo != null)
                return pInfo.PropertyType;
            FieldInfo fInfo = mInfo as FieldInfo;
            if(fInfo != null)
                return fInfo.FieldType;
            throw new NotSupportedException("MemberInfo");
        }
        static MemberInfo GetMemberInfo(Type type, string memberName) {
            return (MemberInfo)type.GetProperty(memberName) ??
                (MemberInfo)type.GetField(memberName);
        }
        static MemberInfo GetStaticMemberInfo(Type type, string memberName) {
            return (MemberInfo)type.GetProperty(memberName, BF.Static | BF.Public) ??
                (MemberInfo)type.GetField(memberName, BF.Static | BF.Public);
        }
        static Func<object> MakeStaticAccessor(Type type, string memberName) {
            var mInfo = GetStaticMemberInfo(type, memberName);
            return Expression.Lambda<Func<object>>(Expression.MakeMemberAccess(null, mInfo)).Compile();
        }
        static Action<TValue> MakeStaticMutator<TValue>(Type type, string memberName) {
            var mInfo = GetStaticMemberInfo(type, memberName);
            var value = Expression.Parameter(typeof(TValue), "value");
            var assign = Expression.Assign(Expression.MakeMemberAccess(null, mInfo), value);
            return Expression.Lambda<Action<TValue>>(assign, value).Compile();
        }
        static Func<object, object> MakeAccessorChecked(Type type, string memberName) {
            var mInfo = GetMemberInfo(type, memberName);
            ParameterExpression parameter;
            var accessor = CreateAccessor(type, mInfo, out parameter);
            return Expression.Lambda<Func<object, object>>(CheckAccessor(mInfo, accessor), parameter).Compile();
        }
        static Func<object, object> MakeAccessor(Type type, string memberName) {
            ParameterExpression parameter;
            var accessor = CreateAccessor(type, GetMemberInfo(type, memberName), out parameter);
            return Expression.Lambda<Func<object, object>>(accessor, parameter).Compile();
        }
        static Expression CreateAccessor(Type type, MemberInfo mInfo, out ParameterExpression parameter) {
            parameter = Expression.Parameter(typeof(object), "instance");
            var instance = Expression.Convert(parameter, type);
            return Expression.MakeMemberAccess(instance, mInfo);
        }
        static Expression CheckAccessor(MemberInfo mInfo, Expression accessor) {
            Type mType = GetMemberType(mInfo);
            if(mType.IsValueType)
                accessor = Expression.Convert(accessor, typeof(object));
            return accessor;
        }
        static Action<object, object> MakeMutator(Type type, string memberName) {
            var mInfo = GetMemberInfo(type, memberName);
            var parameter = Expression.Parameter(typeof(object), "instance");
            var instance = Expression.Convert(parameter, type);
            var value = Expression.Parameter(typeof(object), "value");
            var assign = Expression.Assign(
                Expression.MakeMemberAccess(instance, mInfo), value);
            return Expression.Lambda<Action<object, object>>(assign, parameter, value).Compile();
        }
        static Action<object, object> MakeMutator(Type type, Type valueType, string memberName) {
            var mInfo = GetMemberInfo(type, memberName);
            var parameter = Expression.Parameter(typeof(object), "instance");
            var instance = Expression.Convert(parameter, type);
            var value = Expression.Parameter(typeof(object), "value");
            var assign = Expression.Assign(
                Expression.MakeMemberAccess(instance, mInfo), Expression.Convert(value, valueType));
            return Expression.Lambda<Action<object, object>>(assign, parameter, value).Compile();
        }
        #endregion Member(Accessor & Mutator)
        static IDictionary<Type, IEnumerable<IGetServiceProxy>> getServiceCache = new Dictionary<Type, IEnumerable<IGetServiceProxy>>();
        internal static TService GetService<TService>(Type serviceContainerType, object serviceContainer, params object[] parameters) where TService : class {
            IEnumerable<IGetServiceProxy> proxies;
            if(!getServiceCache.TryGetValue(serviceContainerType, out proxies)) {
                var getServiceMethods = serviceContainerType.GetMember("GetService", System.Reflection.MemberTypes.Method, BF.Public | BF.Instance);
                List<IGetServiceProxy> proxiesList = new List<IGetServiceProxy>(getServiceMethods.Length);
                for(int i = 0; i < getServiceMethods.Length; i++) {
                    MethodInfo mInfo = getServiceMethods[i] as MethodInfo;
                    proxiesList.Add(new GetServiceProxy(mInfo, serviceContainerType, mInfo.GetParameters()));
                }
                proxies = proxiesList;
                getServiceCache.Add(serviceContainerType, proxies);
            }
            return TryGetService<TService>(serviceContainer, parameters, proxies);
        }
        static TService TryGetService<TService>(object serviceContainer, object[] parameters, IEnumerable<IGetServiceProxy> proxies) where TService : class {
            while(true) {
                var proxy = proxies.FirstOrDefault(p => p.Match(parameters));
                if(proxy != null)
                    return proxy.GetService<TService>(serviceContainer, parameters);
                if(parameters.Length != 0)
                    parameters = ProxyBase.Reduce(parameters);
                else return null;
            }
        }
        static IDictionary<Type, IEnumerable<IRegisterServiceProxy>> registerServiceCache = new Dictionary<Type, IEnumerable<IRegisterServiceProxy>>();
        internal static void RegisterService(Type serviceContainerType, object serviceContainer, params object[] parameters) {
            IEnumerable<IRegisterServiceProxy> proxies;
            if(!registerServiceCache.TryGetValue(serviceContainerType, out proxies)) {
                var registerServiceMethods = serviceContainerType.GetMember("RegisterService", System.Reflection.MemberTypes.Method, BF.Public | BF.Instance);
                List<IRegisterServiceProxy> proxiesList = new List<IRegisterServiceProxy>(registerServiceMethods.Length);
                for(int i = 0; i < registerServiceMethods.Length; i++) {
                    MethodInfo mInfo = registerServiceMethods[i] as MethodInfo;
                    proxiesList.Add(new RegisterServiceProxy(mInfo, serviceContainerType, mInfo.GetParameters()));
                }
                proxies = proxiesList;
                registerServiceCache.Add(serviceContainerType, proxies);
            }
            TryRegisterService(serviceContainer, parameters, proxies);
        }
        static void TryRegisterService(object serviceContainer, object[] parameters, IEnumerable<IRegisterServiceProxy> proxies) {
            while(true) {
                var proxy = proxies.FirstOrDefault(p => p.Match(parameters));
                if(proxy != null) {
                    proxy.RegisterService(serviceContainer, parameters);
                    break;
                }
                if(parameters.Length != 0)
                    parameters = ProxyBase.Reduce(parameters);
                else break;
            }
        }
        #region  GetService proxy
        interface IGetServiceProxy {
            TService GetService<TService>(object serviceContainer, params object[] parameters) where TService : class;
            bool Match(params object[] parameters);
        }
        sealed class GetServiceProxy : ProxyBase, IGetServiceProxy {
            MethodInfo mInfo;
            Type serviceContainerType;
            IDictionary<Type, Func<object, object[], object>> getServiceCache = new Dictionary<Type, Func<object, object[], object>>();
            public GetServiceProxy(MethodInfo mInfo, Type serviceContainerType, ParameterInfo[] mInfoParameters)
                : base(mInfoParameters) {
                this.mInfo = mInfo;
                this.serviceContainerType = serviceContainerType;
            }
            TService IGetServiceProxy.GetService<TService>(object serviceContainer, params object[] parameters) {
                Func<object, object[], object> getService;
                if(!getServiceCache.TryGetValue(typeof(TService), out getService)) {
                    var pServiceContainer = Expression.Parameter(typeof(object), "serviceContainer");
                    var instance = Expression.Convert(pServiceContainer, serviceContainerType);
                    var paramsExpression = Expression.Parameter(typeof(object[]), "parameters");

                    var call = Expression.Call(instance, mInfo.MakeGenericMethod(typeof(TService)),
                        CreateCallParameters(paramsExpression));
                    getService = Expression.Lambda<Func<object, object[], object>>(
                                    call, pServiceContainer, paramsExpression
                                ).Compile();
                    getServiceCache.Add(typeof(TService), getService);
                }
                return getService(serviceContainer, parameters) as TService;
            }
            bool IGetServiceProxy.Match(params object[] parameters) {
                return MatchCore(parameters);
            }
        }
        #endregion  GetService proxy
        #region RegisterService proxy
        interface IRegisterServiceProxy {
            void RegisterService(object serviceContainer, params object[] parameters);
            bool Match(params object[] parameters);
        }
        sealed class RegisterServiceProxy : ProxyBase, IRegisterServiceProxy {
            Action<object, object[]> registerService;
            Expression<Action<object, object[]>> expression;
            public RegisterServiceProxy(MethodInfo mInfo, Type serviceContainerType, ParameterInfo[] mInfoParameters)
                : base(mInfoParameters) {
                var pServiceContainer = Expression.Parameter(typeof(object), "serviceContainer");
                var instance = Expression.Convert(pServiceContainer, serviceContainerType);
                var paramsExpression = Expression.Parameter(typeof(object[]), "parameters");
                var call = Expression.Call(instance, mInfo,
                    CreateCallParameters(paramsExpression));
                expression = Expression.Lambda<Action<object, object[]>>(
                                call, pServiceContainer, paramsExpression
                            );
            }
            void IRegisterServiceProxy.RegisterService(object serviceContainer, params object[] parameters) {
                if(registerService == null)
                    registerService = expression.Compile();
                registerService(serviceContainer, parameters);
            }
            bool IRegisterServiceProxy.Match(params object[] parameters) {
                return MatchCore(parameters);
            }
        }
        #endregion RegisterService proxy
        static IDictionary<Type, IOnCloseProxy> onCloseCache = new Dictionary<Type, IOnCloseProxy>();
        internal static void OnClose(Type documentContentType, object documentContent, System.ComponentModel.CancelEventArgs e) {
            IOnCloseProxy proxy;
            if(!onCloseCache.TryGetValue(documentContentType, out proxy)) {
                var onCloseMethod = documentContentType.GetMethod("OnClose", new Type[] { typeof(System.ComponentModel.CancelEventArgs) });
                if(onCloseMethod != null) {
                    proxy = new OnCloseProxy(onCloseMethod, documentContentType);
                    onCloseCache.Add(documentContentType, proxy);
                }
            }
            if(proxy != null) proxy.OnClose(documentContent, e);
        }
        static IDictionary<Type, IOnDestroyProxy> onDestroyCache = new Dictionary<Type, IOnDestroyProxy>();
        internal static void OnDestroy(Type documentContentType, object documentContent) {
            IOnDestroyProxy proxy;
            if(!onDestroyCache.TryGetValue(documentContentType, out proxy)) {
                var onDestroyMethod = documentContentType.GetMethod("OnDestroy", Type.EmptyTypes);
                if(onDestroyMethod != null) {
                    proxy = new OnDestroyProxy(onDestroyMethod, documentContentType);
                    onDestroyCache.Add(documentContentType, proxy);
                }
            }
            if(proxy != null) proxy.OnDestroy(documentContent);
        }
        #region  OnClose proxy
        interface IOnCloseProxy {
            void OnClose(object documentContent, System.ComponentModel.CancelEventArgs e);
        }
        sealed class OnCloseProxy : IOnCloseProxy {
            Action<object, System.ComponentModel.CancelEventArgs> onClose;
            public OnCloseProxy(MethodInfo mInfo, Type documentContentType) {
                var pDocumentContent = Expression.Parameter(typeof(object), "documentContent");
                var instance = Expression.Convert(pDocumentContent, documentContentType);
                var argsExpression = Expression.Parameter(typeof(System.ComponentModel.CancelEventArgs), "e");
                onClose = Expression.Lambda<Action<object, System.ComponentModel.CancelEventArgs>>(
                                Expression.Call(instance, mInfo, argsExpression), pDocumentContent, argsExpression).Compile();
            }
            void IOnCloseProxy.OnClose(object documentContent, System.ComponentModel.CancelEventArgs e) {
                onClose(documentContent, e);
            }
        }
        #endregion  OnClose proxy
        #region  OnDestroy proxy
        interface IOnDestroyProxy {
            void OnDestroy(object documentContent);
        }
        sealed class OnDestroyProxy : IOnDestroyProxy {
            Action<object> onDestroy;
            public OnDestroyProxy(MethodInfo mInfo, Type documentContentType) {
                var pDocumentContent = Expression.Parameter(typeof(object), "documentContent");
                var instance = Expression.Convert(pDocumentContent, documentContentType);
                onDestroy = Expression.Lambda<Action<object>>(
                                Expression.Call(instance, mInfo), pDocumentContent).Compile();
            }
            void IOnDestroyProxy.OnDestroy(object documentContent) {
                onDestroy(documentContent);
            }
        }
        #endregion  OnDestroy proxy
        static IDictionary<Type, IGetExtenalAndFluentAPIAttributesProxy> getExtenalAndFluentAPIAttributesCache = new Dictionary<Type, IGetExtenalAndFluentAPIAttributesProxy>();
        internal static IEnumerable<Attribute> GetExtenalAndFluentAPIAttributes(Type metadataHelperType, Type componentType, string memberName) {
            IGetExtenalAndFluentAPIAttributesProxy proxy;
            if(!getExtenalAndFluentAPIAttributesCache.TryGetValue(metadataHelperType, out proxy)) {
                var getMethod =
                    metadataHelperType.GetMethod("GetExtenalAndFluentAPIAttrbutes", new Type[] { typeof(Type), typeof(string) }) ??
                    metadataHelperType.GetMethod("GetExtenalAndFluentAPIAttributes", new Type[] { typeof(Type), typeof(string) });
                if(getMethod != null) {
                    proxy = new GetExtenalAndFluentAPIAttributesProxy(getMethod, metadataHelperType);
                    getExtenalAndFluentAPIAttributesCache.Add(metadataHelperType, proxy);
                }
            }
            return (proxy != null) ? proxy.Get(componentType, memberName) : new Attribute[0];
        }
        #region  GetExtenalAndFluentAPIAttributes proxy
        interface IGetExtenalAndFluentAPIAttributesProxy {
            IEnumerable<Attribute> Get(Type componentType, string memberName);
        }
        sealed class GetExtenalAndFluentAPIAttributesProxy : IGetExtenalAndFluentAPIAttributesProxy {
            Func<Type, string, IEnumerable<Attribute>> get;
            public GetExtenalAndFluentAPIAttributesProxy(MethodInfo mInfo, Type metadataHelperType) {
                var componentType = Expression.Parameter(typeof(Type), "componentType");
                var memberName = Expression.Parameter(typeof(string), "memberName");
                get = Expression.Lambda<Func<Type, string, IEnumerable<Attribute>>>(
                                Expression.Call(mInfo, componentType, memberName), componentType, memberName).Compile();
            }
            IEnumerable<Attribute> IGetExtenalAndFluentAPIAttributesProxy.Get(Type componentType, string memberName) {
                return get(componentType, memberName);
            }
        }
        #endregion  GetExtenalAndFluentAPIAttributes proxy
        internal static void Reset() {
            getParentViewModelCache.Clear();
            setParentViewModelCache.Clear();
            getServiceContainerCache.Clear();
            getDefaultServiceContainerCache.Clear();
            setParameterCache.Clear();
            setDocumentOwnerCache.Clear();
            getServiceCache.Clear();
            registerServiceCache.Clear();
            onCloseCache.Clear();
            onDestroyCache.Clear();
            getTagCache.Clear();
            setDefaultUseCommandManagerCache.Clear();
            getCancelCommandCache.Clear();
            getAttributePropertyCache.Clear();
            getExtenalAndFluentAPIAttributesCache.Clear();
            getAccessorsCache.Clear();
        }
    }
}