using Gtk;
using Mvvm.Examples;
using Mvvm.Utils.UI.GTK;

namespace Mvvm.UI.GTK {
    public partial class SimpleBindingWindow : Window {
        GTKMVVMContext mvvmContext;
        public SimpleBindingWindow() : base(WindowType.Toplevel) {
            this.mvvmContext = new GTKMVVMContext(this);
            InitializeComponent();
            InitBindings();

            ShowAll();
        }
        void InitBindings() {
            mvvmContext.ViewModelType = typeof(ViewModelForSimpleBinding);

            var fluent = mvvmContext.OfType<ViewModelForSimpleBinding>();
            fluent.SetBinding(this, f => f.Title, x => x.Title);
            fluent.SetBinding(entryTitle.AsEditableEntry(), e => e.Text, x => x.Title);
            fluent.BindCommand(btnResetTtile.AsCommandBindable(), x => x.ResetTitle());
        }
    }
}