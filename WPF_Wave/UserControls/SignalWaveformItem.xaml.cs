using Serilog;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using WPF_Wave.Helpers;
using WPF_Wave.Models;
using WPF_Wave.ViewModels;

namespace WPF_Wave.UserControls;

/// <summary>
/// SignalWaveformItem.xaml の相互作用ロジック
/// </summary>
public partial class SignalWaveformItem : UserControl
{
    public SignalWaveformItem()
    {
        InitializeComponent();
    }

    public static readonly DependencyProperty VcdDataProperty =
        DependencyProperty.Register(nameof(VcdData), typeof(Vcd), typeof(SignalWaveformItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, NeedRenderCallback));

    public Vcd? VcdData
    {
        get => (Vcd?)GetValue(VcdDataProperty);
        set => SetValue(VcdDataProperty, value);
    }


    public static readonly DependencyProperty DrawSignalVariableDisplayItemProperty =
        DependencyProperty.Register(nameof(DrawSignalVariableDisplayItem), typeof(VariableDisplayItem), typeof(SignalWaveformItem),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, DrawSignalVariableDisplayItemsChangedCallback));

    public VariableDisplayItem DrawSignalVariableDisplayItem
    {
        get => (VariableDisplayItem)GetValue(DrawSignalVariableDisplayItemProperty);
        set => SetValue(DrawSignalVariableDisplayItemProperty, value);
    }


    public static readonly DependencyProperty SingleWaveHeightProperty =
        DependencyProperty.Register(nameof(SingleWaveHeight), typeof(double), typeof(SignalWaveformItem),
            new FrameworkPropertyMetadata(50.0, FrameworkPropertyMetadataOptions.AffectsRender, NeedRenderCallback));

    /// <summary>
    /// ある信号の波形の高さ
    /// </summary>
    public double SingleWaveHeight
    {
        get => (double)GetValue(SingleWaveHeightProperty);
        set => SetValue(SingleWaveHeightProperty, value);
    }

    public static readonly DependencyProperty WaveMarginProperty =
        DependencyProperty.Register(nameof(WaveMargin), typeof(double), typeof(SignalWaveformItem),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsRender, NeedRenderCallback));

    /// <summary>
    /// 波形同士の間隔
    /// </summary>
    public double WaveMargin
    {
        get => (double)GetValue(WaveMarginProperty);
        set => SetValue(WaveMarginProperty, value);
    }

    /// <summary>
    /// Magnification最小値
    /// </summary>
    public static readonly DependencyProperty MinMagnificationProperty =
        DependencyProperty.Register(
            nameof(MinMagnification),
            typeof(double),
            typeof(SignalWaveformItem),
            new FrameworkPropertyMetadata(0.1, OnMinMaxMagnificationChanged));

    public double MinMagnification
    {
        get => (double)GetValue(MinMagnificationProperty);
        set => SetValue(MinMagnificationProperty, value);
    }

    /// <summary>
    /// Magnification最大値
    /// </summary>
    public static readonly DependencyProperty MaxMagnificationProperty =
        DependencyProperty.Register(
            nameof(MaxMagnification),
            typeof(double),
            typeof(SignalWaveformItem),
            new FrameworkPropertyMetadata(5000.0, OnMinMaxMagnificationChanged));

    public double MaxMagnification
    {
        get => (double)GetValue(MaxMagnificationProperty);
        set => SetValue(MaxMagnificationProperty, value);
    }

    private static void OnMinMaxMagnificationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        // Min/Max の変更時に Magnification が範囲外なら補正
        if (d is SignalWaveformItem c)
        {
            // Coerce を明示的に呼び出し、現在の値を範囲に収める
            c.CoerceValue(MagnificationProperty);
        }
    }

    public static readonly DependencyProperty MagnificationProperty =
        DependencyProperty.Register(nameof(Magnification), typeof(double), typeof(SignalWaveformItem),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, MagnificationChangedCallback));

    public double Magnification
    {
        get => (double)GetValue(MagnificationProperty);
        set => SetValue(MagnificationProperty, value);
    }

    private static void MagnificationChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SignalWaveformItem SignalWaveformItem) return;
        if (e.OldValue is not double oldVal) return;
        if (e.NewValue is not double newVal) return;

        if (double.IsNaN(newVal) || double.IsInfinity(newVal))
        {
            Log.Error("Invalid magnification value: {Value}", newVal);
            return;
        }
        if (newVal < SignalWaveformItem.MinMagnification || newVal > SignalWaveformItem.MaxMagnification)
        {
            SignalWaveformItem.Magnification = newVal.AdaptRestrictions(SignalWaveformItem.MinMagnification, SignalWaveformItem.MaxMagnification);
            return;
        }

        if (newVal <= 0)
        {
            Log.Fatal("Magnification must be greater than 0. Current value: {Value}", newVal);
            // 値は制限されているはずだが、念のため早期リターン
            if (SignalWaveformItem.MinMagnification > 0) return;
        }

        double newWidth = 100d * newVal;
        SignalWaveformItem.WaveDrawGrid.Width = newWidth;

        NeedRenderCallback(d, e);
    }


    public double WaveWidth
    {
        get
        {
            if (double.IsNaN(WaveDrawGrid.ActualWidth))
                return WaveDrawGrid.Width;
            return WaveDrawGrid.ActualWidth;
        }
    }

    private static readonly DependencyPropertyKey WaveFromImageSourcePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(WaveFromImageSource),
            typeof(WriteableBitmap),
            typeof(SignalWaveformItem),
            new FrameworkPropertyMetadata(new WriteableBitmap(100, 100, 96, 96, PixelFormats.Bgr32, null), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    public static readonly DependencyProperty WaveFromImageSourceProperty = WaveFromImageSourcePropertyKey.DependencyProperty;

    public WriteableBitmap WaveFromImageSource
    {
        get => (WriteableBitmap)GetValue(WaveFromImageSourceProperty);
        protected set => SetValue(WaveFromImageSourcePropertyKey, value);
    }


    public static readonly DependencyProperty CrossWidthProperty =
        DependencyProperty.Register(nameof(CrossWidth), typeof(double), typeof(SignalWaveformItem),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsRender, NeedRenderCallback));

    /// <summary>
    /// クロス（交差）幅
    /// </summary>
    public double CrossWidth
    {
        get => (double)GetValue(CrossWidthProperty);
        set => SetValue(CrossWidthProperty, value);
    }

    private static void NeedRenderCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SignalWaveformItem signalWaveformItem)
            return;

        signalWaveformItem.SignalNameTextBlock.Text = signalWaveformItem.DrawSignalVariableDisplayItem?.Name ?? string.Empty;
        signalWaveformItem.SignalTypeTextBlock.Text = signalWaveformItem.DrawSignalVariableDisplayItem?.DescriptionText ?? string.Empty;
        signalWaveformItem.SignalNameAndTypeBorder.Height = signalWaveformItem.SingleWaveHeight + signalWaveformItem.WaveMargin * 2;
        signalWaveformItem.RenderWaveforms();
    }

    public DpiScale? DpiScale;

    private struct ViewportInfo
    {
        public double ContentWidth;
        public double ViewportWidth;
        public double Offset;
    }

    // 追加: 自身をホストしている ScrollViewer をビジュアルツリーから取得
    private ScrollViewer? FindScrollViewer()
    {
        DependencyObject? p = WaveDrawGrid;
        while (p != null && p is not ScrollViewer)
        {
            var parent = VisualTreeHelper.GetParent(p);
            if (parent is null && p is FrameworkElement fe)
            {
                parent = fe.Parent as DependencyObject;
            }
            p = parent;
        }
        return p as ScrollViewer;
    }

    private ViewportInfo GetViewport()
    {
        double contentWidth = WaveWidth;
        var sv = FindScrollViewer();
        double viewportWidth = sv?.ViewportWidth ?? double.NaN;
        if (double.IsNaN(viewportWidth) || viewportWidth <= 0)
        {
            viewportWidth = sv?.ActualWidth ?? contentWidth;
        }
        double offset = sv?.HorizontalOffset ?? 0.0;
        if (offset < 0) offset = 0;
        if (contentWidth > 0 && offset > contentWidth) offset = contentWidth;
        return new ViewportInfo { ContentWidth = contentWidth, ViewportWidth = viewportWidth, Offset = offset };
    }

    private WriteableBitmap CreateWriteableBitmap(double width, double height)
    {
        if (DpiScale is null)
        {
            Log.Error("DpiScale is null. I make a WriteableBitmap with default DPI(96x96).");
            return new WriteableBitmap((int)Math.Round(width), (int)Math.Round(height), 96, 96, PixelFormats.Bgr32, null);
        }
        return new WriteableBitmap((int)Math.Round(width), (int)Math.Round(height), DpiScale.Value.PixelsPerInchX, DpiScale.Value.PixelsPerInchY, PixelFormats.Bgr32, null);
    }

    private void CreateAndSetWriteableBitmap()
    {
        var vp = GetViewport();
        double imageWidth = vp.ViewportWidth > 0 ? vp.ViewportWidth : WaveWidth;
        if (imageWidth <= 0) imageWidth = 1; // guard
        // WaveMarginを考慮した正しい高さ計算
        double imageHeight = SingleWaveHeight + WaveMargin;
        WaveFromImageSource = CreateWriteableBitmap(imageWidth, imageHeight);
        WaveFromImage.Source = WaveFromImageSource;

        // 表示位置をスクロールオフセットに合わせる
        var left = vp.Offset;
        WaveFromImage.Margin = new Thickness(left, 0, 0, 0);
        SignalValueDrawer.Margin = new Thickness(left, 0, 0, 0);
    }

    private static bool TryIntersect(double a0, double a1, double b0, double b1, out double i0, out double i1)
    {
        // [a0,a1] ∩ [b0,b1]
        i0 = Math.Max(a0, b0);
        i1 = Math.Min(a1, b1);
        return i1 > i0; // positive length
    }

    private void RenderOneBitWaveFragment(double waveHeight, double contentWidth, double viewportLeft, double viewportWidth, double topMargin,
        long startTime, long endTime, VariableValue beginVal, VariableValue endVal)
    {
        if (VcdData is null) return;
        if (contentWidth <= 0 || viewportWidth <= 0) return;

        bool isBeginHigh = beginVal[0] == VariableValue.BitType.One;
        bool isEndHigh = endVal[0] == VariableValue.BitType.One;

        long totalTime = Math.Max(1, VcdData.SimulationTime);
        double startXGlobal = startTime * contentWidth / totalTime;
        double endXGlobal = endTime * contentWidth / totalTime;
        if (!TryIntersect(startXGlobal, endXGlobal, viewportLeft, viewportLeft + viewportWidth, out var visStart, out var visEnd))
        {
            return; // not visible
        }

        double yBegin = topMargin + (isBeginHigh ? 0 : waveHeight);
        double yEnd = topMargin + (isEndHigh ? 0 : waveHeight);

        // ローカル座標(ビューポート左を原点)に変換
        var startPoint = new IntVector2d(visStart - viewportLeft, yBegin);
        var middlePoint = new IntVector2d(visEnd - viewportLeft, yBegin);

        Color lineColor = beginVal.IsUndefined ? Colors.Red : beginVal.IsHighImpedance ? Colors.Brown : Colors.Blue;

        WaveFromImageSource.DrawLine(startPoint, middlePoint, lineColor);

        // 遷移点がビューポート内にある場合のみ縦線を描画
        if (endXGlobal >= viewportLeft && endXGlobal <= viewportLeft + viewportWidth)
        {
            var endPoint = new IntVector2d(middlePoint.X, yEnd);
            WaveFromImageSource.DrawLine(middlePoint, endPoint, lineColor);
        }
    }

    /// <summary>
    /// 信号値を表示するのに必要な最小幅（ピクセル）
    /// </summary>
    private const double MinValueDisplayWidth = 50.0;

    private void RenderMultiBitWaveFragment(double waveHeight, double contentWidth, double viewportLeft, double viewportWidth, double topMargin,
        long startTime, long endTime, VariableValue value, VariableDisplayItem item, double waveTopMargin)
    {
        if (VcdData is null) return;
        if (contentWidth <= 0 || viewportWidth <= 0) return;

        Color lineColor = value.IsUndefined ? Colors.Red : value.IsHighImpedance ? Colors.Brown : Colors.Blue;

        long totalTime = Math.Max(1, VcdData.SimulationTime);
        double startXGlobal = startTime * contentWidth / totalTime;
        double endXGlobal = endTime * contentWidth / totalTime;
        if (!TryIntersect(startXGlobal, endXGlobal, viewportLeft, viewportLeft + viewportWidth, out var visStart, out var visEnd))
        {
            return; // not visible
        }

        double durationX = visEnd - visStart;
        var startPoint = new IntVector2d(visStart - viewportLeft, topMargin + (waveHeight / 2));
        var endPoint = new IntVector2d(visEnd - viewportLeft, topMargin + (waveHeight / 2));

        if (durationX < CrossWidth * 2)
        {
            // クロス幅の2倍より短い場合は、クロスは中間に描画
            double middleLocalX = startPoint.X + durationX / 2;
            var middleTopPoint = new IntVector2d(middleLocalX, topMargin);
            var middleBottomPoint = new IntVector2d(middleLocalX, topMargin + waveHeight);

            WaveFromImageSource.DrawLine(startPoint, middleTopPoint, lineColor);
            WaveFromImageSource.DrawLine(startPoint, middleBottomPoint, lineColor);
            WaveFromImageSource.DrawLine(middleTopPoint, endPoint, lineColor);
            WaveFromImageSource.DrawLine(middleBottomPoint, endPoint, lineColor);
        }
        else
        {
            var middleTopLeftPoint = new IntVector2d(startPoint.X + CrossWidth, topMargin);
            var middleTopRightPoint = new IntVector2d(endPoint.X - CrossWidth, topMargin);
            var middleBottomLeftPoint = new IntVector2d(startPoint.X + CrossWidth, topMargin + waveHeight);
            var middleBottomRightPoint = new IntVector2d(endPoint.X - CrossWidth, topMargin + waveHeight);

            WaveFromImageSource.DrawLine(startPoint, middleTopLeftPoint, lineColor);
            WaveFromImageSource.DrawLine(startPoint, middleBottomLeftPoint, lineColor);
            WaveFromImageSource.DrawLine(middleTopLeftPoint, middleTopRightPoint, lineColor);
            WaveFromImageSource.DrawLine(middleBottomLeftPoint, middleBottomRightPoint, lineColor);
            WaveFromImageSource.DrawLine(middleTopRightPoint, endPoint, lineColor);
            WaveFromImageSource.DrawLine(middleBottomRightPoint, endPoint, lineColor);
        }

        // 十分なスペースがある場合のみ値を表示（ローカル座標で）
        if (durationX >= MinValueDisplayWidth)
        {
            AddSignalValueTextBlock(value, visStart - viewportLeft, visEnd - viewportLeft, waveTopMargin, item);
        }
    }

    /// <summary>
    /// 信号値を表示するTextBlockを追加
    /// </summary>
    /// <param name="value">表示する値</param>
    /// <param name="startX">開始X座標（ローカル/ビューポート座標）</param>
    /// <param name="endX">終了X座標（ローカル/ビューポート座標）</param>
    /// <param name="waveTopMargin">波形の上端マージン</param>
    /// <param name="item">信号表示アイテム</param>
    private void AddSignalValueTextBlock(VariableValue value, double startX, double endX, double waveTopMargin, VariableDisplayItem item)
    {
        var textBlock = new TextBlock
        {
            Text = value.ToString(),
            FontSize = Math.Min(12, SingleWaveHeight / 4), // 波形の高さに応じてフォントサイズを調整
            Foreground = value.IsUndefined ? Brushes.Red :
                        value.IsHighImpedance ? Brushes.Brown : Brushes.Black,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            Background = Brushes.White,
            Padding = new Thickness(2),
            FontFamily = new FontFamily("Consolas") // 等幅フォントを使用
        };

        // TextBlockの位置を設定（ローカルX座標）
        Canvas.SetLeft(textBlock, startX + (endX - startX) / 2 - 25); // 中央配置のため25px左にオフセット
        Canvas.SetTop(textBlock, waveTopMargin + WaveMargin + SingleWaveHeight / 2 - 8); // 中央配置のため8px上にオフセット

        SignalValueDrawer.Children.Add(textBlock);
    }

    private void RenderSingleWaveform(VariableDisplayItem item, double topMargin)
    {
        if (VcdData is null) return;

        double waveHeight = SingleWaveHeight - 2 * WaveMargin;
        if (waveHeight <= 0) return;

        var vp = GetViewport();
        double contentWidth = vp.ContentWidth;
        double viewportLeft = vp.Offset;
        double viewportWidth = vp.ViewportWidth;

        List<TimeValuePair> timeVal = VcdData.GetTimeValuePairs(item.VariableData.Id);
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

        // 最後の値を描画
        beginTime = timeVal[^1].Time;
        endTime = VcdData.SimulationTime; // 最後の時間はシミュレーション時間
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
        if (DrawSignalVariableDisplayItem is null) return;

        CreateAndSetWriteableBitmap();

        // SignalValueDrawerをクリア
        SignalValueDrawer.Children.Clear();
        RenderSingleWaveform(DrawSignalVariableDisplayItem, 0);
    }


    private static void DrawSignalVariableDisplayItemsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not SignalWaveformItem SignalWaveformItem) return;

        // 古いコレクションのイベントハンドラを削除
        if (e.OldValue is ObservableCollection<VariableDisplayItem> oldCollection)
        {
            SignalWaveformItem.UnsubscribeFromCollection(oldCollection);
        }

        // 新しいコレクションのイベントハンドラを登録
        if (e.NewValue is ObservableCollection<VariableDisplayItem> newCollection)
        {
            SignalWaveformItem.SubscribeToCollection(newCollection);
        }

        NeedRenderCallback(d, e);
    }

    /// <summary>
    /// コレクションとその要素のイベントに登録
    /// </summary>
    /// <param name="collection">登録対象のコレクション</param>
    private void SubscribeToCollection(ObservableCollection<VariableDisplayItem> collection)
    {
        if (collection == null) return;

        // コレクション変更イベントに登録
        collection.CollectionChanged += OnCollectionChanged;

        // 既存の要素のPropertyChangedイベントに登録
        foreach (var item in collection)
        {
            if (item != null)
            {
                item.PropertyChanged += OnItemPropertyChanged;
            }
        }
    }

    /// <summary>
    /// コレクションとその要素のイベントから登録解除
    /// </summary>
    /// <param name="collection">登録解除対象のコレクション</param>
    private void UnsubscribeFromCollection(ObservableCollection<VariableDisplayItem> collection)
    {
        if (collection == null) return;

        // コレクション変更イベントから登録解除
        collection.CollectionChanged -= OnCollectionChanged;

        // 要素のPropertyChangedイベントから登録解除
        foreach (var item in collection)
        {
            if (item != null)
            {
                item.PropertyChanged -= OnItemPropertyChanged;
            }
        }
    }

    /// <summary>
    /// コレクションが変更された際のイベントハンドラ
    /// </summary>
    /// <param name="sender">イベント送信者</param>
    /// <param name="e">イベント引数</param>
    private void OnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // 削除された要素のイベントハンドラを削除
        if (e.OldItems != null)
        {
            foreach (VariableDisplayItem item in e.OldItems)
            {
                if (item != null)
                {
                    item.PropertyChanged -= OnItemPropertyChanged;
                }
            }
        }

        // 追加された要素のイベントハンドラを登録
        if (e.NewItems != null)
        {
            foreach (VariableDisplayItem item in e.NewItems)
            {
                if (item != null)
                {
                    item.PropertyChanged += OnItemPropertyChanged;
                }
            }
        }

        // 波形を再描画
        RenderWaveforms();
    }

    /// <summary>
    /// 要素のプロパティが変更された際のイベントハンドラ
    /// </summary>
    /// <param name="sender">イベント送信者</param>
    /// <param name="e">イベント引数</param>
    private void OnItemPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 波形描画に影響するプロパティが変更された場合のみ再描画
        if (e.PropertyName == nameof(VariableDisplayItem.Name) ||
            e.PropertyName == nameof(VariableDisplayItem.Type) ||
            e.PropertyName == nameof(VariableDisplayItem.BitWidth) ||
            e.PropertyName == nameof(VariableDisplayItem.VariableData))
        {
            RenderWaveforms();
        }
    }

    private void UserControl_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
    {
        if(Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl))
        {
            // Ctrlキーが押されている場合、拡大縮小を行う

            double mag = 1.1;
            if (Keyboard.IsKeyDown(Key.LeftShift) || Keyboard.IsKeyDown(Key.RightShift))
            {
                // Shiftキーも押されている場合は、より大きな変化量で拡大縮小
                mag = 2.0;
            }

            double newMagnification;
            if (e.Delta > 0)
            {
                newMagnification = (Magnification * mag);
            }
            else
            {
                newMagnification = (Magnification / mag);
            }
            // 依存関係プロパティで定義した最小/最大値を使用して補正
            Magnification = newMagnification.AdaptRestrictions(MinMagnification, MaxMagnification);


            e.Handled = true; // イベントを処理済みにする
        }
        else
        {
            // Ctrlキーが押されていない場合はスクロールイベントを伝播させる
            e.Handled = false;
        }
    }

    private void UserControl_Loaded(object sender, RoutedEventArgs e)
    {
        DpiScale = VisualTreeHelper.GetDpi(this);
    }

    private void WaveScrollViewer_ScrollChanged(object sender, ScrollChangedEventArgs e)
    {
        // スクロール位置に合わせて再描画
        RenderWaveforms();
    }

    private void WaveScrollViewer_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        // ビューポートサイズ変更時に再描画
        RenderWaveforms();
    }
}
