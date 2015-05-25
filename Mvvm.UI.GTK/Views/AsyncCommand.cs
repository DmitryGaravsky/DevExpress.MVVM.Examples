using Gtk;
using Mvvm.Examples;
using Mvvm.Utils.UI.GTK;

namespace Mvvm.UI.GTK {
    public partial class AsyncCommandWindow : Window {
        GTKMVVMContext mvvmContext;
        public AsyncCommandWindow() : base(WindowType.Toplevel) {
            this.mvvmContext = new GTKMVVMContext(this);
            //
            InitializeComponent();
            InitBindings();

            ShowAll();
        }
        void InitBindings() {
            mvvmContext.ViewModelType = typeof(ViewModelForAsyncCommand);
            mvvmContext.RegisterService(Mvvm.Utils.UI.GTK.Services.DispatcherService.Create());

            var fluent = mvvmContext.OfType<ViewModelForAsyncCommand>();
            fluent.BindCommand(btnCalc.AsCommandBindable(), x => x.Calculate());
            fluent.BindCancelCommand(btnCancel.AsCommandBindable(), x => x.Calculate());
            fluent.SetBinding(progressBar, pb => pb.Fraction, x => x.Percent);
        }
    }
}