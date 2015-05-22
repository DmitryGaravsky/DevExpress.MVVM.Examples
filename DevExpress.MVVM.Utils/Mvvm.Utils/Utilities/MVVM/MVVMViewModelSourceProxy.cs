namespace Mvvm.Utils {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using BF = System.Reflection.BindingFlags;

    static class MVVMViewModelSourceProxy {
        static IDictionary<Type, IEnumerable<ICreateProxy>> proxiesCache = new Dictionary<Type, IEnumerable<ICreateProxy>>();
        internal static void Reset() {
            proxiesCache.Clear();
        }
        internal static object Create(Type viewModelSourceType, Type type, params object[] parameters) {
            IEnumerable<ICreateProxy> proxies;
            if(!proxiesCache.TryGetValue(type, out proxies)) {
                var createMethods = viewModelSourceType.GetMember("Create", System.Reflection.MemberTypes.Method, BF.Static | BF.Public);
                List<ICreateProxy> proxiesList = new List<ICreateProxy>(createMethods.Length);
                var constructors = type.GetConstructors(BF.Instance | BF.Public | BF.NonPublic);
                for(int i = 0; i < constructors.Length; i++) {
                    var ctorParameters = constructors[i].GetParameters();
                    if(ctorParameters.Length == 0 && !HasDefaultConstructorConstraint(createMethods[0] as MethodInfo))
                        proxiesList.Add(new CreateProxy(createMethods[0] as MethodInfo, type));
                    else
                        proxiesList.Add(new CreateProxyParametrized(createMethods[1] as MethodInfo, type, constructors[i], ctorParameters));
                }
                proxies = proxiesList;
                proxiesCache.Add(type, proxies);
            }
            return TryCreate(parameters, proxies);
        }
        static bool HasDefaultConstructorConstraint(MethodInfo mInfo) {
            Type[] typeArgs = mInfo.GetGenericArguments();
            if(typeArgs.Length == 1 && typeArgs[0].GenericParameterAttributes.HasFlag(GenericParameterAttributes.DefaultConstructorConstraint))
                return true;
            return false;
        }
        static object TryCreate(object[] parameters, IEnumerable<ICreateProxy> proxies) {
            while(true) {
                var proxy = proxies.FirstOrDefault(p => p.Match(parameters));
                if(proxy != null)
                    return proxy.Create(parameters);
                if(parameters.Length != 0)
                    parameters = Reduce(parameters);
                else return null;
            }
        }
        static object[] Reduce(object[] parameters) {
            object[] result = new object[parameters.Length - 1];
            Array.Copy(parameters, result, result.Length);
            return result;
        }
        #region CreateProxy
        interface ICreateProxy {
            object Create(params object[] parameters);
            bool Match(params object[] parameters);
        }
        sealed class CreateProxy : ICreateProxy {
            Func<object> create;
            public CreateProxy(MethodInfo mInfo, Type type) {
                var call = Expression.Call(mInfo.MakeGenericMethod(type));
                create = Expression.Lambda<Func<object>>(call).Compile();
            }
            object ICreateProxy.Create(params object[] parameters) {
                return create();
            }
            bool ICreateProxy.Match(params object[] parameters) {
                return parameters.Length == 0;
            }
        }
        sealed class CreateProxyParametrized : ICreateProxy {
            Func<object[], object> create;
            ParameterInfo[] ctorParameters;
            public CreateProxyParametrized(MethodInfo mInfo, Type type, ConstructorInfo ctorInfo, ParameterInfo[] ctorParameters) {
                this.ctorParameters = ctorParameters;
                var pp = Expression.Parameter(typeof(object[]), "parameters");
                Expression[] parameters = new Expression[ctorParameters.Length];
                for(int i = 0; i < ctorParameters.Length; i++) {
                    var ppi = Expression.ArrayIndex(pp, Expression.Constant(i));
                    parameters[i] = Expression.Convert(ppi, ctorParameters[i].ParameterType);
                }
                var ctorExpression = Expression.New(ctorInfo, parameters);
                var call = Expression.Call(mInfo.MakeGenericMethod(type),
                        Expression.Lambda(ctorExpression)
                    );
                create = Expression.Lambda<Func<object[], object>>(call, pp).Compile();
            }
            object ICreateProxy.Create(params object[] parameters) {
                object[] callParameters = new object[ctorParameters.Length];
                for(int i = 0; i < callParameters.Length; i++)
                    callParameters[i] = (i < parameters.Length) ? parameters[i] : ctorParameters[i].DefaultValue;
                return create(callParameters);
            }
            bool ICreateProxy.Match(params object[] parameters) {
                int length = ctorParameters.Length;
                var ctorParameterTypes = GetCtorParameterTypes(length);
                while(true) {
                    if(parameters.SequenceEqual(ctorParameterTypes, ProxyBase.ParameterTypesComparer))
                        return true;
                    if(length > 0)
                        ctorParameterTypes = GetCtorParameterTypes(--length);
                    else break;
                }
                return false;
            }
            IEnumerable<Type> GetCtorParameterTypes(int length) {
                return ctorParameters.Where((p, index) => !p.IsOptional || index < length).Select(p => p.ParameterType);
            }
        }
        #endregion CreateProxy
    }
}