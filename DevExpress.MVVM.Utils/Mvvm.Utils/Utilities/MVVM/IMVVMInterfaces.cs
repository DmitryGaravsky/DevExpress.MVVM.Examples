namespace Mvvm.Utils {
    using System.ComponentModel;

    internal interface IMVVMInterfaces {
        object GetDefaultServiceContainer();
        object GetServiceContainer(object viewModel);
        object GetParentViewModel(object viewModel);
        void SetParentViewModel(object viewModel, object parentViewModel);
        void SetParameter(object viewModel, object parameter);
        //
        TService GetService<TService>(object serviceContainer, params object[] parameters) where TService : class;
        void RegisterService(object serviceContainer, params object[] parameters);
        //
        void SetDocumentOwner(object viewModel, object documentOwner);
        void OnClose(object viewModel, CancelEventArgs e);
        void OnDestroy(object viewModel);
        object GetTitle(object viewModel);
    }
}