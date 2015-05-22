namespace Mvvm.Utils.Behaviors {
    using System;
    using System.Linq.Expressions;

    public abstract class EventToCommandBehaviorBase<TViewModel, TEventArgs> :
        EventTriggerBase<TEventArgs>
        where TEventArgs : EventArgs
        where TViewModel : class {
        Expression<Action<TViewModel>> commandSelector;
        Predicate<TEventArgs> eventFilter;
        protected EventToCommandBehaviorBase(string eventName, Expression<Action<TViewModel>> commandSelector)
            : base(eventName) {
            if(commandSelector == null || !(commandSelector.Body is MethodCallExpression))
                throw new NotSupportedException("commandSelector");
            this.commandSelector = commandSelector;
        }
        protected EventToCommandBehaviorBase(string eventName, Expression<Action<TViewModel>> commandSelector, Predicate<TEventArgs> eventFilter)
            : base(eventName) {
            if(commandSelector == null || !(commandSelector.Body is MethodCallExpression))
                throw new NotSupportedException("commandSelector");
            this.commandSelector = commandSelector;
            this.eventFilter = eventFilter;
        }
        protected TViewModel ViewModel {
            get { return GetViewModel<TViewModel>(); }
        }
        Func<bool> canExecute;
        Action execute;
        protected sealed override void OnEvent() {
            if(!CanProcessEvent(Args)) return;
            if(execute == null) {
                TViewModel viewModel = GetViewModel<TViewModel>();
                Func<object> queryCommandParameter = GetQueryCommandParameter(viewModel);
                Type commandType;
                object command = Commands.CommandHelper.GetCommand(commandSelector, viewModel, out commandType);
                canExecute = Commands.CommandHelper.GetCanExecute(command, commandType, queryCommandParameter);
                execute = Commands.CommandHelper.GetExecute(command, commandType, queryCommandParameter);
            }
            if(canExecute())
                execute();
        }
        protected virtual bool CanProcessEvent(TEventArgs args) {
            return (eventFilter == null) || eventFilter(args);
        }
        protected abstract Func<object> GetQueryCommandParameter(TViewModel viewModel);
    }
    //
    public class EventToCommandBehavior<TViewModel, TEventArgs> :
        EventToCommandBehaviorBase<TViewModel, TEventArgs>
        where TEventArgs : EventArgs
        where TViewModel : class {
        Func<TEventArgs, object> convertArgs;
        public EventToCommandBehavior(string eventName, Expression<Action<TViewModel>> commandSelector)
            : base(eventName, commandSelector) {
            this.convertArgs = ((args) => args);
        }
        public EventToCommandBehavior(string eventName, Expression<Action<TViewModel>> commandSelector,
            Func<TEventArgs, object> eventArgsToCommandParameterConverter)
            : base(eventName, commandSelector) {
            this.convertArgs = eventArgsToCommandParameterConverter ?? ((args) => args);
        }
        public EventToCommandBehavior(string eventName, Expression<Action<TViewModel>> commandSelector,
            Predicate<TEventArgs> eventFilter)
            : base(eventName, commandSelector, eventFilter) {
        }
        public EventToCommandBehavior(string eventName, Expression<Action<TViewModel>> commandSelector,
            Func<TEventArgs, object> eventArgsToCommandParameterConverter,
            Predicate<TEventArgs> eventFilter)
            : base(eventName, commandSelector, eventFilter) {
            this.convertArgs = eventArgsToCommandParameterConverter ?? ((args) => args);
        }
        protected virtual object GetCommandParameter() {
            return convertArgs(Args);
        }
        protected sealed override Func<object> GetQueryCommandParameter(TViewModel viewModel) {
            return () => GetCommandParameter();
        }
    }
    public class EventToCommandBehavior<TViewModel, T, TEventArgs> :
        EventToCommandBehaviorBase<TViewModel, TEventArgs>
        where TEventArgs : EventArgs
        where TViewModel : class {
        Expression<Func<TViewModel, T>> commandParameterSelector;
        public EventToCommandBehavior(string eventName, Expression<Action<TViewModel>> commandSelector, Expression<Func<TViewModel, T>> commandParameterSelector)
            : base(eventName, commandSelector) {
            if(commandParameterSelector == null || !(commandParameterSelector.Body is MemberExpression))
                throw new NotSupportedException("commandParameterSelector");
            this.commandParameterSelector = commandParameterSelector;
        }
        public EventToCommandBehavior(string eventName, Expression<Action<TViewModel>> commandSelector, Expression<Func<TViewModel, T>> commandParameterSelector, Predicate<TEventArgs> eventFilter)
            : base(eventName, commandSelector, eventFilter) {
            if(commandParameterSelector == null || !(commandParameterSelector.Body is MemberExpression))
                throw new NotSupportedException("commandParameterSelector");
            this.commandParameterSelector = commandParameterSelector;
        }
        protected sealed override Func<object> GetQueryCommandParameter(TViewModel viewModel) {
            return Commands.CommandHelper.GetQueryCommandParameter<TViewModel, T>(commandParameterSelector, viewModel);
        }
    }
}