namespace Mvvm.Utils.Behaviors {
    using System;

    public abstract class BehaviorBase {
        object sourceCore;
        public object Source {
            get { return sourceCore; }
            internal set {
                if(sourceCore == value) return;
                if(sourceCore != null)
                    OnDetach();
                this.sourceCore = value;
                this.sourceTypeCore = null;
                if(sourceCore != null) {
                    sourceTypeCore = Source.GetType();
                    OnAttach();
                }
            }
        }
        Type sourceTypeCore;
        public Type SourceType {
            get { return sourceTypeCore; }
        }
        public bool IsAttached {
            get { return sourceCore != null; }
        }
        protected abstract void OnAttach();
        protected abstract void OnDetach();
        #region Services
        internal IMVVMInterfaces MVVMInterfaces;
        protected TService GetService<TService>()
            where TService : class {
            object serviceContainer = (MVVMInterfaces != null) ? MVVMInterfaces.GetServiceContainer(this) : null;
            return (serviceContainer != null) ? MVVMInterfaces.GetService<TService>(serviceContainer) : null;
        }
        protected TService GetService<TService>(string key)
            where TService : class {
            object serviceContainer = (MVVMInterfaces != null) ? MVVMInterfaces.GetServiceContainer(this) : null;
            return (serviceContainer != null) ? MVVMInterfaces.GetService<TService>(serviceContainer, key) : null;
        }
        #endregion Services
        #region ViewModel
        protected TViewModel GetViewModel<TViewModel>()
            where TViewModel : class {
            return (TViewModel)GetViewModel();
        }
        protected object GetViewModel() {
            return (MVVMInterfaces != null) ? MVVMInterfaces.GetParentViewModel(this) : null;
        }
        #endregion ViewModel
    }
}
