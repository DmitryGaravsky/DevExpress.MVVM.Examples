namespace Mvvm.Utils.Commands {
    using System;
    using System.Linq.Expressions;

    public interface ISupportCommandBinding {
        void BindCommand(object command, Func<object> queryCommandParameter = null);
        void BindCommand<T>(Expression<Action<T>> commandSelector, object source, Func<T> queryCommandParameter = null);
        void BindCommand(Expression<Action> commandSelector, object source, Func<object> queryCommandParameter = null);
    }
}