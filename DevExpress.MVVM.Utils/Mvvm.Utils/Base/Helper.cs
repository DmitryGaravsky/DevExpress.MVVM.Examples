namespace Mvvm.Utils {
    using System;
    using System.Collections.Generic;

    internal static class Ref {
        [System.Diagnostics.DebuggerStepThrough]
        public static void Dispose<T>(ref T refToDispose)
            where T : class, IDisposable {
            DisposeCore(refToDispose);
            refToDispose = null;
        }
        static void DisposeCore(IDisposable refToDispose) {
            if(refToDispose != null) refToDispose.Dispose();
        }
    }
    internal sealed class DisposableObjectsContainer : IDisposable {
        List<IDisposable> disposableObjects;
        public DisposableObjectsContainer() {
            disposableObjects = new List<IDisposable>(8);
        }
        void IDisposable.Dispose() {
            OnDisposing();
            GC.SuppressFinalize(this);
        }
        void OnDisposing() {
            foreach(IDisposable disposable in disposableObjects)
                disposable.Dispose();
            disposableObjects.Clear();
        }
        [System.Diagnostics.DebuggerStepThrough]
        public T Register<T>(T obj) where T : IDisposable {
            if(!object.Equals(obj, null) && !disposableObjects.Contains(obj))
                disposableObjects.Add(obj);
            return obj;
        }
    }
}