using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace VergolaModelViewer
{
    /// <summary>
    /// Đổi file người dùng mở thành 1 file GLB để phục vụ trang:
    ///  • .glb  → chính nó (kiểm 4 byte đầu "glTF").
    ///  • .html Vergola xuất → GLB nhúng (base64 trong &lt;script id="vg-embedded"&gt;) giải ra %TEMP%\Vergola\ModelViewer\;
    ///    trang không nhúng (GLB &gt; 120 MB) → dùng file GLB cùng tên (meta vg-model) đặt cạnh trang.
    /// </summary>
    internal static class ModelSource
    {
        private static readonly string TempDir = Path.Combine(Path.GetTempPath(), "Vergola", "ModelViewer");

        public static string ToGlb(string path, out string name)
        {
            string ext = Path.GetExtension(path).ToLowerInvariant();
            if (ext == ".gltf") throw new InvalidOperationException("File .gltf (nhiều file rời) chưa hỗ trợ → xuất lại dạng .glb.");
            if (ext == ".html" || ext == ".htm") return FromHtml(path, out name);
            CheckGlb(path);
            name = Path.GetFileName(path);
            return path;
        }

        private static string FromHtml(string html, out string name)
        {
            string text = File.ReadAllText(html, Encoding.UTF8);
            Match meta = Regex.Match(text, "<meta\\s+name=\"vg-model\"\\s+content=\"([^\"]*)\"", RegexOptions.IgnoreCase);
            name = meta.Success ? WebUtility.HtmlDecode(meta.Groups[1].Value) : Path.ChangeExtension(Path.GetFileName(html), ".glb");
            if (string.IsNullOrWhiteSpace(name) || name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0) name = Path.ChangeExtension(Path.GetFileName(html), ".glb");

            int tag = text.IndexOf("id=\"vg-embedded\"", StringComparison.Ordinal);
            if (tag < 0) throw new InvalidOperationException("Không phải trang Vergola Web Viewer (thiếu khối mô hình nhúng) → mở thẳng file .glb.");
            int start = text.IndexOf('>', tag) + 1;
            int end = text.IndexOf("</script>", start, StringComparison.OrdinalIgnoreCase);
            if (start <= 0 || end < 0) throw new InvalidOperationException("Trang .html bị hỏng (khối mô hình không đóng) → xuất lại từ Revit.");
            string b64 = text.Substring(start, end - start).Trim();

            if (b64.Length == 0 || b64.StartsWith("/*", StringComparison.Ordinal))
            {
                // Trang không nhúng (mô hình lớn): GLB cùng tên nằm cạnh trang
                string beside = Path.Combine(Path.GetDirectoryName(html) ?? "", name);
                if (File.Exists(beside)) { CheckGlb(beside); return beside; }
                throw new InvalidOperationException($"Trang này không nhúng mô hình (GLB > 120 MB) → đặt {name} cạnh trang rồi mở lại, hoặc mở thẳng file .glb.");
            }

            byte[] bytes;
            try { bytes = Convert.FromBase64String(b64); }
            catch (FormatException) { throw new InvalidOperationException("Mô hình nhúng trong trang bị hỏng (base64 sai) → xuất lại từ Revit."); }
            if (bytes.Length < 12 || bytes[0] != 0x67 || bytes[1] != 0x6C || bytes[2] != 0x54 || bytes[3] != 0x46)
                throw new InvalidOperationException("Mô hình nhúng không phải GLB → xuất lại từ Revit.");

            Directory.CreateDirectory(TempDir);
            string key;
            using (var sha = SHA1.Create()) key = BitConverter.ToString(sha.ComputeHash(Encoding.UTF8.GetBytes(html.ToLowerInvariant()))).Replace("-", "").Substring(0, 10);
            string dir = Path.Combine(TempDir, key);
            Directory.CreateDirectory(dir);
            string glb = Path.Combine(dir, name);
            File.WriteAllBytes(glb, bytes);
            return glb;
        }

        private static void CheckGlb(string path)
        {
            var head = new byte[4];
            using (var fs = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete))
                if (fs.Read(head, 0, 4) < 4 || head[0] != 0x67 || head[1] != 0x6C || head[2] != 0x54 || head[3] != 0x46)
                    throw new InvalidOperationException("Không phải file GLB (glTF nhị phân) → xuất lại từ Revit bằng Xuất 3D Web.");
        }

        /// <summary>Xoá mô hình giải nén từ trang .html quá 2 ngày.</summary>
        public static void CleanTemp()
        {
            try
            {
                if (!Directory.Exists(TempDir)) return;
                foreach (string d in Directory.GetDirectories(TempDir))
                    if (Directory.GetLastWriteTime(d) < DateTime.Now.AddDays(-2)) Directory.Delete(d, true);
            }
            catch { }
        }
    }

    /// <summary>Danh sách mô hình mở gần đây: %APPDATA%\Vergola\ModelViewer\recent.txt (tối đa 10 dòng).</summary>
    internal static class Recent
    {
        private const int Max = 10;
        private static readonly string FilePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "Vergola", "ModelViewer", "recent.txt");

        public static List<string> Load()
        {
            try { return File.Exists(FilePath) ? File.ReadAllLines(FilePath, Encoding.UTF8).Where(l => l.Length > 0).Distinct(StringComparer.OrdinalIgnoreCase).Take(Max).ToList() : new List<string>(); }
            catch { return new List<string>(); }
        }

        public static void Add(string path)
        {
            var list = Load();
            list.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase));
            list.Insert(0, path);
            Save(list.Take(Max));
        }

        public static void Remove(string path)
        {
            var list = Load();
            if (list.RemoveAll(p => string.Equals(p, path, StringComparison.OrdinalIgnoreCase)) > 0) Save(list);
        }

        public static void Clear() => Save(Enumerable.Empty<string>());

        private static void Save(IEnumerable<string> lines)
        {
            try { Directory.CreateDirectory(Path.GetDirectoryName(FilePath)); File.WriteAllLines(FilePath, lines, new UTF8Encoding(false)); }
            catch { }
        }
    }
}
