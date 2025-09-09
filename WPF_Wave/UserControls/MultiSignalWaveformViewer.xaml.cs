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
/// �����M���̔g�`���c�ɕ��ׂē����\������r���[�A
/// - �����Y�[���iCtrl+�z�C�[���j
/// - �����X�N���[������
/// - �����̃��x�����h���b�O���ĕ��בւ��iDragableList �̃��W�b�N���ȗ��ڐA�j
/// - SignalWaveformItem �̃����_�����O���W�b�N���Q�l�ɁA1���̃r�b�g�}�b�v�֕����s��`��
/// </summary>
public partial class MultiSignalWaveformViewer : UserControl
{
    public MultiSignalWaveformViewer()
    {
        InitializeComponent();
        Loaded += (_, __) => { _dpi = VisualTreeHelper.GetDpi(this); RenderWaveforms(); };
    }

    // �ˑ��֌W�v���p�e�B
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

    // ===== ���Ԏ��̌����ڐ���p �ˑ��֌W�v���p�e�B =====
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

    // ���x��(Border)����ѕ����F�ݒ�p �ˑ��֌W�v���p�e�B
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

        // ���͔{���ɉ����čX�V�i���=100�j
        v.WaveDrawGrid.Width = 100d * newVal;
        v.RenderWaveforms();
    }

    // �������
    private DpiScale? _dpi;
    private List<Border> _labelBorders = new();
    private Border? _dragged;
    private bool _isDragging;
    private int _lastIndex = -1;

    // �����x���̌����ځiDragableList ���j
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
            // �\���t�H�[�}�b�g�ύX���͔g�`�̒l�\���̂ݍX�V
            RenderWaveforms();
        }
    }

    private double TotalImageHeight()
    {
        if (Signals == null || Signals.Count == 0) return SingleWaveHeight;
        // �s�Ԋ܂߂�����: N*(H+M) - M
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

        // �X�N���[���ɍ��킹�č��}�[�W�������iDIP�j
        var left = WaveScrollViewer.HorizontalOffset;
        WaveFromImage.Margin = new Thickness(left, 0, 0, 0);
        SignalValueDrawer.Margin = new Thickness(left, 0, 0, 0);

        // ���Ԏ��L�����o�X�̕���g�`�`��O���b�h�ɍ��킹��
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

        // ���[�J�����W(�r���[�|�[�g�������_)�ɕϊ��iDIP�j
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

        // ���[�J�����W(�r���[�|�[�g�������_)�ɕϊ��iDIP�j
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
            // ������Canvas��DIP���W�Ŕz�u����
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

        // �w�i�F�̓K�p
        TimeAxisCanvas.Background = TimeAxisBackground;

        long totalTime = Math.Max(1, VcdData.SimulationTime);
        double contentWidth = WaveDrawGrid.ActualWidth > 0 ? WaveDrawGrid.ActualWidth : WaveDrawGrid.Width;
        if (contentWidth <= 0)
            return;

        double viewportLeft = WaveScrollViewer.HorizontalOffset;
        double viewportWidth = WaveScrollViewer.ViewportWidth > 0 ? WaveScrollViewer.ViewportWidth : contentWidth;

        double pixelsPerTime = contentWidth / totalTime; // px (DIP) / time-unit
        if (pixelsPerTime <= 0 || double.IsInfinity(pixelsPerTime)) return;

        // �ڕW�Ԋu(px)
        const double targetPixelSpacing = 100.0;
        double rawTimeStep = targetPixelSpacing / pixelsPerTime; // time units
        long niceTimeStep = (long)FindNiceStep(rawTimeStep);
        if (niceTimeStep <= 0) niceTimeStep = 1;

        // ���̈�̊J�n/�I������
        long startTimeVisible = (long)Math.Floor(viewportLeft / pixelsPerTime);
        long endTimeVisible = (long)Math.Ceiling((viewportLeft + viewportWidth) / pixelsPerTime);
        if (endTimeVisible > totalTime) endTimeVisible = totalTime;

        // �ŏ��̖ڐ���istartTimeVisible �ȏ�̍ŏ��� niceTimeStep �̔{���j
        long firstTick = ((startTimeVisible + niceTimeStep - 1) / niceTimeStep) * niceTimeStep;

        const double tickHeight = 10.0;
        var tickBrush = TimeAxisTickBrush;
        var labelBrush = TimeAxisTextBrush;

        for (long t = firstTick; t <= endTimeVisible; t += niceTimeStep)
        {
            double xGlobal = t * pixelsPerTime; // �R���e���c�S�̂�X (DIP)
            double xLocal = xGlobal - viewportLeft; // �r���[�|�[�g���[�����_�Ƃ���X (DIP)

            // ���x���e�L�X�g
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

            // �ڐ�����i�e�L�X�g���ɕ\���j
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

    // ���Ԓl(timeValue [ps])��K�؂ȒP��(s, ms, us, ns, ps)�֕ϊ����ĕ�����
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
        double maxWidth = 0; // �R���e���c�Ɋ�Â��ő啝
        for (int i = 0; i < Signals.Count; i++)
        {
            var sig = Signals[i];
            var border = new Border
            {
                // Width �͎����v�������邽�ߎw�肵�Ȃ�
                Height = LabelHeight,
                Background = LabelBackground,
                BorderBrush = LabelBorderBrush,
                CornerRadius = LabelCornerRadius,
                Cursor = Cursors.Hand,
                Tag = i,
                Padding = new Thickness(8)
            };
            border.MouseLeftButtonDown += Label_MouseLeftButtonDown;

            // �R���e�L�X�g���j���[�i�E�N���b�N�j
            var cm = new ContextMenu();

            // �\���t�H�[�}�b�g�ύX
            var changeFormat = new MenuItem { Header = "�\���t�H�[�}�b�g�̕ύX" };
            
            cm.Items.Add(changeFormat);
            // Binary
            var fmtBin = new MenuItem { Header = "2�i�� (Binary)", IsCheckable = true, IsChecked = sig.DisplayFormat == StringFormat.Binary, Tag = (changeFormat, sig, StringFormat.Binary) };
            fmtBin.Click += ChangeFormatMenuItem_Click;
            changeFormat.Items.Add(fmtBin);
            // Decimal
            var fmtDec = new MenuItem { Header = "10�i�� (Decimal)", IsCheckable = true, IsChecked = sig.DisplayFormat == StringFormat.Decimal, Tag = (changeFormat, sig, StringFormat.Decimal) };
            fmtDec.Click += ChangeFormatMenuItem_Click;
            changeFormat.Items.Add(fmtDec);
            // Hex
            var fmtHex = new MenuItem { Header = "16�i�� (Hex)", IsCheckable = true, IsChecked = sig.DisplayFormat == StringFormat.Hexadecimal, Tag = (changeFormat, sig, StringFormat.Hexadecimal) };
            fmtHex.Click += ChangeFormatMenuItem_Click;
            changeFormat.Items.Add(fmtHex);

            // �폜
            var deleteItem = new MenuItem { Header = "�폜", Tag = sig };
            deleteItem.Click += DeleteSignalMenuItem_Click;
            cm.Items.Add(deleteItem);

            border.ContextMenu = cm;

            var sp = new StackPanel { Orientation = Orientation.Vertical };
            sp.Children.Add(new TextBlock { Text = sig.Name, FontWeight = FontWeights.Bold, Foreground = LabelNameForeground });
            sp.Children.Add(new TextBlock { Text = sig.DescriptionText, FontSize = 10, Foreground = LabelDescriptionForeground });
            border.Child = sp;

            // ��� Children �ɒǉ����Ă���v��
            LeftCanvas.Children.Add(border);
            border.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));
            double desiredWidth = border.DesiredSize.Width;
            if (desiredWidth > maxWidth) maxWidth = desiredWidth;

            double top = i * (LabelHeight + LabelMargin);
            Canvas.SetTop(border, top);
            _labelBorders.Add(border);
            totalHeight = top + LabelHeight;
        }

        // �S�Ă�Border�̕����ő啝�ɑ�����
        if (maxWidth <= 0) maxWidth = 50; // �t�H�[���o�b�N
        foreach (var b in _labelBorders)
        {
            b.Width = maxWidth;
        }

        // �ǂݎ��p�ɕێ��i�O���Q�Ɨp�j
        LabelWidth = maxWidth;

        LeftCanvas.Width = maxWidth;
        LeftCanvas.Height = totalHeight;

        // ���E�̍��������i�ȈՁj
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

    // �R���e�L�X�g���j���[: �폜
    private void DeleteSignalMenuItem_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not MenuItem mi) return;
        if (mi.Tag is not VariableDisplayItem item) return;
        if (Signals == null) return;

        // ���ڍ폜�iCollectionChanged�ɂ��UI�͍č\�z�����j
        Signals.Remove(item);
    }

    // ���בւ��i�h���b�O&�h���b�v�j�ȈՎ���
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
            // �Ȉ�: ���ڔz�u
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
            // 1) ���x���̌����ڏ����ɍX�V
            if (oldIndex >= 0 && newIndex >= 0 && newIndex < _labelBorders.Count)
            {
                _labelBorders.RemoveAt(oldIndex);
                _labelBorders.Insert(newIndex, _dragged);
            }

            // 2) ���C���f�b�N�X(Tag)�Ɋ�Â�Signals���č\�z
            var original = Signals.ToList();
            var reordered = _labelBorders.Select(b => original[(int)b.Tag!]).ToList();
            list.Clear();
            foreach (var it in reordered) list.Add(it);
            // �ȍ~�ABuildLeftLabels�ɂ��_labelBorders���Đ��������̂ŐG��Ȃ�
        }
        else
        {
            // �ʒu��������
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

    // ����: Ctrl+�z�C�[��=�Y�[���AShift+�z�C�[��=�����X�N���[���A��=�e�ցi�����X�N���[���j
    protected override void OnPreviewMouseWheel(MouseWheelEventArgs e)
    {
        base.OnPreviewMouseWheel(e);

        bool ctrl = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);
        bool shift = Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift);

        if (ctrl)
        {
            // �g��O�̃}�E�X�̈ʒu���擾
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
            // �����X�N���[��
            double viewport = WaveScrollViewer.ViewportWidth;
            double step = viewport > 0 ? viewport * 0.15 : 120; // �r���[�|�[�g��15%����Ɉړ�
            double current = WaveScrollViewer.HorizontalOffset;
            double newOffset = current - System.Math.Sign(e.Delta) * step; // �z�C�[����=���A��=�E
            newOffset = System.Math.Max(0, System.Math.Min(newOffset, WaveDrawGrid.ActualWidth - viewport));
            WaveScrollViewer.ScrollToHorizontalOffset(newOffset);
            // ScrollChanged�ōĕ`�悳���
            e.Handled = true; // �e�ւ̐����X�N���[����}�~
            return;
        }

        // �����ł͏c�X�N���[���͐e��ScrollViewer�֔C����iHandled�ɂ��Ȃ��j
        e.Handled = false;
    }
}
