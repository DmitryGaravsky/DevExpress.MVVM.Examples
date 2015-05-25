namespace Mvvm.ViewModels {

    public class ViewModelWithCommands_BaseImpl {
        public ViewModelWithCommands_BaseImpl() {
            SomeCommand = new SomeLegacyCommand();
        }
        // Property for SomeLegacyCommand
        public SomeLegacyCommand SomeCommand {
            get;
            private set;
        }
        //.. other commands
    }

    public class SomeLegacyCommand {
        public virtual void Execute(object parameter) {
            // do something
        }
    }
}