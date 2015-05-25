using System.Windows.Forms;
using Mvvm.Examples;
using Mvvm.Utils.UI.Win;

namespace Mvvm.UI.Win {
    public partial class AsyncCommandForm : Form {
        WinMVVMContext mvvmContext;
        public AsyncCommandForm() {
            this.mvvmContext = new WinMVVMContext(components);
            this.mvvmContext.ContainerControl = this;
            //
            InitializeComponent();
            if(!DesignMode)
                InitBindings();
        }
        void InitBindings() {
            mvvmContext.ViewModelType = typeof(ViewModelForAsyncCommand);
            mvvmContext.RegisterService(Mvvm.Utils.UI.Win.Services.DispatcherService.Create());

            var fluent = mvvmContext.OfType<ViewModelForAsyncCommand>();
            fluent.BindCommand(btnCalc.AsCommandBindable(), x => x.Calculate());
            fluent.BindCancelCommand(btnCancel.AsCommandBindable(), x => x.Calculate());
            fluent.SetBinding(progressBar, pb => pb.Value, x => x.Progress);
        }
    }
}