namespace Mvvm.Utils {
    using System;

    internal interface IMVVMViewModelSource {
        object Create(Type viewModelType, params object[] parameters);
    }
}