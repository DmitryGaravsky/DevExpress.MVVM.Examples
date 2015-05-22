namespace Mvvm.Utils.Bindings {
    using System;
    using System.Linq.Expressions;

    public interface IPropertyChangedTrigger {
        void Unregister(ITriggerAction action);
        void Register(ITriggerAction action);
        void Execute(ITriggerAction action = null);
    }
    public interface INotifyPropertyChangedTrigger {
        void Unregister(string propertyName, ITriggerAction action);
        void Register(Expression<Func<object, object>> selectorExpression, ITriggerAction action);
        void Execute(string propertyName, ITriggerAction action = null);
    }
    public interface ITriggerAction {
        bool CanExecute(object value);
        void Execute(object value);
        bool IsExecuting { get; }
    }
    public interface ISourceChangeAware {
        void SourceChanged(object source);
    }
    //
    public interface IPropertyBinding : IDisposable {
        bool IsOneWay { get; }
        bool IsTwoWay { get; }
        void UpdateTarget();
        void UpdateSource();
    }
}