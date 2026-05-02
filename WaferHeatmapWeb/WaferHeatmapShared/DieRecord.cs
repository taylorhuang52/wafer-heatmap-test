#nullable enable
using System.Collections.Generic;

namespace WaferHeatmap.Shared
{
    /// <summary>
    /// 單顆 Die 的量測資料。
    /// 此類別不依賴任何 Windows / GDI+ API，可直接用於 Blazor WASM。
    /// </summary>
    public class DieRecord
    {
        public int WaferId    { get; set; }
        public int X          { get; set; }
        public int Y          { get; set; }
        public double? IR0    { get; set; }
        public double? VF0    { get; set; }
        public double? VF1    { get; set; }
        public double? IDSS2  { get; set; }
        public double? IGSS2F { get; set; }
        public double? BVDSS3 { get; set; }

        /// <summary>存所有數值欄位，供 Explorer 動態查詢任意欄位名稱。</summary>
        public Dictionary<string, double?> Extra { get; } =
            new(System.StringComparer.OrdinalIgnoreCase);

        public double? GetExtra(string col) =>
            Extra.TryGetValue(col, out var v) ? v : null;
    }
}
