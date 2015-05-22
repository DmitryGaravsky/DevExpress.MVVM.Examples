namespace Mvvm.Utils {
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    static class MemberInfoHelper {
        const string CommandSuffix = "Command";
        const string ChangedSuffix = "Changed";
        //
        internal static MethodInfo GetMethod(LambdaExpression expression) {
            return ((MethodCallExpression)expression.Body).Method;
        }
        internal static MemberInfo GetMember(LambdaExpression expression) {
            return ((MemberExpression)expression.Body).Member;
        }
        internal static bool HasChangedEvent(Type sourceType, string propertyName, BindingFlags flags = BindingFlags.Public | BindingFlags.Instance) {
            return MemberInfoHelper.GetEventInfo(sourceType, propertyName + ChangedSuffix, flags) != null;
        }
        internal static EventInfo GetEventInfo(Type sourceType, string eventName, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) {
            return GetMemberInfo(sourceType, eventName, flags, (type) => type.GetEvent(eventName, flags));
        }
        internal static MethodInfo GetMethodInfo(Type sourceType, string methodName, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) {
            return GetMemberInfo(sourceType, methodName, flags, (type) => type.GetMethod(methodName, flags));
        }
        internal static MethodInfo GetMethodInfo(Type sourceType, string methodName, Type[] types, BindingFlags flags = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance) {
            return GetMemberInfo(sourceType, methodName, flags, (type) => type.GetMethod(methodName, flags, null, types, null));
        }
        static TMemberInfo GetMemberInfo<TMemberInfo>(Type sourceType, string methodName, BindingFlags flags, Func<Type, TMemberInfo> getMember)
            where TMemberInfo : MemberInfo {
            var interfaces = sourceType.GetInterfaces();
            Type[] typesList = new Type[interfaces.Length + 1];
            Array.Copy(interfaces, typesList, interfaces.Length);
            typesList[interfaces.Length] = sourceType;
            for(int i = 0; i < typesList.Length; i++) {
                var memberInfo = getMember(typesList[i]);
                if(memberInfo != null)
                    return memberInfo;
            }
            return null;
        }
        readonly internal static Type[] SingleObjectParameterTypes = new Type[] { typeof(object) };
        internal static PropertyInfo GetCommandProperty(object source, IMVVMTypesResolver typesResolver, MemberInfo mInfo) {
            return source.GetType().GetProperty(GetCommandPropertyName(typesResolver, mInfo));
        }
        static string GetCommandPropertyName(IMVVMTypesResolver typesResolver, MemberInfo mInfo) {
            Type commandAttributeType = typesResolver.GetCommandAttributeType();
            Attribute attribute = GetAttribute(typesResolver, mInfo, commandAttributeType);
            return GetCommandPropertyName(mInfo, commandAttributeType, attribute);
        }
        static string GetCommandPropertyName(MemberInfo mInfo, Type commandAttributeType, Attribute attribute) {
            if(attribute == null)
                return GetCommandPropertyName(mInfo.Name);
            return MVVMInterfacesProxy.GetAttributeName(commandAttributeType, attribute) ?? GetCommandPropertyName(mInfo.Name);
        }
        static Attribute GetAttribute(IMVVMTypesResolver typesResolver, MemberInfo mInfo, Type attributeType) {
            var attributes = mInfo.GetCustomAttributes(attributeType, false).OfType<Attribute>() ?? new Attribute[0];
            return attributes.Concat(GetExternalAndFluentAPIAttributes(typesResolver, mInfo)).FirstOrDefault();
        }
        static IEnumerable<Attribute> GetExternalAndFluentAPIAttributes(IMVVMTypesResolver typesResolver, MemberInfo mInfo) {
            Type metadataHelperType = typesResolver.GetMetadataHelperType();
            return MVVMInterfacesProxy.GetExtenalAndFluentAPIAttributes(metadataHelperType, mInfo.ReflectedType, mInfo.Name);
        }
        static string GetCommandPropertyName(string methodName) {
            return methodName.EndsWith(CommandSuffix) ? 
                methodName : methodName + CommandSuffix;
        }
    }
}