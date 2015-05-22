namespace Mvvm.Utils {
    using System;
    using System.Collections.Generic;
    using System.Reflection;

    sealed class MVVMTypesResolver : IMVVMTypesResolver {
        readonly internal static IMVVMTypesResolver Instance = new MVVMTypesResolver();
        //
        static Type viewModelSourceType;
        Type IMVVMTypesResolver.GetViewModelSourceType() {
            return GetMvvmType(ref viewModelSourceType, "POCO.ViewModelSource");
        }
        static Type supportParentViewModelType;
        Type IMVVMTypesResolver.GetSupportParentViewModelType() {
            return GetMvvmType(ref supportParentViewModelType, "ISupportParentViewModel");
        }
        static Type supportParameterType;
        Type IMVVMTypesResolver.GetSupportParameterType() {
            return GetMvvmType(ref supportParameterType, "ISupportParameter");
        }
        static Type supportServicesType;
        Type IMVVMTypesResolver.GetSupportServicesType() {
            return GetMvvmType(ref supportServicesType, "ISupportServices");
        }
        static Type serviceContainerType;
        Type IMVVMTypesResolver.GetServiceContainerType() {
            return GetMvvmType(ref serviceContainerType, "IServiceContainer");
        }
        static Type messageBoxServiceType;
        Type IMVVMTypesResolver.GetIMessageBoxServiceType() {
            return GetMvvmType(ref messageBoxServiceType, "IMessageBoxService");
        }
        static Type documentManagerServiceType;
        Type IMVVMTypesResolver.GetIDocumentManagerServiceType() {
            return GetMvvmType(ref documentManagerServiceType, "IDocumentManagerService");
        }
        static Type dispatcherServiceType;
        Type IMVVMTypesResolver.GetIDispatcherServiceType() {
            return GetMvvmType(ref dispatcherServiceType, "IDispatcherService");
        }
        static Type dialogServiceType;
        Type IMVVMTypesResolver.GetIDialogServiceType() {
            return GetMvvmType(ref dialogServiceType, "IDialogService");
        }
        static Type documentContentType;
        Type IMVVMTypesResolver.GetIDocumentContentType() {
            return GetMvvmType(ref documentContentType, "IDocumentContent");
        }
        static Type documentOwnerType;
        Type IMVVMTypesResolver.GetIDocumentOwnerType() {
            return GetMvvmType(ref documentOwnerType, "IDocumentOwner");
        }
        static Type documentType;
        Type IMVVMTypesResolver.GetIDocumentType() {
            return GetMvvmType(ref documentType, "IDocument");
        }
        static Type documentInfoType;
        Type IMVVMTypesResolver.GetIDocumentInfoType() {
            return GetMvvmType(ref documentInfoType, "IDocumentInfo");
        }
        static Type messageButtonLocalizerType;
        Type IMVVMTypesResolver.GetIMessageButtonLocalizerType() {
            return GetMvvmType(ref messageButtonLocalizerType, "IMessageButtonLocalizer");
        }
        static Type uiCommandType;
        Type IMVVMTypesResolver.GetUICommandType() {
            return GetMvvmType(ref uiCommandType, "UICommand");
        }
        static Type commandBaseType;
        Type IMVVMTypesResolver.GetCommandBaseType() {
            return GetMvvmType(ref commandBaseType, "CommandBase");
        }
        static Type commandAttributeType;
        Type IMVVMTypesResolver.GetCommandAttributeType() {
            return GetMvvmType(ref commandAttributeType, "DataAnnotations.CommandAttribute");
        }
        static Type commandParameterAttributeType;
        Type IMVVMTypesResolver.GetCommandParameterAttributeType() {
            return GetMvvmType(ref commandParameterAttributeType, "DataAnnotations.CommandParameterAttribute");
        }
        static Type bindablePropertyAttributeType;
        Type IMVVMTypesResolver.GetBindablePropertyAttributeType() {
            return GetMvvmType(ref bindablePropertyAttributeType, "DataAnnotations.BindablePropertyAttribute");
        }
        static Type asyncCommandType;
        Type IMVVMTypesResolver.GetAsyncCommandType() {
            return GetMvvmType(ref asyncCommandType, "Native.IAsyncCommand");
        }
        static Type defaultServiceContainerType;
        Type IMVVMTypesResolver.GetDefaultServiceContainerType() {
            return GetMvvmType(ref defaultServiceContainerType, "ServiceContainer");
        }
        static Type metadataHelperType;
        Type IMVVMTypesResolver.GetMetadataHelperType() {
            return GetMvvmType(ref metadataHelperType, "Native.MetadataHelper");
        }
        static IDictionary<string, Type> attributeTypes = new Dictionary<string, Type>();
        Type IMVVMTypesResolver.GetAttributeType(string attributeTypeName) {
            Type attributeType;
            if(!attributeTypes.TryGetValue(attributeTypeName, out attributeType)) {
                GetMvvmType(ref attributeType, attributeTypeName);
                attributeTypes.Add(attributeTypeName, attributeType);
            }
            return attributeType;
        }
        static Assembly mvvmAssembly;
        static Assembly GetMVVMAssembly() {
            if(mvvmAssembly == null)
                EnsureMvvmAssemblyLoaded();
            return mvvmAssembly;
        }
        static void EnsureMvvmAssemblyLoaded() {
            mvvmAssembly =
                AssemblyHelper.GetLoadedAssembly(AssemblyInfo.SRAssemblyMvvm) ??
                Assembly.Load(AssemblyInfo.SRAssemblyMvvmFull);
        }
        static Type GetMvvmType(ref Type typeRef, string typeName) {
            if(typeRef == null)
                typeRef = GetMvvmType(typeName);
            return typeRef;
        }
        static Type GetMvvmType(string typeName) {
            var mvvmAssembly = GetMVVMAssembly();
            if(mvvmAssembly != null)
                return mvvmAssembly.GetType(typePrefix + typeName);
            return null;
        }
#if !DEBUGTEST
        const string typePrefix = "DevExpress.Mvvm.";
#else
        static string typePrefix = "DevExpress.Mvvm.";
        internal static void SetUpMVVMAssembly(string prefix = "Mvvm.Utils.Tests.") {
            mvvmAssembly = typeof(MVVMTypesResolver).Assembly;
            typePrefix = prefix;
        }
#endif
        internal static void Reset() {
            mvvmAssembly = null;
#if DEBUGTEST
            typePrefix = "DevExpress.Mvvm.";
#endif
            attributeTypes.Clear();
        }
    }
}