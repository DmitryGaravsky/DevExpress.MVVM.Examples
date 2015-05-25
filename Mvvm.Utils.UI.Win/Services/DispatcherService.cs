namespace Mvvm.Utils.UI.Win.Services {
    using System;
    using System.Threading;
    using Mvvm.Utils.Services;

    public class DispatcherService {
        SynchronizationContext context;
        protected DispatcherService() {
            context = SynchronizationContext.Current;
        }
        public void BeginInvoke(Action action) {
            if(action != null && context != null)
                context.Post((s) => action(), null);
        }
        #region static
        public static DispatcherService Create() {
            return DynamicServiceSource.Create<DispatcherService>(
                new Type[] { 
                    MVVMTypesResolver.Instance.GetIDispatcherServiceType()
                });
        }
        #endregion static
    }
}