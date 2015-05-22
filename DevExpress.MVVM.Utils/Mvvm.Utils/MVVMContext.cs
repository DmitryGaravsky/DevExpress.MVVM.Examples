namespace Mvvm.Utils {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using Mvvm.Utils.Behaviors;
    using Mvvm.Utils.Bindings;
    using Mvvm.Utils.Commands;

    public class MVVMContext : IDisposable {
        DisposableObjectsContainer disposableObjects = new DisposableObjectsContainer();
        public MVVMContext() : this(null) { }
        public MVVMContext(Type viewModelType, params  object[] viewModelConstructorParameters) {
            lock(syncObj)
                contexts.Add(this);
            this.viewModelTypeCore = viewModelType;
            if(viewModelConstructorParameters != null) {
                if(viewModelConstructorParameters.Length == 1)
                    this.viewModelConstructorParameterCore = viewModelConstructorParameters[0];
                else
                    this.viewModelConstructorParametersCore = viewModelConstructorParameters;
            }
        }
        public void Dispose() {
            Dispose(true);
            Ref.Dispose(ref disposableObjects);
        }
        bool isDisposing;
        protected virtual void Dispose(bool disposing) {
            if(!isDisposing) {
                isDisposing = true;
                OnDisposing();
                lock(syncObj)
                    contexts.Remove(this);
            }
        }
        protected virtual void OnDisposing() {
            SetParentViewModelCore(null);
            Ref.Dispose(ref disposableObjects);
            this.parentViewModelRefCore = null;
            this.parameterRefCore = null;
            this.viewModelTypeCore = null;
            this.viewModelConstructorParametersCore = null;
            this.viewModelConstructorParameterCore = null;
            this.viewModelCore = null;
        }
        #region Properties
        object containerCore;
        [DefaultValue(null), Category("Behavior"), RefreshProperties(RefreshProperties.All)]
        public object Container {
            [System.Diagnostics.DebuggerStepThrough]
            get { return containerCore; }
            set {
                if(value == containerCore) return;
                containerCore = value;
                ResetViewModel();
            }
        }
        Type viewModelTypeCore;
        [DefaultValue(null), Category("Data"), RefreshProperties(RefreshProperties.All)]
        public Type ViewModelType {
            [System.Diagnostics.DebuggerStepThrough]
            get { return viewModelTypeCore; }
            set {
                if(viewModelTypeCore == value) return;
                viewModelTypeCore = value;
                ResetViewModel();
            }
        }
        object viewModelConstructorParameterCore;
        [DefaultValue(null), Browsable(false), Category("Data")]
        public object ViewModelConstructorParameter {
            [System.Diagnostics.DebuggerStepThrough]
            get { return viewModelConstructorParameterCore; }
            set {
                if(viewModelConstructorParameterCore == value) return;
                viewModelConstructorParameterCore = value;
                ResetViewModel();
            }
        }
        object[] viewModelConstructorParametersCore;
        [DefaultValue(null), Browsable(false), Category("Data")]
        public object[] ViewModelConstructorParameters {
            [System.Diagnostics.DebuggerStepThrough]
            get { return viewModelConstructorParametersCore; }
            set {
                if(viewModelConstructorParametersCore == value) return;
                viewModelConstructorParametersCore = value;
                ResetViewModel();
            }
        }
        #endregion Properties
        object viewModelCore;
        protected internal object ViewModel {
            [System.Diagnostics.DebuggerStepThrough]
            get {
                if(viewModelCore == null)
                    EnsureViewModelCreated();
                return viewModelCore;
            }
        }
        protected virtual void OnViewModelCreated() { }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public bool IsViewModelCreated {
            [System.Diagnostics.DebuggerStepThrough]
            get { return viewModelCore != null; }
        }
        public void SetViewModel(Type viewModelType, object viewModel) {
            if(viewModelType == null || viewModel == null || !viewModelType.IsAssignableFrom(viewModel.GetType())) return;
            this.viewModelTypeCore = viewModelType;
            this.viewModelCore = viewModel;
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object ParentViewModel {
            get { return GetParentViewModel(); }
            set { SetParentViewModel(value); }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public object Parameter {
            get { return GetParameter(); }
            set { SetParameter(value); }
        }
        WeakReference parentViewModelRefCore;
        [System.Diagnostics.DebuggerStepThrough]
        protected object GetParentViewModel() {
            return (parentViewModelRefCore != null) ? parentViewModelRefCore.Target : null;
        }
        protected void SetParentViewModel(object parentViewModel) {
            this.parentViewModelRefCore = (parentViewModel != null) ? new WeakReference(parentViewModel) : null;
            SetParentViewModelCore(parentViewModel);
        }
        void SetParentViewModelCore(object parentViewModel) {
            if(viewModelCore != null) {
                var pocoInterfaces = GetMVVMInterfaces();
                if(pocoInterfaces != null)
                    pocoInterfaces.SetParentViewModel(viewModelCore, parentViewModel);
            }
        }
        void CheckParentViewModel() {
            if(parentViewModelRefCore == null) return;
            SetParentViewModelCore(GetParentViewModel());
        }
        WeakReference parameterRefCore;
        [System.Diagnostics.DebuggerStepThrough]
        protected object GetParameter() {
            return (parameterRefCore != null) ? parameterRefCore.Target : null;
        }
        protected void SetParameter(object parameter) {
            this.parameterRefCore = (parameter != null) ? new WeakReference(parameter) : null;
            SetParameterCore(parameter);
        }
        void SetParameterCore(object parameter) {
            if(viewModelCore != null) {
                var pocoInterfaces = GetMVVMInterfaces();
                if(pocoInterfaces != null)
                    pocoInterfaces.SetParameter(viewModelCore, parameter);
            }
        }
        void CheckParameter() {
            if(parameterRefCore == null) return;
            SetParameterCore(GetParameter());
        }
        void ResetViewModel() {
            this.viewModelCore = null;
        }
        [System.Diagnostics.DebuggerStepThrough]
        internal IMVVMViewModelSource GetViewModelSource() {
            return MVVMViewModelSource.Instance;
        }
        [System.Diagnostics.DebuggerStepThrough]
        internal IMVVMInterfaces GetMVVMInterfaces() {
            return GetDefaultMMVMInterfaces();
        }
        static IMVVMInterfaces GetDefaultMMVMInterfaces() {
            return MVVMInterfaces.Instance;
        }
        void EnsureViewModelCreated() {
            if(IsViewModelCreated) return;
            if(viewModelTypeCore != null) {
                viewModelCore = CreateViewModel();
                CheckParentViewModel();
                CheckParameter();
                OnViewModelCreated();
            }
        }
        object CreateViewModel() {
            IMVVMViewModelSource viewModelSource = GetViewModelSource();
            return viewModelSource.Create(ViewModelType, GetViewModelConstructorParameters());
        }
        object[] GetViewModelConstructorParameters() {
            if(ViewModelConstructorParameters != null)
                return ViewModelConstructorParameters;
            if(ViewModelConstructorParameter != null)
                return new object[] { ViewModelConstructorParameter };
            return new object[] { };
        }
        //
        #region API
        protected TDisposable Register<TDisposable>(TDisposable obj) where TDisposable : IDisposable {
            return (disposableObjects != null) ? disposableObjects.Register(obj) : obj;
        }
        #region GetViewModel
        [System.Diagnostics.DebuggerStepThrough]
        public TViewModel GetViewModel<TViewModel>() {
            return (TViewModel)ViewModel;
        }
        [System.Diagnostics.DebuggerStepThrough]
        public TViewModel GetParentViewModel<TViewModel>() {
            return (TViewModel)GetMVVMInterfaces().GetParentViewModel(ViewModel);
        }
        #endregion GetViewModel
        #region GetService
        public static TService GetService<TService>(object viewModel) where TService : class {
            return GetServiceFromViewModel<TService>(GetDefaultMMVMInterfaces(), viewModel);
        }
        public static TService GetService<TService>(object viewModel, string key) where TService : class {
            return GetServiceFromViewModel<TService>(GetDefaultMMVMInterfaces(), viewModel, key);
        }
        public static TService GetDefaultService<TService>() where TService : class {
            var mvvmInterfaces = GetDefaultMMVMInterfaces();
            return GetServiceCore<TService>(mvvmInterfaces, mvvmInterfaces.GetDefaultServiceContainer());
        }
        public static TService GetDefaultService<TService>(string key) where TService : class {
            var mvvmInterfaces = GetDefaultMMVMInterfaces();
            return GetServiceCore<TService>(mvvmInterfaces, mvvmInterfaces.GetDefaultServiceContainer(), key);
        }
        public TService GetService<TService>() where TService : class {
            return GetServiceFromViewModel<TService>(GetMVVMInterfaces(), ViewModel);
        }
        public TService GetService<TService>(string key) where TService : class {
            return GetServiceFromViewModel<TService>(GetMVVMInterfaces(), ViewModel, key);
        }
        static TService GetServiceFromViewModel<TService>(IMVVMInterfaces mvvmInterfaces, object viewModel) where TService : class {
            return (viewModel != null) ? GetServiceCore<TService>(mvvmInterfaces, mvvmInterfaces.GetServiceContainer(viewModel)) : null;
        }
        static TService GetServiceFromViewModel<TService>(IMVVMInterfaces mvvmInterfaces, object viewModel, string key) where TService : class {
            return (viewModel != null) ? GetServiceCore<TService>(mvvmInterfaces, mvvmInterfaces.GetServiceContainer(viewModel), key) : null;
        }
        static TService GetServiceCore<TService>(IMVVMInterfaces mvvmInterfaces, object serviceContainer, params object[] parameters) where TService : class {
            return (serviceContainer != null) ? mvvmInterfaces.GetService<TService>(serviceContainer, parameters) : null;
        }
        #endregion GetService
        #region RegisterService
        public void RegisterService(object service) {
            if(service == null) return;
            var mvvmInterfaces = GetMVVMInterfaces();
            object serviceContainer = mvvmInterfaces.GetServiceContainer(ViewModel);
            if(serviceContainer != null)
                mvvmInterfaces.RegisterService(serviceContainer, service);
        }
        public void RegisterService(string key, object service) {
            if(service == null) return;
            var mvvmInterfaces = GetMVVMInterfaces();
            object serviceContainer = mvvmInterfaces.GetServiceContainer(ViewModel);
            if(serviceContainer != null)
                mvvmInterfaces.RegisterService(serviceContainer, key, service);
        }
        public void RegisterDefaultService(object service) {
            RegisterDefaultServiceCore(service, GetMVVMInterfaces());
        }
        public void RegisterDefaultService(string key, object service) {
            RegisterDefaultServiceCore(key, service, GetMVVMInterfaces());
        }
        static void RegisterDefaultServiceCore(object service, IMVVMInterfaces mvvmInterfaces) {
            if(service == null) return;
            object serviceContainer = mvvmInterfaces.GetDefaultServiceContainer();
            mvvmInterfaces.RegisterService(serviceContainer, service);
        }
        static void RegisterDefaultServiceCore(string key, object service, IMVVMInterfaces pocoInterfaces) {
            if(service == null) return;
            object serviceContainer = pocoInterfaces.GetDefaultServiceContainer();
            pocoInterfaces.RegisterService(serviceContainer, key, service);
        }
        #endregion RegisterService
        #region SetBinding
        public IDisposable SetTrigger<TViewModel, TValue>(Expression<Func<TViewModel, TValue>> selectorExpression, Action<TValue> triggerAction)
            where TViewModel : class {
            return Register(BindingHelper.SetNPCTrigger<TViewModel, TValue>(GetViewModel<TViewModel>(), selectorExpression, triggerAction));
        }
        public IPropertyBinding SetBinding<TDestination, TValue>(TDestination dest, Expression<Func<TDestination, TValue>> selectorExpression, string propertyName)
            where TDestination : class {
            return Register(BindingHelper.SetBinding<TDestination, TValue>(dest, selectorExpression, ViewModel, ViewModelType, propertyName));
        }
        public IPropertyBinding SetParentBinding<TDestination, TValue>(TDestination dest, Expression<Func<TDestination, TValue>> selectorExpression, string propertyName)
            where TDestination : class {
            object parentViewModel = GetMVVMInterfaces().GetParentViewModel(ViewModel);
            return Register(BindingHelper.SetBinding<TDestination, TValue>(dest, selectorExpression, parentViewModel, parentViewModel.GetType(), propertyName));
        }
        public IPropertyBinding SetBinding<TDestination, TViewModel, TValue>(TDestination dest, Expression<Func<TDestination, TValue>> destSelectorExpression, Expression<Func<TViewModel, TValue>> sourceSelectorExpression)
            where TDestination : class {
            string propertyName = ExpressionHelper.GetPath(sourceSelectorExpression);
            return Register(BindingHelper.SetBinding<TDestination, TValue>(dest, destSelectorExpression, GetViewModel<TViewModel>(), typeof(TViewModel), propertyName));
        }
        public IPropertyBinding SetBinding<TSourceEventArgs, TViewModel, TValue>(Expression<Func<TViewModel, TValue>> selectorExpression,
            object source, string sourceEventName, Expression<Func<TSourceEventArgs, TValue>> sourceEventArgsConverterExpression)
            where TViewModel : class
            where TSourceEventArgs : EventArgs {
            return Register(BindingHelper.SetBinding<TSourceEventArgs, TViewModel, TValue>(GetViewModel<TViewModel>(), selectorExpression, source, sourceEventName, sourceEventArgsConverterExpression));
        }
        public IPropertyBinding SetBinding<TSourceEventArgs, TSource, TViewModel, TValue>(Expression<Func<TViewModel, TValue>> selectorExpression,
            TSource source, string sourceEventName, Expression<Func<TSourceEventArgs, TValue>> sourceEventArgsConverterExpression, Action<TSource, TValue> updateSourceAction)
            where TViewModel : class
            where TSourceEventArgs : EventArgs {
            return Register(BindingHelper.SetBinding<TSourceEventArgs, TSource, TViewModel, TValue>(GetViewModel<TViewModel>(), selectorExpression, source, sourceEventName, sourceEventArgsConverterExpression, updateSourceAction));
        }
        #endregion SetBinding
        #region AttachBehavior
        public IDisposable AttachBehavior<TBehavior>(object source)
            where TBehavior : BehaviorBase {
            return Register((source != null) ? BehaviorHelper.AttachCore<TBehavior>(source, ViewModel, GetViewModelSource(), GetMVVMInterfaces()) : null);
        }
        public IDisposable AttachBehavior<TBehavior>(object source, Action<TBehavior> behaviorSettings, params object[] parameters)
            where TBehavior : BehaviorBase {
            return Register((source != null) ? BehaviorHelper.AttachCore<TBehavior>(source, ViewModel, GetViewModelSource(), GetMVVMInterfaces(), behaviorSettings, parameters) : null);
        }
        public void DetachBehavior<TBehavior>(object source)
            where TBehavior : BehaviorBase {
            BehaviorHelper.Detach<TBehavior>(source);
        }
        public void DetachBehavior(object source) {
            BehaviorHelper.Detach(source);
        }
        #endregion AttachBehavior
        #region BindCommand
        public void BindCommand<TViewModel, T>(ISupportCommandBinding control, Expression<Action<TViewModel, T>> commandSelector, Expression<Func<TViewModel, T>> commandParameterSelector = null) {
            TViewModel viewModel = GetViewModel<TViewModel>();
            if(control != null)
                control.BindCommand<T>(ExpressionHelper.Reduce(commandSelector, viewModel), viewModel, ExpressionHelper.ReduceAndCompile(commandParameterSelector, viewModel));
        }
        public void BindCommand<TViewModel>(ISupportCommandBinding control, Expression<Action<TViewModel>> commandSelector, Expression<Func<TViewModel, object>> commandParameterSelector = null) {
            TViewModel viewModel = GetViewModel<TViewModel>();
            if(control != null)
                control.BindCommand(ExpressionHelper.Reduce(commandSelector, viewModel), viewModel, ExpressionHelper.ReduceAndCompile(commandParameterSelector, viewModel));
        }
        public void BindCancelCommand<TViewModel, T>(ISupportCommandBinding control, Expression<Action<TViewModel, T>> asyncCommandSelector) {
            TViewModel viewModel = GetViewModel<TViewModel>();
            if(control != null)
                control.BindCommand(GetCancelCommand(ExpressionHelper.Reduce(asyncCommandSelector, viewModel), viewModel));
        }
        public void BindCancelCommand<TViewModel>(ISupportCommandBinding control, Expression<Action<TViewModel>> asyncCommandSelector) {
            TViewModel viewModel = GetViewModel<TViewModel>();
            if(control != null)
                control.BindCommand(GetCancelCommand(asyncCommandSelector, viewModel));
        }
        static object GetCancelCommand(Expression<Action> asyncCommandSelector, object source) {
            Type commandType;
            return GetCancelCommandCore(CommandHelper.GetCommand(asyncCommandSelector, source, out commandType));
        }
        static object GetCancelCommand<T>(Expression<Action<T>> asyncCommandSelector, object source) {
            Type commandType;
            return GetCancelCommandCore(CommandHelper.GetCommand(asyncCommandSelector, source, out commandType));
        }
        internal static object GetCancelCommandCore(object command) {
            return MVVMInterfacesProxy.GetCancelCommand(MVVMTypesResolver.Instance.GetAsyncCommandType(), command);
        }
        #endregion BindCommand
        #endregion API
        #region Fluent API
        public MVVMContextFluentAPI<TViewModel> OfType<TViewModel>()
            where TViewModel : class {
            return MVVMContextFluentAPI<TViewModel>.OfType(this);
        }
        public MVVMContextFluentAPI<TViewModel, TEventArgs> WithEvent<TViewModel, TEventArgs>(object source, string eventName)
            where TViewModel : class
            where TEventArgs : EventArgs {
            return MVVMContextFluentAPI<TViewModel, TEventArgs>.WithEvent(this, source, eventName);
        }
        public MVVMContextFluentAPI<TViewModel, TSource, TSourceEventArgs> WithEvent<TViewModel, TSource, TSourceEventArgs>(TSource source, string eventName)
            where TViewModel : class
            where TSourceEventArgs : EventArgs {
            return MVVMContextFluentAPI<TViewModel, TSource, TSourceEventArgs>.WithEvent(this, source, eventName);
        }
        public MVVMContextConfirmationFluentAPI<TCancelEventArgs> WithEvent<TCancelEventArgs>(object source, string eventName)
            where TCancelEventArgs : CancelEventArgs {
            return MVVMContextConfirmationFluentAPI<TCancelEventArgs>.WithEvent(this, source, eventName);
        }
        #endregion Fluent API
        //
        readonly static List<MVVMContext> contexts = new List<MVVMContext>();
        readonly static object syncObj = new object();
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static MVVMContext FromContainer(object container) {
            MVVMContext context = null;
            while(container != null) {
                context = FromContainerCore(container);
                if(context != null)
                    return context;
                container = GetParentContainer(container);
            }
            return null;
        }
        static MVVMContext FromContainerCore(object container) {
            lock(syncObj) {
                foreach(MVVMContext context in contexts) {
                    if(context.Container == container)
                        return context;
                }
                return null;
            }
        }
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
        public static Func<object, object> GetParentContainerFunction;
        static object GetParentContainer(object container) {
            return (GetParentContainerFunction ?? new Func<object, object>((c) => null))(container);
        }
    }
    #region Fluent API
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class MVVMContextFluentAPI<TViewModel>
        where TViewModel : class {
        MVVMContext context;
        MVVMContextFluentAPI(MVVMContext context) {
            this.context = context;
        }
        internal static MVVMContextFluentAPI<TViewModel> OfType(MVVMContext context) {
            return new MVVMContextFluentAPI<TViewModel>(context);
        }
        //
        public TViewModel ViewModel {
            get { return context.GetViewModel<TViewModel>(); }
        }
        public IDisposable SetTrigger<TValue>(Expression<Func<TViewModel, TValue>> selectorExpression, Action<TValue> triggerAction) {
            return context.SetTrigger<TViewModel, TValue>(selectorExpression, triggerAction);
        }
        public IPropertyBinding SetBinding<TDestination, TValue>(TDestination dest, Expression<Func<TDestination, TValue>> destSelectorExpression, Expression<Func<TViewModel, TValue>> sourceSelectorExpression)
            where TDestination : class {
            return context.SetBinding<TDestination, TViewModel, TValue>(dest, destSelectorExpression, sourceSelectorExpression);
        }
        public IPropertyBinding SetBinding<TSourceEventArgs, TValue>(Expression<Func<TViewModel, TValue>> selectorExpression,
            object source, string sourceEventName, Expression<Func<TSourceEventArgs, TValue>> sourceEventArgsConverterExpression)
            where TSourceEventArgs : EventArgs {
            return context.SetBinding<TSourceEventArgs, TViewModel, TValue>(selectorExpression, source, sourceEventName, sourceEventArgsConverterExpression);
        }
        public IPropertyBinding SetBinding<TSourceEventArgs, TSource, TValue>(Expression<Func<TViewModel, TValue>> selectorExpression,
            TSource source, string sourceEventName, Expression<Func<TSourceEventArgs, TValue>> sourceEventArgsConverterExpression, Action<TSource, TValue> updateSourceAction)
            where TSourceEventArgs : EventArgs {
            return context.SetBinding<TSourceEventArgs, TSource, TViewModel, TValue>(selectorExpression, source, sourceEventName, sourceEventArgsConverterExpression, updateSourceAction);
        }
        //
        public void BindCommand<T>(ISupportCommandBinding control, Expression<Action<TViewModel, T>> commandSelector, Expression<Func<TViewModel, T>> commandParameterSelector = null) {
            context.BindCommand<TViewModel, T>(control, commandSelector, commandParameterSelector);
        }
        public void BindCommand(ISupportCommandBinding control, Expression<Action<TViewModel>> commandSelector, Expression<Func<TViewModel, object>> commandParameterSelector = null) {
            context.BindCommand<TViewModel>(control, commandSelector, commandParameterSelector);
        }
        public void BindCancelCommand<T>(ISupportCommandBinding control, Expression<Action<TViewModel, T>> asyncCommandSelector) {
            context.BindCancelCommand<TViewModel, T>(control, asyncCommandSelector);
        }
        public void BindCancelCommand(ISupportCommandBinding control, Expression<Action<TViewModel>> asyncCommandSelector) {
            context.BindCancelCommand<TViewModel>(control, asyncCommandSelector);
        }
        //
        public IDisposable EventToCommand<TEventArgs>(object source, string eventName, Expression<Action<TViewModel>> commandSelector)
            where TEventArgs : EventArgs {
            return context.AttachBehavior<EventToCommandBehavior<TViewModel, TEventArgs>>(source, null, eventName, commandSelector);
        }
        public IDisposable EventToCommand<TEventArgs>(object source, string eventName, Expression<Action<TViewModel>> commandSelector, Func<TEventArgs, object> eventArgsToCommandParameterConverter)
            where TEventArgs : EventArgs {
            return context.AttachBehavior<EventToCommandBehavior<TViewModel, TEventArgs>>(source, null, eventName, commandSelector, eventArgsToCommandParameterConverter);
        }
        public IDisposable EventToCommand<TEventArgs>(object source, string eventName, Expression<Action<TViewModel>> commandSelector, Predicate<TEventArgs> eventFilter)
            where TEventArgs : EventArgs {
            return context.AttachBehavior<EventToCommandBehavior<TViewModel, TEventArgs>>(source, null, eventName, commandSelector, eventFilter);
        }
        public IDisposable EventToCommand<TEventArgs, T>(object source, string eventName, Expression<Action<TViewModel>> commandSelector, Expression<Func<TViewModel, T>> commandParameterSelector)
            where TEventArgs : EventArgs {
            return context.AttachBehavior<EventToCommandBehavior<TViewModel, T, TEventArgs>>(source, null, eventName, commandSelector, commandParameterSelector);
        }
        public IDisposable EventToCommand<TEventArgs, T>(object source, string eventName, Expression<Action<TViewModel>> commandSelector, Expression<Func<TViewModel, T>> commandParameterSelector, Predicate<TEventArgs> eventFilter)
            where TEventArgs : EventArgs {
            return context.AttachBehavior<EventToCommandBehavior<TViewModel, T, TEventArgs>>(source, null, eventName, commandSelector, commandParameterSelector, eventFilter);
        }
        //
        public MVVMContextFluentAPI<TViewModel, TEventArgs> WithEvent<TEventArgs>(object source, string eventName)
            where TEventArgs : EventArgs {
            return MVVMContextFluentAPI<TViewModel, TEventArgs>.WithEvent(context, source, eventName);
        }
        public MVVMContextFluentAPI<TViewModel, TSource, TSourceEventArgs> WithEvent<TSource, TSourceEventArgs>(TSource source, string eventName)
            where TSourceEventArgs : EventArgs {
            return MVVMContextFluentAPI<TViewModel, TSource, TSourceEventArgs>.WithEvent(context, source, eventName);
        }
    }
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class MVVMContextFluentAPI<TViewModel, TEventArgs>
        where TViewModel : class
        where TEventArgs : EventArgs {
        MVVMContext context;
        string eventName;
        object source;
        MVVMContextFluentAPI(MVVMContext context, object source, string eventName) {
            this.context = context;
            this.eventName = eventName;
            this.source = source;
        }
        internal static MVVMContextFluentAPI<TViewModel, TEventArgs> WithEvent(MVVMContext context, object source, string eventName) {
            return new MVVMContextFluentAPI<TViewModel, TEventArgs>(context, source, eventName);
        }
        //
        public IPropertyBinding SetBinding<TValue>(Expression<Func<TViewModel, TValue>> selectorExpression, Expression<Func<TEventArgs, TValue>> sourceEventArgsConverterExpression) {
            return context.SetBinding<TEventArgs, TViewModel, TValue>(selectorExpression, source, eventName, sourceEventArgsConverterExpression);
        }
        //
        public IDisposable EventToCommand(Expression<Action<TViewModel>> commandSelector) {
            return context.AttachBehavior<EventToCommandBehavior<TViewModel, TEventArgs>>(source, null, eventName, commandSelector);
        }
        public IDisposable EventToCommand(Expression<Action<TViewModel>> commandSelector, Func<TEventArgs, object> eventArgsToCommandParameterConverter) {
            return context.AttachBehavior<EventToCommandBehavior<TViewModel, TEventArgs>>(source, null, eventName, commandSelector, eventArgsToCommandParameterConverter);
        }
        public IDisposable EventToCommand(Expression<Action<TViewModel>> commandSelector, Predicate<TEventArgs> eventFilter) {
            return context.AttachBehavior<EventToCommandBehavior<TViewModel, TEventArgs>>(source, null, eventName, commandSelector, eventFilter);
        }
        public IDisposable EventToCommand<T>(Expression<Action<TViewModel>> commandSelector, Expression<Func<TViewModel, T>> commandParameterSelector) {
            return context.AttachBehavior<EventToCommandBehavior<TViewModel, T, TEventArgs>>(source, null, eventName, commandSelector, commandParameterSelector);
        }
        public IDisposable EventToCommand<T>(Expression<Action<TViewModel>> commandSelector, Expression<Func<TViewModel, T>> commandParameterSelector, Predicate<TEventArgs> eventFilter) {
            return context.AttachBehavior<EventToCommandBehavior<TViewModel, T, TEventArgs>>(source, null, eventName, commandSelector, commandParameterSelector, eventFilter);
        }
    }
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class MVVMContextFluentAPI<TViewModel, TSource, TSourceEventArgs>
        where TViewModel : class
        where TSourceEventArgs : EventArgs {
        MVVMContext context;
        string eventName;
        TSource source;
        MVVMContextFluentAPI(MVVMContext context, TSource source, string eventName) {
            this.context = context;
            this.eventName = eventName;
            this.source = source;
        }
        internal static MVVMContextFluentAPI<TViewModel, TSource, TSourceEventArgs> WithEvent(MVVMContext context, TSource source, string eventName) {
            return new MVVMContextFluentAPI<TViewModel, TSource, TSourceEventArgs>(context, source, eventName);
        }
        //
        public IPropertyBinding SetBinding<TValue>(Expression<Func<TViewModel, TValue>> selectorExpression, Expression<Func<TSourceEventArgs, TValue>> sourceEventArgsConverterExpression, Action<TSource, TValue> updateSourceAction) {
            return context.SetBinding<TSourceEventArgs, TSource, TViewModel, TValue>(selectorExpression, source, eventName, sourceEventArgsConverterExpression, updateSourceAction);
        }
    }
    [Browsable(false), EditorBrowsable(EditorBrowsableState.Never)]
    public sealed class MVVMContextConfirmationFluentAPI<TEventArgs>
        where TEventArgs : CancelEventArgs {
        MVVMContext context;
        string eventName;
        object source;
        MVVMContextConfirmationFluentAPI(MVVMContext context, object source, string eventName) {
            this.context = context;
            this.eventName = eventName;
            this.source = source;
        }
        internal static MVVMContextConfirmationFluentAPI<TEventArgs> WithEvent(MVVMContext context, object source, string eventName) {
            return new MVVMContextConfirmationFluentAPI<TEventArgs>(context, source, eventName);
        }
        //
        public IDisposable Confirmation(Action<ConfirmationBehavior<TEventArgs>> behaviorSettings = null) {
            return context.AttachBehavior<ConfirmationBehavior<TEventArgs>>(source, behaviorSettings, eventName);
        }
    }
    #endregion Fluent API
}