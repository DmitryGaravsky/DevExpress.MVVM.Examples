namespace Mvvm.Utils.Bindings {
    using System;
    using System.ComponentModel;

    static class NestedPropertiesHelper {
        internal static string GetRootPath(ref string path) {
            if(string.IsNullOrEmpty(path)) return null;
            int pathSeparatorPos = path.IndexOf('.');
            if(pathSeparatorPos > 0) {
                string rootPath = path.Substring(0, pathSeparatorPos);
                path = path.Substring(pathSeparatorPos + 1);
                return rootPath;
            }
            return null;
        }
        internal static PropertyDescriptor GetProperty(string path, ref object source, ref Type sourceType) {
            return GetProperty(path, TypeDescriptor.GetProperties(sourceType), ref source, ref sourceType);
        }
        internal static PropertyDescriptor GetProperty(string path, PropertyDescriptorCollection properties, ref object source, ref Type sourceType) {
            if(string.IsNullOrEmpty(path)) return null;
            string rootPath = GetRootPath(ref path);
            if(!string.IsNullOrEmpty(rootPath)) {
                PropertyDescriptor rootDescriptor = properties[rootPath];
                if(rootDescriptor != null) {
                    source = rootDescriptor.GetValue(source);
                    sourceType = rootDescriptor.PropertyType;
                    return GetProperty(path, rootDescriptor.GetChildProperties(), ref source, ref sourceType);
                }
                return GetCollectionItemProperty(path, properties, rootPath, ref source, ref sourceType);
            }
            return properties[path];
        }
        internal static object GetSource(string path, object source, Type sourceType) {
            string rootPath = GetRootPath(ref path);
            if(!string.IsNullOrEmpty(rootPath)) {
                var properties = TypeDescriptor.GetProperties(sourceType);
                PropertyDescriptor rootDescriptor = properties[rootPath];
                if(rootDescriptor != null)
                    return GetSource(path, rootDescriptor.GetValue(source), rootDescriptor.PropertyType);
            }
            return source;
        }
        //
        static PropertyDescriptor GetCollectionDescriptor(string path, PropertyDescriptorCollection properties, out int index) {
            index = -1;
            int openBracketPos = path.IndexOf('[');
            if(openBracketPos > 0) {
                string collectionPath = path.Substring(0, openBracketPos);
                int closeBracketPos = path.IndexOf(']');
                int indexPos = openBracketPos + 1;
                string indexStr = path.Substring(indexPos, closeBracketPos - indexPos);
                int.TryParse(indexStr, out index);
                return properties[collectionPath];
            }
            return null;
        }
        static PropertyDescriptor GetCollectionItemProperty(string path, PropertyDescriptorCollection properties, string rootPath, ref object source, ref Type sourceType) {
            int index;
            PropertyDescriptor collectionDescriptor = GetCollectionDescriptor(rootPath, properties, out index);
            if(collectionDescriptor != null) {
                Type collectionItemType = GetCollectionItemType(collectionDescriptor.PropertyType);
                if(collectionItemType != null) {
                    PropertyDescriptorCollection itemProperties = TypeDescriptor.GetProperties((Type)collectionItemType);
                    return GetProperty(path, itemProperties, ref source, ref sourceType);
                }
            }
            return null;
        }
        static Type GetCollectionItemType(Type collectionType) {
            if(collectionType == null) return null;
            if(collectionType.IsGenericType) {
                Type[] args = collectionType.GetGenericArguments();
                if(args.Length == 1)
                    return args[0];
                return null;
            }
            return GetCollectionItemType(collectionType.BaseType);
        }
    }
}