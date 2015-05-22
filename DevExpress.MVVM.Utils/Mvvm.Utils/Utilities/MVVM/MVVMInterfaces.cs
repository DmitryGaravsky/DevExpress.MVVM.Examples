namespace Mvvm.Utils {
    sealed class MVVMInterfaces : IMVVMInterfaces {
        internal static IMVVMInterfaces Instance = new MVVMInterfaces();
        //
        object IMVVMInterfaces.GetDefaultServiceContainer() {
            var defaultServiceContainerType = MVVMTypesResolver.Instance.GetDefaultServiceContainerType();
            return MVVMInterfacesProxy.GetDefaultServiceContainer(defaultServiceContainerType);
        }
        object IMVVMInterfaces.GetServiceContainer(object viewModel) {
            var supportServicesType = MVVMTypesResolver.Instance.GetSupportServicesType();
            if(!supportServicesType.IsAssignableFrom(viewModel.GetType())) return null;
            return MVVMInterfacesProxy.GetServiceContainer(supportServicesType, viewModel);
        }
        object IMVVMInterfaces.GetParentViewModel(object viewModel) {
            var supportParentViewModelType = MVVMTypesResolver.Instance.GetSupportParentViewModelType();
            if(!supportParentViewModelType.IsAssignableFrom(viewModel.GetType())) return null;
            return MVVMInterfacesProxy.GetParentViewModel(supportParentViewModelType, viewModel);
        }
        void IMVVMInterfaces.SetParentViewModel(object viewModel, object parentViewModel) {
            var supportParentViewModelType = MVVMTypesResolver.Instance.GetSupportParentViewModelType();
            if(supportParentViewModelType.IsAssignableFrom(viewModel.GetType()))
                MVVMInterfacesProxy.SetParentViewModel(supportParentViewModelType, viewModel, parentViewModel);
        }
        void IMVVMInterfaces.SetParameter(object viewModel, object parameter) {
            var supportParameterType = MVVMTypesResolver.Instance.GetSupportParameterType();
            if(supportParameterType.IsAssignableFrom(viewModel.GetType()))
                MVVMInterfacesProxy.SetParameter(supportParameterType, viewModel, parameter);
        }
        TService IMVVMInterfaces.GetService<TService>(object serviceContainer, params object[] parameters) {
            var serviceContainerType = MVVMTypesResolver.Instance.GetServiceContainerType();
            if(serviceContainer == null || !serviceContainerType.IsAssignableFrom(serviceContainer.GetType())) return null;
            return MVVMInterfacesProxy.GetService<TService>(serviceContainerType, serviceContainer, parameters);
        }
        void IMVVMInterfaces.RegisterService(object serviceContainer, params object[] parameters) {
            var serviceContainerType = MVVMTypesResolver.Instance.GetServiceContainerType();
            if(serviceContainer != null && serviceContainerType.IsAssignableFrom(serviceContainer.GetType()))
                MVVMInterfacesProxy.RegisterService(serviceContainerType, serviceContainer, parameters);
        }
        void IMVVMInterfaces.SetDocumentOwner(object viewModel, object documentOwner) {
            var documentContentType = MVVMTypesResolver.Instance.GetIDocumentContentType();
            if(documentContentType.IsAssignableFrom(viewModel.GetType())) {
                var documentOwnerType = MVVMTypesResolver.Instance.GetIDocumentOwnerType();
                if(documentOwner != null && documentOwnerType.IsAssignableFrom(documentOwner.GetType()))
                    MVVMInterfacesProxy.SetDocumentOwner(documentContentType, documentOwnerType, viewModel, documentOwner);
            }
        }
        void IMVVMInterfaces.OnClose(object viewModel, System.ComponentModel.CancelEventArgs e) {
            var documentContentType = MVVMTypesResolver.Instance.GetIDocumentContentType();
            if(documentContentType.IsAssignableFrom(viewModel.GetType()))
                MVVMInterfacesProxy.OnClose(documentContentType, viewModel, e);
        }
        void IMVVMInterfaces.OnDestroy(object viewModel) {
            var documentContentType = MVVMTypesResolver.Instance.GetIDocumentContentType();
            if(documentContentType.IsAssignableFrom(viewModel.GetType()))
                MVVMInterfacesProxy.OnDestroy(documentContentType, viewModel);
        }
        object IMVVMInterfaces.GetTitle(object viewModel) {
            object result = null;
            if(viewModel != null) {
                var documentContentType = MVVMTypesResolver.Instance.GetIDocumentContentType();
                if(documentContentType.IsAssignableFrom(viewModel.GetType()))
                    result = MVVMInterfacesProxy.GetTitle(documentContentType, viewModel);
            }
            return result;
        }
    }
}