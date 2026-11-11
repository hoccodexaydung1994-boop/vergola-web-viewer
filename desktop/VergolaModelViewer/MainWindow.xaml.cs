using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.Web.WebView2.Core;
using Microsoft.Win32;

namespace VergolaModelViewer
{
    /// <summary>
    /// Khung WebView2 chạy Vergola Web Viewer (web\index.html) hoàn toàn trên máy:
    ///  • https://vergola.local/…            → thư mục web\ cạnh .exe (SetVirtualHostNameToFolderMapping).
    ///  • https://cdn.jsdelivr.net/npm/three@0.160.0/… → trả từ lib\three@0.160.0\ (không ra mạng). File nào chưa có thì
    ///    tải 1 lần khi có mạng rồi lưu lại → các lần sau offline. Nhờ vậy cả trang .html Vergola xuất (importmap trỏ CDN)
    ///    cũng chạy offline trong phần mềm này.
    ///  • https://model.vergola.local/&lt;số&gt;/&lt;tên&gt;.glb → file mô hình đang mở (đọc thẳng từ đĩa, không chép).
    /// </summary>
    public partial class MainWindow : Window
    {
        private const string AppHost = "vergola.local";
        private const string ModelHost = "model.vergola.local";
        private const string ThreeVersion = "0.160.0";
        private const string CdnPrefix = "https://cdn.jsdelivr.net/npm/three@" + ThreeVersion + "/";
        private const string RuntimeUrl = "https://go.microsoft.com/fwlink/p/?LinkId=2124703";
        private static readonly HttpClient Http = new HttpClient { Timeout = TimeSpan.FromSeconds(60) };

        private readonly string _baseDir = AppContext.BaseDirectory;
        private readonly string _version;
        private CoreWebView2Environment _env;
        private string _pending;      // file truyền từ dòng lệnh, mở khi WebView2 sẵn sàng
        private string _source;       // file người dùng mở (.glb hoặc .html)
        private string _modelFile;    // file GLB thật đang phục vụ cho trang
        private int _missingLibs;

        private string WebDir => Path.Combine(_baseDir, "web");
        private string LibDir => Path.Combine(_baseDir, "lib", "three@" + ThreeVersion);

        public MainWindow(string startFile)
        {
            InitializeComponent();
            _pending = startFile;
            _version = typeof(MainWindow).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion ?? "";
            int plus = _version.IndexOf('+'); if (plus > 0) _version = _version.Substring(0, plus);
            Title = "Vergola Model Viewer " + _version;
            Loaded += async (s, e) => await InitAsync();
            RebuildRecent();
            ModelSource.CleanTemp();
        }

        // ------------------------------------------------------------------ khởi động WebView2
        private async Task InitAsync()
        {
            try
            {
                string data = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Vergola", "ModelViewer", "WebView2");
                _env = await CoreWebView2Environment.CreateAsync(null, data);
                await Web.EnsureCoreWebView2Async(_env);
            }
            catch (WebView2RuntimeNotFoundException)
            {
                ShowError("Máy chưa có Microsoft Edge WebView2 Runtime (Windows 10/11 thường có sẵn). Bấm nút dưới để tải bản cài miễn phí của Microsoft, cài xong mở lại phần mềm.");
                return;
            }
            catch (Exception ex)
            {
                ShowError("WebView2 báo lỗi: " + ex.Message + " → đóng hết cửa sổ Vergola Model Viewer rồi mở lại.");
                return;
            }

            if (!File.Exists(Path.Combine(WebDir, "index.html")))
            {
                ShowError("Thiếu web\\index.html cạnh VergolaModelViewer.exe → build lại project hoặc chép lại cả thư mục phần mềm.");
                return;
            }

            CoreWebView2 core = Web.CoreWebView2;
            core.SetVirtualHostNameToFolderMapping(AppHost, WebDir, CoreWebView2HostResourceAccessKind.Allow);
            core.AddWebResourceRequestedFilter(CdnPrefix + "*", CoreWebView2WebResourceContext.All);
            core.AddWebResourceRequestedFilter("https://" + ModelHost + "/*", CoreWebView2WebResourceContext.All);
            core.WebResourceRequested += OnResource;
            core.Settings.IsStatusBarEnabled = false;
            core.Settings.IsZoomControlEnabled = false;            // Ctrl + lăn chuột để viewer dùng, không phóng chữ trang
            core.Settings.AreDevToolsEnabled = Debugger.IsAttached;
            core.NewWindowRequested += (s, e) => { e.Handled = true; OpenExternal(e.Uri); };
            core.NavigationStarting += (s, e) =>
            {
                // Chỉ ở lại trong viewer; link ra ngoài (README, tài liệu…) mở bằng trình duyệt mặc định
                if (!e.Uri.StartsWith("https://" + AppHost + "/", StringComparison.OrdinalIgnoreCase)) { e.Cancel = true; OpenExternal(e.Uri); }
            };

            UpdateLibText();
            if (!string.IsNullOrEmpty(_pending)) { string p = _pending; _pending = null; Open(p); }
            else NavigateViewer(null);
        }

