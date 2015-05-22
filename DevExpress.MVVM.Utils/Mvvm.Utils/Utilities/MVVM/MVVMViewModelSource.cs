namespace Mvvm.Utils {
    using System;

    sealed class MVVMViewModelSource : IMVVMViewModelSource {
        internal static IMVVMViewModelSource Instance = new MVVMViewModelSource();
        //
        object IMVVMViewModelSource.Create(Type viewModelType, params object[] parameters) {
            var viewModelSourceType = MVVMTypesResolver.Instance.GetViewModelSourceType();
            return MVVMViewModelSourceProxy.Create(viewModelSourceType, viewModelType, parameters);
        }
    }
}