namespace Mvvm.ViewModels {
    using System.ComponentModel;

    public class ViewModel_BaseImpl : INotifyPropertyChanged {
        string titleCore;
        public string Title {
            get { return titleCore; }
            set {
                if(titleCore == value) return;
                this.titleCore = value;
                RaisePropertyChanged("Title");
            }
        }
        protected void RaisePropertyChanged(string propertyName) {
            var handler = PropertyChanged;
            if(handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }
        public event PropertyChangedEventHandler PropertyChanged;
    }
}