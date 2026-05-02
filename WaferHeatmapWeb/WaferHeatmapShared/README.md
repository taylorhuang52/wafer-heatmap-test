# WaferHeatmap 拆檔說明

## 架構

原本的 WaferHeatmap.cs（單一檔案）拆成：

```
WaferHeatmap.Shared/          ← 跨平台共用邏輯（不含 Windows API）
  ├── DieRecord.cs            ← 資料模型
  ├── CsvLoader.cs            ← CSV 解析
  ├── ColorScale.cs           ← 顏色計算（回傳 RGB tuple）
  ├── WaferLayout.cs          ← 佈局計算（像素位置、圓形遮罩）
  └── WaferHeatmapShared.csproj

WaferHeatmap/                 ← WinForms 專案（原本的）
  ├── WaferHeatmap.cs         ← MainForm + GDI+ 渲染
  └── WaferHeatmap.csproj     ← 引用 WaferHeatmapShared

WaferHeatmapWeb/              ← Blazor WASM 專案（新建）
  ├── Pages/
  │   ├── IR0Heatmap.razor    ← 各分頁元件
  │   ├── VF0Heatmap.razor
  │   └── Explorer.razor
  ├── Components/
  │   └── WaferCanvas.razor   ← SVG 或 SkiaSharp 繪製晶圓
  └── WaferHeatmapWeb.csproj  ← 引用 WaferHeatmapShared
```

## 各檔案責任

| 檔案 | 可用於 | 說明 |
|------|--------|------|
| DieRecord.cs | WinForms ✅ Blazor ✅ | 純資料，無任何 UI 相依 |
| CsvLoader.cs | WinForms ✅ Blazor ✅ | Blazor 請用 LoadFromText() |
| ColorScale.cs | WinForms ✅ Blazor ✅ | 回傳 (R,G,B) tuple 或 CSS 字串 |
| WaferLayout.cs | WinForms ✅ Blazor ✅ | 計算像素位置，不畫圖 |

## Blazor 中如何使用 ColorScale

```csharp
// 取得 CSS 顏色字串
string bgColor = ColorScale.GetCss(rec.IR0, 0, 45);

// 決定文字顏色（確保對比度）
string textColor = ColorScale.IsLightBackground(rec.IR0, 0, 45)
    ? "black" : "white";
```

```html
<div style="background:@bgColor; color:@textColor">
    @rec.IR0?.ToString("F2")
</div>
```

## WinForms 更新 .csproj（加入引用）

```xml
<ItemGroup>
  <ProjectReference Include="..\WaferHeatmapShared\WaferHeatmapShared.csproj" />
</ItemGroup>
```

## 發布獨立執行檔

```bash
dotnet publish WaferHeatmap/WaferHeatmap.csproj -c Release
```

## 發布 Blazor 靜態網頁

```bash
dotnet publish WaferHeatmapWeb/WaferHeatmapWeb.csproj -c Release
```
