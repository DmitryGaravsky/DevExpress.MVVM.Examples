namespace Mvvm.UI.GTK {
    partial class SimpleBindingWindow {
        private void InitializeComponent() {
            Gtk.Box box = new Gtk.Box(Gtk.Orientation.Vertical, 4);

            this.btnResetTtile = new Gtk.Button();
            this.entryTitle = new Gtk.Entry();
            // 
            // btnResetTtile
            // 
            this.btnResetTtile.Name = "btnResetTtile";
            this.btnResetTtile.Label = "Reset Title";
            // 
            // tbTitle
            // 
            this.entryTitle.Name = "tbTitle";
            // 
            // SimpleBindingForm
            // 
            box.Add(this.entryTitle);
            box.Add(this.btnResetTtile);
            box.Margin = 12;
            this.Add(box);
            this.Name = "SimpleBindingForm";
            this.Title = "MVVM Practices: Simplle Binding";
            this.WidthRequest = 400;
        }
        private Gtk.Button btnResetTtile;
        private Gtk.Entry entryTitle;

        protected override bool OnDeleteEvent(Gdk.Event e){
            Gtk.Application.Quit();
            return true;
        }
    }
}