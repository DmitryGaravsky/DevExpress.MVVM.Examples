namespace Mvvm.Utils {
    using System;

    public interface IMVVMTypesResolver {
        Type GetViewModelSourceType();
        Type GetSupportParentViewModelType();
        Type GetSupportParameterType();
        Type GetSupportServicesType();
        Type GetServiceContainerType();
        Type GetIDispatcherServiceType();
        Type GetIDialogServiceType();
        Type GetIMessageBoxServiceType();
        Type GetIDocumentManagerServiceType();
        Type GetIDocumentContentType();
        Type GetIDocumentOwnerType();
        Type GetIDocumentType();
        Type GetIDocumentInfoType();
        Type GetIMessageButtonLocalizerType();
        Type GetUICommandType();
        Type GetAsyncCommandType();
        Type GetCommandBaseType();
        Type GetCommandAttributeType();
        Type GetCommandParameterAttributeType();
        Type GetBindablePropertyAttributeType();
        Type GetDefaultServiceContainerType();
        Type GetMetadataHelperType();
        //
        Type GetAttributeType(string attributeType);
    }
}