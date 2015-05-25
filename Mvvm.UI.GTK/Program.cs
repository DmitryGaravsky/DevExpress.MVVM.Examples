using System;
using Gtk;

namespace Mvvm.UI.GTK {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.Init ();

            // Example 01.
            new SimpleBindingWindow();

            // Example 02.
            //new AsyncCommandWindow();

            Application.Run();
        }
    }
}