namespace Mvvm.Examples {
    public class ViewModel {
        public ViewModel() {
            Title = "MVVM Practices";
        }
        public virtual string Title {
            get;
            set;
        }
        public void ResetTitle() {
            Title = "MVVM Practices";
        }
    }
}