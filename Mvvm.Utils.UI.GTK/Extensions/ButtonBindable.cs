using System;
using System.Linq.Expressions;
//using System.Windows.Forms;
using Mvvm.Utils.Commands;

namespace Mvvm.Utils.UI.GTK {
    public static class ButtonExtension {
        public static ISupportCommandBinding AsCommandBindable(this Button button) {
            return new BindableButton(button);
        }
        class BindableButton : ISupportCommandBinding {
            Button targetButton;
            public BindableButton(Button button) {
                this.targetButton = button;
            }
            public void BindCommand(object command, Func<object> queryCommandParameter = null) {
                CommandHelper.Bind(targetButton,
                    (button, execute) => button.Click += (s, e) => execute(),
                    (button, canExecute) => button.Enabled = canExecute(),
                    command, queryCommandParameter);
            }
            public void BindCommand(Expression<Action> commandSelector, object source, Func<object> queryCommandParameter = null) {
                CommandHelper.Bind(targetButton,
                    (button, execute) => button.Click += (s, e) => execute(),
                    (button, canExecute) => button.Enabled = canExecute(),
                    commandSelector, source, queryCommandParameter);
            }
            public void BindCommand<T>(Expression<Action<T>> commandSelector, object source, Func<T> queryCommandParameter = null) {
                CommandHelper.Bind(targetButton,
                    (button, execute) => button.Click += (s, e) => execute(),
                    (button, canExecute) => button.Enabled = canExecute(),
                    commandSelector, source, () => (queryCommandParameter != null) ? queryCommandParameter() : default(T));
            }
        }
    }
}