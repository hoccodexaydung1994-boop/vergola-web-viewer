using System;
using System.Linq;
using System.Windows;

namespace VergolaModelViewer
{
    /// <summary>
    /// Vergola Model Viewer — xem mô hình GLB / trang .html Vergola xuất từ Revit, không cần Revit, không cần Internet.
    /// Dòng lệnh: VergolaModelViewer.exe [đường dẫn .glb hoặc .html]   (Windows "Mở bằng…" / bấm đúp .glb sau khi cài).
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            DispatcherUnhandledException += (s, a) =>
            {
                MessageBox.Show("Lỗi: " + a.Exception.Message, "Vergola Model Viewer", MessageBoxButton.OK, MessageBoxImage.Error);
                a.Handled = true;
            };
            string file = e.Args.FirstOrDefault(x => !string.IsNullOrWhiteSpace(x) && !x.StartsWith("-", StringComparison.Ordinal));
            var w = new MainWindow(file);
            MainWindow = w;
            w.Show();
        }
    }
}
