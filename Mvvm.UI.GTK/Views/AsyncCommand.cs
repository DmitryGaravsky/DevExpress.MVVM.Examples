using System.Windows.Forms;
using Mvvm.Examples;
using Mvvm.Utils.UI.GTK;

namespace Mvvm.UI.GTK {
    public partial class AsyncCommandForm : Form {
        GTKMVVMContext mvvmContext;
        public AsyncCommandForm() {
            this.mvvmContext = new GTKMVVMContext(components);
            this.mvvmContext.ContainerControl = this;
            //
            InitializeComponent();
            if(!DesignMode)
                InitBindings();
        }
        void InitBindings() {
            mvvmContext.ViewModelType = typeof(ViewModelForAsyncCommand);
            mvvmContext.RegisterService(Mvvm.Utils.UI.GTK.Services.DispatcherService.Create());

            var fluent = mvvmContext.OfType<ViewModelForAsyncCommand>();
            fluent.BindCommand(btnCalc.AsCommandBindable(), x => x.Calculate());
            fluent.BindCancelCommand(btnCancel.AsCommandBindable(), x => x.Calculate());
            fluent.SetBinding(progressBar, pb => pb.Value, x => x.Progress);
        }
    }
}