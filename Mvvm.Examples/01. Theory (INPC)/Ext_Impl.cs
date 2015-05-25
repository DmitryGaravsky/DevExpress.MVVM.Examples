namespace Mvvm.ViewModels {
    using DevExpress.Mvvm; // XXX MVVM Framework

    public class ViewModel_ExtImpl1 : ViewModelBase {
        string titleCore;
        public string Title {
            get { return titleCore; }
            set { SetProperty(ref titleCore, value, "Title"); }
        }
    }
}




namespace Mvvm.ViewModels {
    using DevExpress.Mvvm; // XXX MVVM Framework

    public class ViewModel_ExtImpl2 : ViewModelBase {
        public string Title {
            get { return GetProperty(() => Title); }
            set { SetProperty(() => Title, value); }
        }
    }
}