namespace Mvvm.Utils.Bindings {
    using System.Collections.Generic;

    public class PropertyChangedTriggerActions : List<ITriggerAction> {
        internal void Execute(object value, ITriggerAction action = null) {
            if(action != null)
                ExecuteCore(action, value);
            else {
                using(var e = ((IList<ITriggerAction>)this).GetEnumerator()) {
                    while(e.MoveNext())
                        ExecuteCore(e.Current, value);
                }
            }
        }
        static void ExecuteCore(ITriggerAction action, object value) {
            if(action.CanExecute(value))
                action.Execute(value);
        }
    }
}