namespace Mvvm.Utils.UI.GTK {
    using System;

    public interface IEditableEntry {
        string Text{ get; set; }
        event EventHandler TextChanged;
    }
    public static class EditableExtension {
        public static IEditableEntry AsEditableEntry(this Gtk.Entry entry) {
            return new EditableEntry(entry);
        }
        class EditableEntry :IEditableEntry {
            Gtk.Entry entry;
            public EditableEntry(Gtk.Entry entry) {
                this.entry = entry;
                this.entry.TextDeleted += OnTextChanged;
                this.entry.TextInserted += OnTextChanged;
            }
            void OnTextChanged(object sender, EventArgs e) {
                if(TextChanged != null)
                    TextChanged(this, EventArgs.Empty);
            }
            public string Text {
                get{ return entry.Text; }
                set{ entry.Text = value; }
            }
            public event EventHandler TextChanged;
        }
    }
}