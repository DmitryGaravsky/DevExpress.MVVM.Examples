using System.Windows.Forms;
using Mvvm.Examples;
using Mvvm.Utils.UI.Win;

namespace Mvvm.UI.Win {
    public partial class SimpleBindingForm : Form {
        WinMVVMContext mvvmContext;
        public SimpleBindingForm() {
            this.mvvmContext = new WinMVVMContext(components);
            this.mvvmContext.ContainerControl = this;
            InitializeComponent();
            if(!DesignMode)
                InitBindings();
        }
        void InitBindings() {
            mvvmContext.ViewModelType = typeof(ViewModelForSimpleBinding);

            var fluent = mvvmContext.OfType<ViewModelForSimpleBinding>();
            fluent.SetBinding(this, f => f.Text, x => x.Title);
            fluent.SetBinding(tbTitle, t => t.Text, x => x.Title);
            fluent.BindCommand(btnResetTtile.AsCommandBindable(), x => x.ResetTitle());
        }
    }
}