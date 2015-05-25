using System;
using System.Windows.Forms;

namespace Mvvm.UI.GTK {
    static class Program {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main() {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // Example 01.
            Application.Run(new SimpleBindingForm());

            // Example 02.
            //Application.Run(new AsyncCommandForm());
        }
    }
}