#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;

namespace WaferHeatmap.Shared
{
    /// <summary>
    /// 計算晶圓格狀座標，不包含任何繪圖 API。
    /// WinForms 和 Blazor 都用這個類別計算佈局，再各自呼叫對應的繪圖 API。
    /// </summary>
    public static class WaferLayout
    {
        public const int COLS_PER_ROW = 5;
        public const int CELL_SIZE    = 7;
        public const int WAFER_MARGIN = 4;
        public const int LABEL_H      = 16;
        public const int PAD_X        = 8;
        public const int PAD_Y        = 8;
        public const int OUTER_PAD    = 30;
        public const int TITLE_H      = 28;

        // Explorer 模式（die 上顯示數值，需要更大格子）
        public const int EXPLORER_CELL   = 36;
        public const int EXPLORER_MARGIN = 6;
        public const int EXPLORER_LABEL  = 22;
        public const int EXPLORER_OUTER  = 10;

        public record GridInfo(
            int XMin, int XMax, int YMin, int YMax,
            int GridW, int GridH,
            int DieAreaW, int DieAreaH,
            int WaferPx,
            int ImgW, int ImgH,
            int TotalSlots
        );

        public record DieCell(
            int X, int Y,          // CSV 座標
            int PixelX, int PixelY,// 左上角像素位置
            bool InCircle          // 是否在圓形晶圓範圍內
        );

        /// <summary>計算整體圖片尺寸與格子資訊。</summary>
        public static GridInfo CalcGrid(List<DieRecord> records, bool explorerMode = false)
        {
            int cell   = explorerMode ? EXPLORER_CELL   : CELL_SIZE;
            int margin = explorerMode ? EXPLORER_MARGIN : WAFER_MARGIN;
            int outer  = explorerMode ? EXPLORER_OUTER  : OUTER_PAD;
            int titleH = explorerMode ? EXPLORER_LABEL  : TITLE_H;
            int padX   = explorerMode ? 16              : PAD_X;
            int padY   = explorerMode ? 16              : PAD_Y;
            int cols   = explorerMode ? 3               : COLS_PER_ROW;

            int xMin = records.Min(r => r.X), xMax = records.Max(r => r.X);
            int yMin = records.Min(r => r.Y), yMax = records.Max(r => r.Y);
            int gW = xMax - xMin + 1, gH = yMax - yMin + 1;
            int dW = gW * cell, dH = gH * cell;
            int waferPx = Math.Max(dW, dH) + 2 * margin;

            int maxWid = records.Max(r => r.WaferId);
            int totalSlots = explorerMode
                ? records.Select(r => r.WaferId).Distinct().Count()
                : maxWid;
            int rows = (int)Math.Ceiling(totalSlots / (double)cols);

            int imgW = outer * 2 + cols * waferPx + (cols - 1) * padX;
            int imgH = outer * 2 + titleH + rows * (LABEL_H + waferPx) + (rows - 1) * padY;

            return new GridInfo(xMin, xMax, yMin, yMax, gW, gH, dW, dH, waferPx, imgW, imgH, totalSlots);
        }

        /// <summary>
        /// 計算單片晶圓內每個 die 的像素位置與是否在圓形範圍內。
        /// waferOriginX/Y 是這片晶圓圓形左上角的像素座標。
        /// </summary>
        public static List<DieCell> CalcDieCells(
            GridInfo grid, int waferOriginX, int waferOriginY,
            bool explorerMode = false)
        {
            int cell   = explorerMode ? EXPLORER_CELL   : CELL_SIZE;
            int margin = explorerMode ? EXPLORER_MARGIN : WAFER_MARGIN;

            int cx = waferOriginX + grid.WaferPx / 2;
            int cy = waferOriginY + grid.WaferPx / 2;
            int radius = grid.WaferPx / 2;

            int dieOffX = waferOriginX + margin +
                          (grid.WaferPx - 2 * margin - grid.DieAreaW) / 2;
            int dieOffY = waferOriginY + margin +
                          (grid.WaferPx - 2 * margin - grid.DieAreaH) / 2;

            var cells = new List<DieCell>();
            for (int gy = 0; gy < grid.GridH; gy++)
            {
                for (int gx = 0; gx < grid.GridW; gx++)
                {
                    int px = dieOffX + gx * cell;
                    int py = dieOffY + gy * cell;
                    int ccx = px + cell / 2 - cx;
                    int ccy = py + cell / 2 - cy;
                    bool inCircle = ccx * ccx + ccy * ccy <=
                                    (radius - margin / 2.0) * (radius - margin / 2.0);
                    cells.Add(new DieCell(
                        grid.XMin + gx, grid.YMin + gy,
                        px, py, inCircle));
                }
            }
            return cells;
        }

        /// <summary>格式化數值顯示（Explorer die 上的讀值）。</summary>
        public static string FormatValue(double v)
        {
            double a = Math.Abs(v);
            if (a >= 1000) return v.ToString("F0");
            if (a >= 100)  return v.ToString("F1");
            if (a >= 10)   return v.ToString("F2");
            if (a >= 1)    return v.ToString("F3");
            return v.ToString("G3");
        }
    }
}
