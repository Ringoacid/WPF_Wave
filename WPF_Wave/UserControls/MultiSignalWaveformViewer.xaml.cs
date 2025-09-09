using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using WPF_Wave.Helpers;
using WPF_Wave.Models;

namespace WPF_Wave.UserControls;

/// <summary>
/// 複数信号の波形を縦に並べて同期表示するビューア
/// - 水平ズーム（Ctrl+ホイール）
/// - 水平スクロール同期
/// - 左側のラベルをドラッグして並べ替え（DragableList のロジックを簡略移植）
/// - SignalWaveformItem のレンダリングロジックを参考に、1枚のビットマップへ複数行を描画
/// </summary>
public partial class MultiSignalWaveformViewer : UserControl
{
    public MultiSignalWaveformViewer()
    {
        InitializeComponent();
        Loaded += (_, __) => { _dpi = VisualTreeHelper.GetDpi(this); RenderWaveforms(); };
    }

    // 依存関係プロパティ
    public static readonly DependencyProperty VcdDataProperty =
        DependencyProperty.Register(nameof(VcdData), typeof(Vcd), typeof(MultiSignalWaveformViewer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, NeedRenderCallback));

    public Vcd? VcdData
    {
        get => (Vcd?)GetValue(VcdDataProperty);
        set => SetValue(VcdDataProperty, value);
    }

    public static readonly DependencyProperty SignalsProperty =
        DependencyProperty.Register(nameof(Signals), typeof(ObservableCollection<VariableDisplayItem>), typeof(MultiSignalWaveformViewer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, SignalsChangedCallback));

    public ObservableCollection<VariableDisplayItem> Signals
    {
        get => (ObservableCollection<VariableDisplayItem>)GetValue(SignalsProperty);
        set => SetValue(SignalsProperty, value);
    }

