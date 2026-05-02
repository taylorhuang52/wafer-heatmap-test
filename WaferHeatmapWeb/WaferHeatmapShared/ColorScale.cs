#nullable enable
using System;

namespace WaferHeatmap.Shared
{
    /// <summary>
    /// 將數值對應成 RGB 顏色（綠→黃→紅漸層）。
    /// 不依賴任何 Windows / GDI+ API。
    /// 顏色以 (R, G, B) tuple 回傳，方便各平台使用：
    ///   - WinForms：Color.FromArgb(r, g, b)
    ///   - Blazor：  $"rgb({r},{g},{b})"
    ///   - SkiaSharp：SKColor(r, g, b)
    /// </summary>
    public static class ColorScale
    {
        // 三個錨點：low→綠, mid→黃, high→紅
        private static readonly (double t, int r, int g, int b)[] Stops =
        {
            (0.00,   0, 200,   0),   // green  (低值)
            (0.50, 255, 255,   0),   // yellow (中值)
            (1.00, 255,   0,   0),   // red    (高值)
        };

        /// <summary>
        /// 計算顏色，回傳 (R, G, B)。
        /// 超出範圍 / null / NaN → 灰色 (160, 160, 160)
        /// </summary>
        public static (int R, int G, int B) GetRgb(double? val, double lo, double hi)
        {
            if (val == null || double.IsNaN(val.Value))  return (160, 160, 160);
            if (val.Value < lo || val.Value > hi)         return (160, 160, 160);

            double t = (val.Value - lo) / (hi - lo);  // 0=green, 1=red

            for (int i = 0; i < Stops.Length - 1; i++)
            {
                var (t0, r0, g0, b0) = Stops[i];
                var (t1, r1, g1, b1) = Stops[i + 1];
                if (t >= t0 && t <= t1)
                {
                    double f = (t - t0) / (t1 - t0);
                    return (
                        (int)(r0 + (r1 - r0) * f),
                        (int)(g0 + (g1 - g0) * f),
                        (int)(b0 + (b1 - b0) * f)
                    );
                }
            }
            var last = Stops[Stops.Length - 1];
            return (last.r, last.g, last.b);
        }

        /// <summary>回傳 CSS rgb() 字串，方便 Blazor 直接使用。</summary>
        public static string GetCss(double? val, double lo, double hi)
        {
            var (r, g, b) = GetRgb(val, lo, hi);
            return $"rgb({r},{g},{b})";
        }

        /// <summary>判斷背景色深淺，決定文字要用黑或白（確保對比度）。</summary>
        public static bool IsLightBackground(double? val, double lo, double hi)
        {
            var (r, g, b) = GetRgb(val, lo, hi);
            double lum = (r * 0.299 + g * 0.587 + b * 0.114) / 255.0;
            return lum > 0.5;
        }
    }
}
