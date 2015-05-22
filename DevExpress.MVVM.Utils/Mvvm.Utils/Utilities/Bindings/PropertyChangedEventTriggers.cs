namespace Mvvm.Utils.Bindings {
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Linq.Expressions;
    using Mvvm.Utils.Behaviors;

    abstract class PropertyChangedEventTriggerBase<TEventArgs> : EventTriggerBase<TEventArgs>, IPropertyChangedTrigger
        where TEventArgs : EventArgs {
        PropertyChangedTriggerActions actionsCore;
        protected PropertyChangedEventTriggerBase(string eventName)
            : base(eventName) {
            actionsCore = new PropertyChangedTriggerActions();
        }
        protected sealed override void OnEvent() {
            ExecuteActionsCore();
        }
        void ExecuteActionsCore(ITriggerAction action = null) {
            if(!CanProcessEvent()) return;
            object value = GetValue();
            actionsCore.Execute(value, action);
        }
        protected abstract bool CanProcessEvent();
        protected abstract object GetValue();
        #region IPropertyChangedTrigger
        void IPropertyChangedTrigger.Register(ITriggerAction action) {
            actionsCore.Add(action);
        }
        void IPropertyChangedTrigger.Unregister(ITriggerAction action) {
            actionsCore.Remove(action);
        }
        void IPropertyChangedTrigger.Execute(ITriggerAction action) {
            ExecuteActionsCore(action);
        }
        #endregion
    }
    //
    sealed class PropertyChangedEventTrigger<TEventArgs, TValue> : PropertyChangedEventTriggerBase<TEventArgs>
        where TEventArgs : EventArgs {
        Func<TEventArgs, TValue> converter;
        Expression<Func<TEventArgs, TValue>> converterExpression;
        public PropertyChangedEventTrigger(string eventName, Expression<Func<TEventArgs, TValue>> converterExpression)
            : base(eventName) {
            this.converterExpression = converterExpression;
        }
        protected override bool CanProcessEvent() {
            return Args != null && (converterExpression != null);
        }
        protected sealed override object GetValue() {
            if(converter == null)
                converter = converterExpression.Compile();
            return converter(Args);
        }
        internal static object GetKey(string eventName) {
            return typeof(PropertyChangedEventTrigger<TEventArgs, TValue>).Name + "." + eventName;
        }
    }
    sealed class CLRPropertyChangedTrigger : PropertyChangedEventTriggerBase<EventArgs> {
        Expression<Func<object, object>> sourcePropertySelector;
        Func<object, object> accessor;
        public CLRPropertyChangedTrigger(Expression<Func<object, object>> sourcePropertySelector)
            : base(ExpressionHelper.GetPropertyName(sourcePropertySelector) + "Changed") {
            this.sourcePropertySelector = sourcePropertySelector;
        }
        protected override bool CanProcessEvent() {
            return (sourcePropertySelector != null);
        }
        protected sealed override object GetValue() {
            if(accessor == null)
                accessor = sourcePropertySelector.Compile();
            return accessor(Source);
        }
        internal static object GetKey(Expression<Func<object, object>> sourcePropertySelector) {
            return typeof(CLRPropertyChangedTrigger).Name + "." + ExpressionHelper.GetPropertyName(sourcePropertySelector) + "Changed";
        }
    }
    sealed class INPCPropertyChangedTrigger : EventTriggerBase<PropertyChangedEventArgs>, INotifyPropertyChangedTrigger {
        IDictionary<string, Func<object, object>> accessorsMap;
        IDictionary<string, PropertyChangedTriggerActions> actionsMap;
        public INPCPropertyChangedTrigger()
            : base("PropertyChanged") {
            actionsMap = new Dictionary<string, PropertyChangedTriggerActions>();
            accessorsMap = new Dictionary<string, Func<object, object>>();
        }
        protected sealed override void OnEvent() {
            ExecuteActionsCore(Args.PropertyName);
        }
        void ExecuteActionsCore(string propertyName, ITriggerAction action = null) {
            PropertyChangedTriggerActions actions;
            if(actionsMap.TryGetValue(propertyName, out actions)) {
                object value = accessorsMap[propertyName](Source);
                actions.Execute(value, action);
            }
        }
        #region INotifyPropertyChangedTrigger
        void INotifyPropertyChangedTrigger.Register(Expression<Func<object, object>> selectorExpression, ITriggerAction action) {
            string propertyName = ExpressionHelper.GetPropertyName(selectorExpression);
            Func<object, object> accessor;
            if(!accessorsMap.TryGetValue(propertyName, out accessor)) {
                accessor = selectorExpression.Compile();
                accessorsMap.Add(propertyName, accessor);
            }
            PropertyChangedTriggerActions actions;
            if(!actionsMap.TryGetValue(propertyName, out actions)) {
                actions = new PropertyChangedTriggerActions();
                actionsMap.Add(propertyName, actions);
            }
            actions.Add(action);
        }
        void INotifyPropertyChangedTrigger.Unregister(string propertyName, ITriggerAction action) {
            PropertyChangedTriggerActions actions;
            if(actionsMap.TryGetValue(propertyName, out actions))
                actions.Remove(action);
        }
        void INotifyPropertyChangedTrigger.Execute(string propertyName, ITriggerAction action) {
            ExecuteActionsCore(propertyName, action);
        }
        #endregion INotifyPropertyChangedTrigger
    }
}