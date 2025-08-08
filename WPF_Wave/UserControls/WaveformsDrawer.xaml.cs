using Serilog;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.Media3D;
using WPF_Wave.Helpers;
using WPF_Wave.Models;
using static WPF_Wave.Models.VariableValue;

namespace WPF_Wave.UserControls;

/// <summary>
/// WaveformDrawer.xaml の相互作用ロジック
/// </summary>
public partial class WaveformsDrawer : UserControl
{
    public static readonly DependencyProperty VcdDataProperty =
        DependencyProperty.Register(nameof(VcdData), typeof(Vcd), typeof(WaveformsDrawer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, NeedRenderCallback));

    public Vcd? VcdData
    {
        get => (Vcd?)GetValue(VcdDataProperty);
        set => SetValue(VcdDataProperty, value);
    }


    public static readonly DependencyProperty DrawSignalVariableDisplayItemsProperty =
        DependencyProperty.Register(nameof(DrawSignalVariableDisplayItems), typeof(ObservableCollection<VariableDisplayItem>), typeof(WaveformsDrawer),
            new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.AffectsRender, DrawSignalVariableDisplayItemsChangedCallback));

    public ObservableCollection<VariableDisplayItem> DrawSignalVariableDisplayItems
    {
        get => (ObservableCollection<VariableDisplayItem>)GetValue(DrawSignalVariableDisplayItemsProperty);
        set => SetValue(DrawSignalVariableDisplayItemsProperty, value);
    }


    public static readonly DependencyProperty SingleWaveHeightProperty =
        DependencyProperty.Register(nameof(SingleWaveHeight), typeof(double), typeof(WaveformsDrawer),
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
        DependencyProperty.Register(nameof(WaveMargin), typeof(double), typeof(WaveformsDrawer),
            new FrameworkPropertyMetadata(5.0, FrameworkPropertyMetadataOptions.AffectsRender, NeedRenderCallback));

    /// <summary>
    /// 波形同士の間隔
    /// </summary>
    public double WaveMargin
    {
        get => (double)GetValue(WaveMarginProperty);
        set => SetValue(WaveMarginProperty, value);
    }

    public static readonly DependencyProperty MagnificationProperty =
        DependencyProperty.Register(nameof(Magnification), typeof(double), typeof(WaveformsDrawer),
            new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender, MagnificationChangedCallback));

    public double Magnification
    {
        get => (double)GetValue(MagnificationProperty);
        set => SetValue(MagnificationProperty, value);
    }

    private static void MagnificationChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not WaveformsDrawer waveformsDrawer) return;
        if (e.OldValue is not double oldVal) return;
        if (e.NewValue is not double newVal) return;

        if (newVal <= 0)
        {
            Log.Fatal("Magnification must be greater than 0. Current value: {Value}", newVal);
        }

        double newWidth = waveformsDrawer.WaveWidth * (newVal / oldVal);
        waveformsDrawer.Width = newWidth;

        NeedRenderCallback(d, e);
    }


    public double WaveWidth
    {
        get
        {
            if (double.IsNaN(this.ActualWidth))
                return Width;
            return this.ActualWidth;
        }
    }

    private static readonly DependencyPropertyKey WaveFromImageSourcePropertyKey =
        DependencyProperty.RegisterReadOnly(
            nameof(WaveFromImageSource),
            typeof(WriteableBitmap),
            typeof(WaveformsDrawer),
            new FrameworkPropertyMetadata(new WriteableBitmap(100,100,96,96,PixelFormats.Bgr32, null), FrameworkPropertyMetadataOptions.AffectsRender)
        );

    public static readonly DependencyProperty WaveFromImageSourceProperty = WaveFromImageSourcePropertyKey.DependencyProperty;

    public WriteableBitmap WaveFromImageSource
    {
        get => (WriteableBitmap)GetValue(WaveFromImageSourceProperty);
        protected set => SetValue(WaveFromImageSourcePropertyKey, value);
    }


    public static readonly DependencyProperty CrossWidthProperty =
        DependencyProperty.Register(nameof(CrossWidth), typeof(double), typeof(WaveformsDrawer),
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
        if (d is not WaveformsDrawer waveformsDrawer)
            return;

        waveformsDrawer.RenderWaveforms();
    }


    private WriteableBitmap CreateWriteableBitmap(double width, double height)
    {
        var dpi = VisualTreeHelper.GetDpi(this);
        return new WriteableBitmap((int)width, (int)height, dpi.PixelsPerInchX, dpi.PixelsPerInchY, PixelFormats.Bgr32, null);
    }

    private void CreateAndSetWriteableBitmap()
    {
        double imageWidth = WaveWidth;
        // WaveMarginを考慮した正しい高さ計算
        double imageHeight = (SingleWaveHeight + WaveMargin) * DrawSignalVariableDisplayItems.Count;
        if (DrawSignalVariableDisplayItems.Count > 0)
        {
            imageHeight -= WaveMargin; // 最後の波形の下のマージンは不要
        }
        WaveFromImageSource = CreateWriteableBitmap(imageWidth, imageHeight);
        WaveFromImage.Source = WaveFromImageSource;
    }

    

    private void RenderOneBitWaveFragment(double waveHeight, double waveWidth, double topMargin, long startTime, long endTime, VariableValue beginVal, VariableValue endVal)
    {
        if (VcdData is null) return;

        bool isBeginHigh = beginVal[0] == VariableValue.BitType.One;
        bool isEndHigh = endVal[0] == VariableValue.BitType.One;

        long totalTime = VcdData.SimulationTime;
        double startX = startTime * waveWidth / totalTime;
        double endX = endTime * waveWidth / totalTime;
        var startPoint = new IntVector2d(startX, topMargin + (isBeginHigh ? 0 : waveHeight));
        var middlePoint = new IntVector2d(endX, topMargin + (isBeginHigh ? 0 : waveHeight));
        var endPoint = new IntVector2d(endX, topMargin + (isEndHigh ? 0 : waveHeight));

        Color lineColor;
        if (beginVal.IsUndefined)
        {
            lineColor = Colors.Red;
        }
        else if (beginVal.IsHighImpedance)
        {
            lineColor = Colors.Brown;
        }
        else
        {
            lineColor = Colors.Blue;
        }

        WaveFromImageSource.DrawLine(startPoint, middlePoint, lineColor);
        WaveFromImageSource.DrawLine(middlePoint, endPoint, lineColor);
    }

    /// <summary>
    /// 信号値を表示するのに必要な最小幅（ピクセル）
    /// </summary>
    private const double MinValueDisplayWidth = 50.0;

    private void RenderMultiBitWaveFragment(double waveHeight, double waveWidth, double topMargin, long startTime, long endTime, VariableValue value, VariableDisplayItem item, double waveTopMargin)
    {
        if (VcdData is null) return;

        Color lineColor;
        if (value.IsUndefined)
        {
            lineColor = Colors.Red;
        }
        else if (value.IsHighImpedance)
        {
            lineColor = Colors.Brown;
        }
        else
        {
            lineColor = Colors.Blue;
        }

        long totalTime = VcdData.SimulationTime;
        double startX = startTime * waveWidth / totalTime;
        double endX = endTime * waveWidth / totalTime;
        double durationX = endX - startX;
        var startPoint = new IntVector2d(startX, topMargin + (waveHeight/2));
        var endPoint = new IntVector2d(endX, topMargin + (waveHeight/2));

        if (durationX < CrossWidth * 2)
        {
            // クロス幅の2倍より短い場合は、クロスは中間に描画
            var middleTopPoint = new IntVector2d(startX + durationX / 2, topMargin);
            var middleBottomPoint = new IntVector2d(startX + durationX / 2, topMargin + waveHeight);

            WaveFromImageSource.DrawLine(startPoint, middleTopPoint, lineColor);
            WaveFromImageSource.DrawLine(startPoint, middleBottomPoint, lineColor);
            WaveFromImageSource.DrawLine(middleTopPoint, endPoint, lineColor);
            WaveFromImageSource.DrawLine(middleBottomPoint, endPoint, lineColor);
            return;
        }
        
        var middleTopLeftPoint = new IntVector2d(startX + CrossWidth, topMargin);
        var middleTopRightPoint = new IntVector2d(endX - CrossWidth, topMargin);
        var middleBottomLeftPoint = new IntVector2d(startX + CrossWidth, topMargin + waveHeight);
        var middleBottomRightPoint = new IntVector2d(endX - CrossWidth, topMargin + waveHeight);

        WaveFromImageSource.DrawLine(startPoint, middleTopLeftPoint, lineColor);
        WaveFromImageSource.DrawLine(startPoint, middleBottomLeftPoint, lineColor);
        WaveFromImageSource.DrawLine(middleTopLeftPoint, middleTopRightPoint, lineColor);
        WaveFromImageSource.DrawLine(middleBottomLeftPoint, middleBottomRightPoint, lineColor);
        WaveFromImageSource.DrawLine(middleTopRightPoint, endPoint, lineColor);
        WaveFromImageSource.DrawLine(middleBottomRightPoint, endPoint, lineColor);

        // 十分なスペースがある場合のみ値を表示
        if (durationX >= MinValueDisplayWidth)
        {
            AddSignalValueTextBlock(value, startX, endX, waveTopMargin, item);
        }
    }

    /// <summary>
    /// 信号値を表示するTextBlockを追加
    /// </summary>
    /// <param name="value">表示する値</param>
    /// <param name="startX">開始X座標</param>
    /// <param name="endX">終了X座標</param>
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

        // TextBlockの位置を設定
        Canvas.SetLeft(textBlock, startX + (endX - startX) / 2 - 25); // 中央配置のため25px左にオフセット
        Canvas.SetTop(textBlock, waveTopMargin + WaveMargin + SingleWaveHeight / 2 - 8); // 中央配置のため8px上にオフセット

        SignalValueDrawer.Children.Add(textBlock);
    }

    private void RenderSingleWaveform(VariableDisplayItem item, double topMargin)
    {
        if(VcdData is null) return;

        double waveHeight = SingleWaveHeight - 2 * WaveMargin;
        if (waveHeight <= 0) return;

        double waveWidth = WaveWidth;
        
        List<TimeValuePair> timeVal = VcdData.GetTimeValuePairs(item.VariableData.Id);
        if(timeVal.Count <= 0) return;
        bool isOneBit = item.VariableData.BitWidth == 1;

        long beginTime, endTime;
        VariableValue beginValue, endValue;
        for (int i=0; i < timeVal.Count-1; i++)
        {
            beginTime = timeVal[i].Time;
            endTime = timeVal[i + 1].Time;
            beginValue = timeVal[i].Value;
            endValue = timeVal[i + 1].Value;

            if (isOneBit)
            {
                RenderOneBitWaveFragment(waveHeight, waveWidth, topMargin + WaveMargin, beginTime, endTime, beginValue, endValue);
            }
            else
            {
                RenderMultiBitWaveFragment(waveHeight, waveWidth, topMargin + WaveMargin, beginTime, endTime, beginValue, item, topMargin);
            }
        }

        // 最後の値を描画
        beginTime  = timeVal[^1].Time;
        endTime = VcdData.SimulationTime; // 最後の時間はシミュレーション時間
        beginValue = timeVal[^1].Value;
        endValue = new(timeVal[^1].Value);

        if (isOneBit)
        {
            RenderOneBitWaveFragment(waveHeight, waveWidth, topMargin + WaveMargin, beginTime, endTime, beginValue, endValue);
        }
        else
        {
            RenderMultiBitWaveFragment(waveHeight, waveWidth, topMargin + WaveMargin, beginTime, endTime, beginValue, item, topMargin);
        }
    }

    public void RenderWaveforms()
    {
        if (VcdData is null) return;

        if (DrawSignalVariableDisplayItems.Count <= 0) return;
        CreateAndSetWriteableBitmap();

        // SignalValueDrawerをクリア
        SignalValueDrawer.Children.Clear();

        double topMargin = 0;
        foreach (var signal in DrawSignalVariableDisplayItems)
        {
            RenderSingleWaveform(signal, topMargin);
            // WaveMarginを考慮した正しいtopMargin計算
            topMargin += SingleWaveHeight + WaveMargin;
        }
    }


    private static void DrawSignalVariableDisplayItemsChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is not WaveformsDrawer waveformsDrawer) return;

        // 古いコレクションのイベントハンドラを削除
        if (e.OldValue is ObservableCollection<VariableDisplayItem> oldCollection)
        {
            waveformsDrawer.UnsubscribeFromCollection(oldCollection);
        }

        // 新しいコレクションのイベントハンドラを登録
        if (e.NewValue is ObservableCollection<VariableDisplayItem> newCollection)
        {
            waveformsDrawer.SubscribeToCollection(newCollection);
        }

        waveformsDrawer.RenderWaveforms();
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

    public WaveformsDrawer()
    {
        InitializeComponent();
    }
}
