using DevExpress.Mvvm.DataAnnotations;
using DevExpress.Mvvm.Native;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Windows;
using System.Windows.Input;
#if !NETFX_CORE
using DevExpress.Mvvm.POCO;
#if !MONO
using System.Windows.Threading;
#endif
#else
using Windows.UI.Xaml;
#endif

namespace DevExpress.Mvvm {
#if !SILVERLIGHT && !NETFX_CORE
    public abstract class ViewModelBase : BindableBase, ISupportParentViewModel, ISupportServices, ISupportParameter, ICustomTypeDescriptor {
#else
    public abstract class ViewModelBase : BindableBase, ISupportParentViewModel, ISupportServices, ISupportParameter
#if !NETFX_CORE
        ,ICustomTypeProvider
#endif
    {
#endif
        static readonly object NotSetParameter = new object();
        private object parameter = NotSetParameter;
        static bool? isInDesignMode;

        public static bool IsInDesignMode {
            get {
                if(ViewModelDesignHelper.IsInDesignModeOverride.HasValue)
                    return ViewModelDesignHelper.IsInDesignModeOverride.Value;
                if(!isInDesignMode.HasValue) {
#if !MONO
#if SILVERLIGHT
                    isInDesignMode = DesignerProperties.IsInDesignTool;
#elif NETFX_CORE
                    isInDesignMode = Windows.ApplicationModel.DesignMode.DesignModeEnabled;
#else
                    DependencyPropertyDescriptor property = DependencyPropertyDescriptor.FromProperty(DesignerProperties.IsInDesignModeProperty, typeof(FrameworkElement));
                    isInDesignMode = (bool)property.Metadata.DefaultValue;
#endif
#else
                    isInDesignMode = false; // MONO TODO
#endif
                }
                return isInDesignMode.Value;
            }
        }


        object parentViewModel;
        object ISupportParentViewModel.ParentViewModel {
            get { return parentViewModel; }
            set {
                if(parentViewModel == value)
                    return;
                parentViewModel = value;
                OnParentViewModelChanged(parentViewModel);
            }
        }
        IServiceContainer serviceContainer;
        IServiceContainer ISupportServices.ServiceContainer { get { return ServiceContainer; } }
        protected IServiceContainer ServiceContainer { get { return serviceContainer ?? (serviceContainer = CreateServiceContainer()); } }
#if !NETFX_CORE
        bool IsPOCOViewModel { get { return this is IPOCOViewModel; } }
#else
        bool IsPOCOViewModel { get { return false; } }
#endif

        public ViewModelBase() {
#if !NETFX_CORE
            BuildCommandProperties();
#endif
            if(IsInDesignMode) {
#if SILVERLIGHT
                Deployment.Current.Dispatcher.BeginInvoke(new Action(OnInitializeInDesignMode));
#elif NETFX_CORE || MONO
                OnInitializeInDesignMode();
#else
                Dispatcher.CurrentDispatcher.BeginInvoke(new Action(OnInitializeInDesignMode));
#endif
            } else {
                OnInitializeInRuntime();
            }
        }
        protected object Parameter {
            get { return object.Equals(parameter, NotSetParameter) ? null : parameter; }
            set {
                if(parameter == value)
                    return;
                parameter = value;
                OnParameterChanged(value);
            }
        }
        object ISupportParameter.Parameter { get { return Parameter; } set { Parameter = value; } }

        protected virtual void OnParameterChanged(object parameter) {
        }
        protected virtual IServiceContainer CreateServiceContainer() {
            return new ServiceContainer(this);
        }
        protected virtual void OnParentViewModelChanged(object parentViewModel) {
        }
        protected virtual void OnInitializeInDesignMode() {
            OnParameterChanged(null);
        }
        protected virtual void OnInitializeInRuntime() {
        }
        protected virtual T GetService<T>() where T : class {
            return GetService<T>(ServiceSearchMode.PreferLocal);
        }
        protected virtual T GetService<T>(string key) where T : class {
            return GetService<T>(key, ServiceSearchMode.PreferLocal);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual T GetService<T>(ServiceSearchMode searchMode) where T : class {
            return ServiceContainer.GetService<T>(searchMode);
        }
        [EditorBrowsable(EditorBrowsableState.Never)]
        protected virtual T GetService<T>(string key, ServiceSearchMode searchMode) where T : class {
            return ServiceContainer.GetService<T>(key, searchMode);
        }
#if !NETFX_CORE
#region CommandAttributeSupport
        protected internal void RaiseCanExecuteChanged(Expression<Action> commandMethodExpression) {
            if(IsPOCOViewModel) {
                POCOViewModelExtensions.RaiseCanExecuteChangedCore(this, commandMethodExpression);
            } else {
                ((IDelegateCommand)commandProperties[ExpressionHelper.GetMethod(commandMethodExpression)]
#if !SILVERLIGHT
                .GetValue(this)
#else
                .GetValue(this, null)
#endif
                ).RaiseCanExecuteChanged();
            }
        }

        internal const string CommandNameSuffix = "Command";
        const string CanExecuteSuffix = "Can";
        const string Error_PropertyWithSameNameAlreadyExists = "Property with the same name already exists: {0}.";
        internal const string Error_MethodShouldBePublic = "Method should be public: {0}.";
        const string Error_MethodCannotHaveMoreThanOneParameter = "Method cannot have more than one parameter: {0}.";
        const string Error_MethodCannotHaveOutORRefParameters = "Method cannot have out or reference parameter: {0}.";
        const string Error_CanExecuteMethodHasIncorrectParameters = "Can execute method has incorrect parameters: {0}.";
        const string Error_MethodNotFound = "Method not found: {0}.";
        Dictionary<MethodInfo, CommandProperty> commandProperties;
        internal static string GetCommandName(MethodInfo commandMethod) {
            return commandMethod.Name + CommandNameSuffix;
        }
        internal static string GetCanExecuteMethodName(MethodInfo commandMethod) {
            return CanExecuteSuffix + commandMethod.Name;
        }
        internal static T GetAttribute<T>(MethodInfo method) {
            return MetadataHelper.GetAllAttributes(method).OfType<T>().FirstOrDefault();
        }

        static readonly Dictionary<Type, Dictionary<MethodInfo, CommandProperty>> propertiesCache = new Dictionary<Type, Dictionary<MethodInfo, CommandProperty>>();
        void BuildCommandProperties() {
            commandProperties = IsPOCOViewModel ? new Dictionary<MethodInfo, CommandProperty>() : GetCommandProperties(GetType());
        }
        static Dictionary<MethodInfo, CommandProperty> GetCommandProperties(Type type) {
            Dictionary<MethodInfo, CommandProperty> result = propertiesCache.GetOrAdd(type, () => CreateCommandProperties(type));
            return result;
        }
        static Dictionary<MethodInfo, CommandProperty> CreateCommandProperties(Type type) {
            Dictionary<MethodInfo, CommandProperty> commandProperties = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance)
                .Where(x => GetAttribute<CommandAttribute>(x) != null).ToArray()
                .Select(x => {
                    CommandAttribute attribute = GetAttribute<CommandAttribute>(x);
                    string name = attribute.Name ?? (x.Name.EndsWith(CommandNameSuffix) ? x.Name : GetCommandName(x));

                    MethodInfo canExecuteMethod = GetCanExecuteMethod(type, x, attribute, s => new CommandAttributeException(s));
                    var attributes = MetadataHelper.GetAllAttributes(x);
                    return new CommandProperty(x, canExecuteMethod, name, attribute.GetUseCommandManager(), attributes, type);
                })
                .ToDictionary(x => x.Method);
            foreach(var property in commandProperties.Values) {
                if(type.GetProperty(property.Name) != null || commandProperties.Values.Any(x => x.Name == property.Name && x != property))
                    throw new CommandAttributeException(string.Format(Error_PropertyWithSameNameAlreadyExists, property.Name));
                if(!property.Method.IsPublic)
                    throw new CommandAttributeException(string.Format(Error_MethodShouldBePublic, property.Method.Name));
                ValidateCommandMethodParameters(property.Method, x => new CommandAttributeException(x));
            }
            return commandProperties;
        }
        internal static bool ValidateCommandMethodParameters(MethodInfo method, Func<string, Exception> createException) {
            ParameterInfo[] parameters = method.GetParameters();
            if(CheckCommandMethodConditionValue(parameters.Length <= 1, method, Error_MethodCannotHaveMoreThanOneParameter, createException))
                return false;
            bool isValidSingleParameter = parameters.Length == 1 && (parameters[0].IsOut || parameters[0].ParameterType.IsByRef);
            if(CheckCommandMethodConditionValue(!isValidSingleParameter, method, Error_MethodCannotHaveOutORRefParameters, createException)) {
                return false;
            }
            return true;
        }
        static bool CheckCommandMethodConditionValue(bool value, MethodInfo method, string errorString, Func<string, Exception> createException) {
            CommandAttribute attribute = GetAttribute<CommandAttribute>(method);
            if(!value && attribute != null && attribute.IsCommand)
                throw createException(string.Format(errorString, method.Name));
            return !value;
        }
        internal static MethodInfo GetCanExecuteMethod(Type type, MethodInfo methodInfo, CommandAttribute commandAttribute, Func<string, Exception> createException) {
            if(commandAttribute != null && commandAttribute.CanExecuteMethod != null) {
                CheckCanExecuteMethod(methodInfo, createException, commandAttribute.CanExecuteMethod);
                return commandAttribute.CanExecuteMethod;
            }
            bool hasCustomCanExecuteMethod = commandAttribute != null && !string.IsNullOrEmpty(commandAttribute.CanExecuteMethodName);
            string canExecuteMethodName = hasCustomCanExecuteMethod ? commandAttribute.CanExecuteMethodName : GetCanExecuteMethodName(methodInfo);
            MethodInfo canExecuteMethod = type.GetMethod(canExecuteMethodName, BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
            if(hasCustomCanExecuteMethod && canExecuteMethod == null)
                throw createException(string.Format(Error_MethodNotFound, commandAttribute.CanExecuteMethodName));
            if(canExecuteMethod != null) {
                CheckCanExecuteMethod(methodInfo, createException, canExecuteMethod);
            }
            return canExecuteMethod;
        }

        static void CheckCanExecuteMethod(MethodInfo methodInfo, Func<string, Exception> createException, MethodInfo canExecuteMethod) {
            ParameterInfo[] parameters = methodInfo.GetParameters();
            ParameterInfo[] canExecuteParameters = canExecuteMethod.GetParameters();
            if(parameters.Length != canExecuteParameters.Length)
                throw createException(string.Format(Error_CanExecuteMethodHasIncorrectParameters, canExecuteMethod.Name));
            if(parameters.Length == 1 && (parameters[0].ParameterType != canExecuteParameters[0].ParameterType || parameters[0].IsOut != canExecuteParameters[0].IsOut))
                throw createException(string.Format(Error_CanExecuteMethodHasIncorrectParameters, canExecuteMethod.Name));
            if(!canExecuteMethod.IsPublic)
                throw createException(string.Format(Error_MethodShouldBePublic, canExecuteMethod.Name));
        }
        public static class CreateCommandHelper<T> {
            public static IDelegateCommand CreateCommand(object owner, MethodInfo method, MethodInfo canExecuteMethod, bool? useCommandManager, bool hasParameter) {
                return new DelegateCommand<T>(
                    x => method.Invoke(owner, GetInvokeParameters(x, hasParameter)),
                    x => canExecuteMethod != null ? (bool)canExecuteMethod.Invoke(owner, GetInvokeParameters(x, hasParameter)) : true
#if !SILVERLIGHT && !MONO
, useCommandManager
#endif
);
            }
            static object[] GetInvokeParameters(object parameter, bool hasParameter) {
                return hasParameter ? new[] { parameter } : new object[0];
            }

        }
        readonly Dictionary<MethodInfo, IDelegateCommand> commands = new Dictionary<MethodInfo, IDelegateCommand>();
        IDelegateCommand GetCommand(MethodInfo method, MethodInfo canExecuteMethod, bool? useCommandManager, bool hasParameter) {
            return commands.GetOrAdd(method, () => CreateCommand(method, canExecuteMethod, useCommandManager, hasParameter));
        }
        IDelegateCommand CreateCommand(MethodInfo method, MethodInfo canExecuteMethod, bool? useCommandManager, bool hasParameter) {
            Type commandType = hasParameter ? method.GetParameters()[0].ParameterType : typeof(object);
            return (IDelegateCommand)typeof(CreateCommandHelper<>).MakeGenericType(commandType).GetMethod("CreateCommand", BindingFlags.Static | BindingFlags.Public).Invoke(null, new object[] { this, method, canExecuteMethod, useCommandManager, hasParameter });
        }
        #region CommandProperty
        class CommandProperty :
#if !SILVERLIGHT  && !NETFX_CORE
            PropertyDescriptor
#else
 PropertyInfo
#endif
        {
            readonly MethodInfo method;
            readonly MethodInfo canExecuteMethod;

            readonly bool? useCommandManager;
            readonly bool hasParameter;
#if SILVERLIGHT
			readonly string name;
            readonly Attribute[] attributes;
            readonly Type reflectedType;
#endif
            public MethodInfo Method { get { return method; } }
            public MethodInfo CanExecuteMethod { get { return canExecuteMethod; } }
            public CommandProperty(MethodInfo method, MethodInfo canExecuteMethod, string name, bool? useCommandManager, Attribute[] attributes, Type reflectedType)
#if !SILVERLIGHT
                : base(name, attributes)
#endif
            {
                this.method = method;
                this.hasParameter = method.GetParameters().Length == 1;
                this.canExecuteMethod = canExecuteMethod;
                this.useCommandManager = useCommandManager;
#if SILVERLIGHT
				this.name = name;
                this.attributes = attributes;
                this.reflectedType = reflectedType;
#endif
            }
            IDelegateCommand GetCommand(object component) {
                return ((ViewModelBase)component).GetCommand(method, canExecuteMethod, useCommandManager, hasParameter);
            }
#if !SILVERLIGHT
            public override bool CanResetValue(object component) { return false; }
            public override Type ComponentType { get { return method.DeclaringType; } }
            public override object GetValue(object component) { return GetCommand(component); }
            public override bool IsReadOnly { get { return true; } }
            public override Type PropertyType { get { return typeof(ICommand); } }
            public override void ResetValue(object component) { throw new NotSupportedException(); }
            public override void SetValue(object component, object value) { throw new NotSupportedException(); }
            public override bool ShouldSerializeValue(object component) { return false; }
#else
            public override PropertyAttributes Attributes { get { return PropertyAttributes.None; } }
            public override bool CanRead { get { return true; } }
            public override bool CanWrite { get { return false; } }
            public override MethodInfo[] GetAccessors(bool nonPublic) { throw new NotSupportedException(); }
            public override MethodInfo GetGetMethod(bool nonPublic) { return null; }
            public override MethodInfo GetSetMethod(bool nonPublic) { return null; }
            public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) { throw new NotSupportedException(); }
            public override ParameterInfo[] GetIndexParameters() { return new ParameterInfo[0]; }
            public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture) { return GetCommand(obj); }
            public override Type PropertyType { get { return typeof(ICommand); } }
            public override Type DeclaringType { get { return method.DeclaringType; } }
            public override object[] GetCustomAttributes(Type attributeType, bool inherit) { return new object[0]; }
            public override object[] GetCustomAttributes(bool inherit) { return attributes; }
            public override bool IsDefined(Type attributeType, bool inherit) { return false; }
            public override string Name { get { return name; } }
            public override Type ReflectedType { get { return reflectedType.GetType(); } }
#endif
        }
        #endregion

#if SILVERLIGHT
        static readonly Dictionary<Type, CustomType> customTypes = new Dictionary<Type, CustomType>();
        CustomType customType;
        static CustomType GetCustomType(Type type, IEnumerable<CommandProperty> properties) {
            return customTypes.GetOrAdd(type, () => new CustomType(type, properties));
        }
        Type ICustomTypeProvider.GetCustomType() {
            return customType ?? (customType = GetCustomType(GetType(), commandProperties.Values));
        }
#else
#if !NETFX_CORE
        #region ICustomTypeDescriptor
        AttributeCollection ICustomTypeDescriptor.GetAttributes() {
            return TypeDescriptor.GetAttributes(this, true);
        }
        string ICustomTypeDescriptor.GetClassName() {
            return TypeDescriptor.GetClassName(this, true);
        }
        string ICustomTypeDescriptor.GetComponentName() {
            return TypeDescriptor.GetComponentName(this, true);
        }
        TypeConverter ICustomTypeDescriptor.GetConverter() {
            return TypeDescriptor.GetConverter(this, true);
        }
        EventDescriptor ICustomTypeDescriptor.GetDefaultEvent() {
            return TypeDescriptor.GetDefaultEvent(this, true);
        }
        PropertyDescriptor ICustomTypeDescriptor.GetDefaultProperty() {
            return TypeDescriptor.GetDefaultProperty(this, true);
        }
        object ICustomTypeDescriptor.GetEditor(Type editorBaseType) {
            return TypeDescriptor.GetEditor(this, editorBaseType, true);
        }
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents(Attribute[] attributes) {
            return TypeDescriptor.GetEvents(this, attributes, true);
        }
        EventDescriptorCollection ICustomTypeDescriptor.GetEvents() {
            return TypeDescriptor.GetEvents(this, true);
        }
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties(Attribute[] attributes) {
            return TypeDescriptor.GetProperties(this, attributes, true);
        }
        PropertyDescriptorCollection ICustomTypeDescriptor.GetProperties() {
            PropertyDescriptorCollection properties = new PropertyDescriptorCollection(TypeDescriptor.GetProperties(this, true).Cast<PropertyDescriptor>().Concat(commandProperties.Values).ToArray());
            return properties;
        }
        object ICustomTypeDescriptor.GetPropertyOwner(PropertyDescriptor pd) {
            return this;
        }
        #endregion
#endif
#endif
#endregion CommandAttributeSupport
#endif
    }
#if !SILVERLIGHT && !NETFX_CORE
    [Serializable]
#endif
    public class CommandAttributeException : Exception {
        public CommandAttributeException() { }
        public CommandAttributeException(string message)
            : base(message) {

        }
    }
}