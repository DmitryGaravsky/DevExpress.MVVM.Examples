namespace Mvvm.Utils {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    class ProxyBase {
        ParameterInfo[] mInfoParameters;
        public ProxyBase(ParameterInfo[] mInfoParameters) {
            this.mInfoParameters = mInfoParameters;
        }
        protected Expression[] CreateCallParameters(ParameterExpression paramsExpression) {
            Expression[] callParameters = new Expression[mInfoParameters.Length];
            for(int i = 0; i < callParameters.Length; i++) {
                var p = mInfoParameters[i];
                if(!p.IsOptional) {
                    var ppi = Expression.ArrayIndex(paramsExpression, Expression.Constant(i));
                    callParameters[i] = Expression.Convert(ppi, mInfoParameters[i].ParameterType);
                }
                else callParameters[i] = Expression.Constant(p.DefaultValue);
            }
            return callParameters;
        }
        protected bool MatchCore(object[] parameters) {
            int length = mInfoParameters.Length;
            var parameterTypes = GetParameterTypes(length);
            while(true) {
                if(parameters.SequenceEqual(parameterTypes, ParameterTypesComparer))
                    return true;
                if(length > 0)
                    parameterTypes = GetParameterTypes(--length);
                else break;
            }
            return false;
        }
        IEnumerable<Type> GetParameterTypes(int length) {
            return mInfoParameters.Where((p, index) => !p.IsOptional || index < length).Select(p => p.ParameterType);
        }
        //
        internal static IEqualityComparer<object> ParameterTypesComparer {
            get { return DefalultParameterTypesComparer.Instance; }
        }
        sealed class DefalultParameterTypesComparer : IEqualityComparer<object> {
            internal static IEqualityComparer<object> Instance = new DefalultParameterTypesComparer();
            bool IEqualityComparer<object>.Equals(object parameter, object type) {
                return parameter != null ? ((Type)type).IsAssignableFrom(parameter.GetType()) : ((Type)type).IsClass;
            }
            int IEqualityComparer<object>.GetHashCode(object obj) {
                throw new NotImplementedException();
            }
        }
        //
        internal static T[] Reduce<T>(T[] parameters) {
            T[] result = new T[parameters.Length - 1];
            Array.Copy(parameters, result, result.Length);
            return result;
        }
    }
}