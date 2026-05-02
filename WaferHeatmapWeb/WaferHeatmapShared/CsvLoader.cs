#nullable enable
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace WaferHeatmap.Shared
{
    /// <summary>
    /// 讀取 CP Log CSV 並轉成 DieRecord 列表。
    /// 不依賴任何 Windows / GDI+ API，可直接用於 Blazor WASM。
    /// 注意：Blazor 環境不能用 File.ReadAllLines，請改用 LoadFromText()。
    /// </summary>
    public static class CsvLoader
    {
        private static readonly HashSet<string> _coordCols =
            new(StringComparer.OrdinalIgnoreCase)
            { "Wafer_ID", "X", "Y", "Serial", "HB", "SB" };

        // ── WinForms 版：從檔案路徑載入 ──────────────────────────────────────
        // （Blazor 請改用 LoadFromText）
        public static List<DieRecord> Load(string path)
        {
            string[] lines = File.ReadAllLines(path);
            return Parse(lines);
        }

        // ── Blazor / 通用版：從已讀取的文字內容載入 ─────────────────────────
        // 用法：將上傳的 CSV 內容以 string 傳入
        public static List<DieRecord> LoadFromText(string csvText)
        {
            var lines = csvText.Split(new[] { "\r\n", "\n" },
                                      StringSplitOptions.None);
            return Parse(lines);
        }

        // ── 核心解析邏輯（共用）──────────────────────────────────────────────
        private static List<DieRecord> Parse(string[] lines)
        {
            var records = new List<DieRecord>();
            if (lines.Length == 0) return records;

            string[] headers = lines[0].Split(',');
            int idxWafer  = Array.IndexOf(headers, "Wafer_ID");
            int idxX      = Array.IndexOf(headers, "X");
            int idxY      = Array.IndexOf(headers, "Y");
            int idxIR0    = Array.IndexOf(headers, "IR0");
            int idxVF0    = Array.IndexOf(headers, "VF0");
            int idxVF1    = Array.IndexOf(headers, "VF1");
            int idxIDSS2  = Array.IndexOf(headers, "IDSS2");
            if (idxIDSS2 < 0) idxIDSS2 = Array.IndexOf(headers, "IR2_1"); // 舊版相容
            int idxIGSS2F = Array.IndexOf(headers, "IGSS2F");
            int idxBVDSS3 = Array.IndexOf(headers, "BVDSS3");

            // 找出所有非座標欄位，供 Extra 字典使用
            var extraCols = new List<(int idx, string name)>();
            for (int i = 0; i < headers.Length; i++)
            {
                string h = headers[i].Trim();
                if (!_coordCols.Contains(h))
                    extraCols.Add((i, h));
            }

            static double? ParseNum(string[] parts, int idx)
            {
                if (idx < 0 || idx >= parts.Length) return null;
                return double.TryParse(parts[idx], NumberStyles.Float,
                                       CultureInfo.InvariantCulture, out double v)
                       ? v : (double?)null;
            }

            for (int i = 1; i < lines.Length; i++)
            {
                if (string.IsNullOrWhiteSpace(lines[i])) continue;
                string[] parts = lines[i].Split(',');
                if (parts.Length <= Math.Max(Math.Max(idxWafer, idxX), Math.Max(idxY, 0)))
                    continue;

                if (!int.TryParse(parts[idxWafer], out int waferId)) continue;
                if (!int.TryParse(parts[idxX],     out int px))      continue;
                if (!int.TryParse(parts[idxY],     out int py))      continue;

                var rec = new DieRecord
                {
                    WaferId = waferId,
                    X       = px,
                    Y       = py,
                    IR0     = ParseNum(parts, idxIR0),
                    VF0     = ParseNum(parts, idxVF0),
                    VF1     = ParseNum(parts, idxVF1),
                    IDSS2   = ParseNum(parts, idxIDSS2),
                    IGSS2F  = ParseNum(parts, idxIGSS2F),
                    BVDSS3  = ParseNum(parts, idxBVDSS3)
                };

                foreach (var (idx, name) in extraCols)
                    rec.Extra[name] = ParseNum(parts, idx);

                records.Add(rec);
            }
            return records;
        }

        /// <summary>從已載入的 DieRecord 取出所有有效數值欄位名稱。</summary>
        public static List<string> GetNumericColumns(List<DieRecord> records)
        {
            if (records.Count == 0) return new();
            return records[0].Extra.Keys
                .Where(k => records.Any(r => r.Extra.TryGetValue(k, out var v) && v.HasValue))
                .ToList();
        }
    }
}
