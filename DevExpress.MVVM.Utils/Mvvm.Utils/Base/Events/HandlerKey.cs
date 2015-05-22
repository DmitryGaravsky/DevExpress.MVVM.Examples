namespace Mvvm.Utils {
    using System;

    class HandlerKey {
        readonly int hashCode;
        readonly Type targetType;
        readonly Type sourceType;
        public HandlerKey(Type targetType, Type sourceType) {
            this.targetType = targetType;
            this.sourceType = sourceType;
            this.hashCode = JonSkeet.CreateHash(targetType, sourceType);
        }
        public override bool Equals(object obj) {
            HandlerKey key = obj as HandlerKey;
            if(object.ReferenceEquals(null, key)) return false;
            return (targetType == key.targetType) && (sourceType == key.sourceType);
        }
        public override int GetHashCode() {
            return hashCode;
        }
    }
    //
    static class JonSkeet {
        const int Basis = unchecked((int)2166136261);
        const int Prime = 16777619;
        public static int CreateHash(params object[] args) {
            unchecked {
                int hash = Basis;
                for(int i = 0; i < args.Length; i++)
                    hash = (hash ^ args[i].GetHashCode()) * Prime;
                return hash;
            }
        }
    }
}