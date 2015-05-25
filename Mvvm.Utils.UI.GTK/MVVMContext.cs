namespace Mvvm.Utils.UI.GTK {
    public class GTKMVVMContext : MVVMContext {
        static GTKMVVMContext() {
            GetParentContainerFunction =
                (container) => ((Gtk.Container)container).Parent;
        }
        public GTKMVVMContext() : this(null) {
        }
        public GTKMVVMContext(Gtk.Container container) {
            Container = container;
        }
        public Gtk.Container GtkContainer {
            get { return Container as Gtk.Container; }
            set { Container = value; }
        }
    }
}