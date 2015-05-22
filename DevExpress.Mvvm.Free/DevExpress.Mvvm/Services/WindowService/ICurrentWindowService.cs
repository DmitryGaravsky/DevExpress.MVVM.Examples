using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;

namespace DevExpress.Mvvm {
    public interface ICurrentWindowService {
        void Close();
#if !MONO
        void SetWindowState(WindowState state);
#endif
        void Activate();
        void Hide();
        void Show();
    }
}