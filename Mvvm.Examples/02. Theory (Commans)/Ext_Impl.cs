namespace Mvvm.ViewModels {
    using System.Windows.Input;
    using DevExpress.Mvvm;

    public class ViewModelWithCommands_ExtImpl {
        public ViewModelWithCommands_ExtImpl() {
            SomeCommand = new DelegateCommand(DoSomething, CanDoSomething);
        }
        // Property for SomeLegacyCommand
        public ICommand SomeCommand {
            get;
            private set;
        }
        public void DoSomething() { /* do something */ }
        public bool CanDoSomething() { return true; }
        
        //.. other commands
    }
}