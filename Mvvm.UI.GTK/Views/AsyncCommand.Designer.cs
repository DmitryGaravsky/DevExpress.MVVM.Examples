namespace Mvvm.UI.GTK {
    partial class AsyncCommandWindow {
        private void InitializeComponent() {
            Gtk.Box box = new Gtk.Box(Gtk.Orientation.Vertical,12);
            this.progressBar = new Gtk.ProgressBar();
            this.btnCalc = new Gtk.Button();
            this.btnCancel = new Gtk.Button();
            // 
            // progressBar
            // 
            this.progressBar.Name = "progressBar";
            // 
            // btnCalc
            // 
            this.btnCalc.Name = "btnCalc";
            this.btnCalc.Label = "Start Calculation";
            // 
            // btnCancel
            // 
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Label = "Cancel Calculation";
            // 
            // AsyncCommandForm
            // 
            box.Add(this.progressBar);
            box.Add(this.btnCalc);
            box.Add(this.btnCancel);

            this.Add(box);
            this.Name = "AsyncCommandForm";
            this.Title = "MVVM Practices: Async Command";
            this.WidthRequest = 400;
        }
        Gtk.ProgressBar progressBar;
        Gtk.Button btnCalc;
        Gtk.Button btnCancel;

        protected override bool OnDeleteEvent(Gdk.Event e){
            Gtk.Application.Quit();
            return true;
        }
    }
}