        private void ShowError(string msg)
        {
            ErrorText.Text = msg;
            ErrorPanel.Visibility = Visibility.Visible;
            Web.Visibility = Visibility.Collapsed;
            OpenBtn.IsEnabled = RecentBtn.IsEnabled = SampleBtn.IsEnabled = false;
        }

        // ------------------------------------------------------------------ phục vụ thư viện + mô hình
        private void OnResource(object sender, CoreWebView2WebResourceRequestedEventArgs e)
        {
            string uri = e.Request.Uri;
            if (uri.StartsWith(CdnPrefix, StringComparison.OrdinalIgnoreCase))
            {
                string rel = Uri.UnescapeDataString(new Uri(uri).AbsolutePath).Substring(("/npm/three@" + ThreeVersion + "/").Length);
                string local = SafeCombine(LibDir, rel);
                if (local == null) return;
                if (File.Exists(local)) { e.Response = Script(File.ReadAllBytes(local)); return; }
                CoreWebView2Deferral def = e.GetDeferral();
                _ = FetchAndCacheAsync(uri, local, e, def);
                return;
            }

            if (uri.StartsWith("https://" + ModelHost + "/", StringComparison.OrdinalIgnoreCase))
            {
                if (string.IsNullOrEmpty(_modelFile) || !File.Exists(_modelFile))
                {
                    e.Response = _env.CreateWebResourceResponse(null, 404, "Not Found", "Access-Control-Allow-Origin: *");
                    return;
                }
                var fs = new FileStream(_modelFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                e.Response = _env.CreateWebResourceResponse(fs, 200, "OK",
                    "Content-Type: model/gltf-binary\nContent-Length: " + fs.Length + "\nAccess-Control-Allow-Origin: *\nCache-Control: no-store");
            }
        }

        /// <summary>File three.js chưa có trong lib\ → tải 1 lần từ jsDelivr, lưu lại, trả cho trang. Không có mạng → để trang báo lỗi tải.</summary>
        private async Task FetchAndCacheAsync(string uri, string local, CoreWebView2WebResourceRequestedEventArgs e, CoreWebView2Deferral def)
        {
            try
            {
                byte[] bytes = await Http.GetByteArrayAsync(uri);
                try { Directory.CreateDirectory(Path.GetDirectoryName(local)); File.WriteAllBytes(local, bytes); }
                catch { /* thư mục cài chỉ đọc (Program Files) → vẫn trả cho trang, lần sau tải lại */ }
                e.Response = Script(bytes);
            }
            catch
            {
                _missingLibs++;
            }
            finally
            {
                def.Complete();
                UpdateLibText();
            }
        }

        private CoreWebView2WebResourceResponse Script(byte[] bytes) =>
            _env.CreateWebResourceResponse(new MemoryStream(bytes), 200, "OK",
                "Content-Type: text/javascript; charset=utf-8\nAccess-Control-Allow-Origin: *");

        private static string SafeCombine(string root, string rel)
        {
            try
            {
                string full = Path.GetFullPath(Path.Combine(root, rel.Replace('/', Path.DirectorySeparatorChar)));
                string r = Path.GetFullPath(root).TrimEnd(Path.DirectorySeparatorChar) + Path.DirectorySeparatorChar;
                return full.StartsWith(r, StringComparison.OrdinalIgnoreCase) ? full : null;
            }
            catch { return null; }
        }

        private void UpdateLibText()
        {
            bool ok = File.Exists(Path.Combine(LibDir, "build", "three.module.js"));
            if (_missingLibs > 0)
            {
                LibText.Text = "Thiếu thư viện three.js offline — nối mạng 1 lần rồi mở lại mô hình";
                LibText.Foreground = (System.Windows.Media.Brush)FindResource("Brush.Warn");
            }
            else LibText.Text = ok ? "Offline · three.js " + ThreeVersion : "Lần đầu cần mạng để lưu three.js";
        }

        // ------------------------------------------------------------------ mở mô hình
        private void Open(string path)
        {
            try
            {
                path = Path.GetFullPath(path);
                if (!File.Exists(path)) { MessageBox.Show(this, "Không thấy file: " + path, Title, MessageBoxButton.OK, MessageBoxImage.Warning); Recent.Remove(path); RebuildRecent(); return; }
                if (Web.CoreWebView2 == null) { _pending = path; return; }

                string glb = ModelSource.ToGlb(path, out string name);
                _source = path;
                _modelFile = glb;
                Recent.Add(path); RebuildRecent();
                FileText.Text = path;
                FolderBtn.IsEnabled = true;
                Title = Path.GetFileName(path) + " — Vergola Model Viewer " + _version;
                NavigateViewer(name);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, Path.GetFileName(path) + ": " + ex.Message, Title, MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private void NavigateViewer(string modelName)
        {
            string url = "https://" + AppHost + "/index.html";
            // số ở giữa đổi mỗi lần mở → trang không dùng lại bản cũ khi mở lại cùng tên file
            if (modelName != null)
                url += "?model=" + Uri.EscapeDataString("https://" + ModelHost + "/" + DateTime.Now.Ticks + "/" + Uri.EscapeDataString(modelName));
            Web.CoreWebView2.Navigate(url);
        }

        private void OnOpenCommand(object sender, ExecutedRoutedEventArgs e) => OnOpen(sender, e);

        private void OnOpen(object sender, RoutedEventArgs e)
        {
            var dlg = new OpenFileDialog
            {
                Title = "Mở mô hình Vergola",
                Filter = "Mô hình Vergola (*.glb;*.html)|*.glb;*.html;*.htm|GLB (*.glb)|*.glb|Trang Vergola (*.html)|*.html;*.htm|Tất cả|*.*",
                InitialDirectory = _source != null ? Path.GetDirectoryName(_source) : ""
            };
            if (dlg.ShowDialog(this) == true) Open(dlg.FileName);
        }

        private void OnSample(object sender, RoutedEventArgs e)
        {
            string s = Directory.Exists(Path.Combine(WebDir, "samples"))
                ? Directory.GetFiles(Path.Combine(WebDir, "samples"), "*.glb").FirstOrDefault() : null;
            if (s == null) { MessageBox.Show(this, "Không có mô hình mẫu trong web\\samples\\.", Title); return; }
            Open(s);
        }

        private void OnFolder(object sender, RoutedEventArgs e)
        {
            if (_source == null || !File.Exists(_source)) return;
            try { Process.Start(new ProcessStartInfo("explorer.exe", "/select,\"" + _source + "\"") { UseShellExecute = true }); } catch { }
        }

        private void OnRecent(object sender, RoutedEventArgs e)
        {
            RebuildRecent();
            RecentMenu.PlacementTarget = RecentBtn;
            RecentMenu.IsOpen = true;
        }

        private void RebuildRecent()
        {
            RecentMenu.Items.Clear();
            var list = Recent.Load();
            if (list.Count == 0) { RecentMenu.Items.Add(new MenuItem { Header = "(chưa mở mô hình nào)", IsEnabled = false }); return; }
            foreach (string p in list)
            {
                var mi = new MenuItem { Header = Path.GetFileName(p) + "   —   " + Path.GetDirectoryName(p), Tag = p, ToolTip = p };
                mi.Click += (s, a) => Open((string)((MenuItem)s).Tag);
                RecentMenu.Items.Add(mi);
            }
            RecentMenu.Items.Add(new Separator());
            var clear = new MenuItem { Header = "Xoá danh sách gần đây" };
            clear.Click += (s, a) => { Recent.Clear(); RebuildRecent(); };
            RecentMenu.Items.Add(clear);
        }

        private void OnGetRuntime(object sender, RoutedEventArgs e) => OpenExternal(RuntimeUrl);

        private static void OpenExternal(string url)
        {
            if (string.IsNullOrEmpty(url) || !(url.StartsWith("https://", StringComparison.OrdinalIgnoreCase) || url.StartsWith("http://", StringComparison.OrdinalIgnoreCase))) return;
            try { Process.Start(new ProcessStartInfo(url) { UseShellExecute = true }); } catch { }
        }
    }
}