    public static readonly DependencyProperty SingleWaveHeightProperty =
        DependencyProperty.Register(nameof(SingleWaveHeight), typeof(double), typeof(MultiSignalWaveformViewer),
            new FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.AffectsRender, NeedRenderCallback));

    public double SingleWaveHeight
    {
        get => (double)GetValue(SingleWaveHeightProperty);
        set => SetValue(SingleWaveHeightProperty, value);
    }

    public static readonly DependencyProperty WaveMarginProperty =
        DependencyProperty.Register(nameof(WaveMargin), typeof(double), typeof(MultiSignalWaveformViewer),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsRender, NeedRenderCallback));

    public double WaveMargin
    {
        get => (double)GetValue(WaveMarginProperty);
        set => SetValue(WaveMarginProperty, value);
    }

    public static readonly DependencyProperty CrossWidthProperty =
        DependencyProperty.Register(nameof(CrossWidth), typeof(double), typeof(MultiSignalWaveformViewer),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsRender, NeedRenderCallback));

    public double CrossWidth
    {
        get => (double)GetValue(CrossWidthProperty);
        set => SetValue(CrossWidthProperty, value);
    }

    public static readonly DependencyProperty MinMagnificationProperty =
        DependencyProperty.Register(nameof(MinMagnification), typeof(double), typeof(MultiSignalWaveformViewer),
            new FrameworkPropertyMetadata(0.1, OnMinMaxMagnificationChanged));

    public double MinMagnification
    {
        get => (double)GetValue(MinMagnificationProperty);
        set => SetValue(MinMagnificationProperty, value);
    }

    public static readonly DependencyProperty MaxMagnificationProperty =
        DependencyProperty.Register(nameof(MaxMagnification), typeof(double), typeof(MultiSignalWaveformViewer),
            new FrameworkPropertyMetadata(5000.0, OnMinMaxMagnificationChanged));

    public double MaxMagnification
    {
        get => (double)GetValue(MaxMagnificationProperty);
        set => SetValue(MaxMagnificationProperty, value);
    }

    public static readonly DependencyProperty MagnificationProperty =
        DependencyProperty.Register(nameof(Magnification), typeof(double), typeof(MultiSignalWaveformViewer),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, MagnificationChangedCallback));

    public double Magnification
    {
        get => (double)GetValue(MagnificationProperty);
        set => SetValue(MagnificationProperty, value);
    }

    // ===== 時間軸の見た目制御用 依存関係プロパティ =====
    public static readonly DependencyProperty TimeAxisTickBrushProperty =
        DependencyProperty.Register(nameof(TimeAxisTickBrush), typeof(Brush), typeof(MultiSignalWaveformViewer),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, OnTimeAxisAppearanceChanged));

    public Brush TimeAxisTickBrush
    {
        get => (Brush)GetValue(TimeAxisTickBrushProperty);
        set => SetValue(TimeAxisTickBrushProperty, value);
    }

    public static readonly DependencyProperty TimeAxisTextBrushProperty =
        DependencyProperty.Register(nameof(TimeAxisTextBrush), typeof(Brush), typeof(MultiSignalWaveformViewer),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, OnTimeAxisAppearanceChanged));

    public Brush TimeAxisTextBrush
    {
        get => (Brush)GetValue(TimeAxisTextBrushProperty);
        set => SetValue(TimeAxisTextBrushProperty, value);
    }

    public static readonly DependencyProperty TimeAxisBackgroundProperty =
        DependencyProperty.Register(nameof(TimeAxisBackground), typeof(Brush), typeof(MultiSignalWaveformViewer),
            new FrameworkPropertyMetadata(Brushes.Transparent, FrameworkPropertyMetadataOptions.AffectsRender, OnTimeAxisAppearanceChanged));

    public Brush TimeAxisBackground
    {
        get => (Brush)GetValue(TimeAxisBackgroundProperty);
        set => SetValue(TimeAxisBackgroundProperty, value);
    }

    private static void OnTimeAxisAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MultiSignalWaveformViewer v)
        {
            v.RenderTimeAxis();
        }
    }

    // ラベル(Border)および文字色設定用 依存関係プロパティ
    public static readonly DependencyProperty LabelBackgroundProperty =
        DependencyProperty.Register(nameof(LabelBackground), typeof(Brush), typeof(MultiSignalWaveformViewer),
            new FrameworkPropertyMetadata(Brushes.LightGray, FrameworkPropertyMetadataOptions.AffectsRender, OnLabelAppearanceChanged));

    public Brush LabelBackground
    {
        get => (Brush)GetValue(LabelBackgroundProperty);
        set => SetValue(LabelBackgroundProperty, value);
    }

    public static readonly DependencyProperty LabelBorderBrushProperty =
        DependencyProperty.Register(nameof(LabelBorderBrush), typeof(Brush), typeof(MultiSignalWaveformViewer),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, OnLabelAppearanceChanged));

    public Brush LabelBorderBrush
    {
        get => (Brush)GetValue(LabelBorderBrushProperty);
        set => SetValue(LabelBorderBrushProperty, value);
    }

    public static readonly DependencyProperty LabelNameForegroundProperty =
        DependencyProperty.Register(nameof(LabelNameForeground), typeof(Brush), typeof(MultiSignalWaveformViewer),
            new FrameworkPropertyMetadata(Brushes.Black, FrameworkPropertyMetadataOptions.AffectsRender, OnLabelAppearanceChanged));

    public Brush LabelNameForeground
    {
        get => (Brush)GetValue(LabelNameForegroundProperty);
        set => SetValue(LabelNameForegroundProperty, value);
    }

    public static readonly DependencyProperty LabelDescriptionForegroundProperty =
        DependencyProperty.Register(nameof(LabelDescriptionForeground), typeof(Brush), typeof(MultiSignalWaveformViewer),
            new FrameworkPropertyMetadata(Brushes.DimGray, FrameworkPropertyMetadataOptions.AffectsRender, OnLabelAppearanceChanged));

    public Brush LabelDescriptionForeground
    {
        get => (Brush)GetValue(LabelDescriptionForegroundProperty);
        set => SetValue(LabelDescriptionForegroundProperty, value);
    }

    private static void OnLabelAppearanceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MultiSignalWaveformViewer v)
        {
            v.BuildLeftLabels();
        }
    }

    private static void OnMinMaxMagnificationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is MultiSignalWaveformViewer v)
        {
            v.CoerceValue(MagnificationProperty);
        }
    }

    private static void MagnificationChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MultiSignalWaveformViewer v) return;
        if (e.NewValue is not double newVal) return;
        if (double.IsNaN(newVal) || double.IsInfinity(newVal)) return;
        if (newVal < v.MinMagnification || newVal > v.MaxMagnification)
        {
            v.Magnification = Math.Clamp(newVal, v.MinMagnification, v.MaxMagnification);
            return;
        }

        // 幅は倍率に応じて更新（基準幅=100）
        v.WaveDrawGrid.Width = 100d * newVal;
        v.RenderWaveforms();
    }

    // 内部状態
    private DpiScale? _dpi;
    private List<Border> _labelBorders = new();
    private Border? _dragged;
    private bool _isDragging;
    private int _lastIndex = -1;

    // 左ラベルの見た目（DragableList 風）
    public double LabelWidth { get; private set; } = 250;
    public double LabelHeight { get; set; } = 50;
    public double LabelMargin { get; set; } = 5;
    public CornerRadius LabelCornerRadius { get; set; } = new(6);

    private static void NeedRenderCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MultiSignalWaveformViewer v) return;
        v.BuildLeftLabels();
        v.RenderWaveforms();
    }

    private static void SignalsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not MultiSignalWaveformViewer v) return;

        if (e.OldValue is ObservableCollection<VariableDisplayItem> oldCol)
        {
            oldCol.CollectionChanged -= v.OnSignalsCollectionChanged;
            foreach (var it in oldCol) it.PropertyChanged -= v.OnSignalPropertyChanged;
        }
        if (e.NewValue is ObservableCollection<VariableDisplayItem> newCol)
        {
            newCol.CollectionChanged += v.OnSignalsCollectionChanged;
            foreach (var it in newCol) it.PropertyChanged += v.OnSignalPropertyChanged;
        }

        v.BuildLeftLabels();
        v.RenderWaveforms();
    }

    private void OnSignalsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (e.OldItems != null)
        {
            foreach (VariableDisplayItem it in e.OldItems)
            {
                it.PropertyChanged -= OnSignalPropertyChanged;
            }
        }
        if (e.NewItems != null)
        {
            foreach (VariableDisplayItem it in e.NewItems)
            {
                it.PropertyChanged += OnSignalPropertyChanged;
            }
        }
        BuildLeftLabels();
        RenderWaveforms();
    }

    private void OnSignalPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(VariableDisplayItem.Name) or nameof(VariableDisplayItem.Type) or nameof(VariableDisplayItem.BitWidth) or nameof(VariableDisplayItem.VariableData))
        {
            BuildLeftLabels();
            RenderWaveforms();
        }
        else if (e.PropertyName == nameof(VariableDisplayItem.DisplayFormat))
        {
            // 表示フォーマット変更時は波形の値表示のみ更新
            RenderWaveforms();
        }
    }

    private double TotalImageHeight()
    {
        if (Signals == null || Signals.Count == 0) return SingleWaveHeight;
        // 行間含めた高さ: N*(H+M) - M
        return Signals.Count * (SingleWaveHeight + WaveMargin) - WaveMargin;
    }

    private WriteableBitmap CreateWriteableBitmap(double width, double height)
    {
        // width/height: DIP (logical units). Allocate pixel buffer using real DPI so that the Image shows the expected DIP size.
        double scaleX = _dpi?.DpiScaleX ?? 1.0;
        double scaleY = _dpi?.DpiScaleY ?? 1.0;
        int pixelWidth = (int)System.Math.Ceiling(width * scaleX);
        int pixelHeight = (int)System.Math.Ceiling(height * scaleY);
        double dpiX = (_dpi?.PixelsPerInchX) ?? 96.0;
        double dpiY = (_dpi?.PixelsPerInchY) ?? 96.0;
        return new WriteableBitmap(pixelWidth, pixelHeight, dpiX, dpiY, PixelFormats.Bgr32, null);
    }

    private void CreateAndSetWriteableBitmap()
    {
        double viewportWidth = WaveScrollViewer.ViewportWidth > 0 ? WaveScrollViewer.ViewportWidth : WaveDrawGrid.ActualWidth;
        if (viewportWidth <= 0) viewportWidth = WaveDrawGrid.Width;
        if (viewportWidth <= 0) viewportWidth = 1;

        double imageHeight = TotalImageHeight();
        if (imageHeight <= 0) imageHeight = SingleWaveHeight;

        WaveFromImage.Source = WaveFromImageSource = CreateWriteableBitmap(viewportWidth, imageHeight);

        // スクロールに合わせて左マージン調整（DIP）
        var left = WaveScrollViewer.HorizontalOffset;
        WaveFromImage.Margin = new Thickness(left, 0, 0, 0);
        SignalValueDrawer.Margin = new Thickness(left, 0, 0, 0);

        // 時間軸キャンバスの幅を波形描画グリッドに合わせる
        if (TimeAxisCanvas != null)
        {
            TimeAxisCanvas.Width = WaveDrawGrid.Width;
        }
    }

    private WriteableBitmap? _waveFromImageSource;
    public WriteableBitmap WaveFromImageSource
    {
        get => _waveFromImageSource!;
        private set => _waveFromImageSource = value;
    }

    private const double MinValueDisplayWidth = 50.0;

    private bool TryIntersect(double a0, double a1, double b0, double b1, out double i0, double bEnd)
    {
        double i1;
        return TryIntersect(a0, a1, b0, bEnd, out i0, out i1);
    }
    private bool TryIntersect(double a0, double a1, double b0, double b1, out double i0, out double i1)
    {
        i0 = System.Math.Max(a0, b0);
        i1 = System.Math.Min(a1, b1);
        return i1 > i0;
    }

    // Helper: DIP -> pixel conversion
    private int DipToPixelX(double x) => (int)System.Math.Round(x * (_dpi?.DpiScaleX ?? 1.0));
    private int DipToPixelY(double y) => (int)System.Math.Round(y * (_dpi?.DpiScaleY ?? 1.0));
    private IntVector2d ToPixel(double xDip, double yDip) => new IntVector2d(DipToPixelX(xDip), DipToPixelY(yDip));

    private void RenderOneBitWaveFragment(double waveHeight, double contentWidth, double viewportLeft, double viewportWidth, double topMargin,
        long startTime, long endTime, VariableValue beginVal, VariableValue endVal)
    {
        if (VcdData is null) return;
        if (contentWidth <= 0 || viewportWidth <= 0) return;

        bool isBeginHigh = beginVal[0] == VariableValue.BitType.One;
        bool isEndHigh = endVal[0] == VariableValue.BitType.One;

        long totalTime = System.Math.Max(1, VcdData.SimulationTime);
        double startXGlobal = startTime * contentWidth / totalTime; // DIP
        double endXGlobal = endTime * contentWidth / totalTime;     // DIP
        if (!TryIntersect(startXGlobal, endXGlobal, viewportLeft, viewportLeft + viewportWidth, out var visStart, out var visEnd))
        {
            return;
        }

        double yBeginDip = topMargin + (isBeginHigh ? 0 : waveHeight);
        double yEndDip = topMargin + (isEndHigh ? 0 : waveHeight);

        // ローカル座標(ビューポート左を原点)に変換（DIP）
        double startLocalXDip = visStart - viewportLeft;
        double endLocalXDip = visEnd - viewportLeft;

        var startPoint = ToPixel(startLocalXDip, yBeginDip);
        var middlePoint = ToPixel(endLocalXDip, yBeginDip);

        var color = beginVal.IsUndefined ? Colors.Red : beginVal.IsHighImpedance ? Colors.Brown : Colors.Blue;
        WaveFromImageSource.DrawLine(startPoint, middlePoint, color);

        if (endXGlobal >= viewportLeft && endXGlobal <= viewportLeft + viewportWidth)
        {
            var endPoint = ToPixel(endLocalXDip, yEndDip);
            WaveFromImageSource.DrawLine(middlePoint, endPoint, color);
        }
    }

    private void RenderMultiBitWaveFragment(double waveHeight, double contentWidth, double viewportLeft, double viewportWidth, double topMargin,
        long startTime, long endTime, VariableValue value, VariableDisplayItem item, double waveTopMargin)
    {
        if (VcdData is null) return;
        if (contentWidth <= 0 || viewportWidth <= 0) return;

        var color = value.IsUndefined ? Colors.Red : value.IsHighImpedance ? Colors.Brown : Colors.Blue;

        long totalTime = System.Math.Max(1, VcdData.SimulationTime);
        double startXGlobal = startTime * contentWidth / totalTime; // DIP
        double endXGlobal = endTime * contentWidth / totalTime;     // DIP
        if (!TryIntersect(startXGlobal, endXGlobal, viewportLeft, viewportLeft + viewportWidth, out var visStart, out var visEnd))
        {
            return;
        }

        // ローカル座標(ビューポート左を原点)に変換（DIP）
        double startLocalXDip = visStart - viewportLeft;
        double endLocalXDip = visEnd - viewportLeft;
        double durationXDip = endLocalXDip - startLocalXDip;

        double yCenterDip = topMargin + (waveHeight / 2);
        double yTopDip = topMargin;
        double yBottomDip = topMargin + waveHeight;

        var startPoint = ToPixel(startLocalXDip, yCenterDip);
        var endPoint = ToPixel(endLocalXDip, yCenterDip);

        if (durationXDip < CrossWidth * 2)
        {
            double middleLocalXDip = startLocalXDip + durationXDip / 2;
            var middleTopPoint = ToPixel(middleLocalXDip, yTopDip);
            var middleBottomPoint = ToPixel(middleLocalXDip, yBottomDip);

            WaveFromImageSource.DrawLine(startPoint, middleTopPoint, color);
            WaveFromImageSource.DrawLine(startPoint, middleBottomPoint, color);
            WaveFromImageSource.DrawLine(middleTopPoint, endPoint, color);
            WaveFromImageSource.DrawLine(middleBottomPoint, endPoint, color);
        }
        else
        {
            var middleTopLeftPoint = ToPixel(startLocalXDip + CrossWidth, yTopDip);
            var middleTopRightPoint = ToPixel(endLocalXDip - CrossWidth, yTopDip);
            var middleBottomLeftPoint = ToPixel(startLocalXDip + CrossWidth, yBottomDip);
            var middleBottomRightPoint = ToPixel(endLocalXDip - CrossWidth, yBottomDip);

            WaveFromImageSource.DrawLine(startPoint, middleTopLeftPoint, color);
            WaveFromImageSource.DrawLine(startPoint, middleBottomLeftPoint, color);
            WaveFromImageSource.DrawLine(middleTopLeftPoint, middleTopRightPoint, color);
            WaveFromImageSource.DrawLine(middleBottomLeftPoint, middleBottomRightPoint, color);
            WaveFromImageSource.DrawLine(middleTopRightPoint, endPoint, color);
            WaveFromImageSource.DrawLine(middleBottomRightPoint, endPoint, color);
        }

        if (durationXDip >= MinValueDisplayWidth)
        {
            // 文字やCanvasはDIP座標で配置する
            AddSignalValueTextBlock(value, startLocalXDip, endLocalXDip, waveTopMargin, item);
        }
    }

    private void AddSignalValueTextBlock(VariableValue value, double startX, double endX, double waveTopMargin, VariableDisplayItem item)
    {
        string text = value.ToString(item.DisplayFormat);
        double fontSize = System.Math.Min(12, SingleWaveHeight / 4);
        var typeface = new Typeface(new FontFamily("Consolas"), FontStyles.Normal, FontWeights.Normal, FontStretches.Normal);
        double pixelsPerDip = VisualTreeHelper.GetDpi(this).PixelsPerDip;
        var brush = value.IsUndefined ? Brushes.Red : value.IsHighImpedance ? Brushes.Brown : Brushes.Black;

        var formatted = new FormattedText(text, CultureInfo.CurrentUICulture, FlowDirection.LeftToRight, typeface, fontSize, brush, pixelsPerDip);

        const double padding = 2.0;
        double textWidth = formatted.WidthIncludingTrailingWhitespace + padding * 2;
        double textHeight = formatted.Height + padding * 2;

        double innerStartX = startX + CrossWidth;
        double innerEndX = endX - CrossWidth;
        double availableWidth = System.Math.Max(0, innerEndX - innerStartX);
        if (textWidth > availableWidth) return;

        var tb = new TextBlock
        {
            Text = text,
            FontSize = fontSize,
            Foreground = brush,
            Background = Brushes.White,
            Padding = new Thickness(padding),
            FontFamily = new FontFamily("Consolas")
        };

        double left = innerStartX + (availableWidth - textWidth) / 2.0;
        double top = waveTopMargin + (SingleWaveHeight - textHeight) / 2.0;

        Canvas.SetLeft(tb, left);
        Canvas.SetTop(tb, top);
        SignalValueDrawer.Children.Add(tb);
    }

    private void RenderSingleWaveform(VariableDisplayItem item, double topMargin)
    {
        if (VcdData is null) return;

        double waveHeight = SingleWaveHeight - 2 * WaveMargin;
        if (waveHeight <= 0) return;

        double contentWidth = WaveDrawGrid.ActualWidth > 0 ? WaveDrawGrid.ActualWidth : WaveDrawGrid.Width; // DIP
        double viewportLeft = WaveScrollViewer.HorizontalOffset;                                          // DIP
        double viewportWidth = WaveScrollViewer.ViewportWidth > 0 ? WaveScrollViewer.ViewportWidth : contentWidth; // DIP

        var timeVal = VcdData.GetTimeValuePairs(item.VariableData.Id);
        if (timeVal.Count <= 0) return;
        bool isOneBit = item.VariableData.BitWidth == 1;

        long beginTime, endTime;
        VariableValue beginValue, endValue;
        for (int i = 0; i < timeVal.Count - 1; i++)
        {
            beginTime = timeVal[i].Time;
            endTime = timeVal[i + 1].Time;
            beginValue = timeVal[i].Value;
            endValue = timeVal[i + 1].Value;

            if (isOneBit)
            {
                RenderOneBitWaveFragment(waveHeight, contentWidth, viewportLeft, viewportWidth, topMargin + WaveMargin, beginTime, endTime, beginValue, endValue);
            }
            else
            {
                RenderMultiBitWaveFragment(waveHeight, contentWidth, viewportLeft, viewportWidth, topMargin + WaveMargin, beginTime, endTime, beginValue, item, topMargin);
            }
        }

        beginTime = timeVal[^1].Time;
        endTime = VcdData.SimulationTime;
        beginValue = timeVal[^1].Value;
        endValue = new(timeVal[^1].Value);

        if (isOneBit)
        {
            RenderOneBitWaveFragment(waveHeight, contentWidth, viewportLeft, viewportWidth, topMargin + WaveMargin, beginTime, endTime, beginValue, endValue);
        }
        else
        {
            RenderMultiBitWaveFragment(waveHeight, contentWidth, viewportLeft, viewportWidth, topMargin + WaveMargin, beginTime, endTime, beginValue, item, topMargin);
        }
    }

    public void RenderWaveforms()
    {
        if (VcdData is null) return;
        if (Signals is null || Signals.Count == 0) { RenderTimeAxis(); return; }

        CreateAndSetWriteableBitmap();

        SignalValueDrawer.Children.Clear();

        double top = 0;
        foreach (var sig in Signals)
        {
            RenderSingleWaveform(sig, top);
            top += SingleWaveHeight + WaveMargin;
        }
        RenderTimeAxis();
    }

    private void RenderTimeAxis()
    {
        if (TimeAxisCanvas == null)
            return;
        TimeAxisCanvas.Children.Clear();
        if (VcdData is null)
            return;

        // 背景色の適用
        TimeAxisCanvas.Background = TimeAxisBackground;

        long totalTime = Math.Max(1, VcdData.SimulationTime);
        double contentWidth = WaveDrawGrid.ActualWidth > 0 ? WaveDrawGrid.ActualWidth : WaveDrawGrid.Width;
        if (contentWidth <= 0)
            return;

        double viewportLeft = WaveScrollViewer.HorizontalOffset;
        double viewportWidth = WaveScrollViewer.ViewportWidth > 0 ? WaveScrollViewer.ViewportWidth : contentWidth;

        double pixelsPerTime = contentWidth / totalTime; // px (DIP) / time-unit
        if (pixelsPerTime <= 0 || double.IsInfinity(pixelsPerTime)) return;

        // 目標間隔(px)
        const double targetPixelSpacing = 100.0;
        double rawTimeStep = targetPixelSpacing / pixelsPerTime; // time units
        long niceTimeStep = (long)FindNiceStep(rawTimeStep);
        if (niceTimeStep <= 0) niceTimeStep = 1;

        // 可視領域の開始/終了時刻
        long startTimeVisible = (long)Math.Floor(viewportLeft / pixelsPerTime);
        long endTimeVisible = (long)Math.Ceiling((viewportLeft + viewportWidth) / pixelsPerTime);
        if (endTimeVisible > totalTime) endTimeVisible = totalTime;

        // 最初の目盛り（startTimeVisible 以上の最初の niceTimeStep の倍数）
        long firstTick = ((startTimeVisible + niceTimeStep - 1) / niceTimeStep) * niceTimeStep;

        const double tickHeight = 10.0;
        var tickBrush = TimeAxisTickBrush;
        var labelBrush = TimeAxisTextBrush;

        for (long t = firstTick; t <= endTimeVisible; t += niceTimeStep)
        {
            double xGlobal = t * pixelsPerTime; // コンテンツ全体のX (DIP)
            double xLocal = xGlobal - viewportLeft; // ビューポート左端を原点とするX (DIP)

            // ラベルテキスト
            string label = FormatTime(t, VcdData);
            var tb = new TextBlock
            {
                Text = label,
                FontSize = 12,
                Foreground = labelBrush,
                Background = Brushes.Transparent
            };
            TimeAxisCanvas.Children.Add(tb);
            tb.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double textWidth = tb.DesiredSize.Width;
            double textHeight = tb.DesiredSize.Height;
            Canvas.SetLeft(tb, xLocal - textWidth / 2);
            Canvas.SetTop(tb, 0);

            // 目盛り線（テキスト下に表示）
            var line = new Line
            {
                X1 = xLocal,
                X2 = xLocal,
                Y1 = textHeight + 2,
                Y2 = textHeight + 2 + tickHeight,
                Stroke = tickBrush,
                StrokeThickness = 1
            };
            TimeAxisCanvas.Children.Add(line);
        }
    }

    private static double FindNiceStep(double raw)
    {
        if (raw <= 0) return 1;
        double exp = Math.Floor(Math.Log10(raw));
        double baseVal = Math.Pow(10, exp);
        double mant = raw / baseVal;
        double niceMant;
        if (mant <= 1) niceMant = 1;
        else if (mant <= 2) niceMant = 2;
        else if (mant <= 5) niceMant = 5;
        else niceMant = 10;
        return niceMant * baseVal;
    }

    // 時間値(timeValue [ps])を適切な単位(s, ms, us, ns, ps)へ変換して文字列化
    private string FormatTime(long timeValue, Vcd vcd)
    {
        if (timeValue <= 0) return "0ps";

        const double psPerS = 1_000_000_000_000d;
        const double psPerMs = 1_000_000_000d;
        const double psPerUs = 1_000_000d;
        const double psPerNs = 1_000d;

        double value;
        string unit;
        if (timeValue >= psPerS)
        {
            value = timeValue / psPerS; unit = "s";
        }
        else if (timeValue >= psPerMs)
        {
            value = timeValue / psPerMs; unit = "ms";
        }
        else if (timeValue >= psPerUs)
        {
            value = timeValue / psPerUs; unit = "us";
        }
        else if (timeValue >= psPerNs)
        {
            value = timeValue / psPerNs; unit = "ns";
        }
        else
        {
            value = timeValue; unit = "ps";
        }

        string fmt = value < 1 ? "0.###"
                    : value < 10 ? "0.###"
                    : value < 100 ? "0.##"
                    : value < 1000 ? "0.#"
                    : "0";

        return value.ToString(fmt, CultureInfo.InvariantCulture) + unit;
    }

    private void WaveScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        RenderWaveforms();
    }

    private void WaveScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        RenderWaveforms();
    }

    private void BuildLeftLabels()
    {
        LeftCanvas.Children.Clear();
        _labelBorders.Clear();

        if (Signals == null || Signals.Count == 0)
        {
            return;
        }

        double totalHeight = 0;
        double maxWidth = 0; // コンテンツに基づく最大幅
        for (int i = 0; i < Signals.Count; i++)
        {
            var sig = Signals[i];
            var border = new Border
            {
                // Width は自動計測させるため指定しない
                Height = LabelHeight,
                Background = LabelBackground,
                BorderBrush = LabelBorderBrush,
                CornerRadius = LabelCornerRadius,
                Cursor = Cursors.Hand,
                Tag = i,
                Padding = new Thickness(8)
            };
            border.MouseLeftButtonDown += Label_MouseLeftButtonDown;

            // コンテキストメニュー（右クリック）
            var cm = new ContextMenu();

            // 表示フォーマット変更
            var changeFormat = new MenuItem { Header = "表示フォーマットの変更" };
            
            cm.Items.Add(changeFormat);
            // Binary
            var fmtBin = new MenuItem { Header = "2進数 (Binary)", IsCheckable = true, IsChecked = sig.DisplayFormat == StringFormat.Binary, Tag = (changeFormat, sig, StringFormat.Binary) };
            fmtBin.Click += ChangeFormatMenuItem_Click;
            changeFormat.Items.Add(fmtBin);
            // Decimal
            var fmtDec = new MenuItem { Header = "10進数 (Decimal)", IsCheckable = true, IsChecked = sig.DisplayFormat == StringFormat.Decimal, Tag = (changeFormat, sig, StringFormat.Decimal) };
            fmtDec.Click += ChangeFormatMenuItem_Click;
            changeFormat.Items.Add(fmtDec);
            // Hex
            var fmtHex = new MenuItem { Header = "16進数 (Hex)", IsCheckable = true, IsChecked = sig.DisplayFormat == StringFormat.Hexadecimal, Tag = (changeFormat, sig, StringFormat.Hexadecimal) };
            fmtHex.Click += ChangeFormatMenuItem_Click;
            changeFormat.Items.Add(fmtHex);

            // 削除
            var deleteItem = new MenuItem { Header = "削除", Tag = sig };
            deleteItem.Click += DeleteSignalMenuItem_Click;
            cm.Items.Add(deleteItem);

            border.ContextMenu = cm;

            var sp = new StackPanel { Orientation = Orientation.Vertical };
            sp.Children.Add(new TextBlock { Text = sig.Name, FontWeight = FontWeights.Bold, Foreground = LabelNameForeground });
            sp.Children.Add(new TextBlock { Text = sig.DescriptionText, FontSize = 10, Foreground = LabelDescriptionForeground });
            border.Child = sp;

            // 先に Children に追加してから計測
            LeftCanvas.Children.Add(border);
            border.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double desiredWidth = border.DesiredSize.Width;
            if (desiredWidth > maxWidth) maxWidth = desiredWidth;

            double top = i * (LabelHeight + LabelMargin);
            Canvas.SetTop(border, top);
            _labelBorders.Add(border);
            totalHeight = top + LabelHeight;
        }

        // 全てのBorderの幅を最大幅に揃える
        if (maxWidth <= 0) maxWidth = 50; // フォールバック
        foreach (var b in _labelBorders)
        {
            b.Width = maxWidth;
        }

        // 読み取り用に保持（外部参照用）
        LabelWidth = maxWidth;

        LeftCanvas.Width = maxWidth;
        LeftCanvas.Height = totalHeight;

        // 左右の高さ同期（簡易）
        var totalWavesHeight = TotalImageHeight();
        var desiredHeight = System.Math.Max(totalWavesHeight, totalHeight);
        LeftScrollViewer.Height = desiredHeight;
    }

    private void ChangeFormatMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem mi) return;
        if (mi.Tag is not ValueTuple<MenuItem, VariableDisplayItem, StringFormat> tuple)
        {
            return;
        }

        var (menuitem, item, format) = tuple;
        if (item.DisplayFormat == format)
        {
            return;
        }

        item.DisplayFormat = format;
        RenderWaveforms();

        StringFormat activeFormat = format;
        foreach (var formatmenu in menuitem.Items)
        {
            if (formatmenu is not MenuItem fmtmi) continue;

            if (fmtmi.Tag is not ValueTuple<MenuItem, VariableDisplayItem, StringFormat> fmtTuple)
            {
                continue;
            }

            (menuitem, item, format) = fmtTuple;

            if(format == activeFormat)
            {
                fmtmi.IsChecked = true;
            }
            else
            {
                fmtmi.IsChecked = false;
            }
        }
    }

    // コンテキストメニュー: 削除
    private void DeleteSignalMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem mi) return;
        if (mi.Tag is not VariableDisplayItem item) return;
        if (Signals == null) return;

        // 直接削除（CollectionChangedによりUIは再構築される）
        Signals.Remove(item);
    }

    // 並べ替え（ドラッグ&ドロップ）簡易実装
    private void Label_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border b) return;
        b.CaptureMouse();
        _isDragging = true;
        _dragged = b;
        _lastIndex = _labelBorders.IndexOf(b);
        b.BeginAnimation(Canvas.TopProperty, null);
        Panel.SetZIndex(b, 1);

        MouseMove += Viewer_MouseMove;
        MouseLeftButtonUp += Viewer_MouseLeftButtonUp;
    }

    private void Viewer_MouseMove(object? sender, MouseEventArgs e)
    {
        if (!_isDragging || _dragged is null) return;
        Point p = e.GetPosition(LeftCanvas);
        Canvas.SetLeft(_dragged, 0);
        Canvas.SetTop(_dragged, p.Y - _dragged.ActualHeight / 2);

        int oldIndex = _labelBorders.IndexOf(_dragged);
        int newIndex = (int)System.Math.Clamp(System.Math.Round((p.Y - _dragged.ActualHeight / 2) / (LabelHeight + LabelMargin)), 0, _labelBorders.Count - 1);
        if (newIndex != _lastIndex)
        {
            RearrangeLabels(oldIndex, newIndex);
            _lastIndex = newIndex;
        }
    }

    private void RearrangeLabels(int oldIndex, int newIndex)
    {
        for (int i = 0; i < _labelBorders.Count; i++)
        {
            var item = _labelBorders[i];
            if (item == _dragged) continue;
            double targetY;
            if (oldIndex < newIndex)
            {
                targetY = (i > oldIndex && i <= newIndex) ? (i - 1) * (LabelHeight + LabelMargin) : i * (LabelHeight + LabelMargin);
            }
            else
            {
                targetY = (i >= newIndex && i < oldIndex) ? (i + 1) * (LabelHeight + LabelMargin) : i * (LabelHeight + LabelMargin);
            }
            // 簡易: 直接配置
            Canvas.SetTop(item, targetY);
        }
    }

    private void Viewer_MouseLeftButtonUp(object? sender, MouseButtonEventArgs e)
    {
        if (!_isDragging || _dragged is null) { CleanupDrag(); return; }

        _dragged.ReleaseMouseCapture();
        int oldIndex = _labelBorders.IndexOf(_dragged);
        int newIndex = _lastIndex;

        bool changed = oldIndex != newIndex;
        if (changed && Signals is ObservableCollection<VariableDisplayItem> list)
        {
            // 1) ラベルの見た目順を先に更新
            if (oldIndex >= 0 && newIndex >= 0 && newIndex < _labelBorders.Count)
            {
                _labelBorders.RemoveAt(oldIndex);
                _labelBorders.Insert(newIndex, _dragged);
            }

            // 2) 旧インデックス(Tag)に基づきSignalsを再構築
            var original = Signals.ToList();
            var reordered = _labelBorders.Select(b => original[(int)b.Tag!]).ToList();
            list.Clear();
            foreach (var it in reordered) list.Add(it);
            // 以降、BuildLeftLabelsにより_labelBordersが再生成されるので触らない
        }
        else
        {
            // 位置だけ復元
            for (int i = 0; i < _labelBorders.Count; i++)
            {
                Canvas.SetTop(_labelBorders[i], i * (LabelHeight + LabelMargin));
            }
        }

        CleanupDrag();
        RenderWaveforms();
    }

    private void CleanupDrag()
    {
        _isDragging = false;
        _dragged = null;
        _lastIndex = -1;
        MouseMove -= Viewer_MouseMove;
        MouseLeftButtonUp -= Viewer_MouseLeftButtonUp;
    }

    // 入力: Ctrl+ホイール=ズーム、Shift+ホイール=水平スクロール、他=親へ（垂直スクロール）
    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        base.OnPreviewMouseWheel(e);

        bool ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

        if (ctrl)
        {
            // 拡大前のマウスの位置を取得
            Point prev_mousePositionOnCanvas = e.GetPosition(WaveDrawGrid);
            Point prev_mousePositionOnScrollViewer = e.GetPosition(WaveScrollViewer);

            double mag = shift ? 2.0 : 1.1;
            double oldMag = Magnification;
            double newMag = e.Delta > 0 ? (Magnification * mag) : (Magnification / mag);
            Magnification = Math.Clamp(newMag, MinMagnification, MaxMagnification);

            double new_mousePositionOnCanvasX = prev_mousePositionOnCanvas.X * (Magnification / oldMag);

            WaveScrollViewer.ScrollToHorizontalOffset(new_mousePositionOnCanvasX - prev_mousePositionOnScrollViewer.X);

            e.Handled = true;
            return;
        }

        if (shift)
        {
            // 水平スクロール
            double viewport = WaveScrollViewer.ViewportWidth;
            double step = viewport > 0 ? viewport * 0.15 : 120; // ビューポートの15%を基準に移動
            double current = WaveScrollViewer.HorizontalOffset;
            double newOffset = current - System.Math.Sign(e.Delta) * step; // ホイール上=左、下=右
            newOffset = System.Math.Max(0, System.Math.Min(newOffset, WaveDrawGrid.ActualWidth - viewport));
            WaveScrollViewer.ScrollToHorizontalOffset(newOffset);
            // ScrollChangedで再描画される
            e.Handled = true; // 親への垂直スクロールを抑止
            return;
        }

        // ここでは縦スクロールは親のScrollViewerへ任せる（Handledにしない）
        e.Handled = false;
    }
}
