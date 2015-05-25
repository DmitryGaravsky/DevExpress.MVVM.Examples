namespace Mvvm.Examples {
    using System.Threading;
    using System.Threading.Tasks;
    using DevExpress.Mvvm;
    using DevExpress.Mvvm.POCO;

    public class ViewModelForAsyncCommand {
        // Asynchronous POCO-command will be created from this method.
        public Task Calculate() {
            return Task.Factory.StartNew(() =>
            {
                var asyncCommand = this.GetAsyncCommand(x => x.Calculate());
                for(int i = 0; i <= 100; i++) {
                    if(asyncCommand.IsCancellationRequested) // cancellation check
                        break;
                    Thread.Sleep(50); // do some work here
                    UpdateProgressOnUIThread(i);
                }
                UpdateProgressOnUIThread(0);
            });
        }
        // Property for progress
        public int Progress { get; private set; }
        protected IDispatcherService DispatcherService {
            get { return this.GetService<IDispatcherService>(); }
        }
        void UpdateProgressOnUIThread(int progress) {
            DispatcherService.BeginInvoke(() =>
            {
                Progress = progress;
                this.RaisePropertyChanged(x => x.Progress);
            });
        }
    }
}