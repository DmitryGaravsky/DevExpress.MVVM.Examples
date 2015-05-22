namespace Mvvm.Utils.Bindings {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using System.Reflection;
    using Mvvm.Utils.Behaviors;

    static class BindingHelper {
        internal static IPropertyBinding SetBinding<TTarget, TValue>(TTarget dest, Expression<Func<TTarget, TValue>> selectorExpression,
            object source, Type sourceType, string path)
            where TTarget : class {
            return new SourceChangeTracker(path, source, sourceType,
                (src, srcType, property) => new Binding<TTarget, TValue>(src, srcType, property.Name, dest, selectorExpression));
        }
        internal static IPropertyBinding SetBinding<TSourceEventArgs, TTarget, TValue>(TTarget target, Expression<Func<TTarget, TValue>> selectorExpression,
            object source, string sourceEventName, Expression<Func<TSourceEventArgs, TValue>> eventArgsConverterExpression)
            where TSourceEventArgs : EventArgs
            where TTarget : class {
            return new Binding<TSourceEventArgs, TTarget, TValue>(source, sourceEventName, eventArgsConverterExpression, target, selectorExpression);
        }
        internal static IPropertyBinding SetBinding<TSourceEventArgs, TSource, TTarget, TValue>(TTarget target, Expression<Func<TTarget, TValue>> selectorExpression,
            TSource source, string sourceEventName, Expression<Func<TSourceEventArgs, TValue>> eventArgsConverterExpression, Action<TSource, TValue> updateSourceAction = null)
            where TSourceEventArgs : EventArgs
            where TTarget : class {
            return new Binding<TSourceEventArgs, TSource, TTarget, TValue>(source, sourceEventName, eventArgsConverterExpression, target, selectorExpression, updateSourceAction);
        }
        //
        class SourceChangeTracker : IPropertyBinding {
            IPropertyBinding bindingCore;
            DisposableObjectsContainer container;
            public SourceChangeTracker(string path, object source, Type sourceType, Func<object, Type, PropertyDescriptor, IPropertyBinding> createBinding) {
                this.container = new DisposableObjectsContainer();
                this.bindingCore = CreateBinding(path, source, sourceType, createBinding);
                TrackSourceChanges(bindingCore as ISourceChangeAware, path, source, sourceType);
            }
            IPropertyBinding CreateBinding(string path, object source, Type sourceType, Func<object, Type, PropertyDescriptor, IPropertyBinding> createBinding) {
                var property = NestedPropertiesHelper.GetProperty(path, ref source, ref sourceType);
                return createBinding(source, sourceType, property);
            }
            void IDisposable.Dispose() {
                Ref.Dispose(ref bindingCore);
                Ref.Dispose(ref container);
                GC.SuppressFinalize(this);
            }
            bool IPropertyBinding.IsOneWay {
                get { return bindingCore.IsOneWay; }
            }
            bool IPropertyBinding.IsTwoWay {
                get { return bindingCore.IsTwoWay; }
            }
            void IPropertyBinding.UpdateTarget() {
                bindingCore.UpdateTarget();
            }
            void IPropertyBinding.UpdateSource() {
                bindingCore.UpdateSource();
            }
            void TrackSourceChanges(ISourceChangeAware binding, string path, object source, Type sourceType) {
                if(binding == null) return;
                do {
                    string rootPath = NestedPropertiesHelper.GetRootPath(ref path);
                    if(string.IsNullOrEmpty(rootPath))
                        break;
                    PropertyDescriptorCollection properties = TypeDescriptor.GetProperties(sourceType);
                    PropertyDescriptor rootDescriptor = properties[rootPath];
                    if(rootDescriptor != null) {
                        var propertySelector = ExpressionHelper.Accessor<object>(sourceType, rootDescriptor.Name);
                        var sourceChangeAction = new SourceChangeAction(path, rootDescriptor.PropertyType, binding);
                        var triggerRef = SetTrigger(source, sourceType, propertySelector, sourceChangeAction);
                        if(triggerRef != null)
                            container.Register(triggerRef);
                        source = rootDescriptor.GetValue(source);
                        sourceType = rootDescriptor.PropertyType;
                    }
                }
                while(true);
            }
            IDisposable SetTrigger(object source, Type sourceType, Expression<Func<object, object>> propertySelector, SourceChangeAction triggerAction) {
                if(source is INotifyPropertyChanged)
                    return SetNPCTriggerCore(source, propertySelector, triggerAction);
                if(MemberInfoHelper.HasChangedEvent(sourceType, ExpressionHelper.GetPropertyName(propertySelector)))
                    return SetCLRTriggerCore(source, propertySelector, triggerAction);
                return null;
            }
            sealed class SourceChangeAction : ITriggerAction {
                string path;
                Type sourceType;
                ISourceChangeAware bindingEx;
                public SourceChangeAction(string path, Type sourceType, ISourceChangeAware binding) {
                    this.path = path;
                    this.sourceType = sourceType;
                    this.bindingEx = binding;
                }
                bool ITriggerAction.CanExecute(object value) {
                    return bindingEx != null;
                }
                void ITriggerAction.Execute(object value) {
                    executing++;
                    bindingEx.SourceChanged(NestedPropertiesHelper.GetSource(path, value, sourceType));
                    executing--;
                }
                int executing;
                bool ITriggerAction.IsExecuting {
                    get { return executing > 0; }
                }
            }
        }
        #region Bindings
        class Binding<TSourceEventArgs, TTarget, TValue> : IPropertyBinding
            where TTarget : class
            where TSourceEventArgs : EventArgs {
            ITriggerAction updateTarget;
            IDisposable sourceTriggerRef;
            string propertyName;
            public Binding(object source, string sourceEventName, Expression<Func<TSourceEventArgs, TValue>> eventArgsConverterExpression, TTarget target, Expression<Func<TTarget, TValue>> selectorExpression) {
                this.propertyName = ExpressionHelper.GetPropertyName(selectorExpression);
                if(eventArgsConverterExpression != null) {
                    this.updateTarget = CreateTriggerAction<TTarget, TValue>(target, selectorExpression);
                    this.sourceTriggerRef = SetPCETriggerCore<TSourceEventArgs, TValue>(source, sourceEventName, eventArgsConverterExpression, updateTarget);
                    UpdateTargetCore();
                }
            }
            bool IPropertyBinding.IsOneWay {
                get { return (sourceTriggerRef != null); }
            }
            bool IPropertyBinding.IsTwoWay {
                get { return false; }
            }
            void IPropertyBinding.UpdateTarget() {
                UpdateTargetCore();
            }
            void IPropertyBinding.UpdateSource() {
                /* do nothing */
            }
            void UpdateTargetCore() {
                if(sourceTriggerRef == null) return;
                ((TriggerRefCounterBase)sourceTriggerRef).ExecuteActions(propertyName, updateTarget);
            }
            void IDisposable.Dispose() {
                if(sourceTriggerRef != null)
                    ((TriggerRefCounterBase)sourceTriggerRef).Release(updateTarget);
                Ref.Dispose(ref sourceTriggerRef);
            }
        }
        class Binding<TSourceEventArgs, TSource, TTarget, TValue> : IPropertyBinding
            where TTarget : class
            where TSourceEventArgs : EventArgs {
            ITriggerAction updateTarget;
            ITriggerAction updateSource;
            IDisposable sourceTriggerRef;
            IDisposable targetTriggerRef;
            string propertyName;
            public Binding(TSource source, string sourceEventName, Expression<Func<TSourceEventArgs, TValue>> eventArgsConverterExpression, TTarget target,
                Expression<Func<TTarget, TValue>> selectorExpression, Action<TSource, TValue> updateSourceAction) {
                this.propertyName = ExpressionHelper.GetPropertyName(selectorExpression);
                if(eventArgsConverterExpression != null) {
                    this.updateTarget = CreateTriggerAction<TTarget, TValue>(target, selectorExpression);
                    this.sourceTriggerRef = SetPCETriggerCore<TSourceEventArgs, TValue>(source, sourceEventName, eventArgsConverterExpression, updateTarget);
                    if(updateSourceAction != null) {
                        Type targetType = target.GetType();
                        if(typeof(INotifyPropertyChanged).IsAssignableFrom(targetType) || MemberInfoHelper.HasChangedEvent(targetType, propertyName)) {
                            updateSource = CreateCallbackTriggerAction<TValue>((value) => updateSourceAction(source, value), updateTarget);
                            var targetSelector = ExpressionHelper.Accessor<TTarget>(typeof(TTarget), ExpressionHelper.GetMember(selectorExpression));
                            targetTriggerRef = SetTrigger<TTarget>(target, targetSelector, updateSource);
                        }
                    }
                    UpdateTargetCore();
                }
            }
            bool IPropertyBinding.IsOneWay {
                get { return (sourceTriggerRef != null) && (targetTriggerRef == null); }
            }
            bool IPropertyBinding.IsTwoWay {
                get { return (sourceTriggerRef != null) && (targetTriggerRef != null); }
            }
            void IPropertyBinding.UpdateTarget() {
                UpdateTargetCore();
            }
            void IPropertyBinding.UpdateSource() {
                UpdateSourceCore();
            }
            void UpdateTargetCore() {
                if(sourceTriggerRef == null) return;
                ((TriggerRefCounterBase)sourceTriggerRef).ExecuteActions(propertyName, updateTarget);
            }
            void UpdateSourceCore() {
                if(targetTriggerRef == null) return;
                ((TriggerRefCounterBase)targetTriggerRef).ExecuteActions(propertyName, updateSource);
            }
            void IDisposable.Dispose() {
                if(sourceTriggerRef != null)
                    ((TriggerRefCounterBase)sourceTriggerRef).Release(updateTarget);
                Ref.Dispose(ref sourceTriggerRef);
                if(targetTriggerRef != null)
                    ((TriggerRefCounterBase)targetTriggerRef).Release(updateSource, propertyName);
                Ref.Dispose(ref targetTriggerRef);
            }
        }
        class Binding<TTarget, TValue> : IPropertyBinding, ISourceChangeAware
            where TTarget : class {
            IDisposable sourceTriggerRef;
            IDisposable targetTriggerRef;
            ITriggerAction updateTarget;
            ITriggerAction updateSource;
            string sourcePropertyName;
            string targetPropertyName;
            public Binding(object source, Type sourceType, string propertyName, TTarget target, Expression<Func<TTarget, TValue>> selectorExpression) {
                this.sourcePropertyName = propertyName;
                this.updateTarget = CreateTriggerAction<TTarget, TValue>(target, selectorExpression);
                var sourceProperty = sourceType.GetProperty(propertyName);
                if(sourceProperty != null) {
                    var sourcePropertySelector = ExpressionHelper.Accessor<object>(sourceType, sourceProperty);
                    sourceTriggerRef = SetNPCTriggerCore(source, sourcePropertySelector, updateTarget);
                    Type targetType = target.GetType();
                    targetPropertyName = ExpressionHelper.GetPropertyName(selectorExpression);
                    if(typeof(INotifyPropertyChanged).IsAssignableFrom(targetType) || MemberInfoHelper.HasChangedEvent(targetType, targetPropertyName)) {
                        this.updateSource = new TriggerAction(source, sourceType, sourceProperty, updateTarget);
                        var targetSelector = ExpressionHelper.Accessor<TTarget>(typeof(TTarget), ExpressionHelper.GetMember(selectorExpression));
                        targetTriggerRef = SetTrigger<TTarget>(target, targetSelector, updateSource);
                    }
                }
                UpdateTargetCore();
            }
            void IPropertyBinding.UpdateTarget() {
                UpdateTargetCore();
            }
            void IPropertyBinding.UpdateSource() {
                UpdateSourceCore();
            }
            bool IPropertyBinding.IsOneWay {
                get { return (sourceTriggerRef != null) && (targetTriggerRef == null); }
            }
            bool IPropertyBinding.IsTwoWay {
                get { return (sourceTriggerRef != null) && (targetTriggerRef != null); }
            }
            void ISourceChangeAware.SourceChanged(object source) {
                if(updateSource != null)
                    ((TriggerAction)updateSource).ChangeTarget(source);
                var triggerRef = sourceTriggerRef as TriggerRefCounterBase;
                if(triggerRef != null) {
					if(triggerRef.Update (source)){
						triggerRef.ExecuteActions (sourcePropertyName, updateTarget);
					}
                }
            }
            void UpdateSourceCore() {
                if(targetTriggerRef == null) return;
                ((TriggerRefCounterBase)targetTriggerRef).ExecuteActions(targetPropertyName, updateSource);
            }
            void UpdateTargetCore() {
                if(sourceTriggerRef == null) return;
                ((TriggerRefCounterBase)sourceTriggerRef).ExecuteActions(sourcePropertyName, updateTarget);
            }
            void IDisposable.Dispose() {
                if(targetTriggerRef != null)
                    ((TriggerRefCounterBase)targetTriggerRef).Release(updateSource, targetPropertyName);
                if(sourceTriggerRef != null)
                    ((TriggerRefCounterBase)sourceTriggerRef).Release(updateTarget, sourcePropertyName);
                Ref.Dispose(ref sourceTriggerRef);
                Ref.Dispose(ref targetTriggerRef);
            }
        }
        #endregion Bindings
        internal static IDisposable SetNPCTrigger<TSource, TValue>(TSource source, Expression<Func<TSource, TValue>> selectorExpression,
            Action<TValue> action = null) {
            return SetNPCTriggerCore(source, ExpressionHelper.Box(selectorExpression), BindingHelper.CreateTriggerAction(action));
        }
        internal static IDisposable SetTrigger<TSource>(TSource source, Expression<Func<TSource, object>> selectorExpression,
            ITriggerAction action = null, bool forceCLRPropertyChanged = false) where TSource : class {
            if(source is INotifyPropertyChanged && !forceCLRPropertyChanged)
                return SetNPCTriggerCore(source, ExpressionHelper.Box(selectorExpression), action);
            else
                return SetCLRTriggerCore(source, ExpressionHelper.Box(selectorExpression), action);
        }
        #region SetTrigger Core
        static IDictionary<object, IDictionary<object, TriggerRefCounterBase>> triggers = new Dictionary<object, IDictionary<object, TriggerRefCounterBase>>();
        static IDisposable SetNPCTriggerCore(object source, Expression<Func<object, object>> sourcePropertySelector, ITriggerAction action = null) {
            return SetTriggerCore(source,
                () => typeof(INPCPropertyChangedTrigger),
                () => new INPCPropertyChangedTrigger(), sourcePropertySelector, action);
        }
        static IDisposable SetCLRTriggerCore(object source, Expression<Func<object, object>> sourcePropertySelector, ITriggerAction action = null) {
            return SetTriggerCore(source,
                () => CLRPropertyChangedTrigger.GetKey(sourcePropertySelector),
                () => new CLRPropertyChangedTrigger(sourcePropertySelector), action);
        }
#if DEBUGTEST
        internal
#endif
 		static IDisposable SetPCETriggerCore<TEventArgs, TValue>(object source, string eventName, Expression<Func<TEventArgs, TValue>> converterExpression, ITriggerAction action = null)
            where TEventArgs : EventArgs {
            return SetTriggerCore(source,
                () => PropertyChangedEventTrigger<TEventArgs, TValue>.GetKey(eventName),
                () => new PropertyChangedEventTrigger<TEventArgs, TValue>(eventName, converterExpression), action);
        }
        //
        static IDisposable SetTriggerCore(object source, Func<object> getKey, Func<EventTriggerBase> createTrigger, Expression<Func<object, object>> selector, ITriggerAction action = null) {
            return CreateTriggerRef(source, getKey, createTrigger, (list, trigger, key) => new NPCTriggerRefCounter(list, trigger, key)).AddRef(source, action, selector);
        }
        static IDisposable SetTriggerCore(object source, Func<object> getKey, Func<EventTriggerBase> createTrigger, ITriggerAction action = null) {
            return CreateTriggerRef(source, getKey, createTrigger, (list, trigger, key) => new PCETriggerRefCounter(list, trigger, key)).AddRef(source, action);
        }
        #endregion SetTrigger
        #region Trigger Reference-Counting
        static TriggerRefCounterBase CreateTriggerRef(object source, Func<object> getKey, Func<EventTriggerBase> createTrigger,
            Func<IDictionary<object, TriggerRefCounterBase>, EventTriggerBase, object, TriggerRefCounterBase> createCommand) {
            IDictionary<object, TriggerRefCounterBase> tList;
            if(!triggers.TryGetValue(source, out tList)) {
                tList = new Dictionary<object, TriggerRefCounterBase>();
                triggers.Add(source, tList);
            }
            TriggerRefCounterBase command;
            object key = getKey();
            if(!tList.TryGetValue(key, out command)) {
                EventTriggerBase trigger = createTrigger();
                command = createCommand(tList, trigger, key);
            }
            return command;
        }
        //
        abstract class TriggerRefCounterBase : IDisposable {
            EventTriggerBase trigger;
            IDictionary<object, TriggerRefCounterBase> list;
            int refCount;
            object key;
            protected TriggerRefCounterBase(IDictionary<object, TriggerRefCounterBase> list, EventTriggerBase trigger, object key) {
                if(trigger != null) {
                    this.list = list;
                    this.trigger = trigger;
                    this.key = key;
                }
            }
            void IDisposable.Dispose() {
                ReleaseCore();
            }
            public bool Update(object source) {
                if(trigger == null)
                    return false;
                object oldSource = trigger.Source;
                trigger.Source = source;
                return !object.Equals(oldSource, source);
            }
            protected void AddRefCore(object source) {
                if(0 == refCount++) {
                    list.Add(key, this);
                    if(trigger != null)
                        trigger.Source = source;
                }
            }
            void ReleaseCore() {
                if(--refCount == 0) {
                    list.Remove(key);
                    if(trigger != null)
                        trigger.Source = null;
                }
            }
            public IDisposable AddRef(object source, ITriggerAction action, params object[] parameters) {
                AddRefCore(source);
                if(action != null)
                    RegisterAction(action, parameters);
                return this;
            }
            public void Release(ITriggerAction action, params object[] parameters) {
                if(action != null)
                    UnregisterAction(action, parameters);
            }
            protected object GetTrigger() {
                return trigger;
            }
            public abstract void ExecuteActions(string propertyName, ITriggerAction action);
            protected abstract void RegisterAction(ITriggerAction action, params object[] parameters);
            protected abstract void UnregisterAction(ITriggerAction action, params object[] parameters);
        }
        sealed class NPCTriggerRefCounter : TriggerRefCounterBase {
            internal NPCTriggerRefCounter(IDictionary<object, TriggerRefCounterBase> list, EventTriggerBase trigger, object key)
                : base(list, trigger, key) {
            }
            protected override void UnregisterAction(ITriggerAction action, params object[] parameters) {
                INotifyPropertyChangedTrigger npcTrigger = GetTrigger() as INotifyPropertyChangedTrigger;
                if(npcTrigger != null)
                    npcTrigger.Unregister((string)parameters[0], action);
            }
            protected override void RegisterAction(ITriggerAction action, params object[] parameters) {
                INotifyPropertyChangedTrigger npcTrigger = GetTrigger() as INotifyPropertyChangedTrigger;
                if(npcTrigger != null)
                    npcTrigger.Register((Expression<Func<object, object>>)parameters[0], action);
            }
            public override void ExecuteActions(string propertyName, ITriggerAction action) {
                INotifyPropertyChangedTrigger pcTrigger = GetTrigger() as INotifyPropertyChangedTrigger;
                if(pcTrigger != null)
                    pcTrigger.Execute(propertyName, action);
            }
        }
        sealed class PCETriggerRefCounter : TriggerRefCounterBase {
            internal PCETriggerRefCounter(IDictionary<object, TriggerRefCounterBase> list, EventTriggerBase trigger, object key)
                : base(list, trigger, key) {
            }
            protected override void UnregisterAction(ITriggerAction action, params object[] parameters) {
                IPropertyChangedTrigger pcTrigger = GetTrigger() as IPropertyChangedTrigger;
                if(pcTrigger != null)
                    pcTrigger.Unregister(action);
            }
            protected override void RegisterAction(ITriggerAction action, params object[] parameters) {
                IPropertyChangedTrigger pcTrigger = GetTrigger() as IPropertyChangedTrigger;
                if(pcTrigger != null)
                    pcTrigger.Register(action);
            }
            public override void ExecuteActions(string propertyName, ITriggerAction action) {
                IPropertyChangedTrigger pcTrigger = GetTrigger() as IPropertyChangedTrigger;
                if(pcTrigger != null)
                    pcTrigger.Execute(action);
            }
        }
        #endregion Trigger Reference-Counting
        #region Trigger Actions
        internal static ITriggerAction CreateTriggerAction<TTarget, TValue>(TTarget target, Expression<Func<TTarget, TValue>> selectorExpression) {
            return new TriggerAction<TTarget, TValue>(target, selectorExpression);
        }
        internal static ITriggerAction CreateTriggerAction<TTarget, T>(TTarget target, Expression<Action<T>> commandSelector) {
            return CreateTriggerAction(Commands.CommandHelper.GetCommandProperty(commandSelector, target));
        }
        internal static ITriggerAction CreateTriggerAction<TTarget>(TTarget target, Expression<Action> commandSelector) {
            return CreateTriggerAction(Commands.CommandHelper.GetCommandProperty(commandSelector, target));
        }
        internal static ITriggerAction CreateTriggerAction(object command) {
            return new CommandTriggerAction(command);
        }
        internal static ITriggerAction CreateTriggerAction<TValue>(Action<TValue> action) {
            return new DelegateTriggerAction<TValue>(action);
        }
        internal static ITriggerAction CreateCallbackTriggerAction<TValue>(Action<TValue> action, ITriggerAction parentAction) {
            return new CallbackTriggerAction<TValue>(action, parentAction);
        }
        //
        sealed class TriggerAction<TTarget, TValue> : ITriggerAction {
            WeakReference tRef;
            Action<TTarget, TValue> setValue;
            public TriggerAction(TTarget target, Expression<Func<TTarget, TValue>> selectorExpression) {
                this.tRef = new WeakReference(target);
                var memberExpression = (MemberExpression)selectorExpression.Body;
                var setMethod = ((PropertyInfo)memberExpression.Member).GetSetMethod();
                if(setMethod != null) {
                    var t = Expression.Parameter(typeof(TTarget), "t");
                    var value = Expression.Parameter(typeof(TValue), "value");
                    setValue = Expression.Lambda<Action<TTarget, TValue>>(
                                    Expression.Call(t, setMethod, value), t, value
                                ).Compile();
                }
            }
            bool ITriggerAction.CanExecute(object value) {
                if(setValue == null)
                    return false;
                if(object.ReferenceEquals(value, null))
                    return typeof(TValue).IsClass;
                return value is TValue;
            }
            void ITriggerAction.Execute(object value) {
                TTarget target = (TTarget)tRef.Target;
                if(target != null) {
                    executing++;
                    setValue(target, (TValue)value);
                    executing--;
                }
            }
            int executing;
            bool ITriggerAction.IsExecuting {
                get { return executing > 0; }
            }
        }
        sealed class TriggerAction : ITriggerAction {
            WeakReference tRef;
            Type valueType;
			Action<object, object> setValue;
            ITriggerAction parentTriggerAction;
            public TriggerAction(object target, Type targetType, PropertyInfo member, ITriggerAction parentTriggerAction) {
                this.tRef = new WeakReference(target);
                this.parentTriggerAction = parentTriggerAction;
                this.valueType = member.PropertyType;
                var setMethod = member.GetSetMethod();
                if(setMethod != null) {
                    var t = Expression.Parameter(typeof(object), "t");
                    var value = Expression.Parameter(typeof(object), "value");
                    setValue = Expression.Lambda<Action<object, object>>(
                                    Expression.Call(
										Expression.Convert(t, targetType), setMethod, Expression.Convert(value, valueType)
								), t, value).Compile();
				}
            }
            bool ITriggerAction.CanExecute(object value) {
                if(parentTriggerAction != null && parentTriggerAction.IsExecuting)
                    return false;
                if(setValue == null)
                    return false;
                if(object.ReferenceEquals(value, null))
                    return valueType.IsClass;
                return valueType.IsAssignableFrom(value.GetType());
            }
            void ITriggerAction.Execute(object value) {
                object target = tRef.Target;
                if(target != null) {
                    executing++;
                    setValue(target, value);
                    executing--;
                }
            }
            int executing;
            bool ITriggerAction.IsExecuting {
                get { return executing > 0; }
            }
            internal void ChangeTarget(object target) {
                tRef.Target = target;
            }
        }
        sealed class DelegateTriggerAction<TValue> : ITriggerAction {
            Action<TValue> action;
            public DelegateTriggerAction(Action<TValue> action) {
                this.action = action;
            }
            bool ITriggerAction.CanExecute(object value) {
                if(action == null)
                    return false;
                if(object.ReferenceEquals(value, null))
                    return typeof(TValue).IsClass;
                return value is TValue;
            }
            void ITriggerAction.Execute(object value) {
                executing++;
                action((TValue)value);
                executing--;
            }
            int executing;
            bool ITriggerAction.IsExecuting {
                get { return executing > 0; }
            }
        }
        sealed class CallbackTriggerAction<TValue> : ITriggerAction {
            Action<TValue> action;
            ITriggerAction parentAction;
            public CallbackTriggerAction(Action<TValue> action, ITriggerAction parentAction) {
                this.parentAction = parentAction;
                this.action = action;
            }
            bool ITriggerAction.CanExecute(object value) {
                if(parentAction != null && parentAction.IsExecuting)
                    return false;
                if(action == null)
                    return false;
                if(object.ReferenceEquals(value, null))
                    return typeof(TValue).IsClass;
                return value is TValue;
            }
            void ITriggerAction.Execute(object value) {
                executing++;
                action((TValue)value);
                executing--;
            }
            int executing;
            bool ITriggerAction.IsExecuting {
                get { return executing > 0; }
            }
        }
        sealed class CommandTriggerAction : ITriggerAction {
            object command;
            Func<object, object, bool> canExecute;
            Action<object, object> execute;
            public CommandTriggerAction(object command) {
                this.command = command;
                if(command != null)
                    Initialize(command.GetType());
            }
            void Initialize(Type commandType) {
                var executeMethod = MemberInfoHelper.GetMethodInfo(commandType, "Execute", MemberInfoHelper.SingleObjectParameterTypes);
                if(executeMethod != null) {
                    var commandObject = Expression.Parameter(typeof(object), "command");
                    var parameter = Expression.Parameter(typeof(object), "parameter");
                    var command = Expression.TypeAs(commandObject, commandType);
                    this.execute = Expression.Lambda<Action<object, object>>(
                                Expression.Call(command, executeMethod, parameter),
                                commandObject, parameter
                            ).Compile();
                    var canExecuteMethod = MemberInfoHelper.GetMethodInfo(commandType, "CanExecute", MemberInfoHelper.SingleObjectParameterTypes);
                    if(canExecuteMethod != null) {
                        this.canExecute = Expression.Lambda<Func<object, object, bool>>(
                                Expression.Call(command, canExecuteMethod, parameter),
                                commandObject, parameter
                            ).Compile();
                    }
                }
            }
            bool ITriggerAction.CanExecute(object value) {
                return (execute != null) && ((canExecute == null) || canExecute(command, value));
            }
            void ITriggerAction.Execute(object value) {
                executing++;
                execute(command, value);
                executing--;
            }
            int executing;
            bool ITriggerAction.IsExecuting {
                get { return executing > 0; }
            }
        }
        #endregion Trigger Actions
    }
}