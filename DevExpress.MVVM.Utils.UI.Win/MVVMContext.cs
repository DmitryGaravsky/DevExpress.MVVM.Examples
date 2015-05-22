using System;
using System.ComponentModel;
using System.Windows.Forms;

namespace Mvvm.Utils.UI.Win {
    public sealed class WinMVVMContext : Mvvm.Utils.MVVMContext, IComponent {
        static WinMVVMContext() {
            GetParentContainerFunction =
                (container) => ((Control)container).Parent;
        }
        public WinMVVMContext()
            : this(null) {
        }
        public WinMVVMContext(IContainer container) {
            if(container != null)
                container.Add(this);
        }
        [DefaultValue(null), Category("Behavior"), RefreshProperties(RefreshProperties.All)]
        public ContainerControl ContainerControl {
            get { return Container as ContainerControl; }
            set { Container = value; }
        }
        public event EventHandler Disposed;
        protected override void OnDisposing() {
            base.OnDisposing();
            if(Disposed != null)
                Disposed(this, EventArgs.Empty);
        }
        ISite IComponent.Site { get; set; }
    }
}