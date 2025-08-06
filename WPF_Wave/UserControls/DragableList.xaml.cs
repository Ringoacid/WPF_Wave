using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;
using System.Reflection;
using System.ComponentModel;
using System.Collections.Specialized;

namespace WPF_Wave.UserControls;

/// <summary>
/// ドラッグ可能なリストコントロール
/// アイテムの並び替え、追加、削除が可能な垂直リスト
/// </summary>
public partial class DragableList : UserControl
{
    #region 依存関係プロパティ
    
    /// <summary>
    /// リストに表示するアイテムのコレクション
    /// </summary>
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(ObservableCollection<object>), typeof(DragableList),
            new PropertyMetadata(new ObservableCollection<object>(), OnItemsSourceChanged));

    /// <summary>
    /// リストに表示するアイテムのコレクション
    /// </summary>
    public ObservableCollection<object> ItemsSource
    {
        get => (ObservableCollection<object>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    /// <summary>
    /// ItemsSourceプロパティが変更された時の処理
    /// </summary>
    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DragableList dragableList)
        {
            // 古いコレクションのイベント購読を解除
            dragableList.UnsubscribeFromItemsSourceEvents(e.OldValue as ObservableCollection<object>);
            // 新しいコレクションのイベントを購読
            dragableList.SubscribeToItemsSourceEvents(e.NewValue as ObservableCollection<object>);
            // UIを更新
            dragableList.RefreshItems();
        }
    }

    /// <summary>
    /// 表示に使用するプロパティのパス（ListViewのDisplayMemberPathと同様）
    /// </summary>
    public static readonly DependencyProperty DisplayMemberPathProperty =
        DependencyProperty.Register(nameof(DisplayMemberPath), typeof(string), typeof(DragableList),
            new PropertyMetadata(string.Empty, OnAnyDependencyPropertyChanged));

    /// <summary>
    /// 表示に使用するプロパティのパス
    /// 空文字列の場合はToString()を使用
    /// </summary>
    public string DisplayMemberPath
    {
        get => (string)GetValue(DisplayMemberPathProperty);
        set => SetValue(DisplayMemberPathProperty, value);
    }

    /// <summary>
    /// 各アイテムの幅（AutoWidthEnabledがfalseの場合に使用）
    /// </summary>
    public static readonly DependencyProperty BorderWidthProperty =
        DependencyProperty.Register(nameof(BorderWidth), typeof(double), typeof(DragableList),
            new PropertyMetadata(300.0, OnAnyDependencyPropertyChanged));

    /// <summary>
    /// 各アイテムの幅
    /// </summary>
    public double BorderWidth
    {
        get => (double)GetValue(BorderWidthProperty);
        set => SetValue(BorderWidthProperty, value);
    }

    /// <summary>
    /// 自動幅調整を有効にするかどうか
    /// </summary>
    public static readonly DependencyProperty AutoWidthEnabledProperty =
        DependencyProperty.Register(nameof(AutoWidthEnabled), typeof(bool), typeof(DragableList),
            new PropertyMetadata(true, OnAnyDependencyPropertyChanged));

    /// <summary>
    /// 自動幅調整が有効かどうか
    /// trueの場合、コンテンツに合わせて幅を自動調整
    /// </summary>
    public bool AutoWidthEnabled
    {
        get => (bool)GetValue(AutoWidthEnabledProperty);
        set => SetValue(AutoWidthEnabledProperty, value);
    }

    /// <summary>
    /// 最小幅
    /// </summary>
    public static readonly new DependencyProperty MinWidthProperty =
        DependencyProperty.Register(nameof(MinWidth), typeof(double), typeof(DragableList),
            new PropertyMetadata(50.0, OnAnyDependencyPropertyChanged));

    /// <summary>
    /// 最小幅（AutoWidthEnabled時の制約）
    /// </summary>
    public　new double MinWidth
    {
        get => (double)GetValue(MinWidthProperty);
        set => SetValue(MinWidthProperty, value);
    }

    /// <summary>
    /// 最大幅
    /// </summary>
    public static readonly new DependencyProperty MaxWidthProperty =
        DependencyProperty.Register(nameof(MaxWidth), typeof(double), typeof(DragableList),
            new PropertyMetadata(500.0, OnAnyDependencyPropertyChanged));

    /// <summary>
    /// 最大幅（AutoWidthEnabled時の制約）
    /// </summary>
    public new double MaxWidth
    {
        get => (double)GetValue(MaxWidthProperty);
        set => SetValue(MaxWidthProperty, value);
    }

    /// <summary>
    /// フォントサイズ
    /// </summary>
    public static readonly new DependencyProperty FontSizeProperty =
        DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(DragableList),
            new PropertyMetadata(14.0, OnAnyDependencyPropertyChanged));

    /// <summary>
    /// 表示テキストのフォントサイズ
    /// </summary>
    public new double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    /// <summary>
    /// フォントファミリ
    /// </summary>
    public static readonly new DependencyProperty FontFamilyProperty =
        DependencyProperty.Register(nameof(FontFamily), typeof(FontFamily), typeof(DragableList),
            new PropertyMetadata(new FontFamily("Segoe UI"), OnAnyDependencyPropertyChanged));

    /// <summary>
    /// 表示テキストのフォントファミリ
    /// </summary>
    public new FontFamily FontFamily
    {
        get => (FontFamily)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    /// <summary>
    /// フォントの太さ
    /// </summary>
    public static readonly new DependencyProperty FontWeightProperty =
        DependencyProperty.Register(nameof(FontWeight), typeof(FontWeight), typeof(DragableList),
            new PropertyMetadata(FontWeights.Normal, OnAnyDependencyPropertyChanged));

    /// <summary>
    /// 表示テキストのフォントの太さ
    /// </summary>
    public new FontWeight FontWeight
    {
        get => (FontWeight)GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    /// <summary>
    /// フォントスタイル
    /// </summary>
    public static readonly new DependencyProperty FontStyleProperty =
        DependencyProperty.Register(nameof(FontStyle), typeof(FontStyle), typeof(DragableList),
            new PropertyMetadata(FontStyles.Normal, OnAnyDependencyPropertyChanged));

    /// <summary>
    /// 表示テキストのフォントスタイル（斜体など）
    /// </summary>
    public new FontStyle FontStyle
    {
        get => (FontStyle)GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    /// <summary>
    /// 各アイテムの高さ
    /// </summary>
    public static readonly DependencyProperty BorderHeightProperty =
        DependencyProperty.Register(nameof(BorderHeight), typeof(double), typeof(DragableList),
            new PropertyMetadata(50.0, OnAnyDependencyPropertyChanged));

    /// <summary>
    /// 各アイテムの高さ
    /// </summary>
    public double BorderHeight
    {
        get => (double)GetValue(BorderHeightProperty);
        set => SetValue(BorderHeightProperty, value);
    }

    /// <summary>
    /// 各アイテム内のパディング
    /// </summary>
    public static readonly DependencyProperty BorderPaddingProperty =
        DependencyProperty.Register(nameof(BorderPadding), typeof(Thickness), typeof(DragableList),
            new PropertyMetadata(new Thickness(10d), OnAnyDependencyPropertyChanged));

    /// <summary>
    /// 各アイテム内のパディング
    /// </summary>
    public Thickness BorderPadding
    {
        get => (Thickness)GetValue(BorderPaddingProperty);
        set => SetValue(BorderPaddingProperty, value);
    }

    /// <summary>
    /// アイテム間のマージン
    /// </summary>
    public static readonly DependencyProperty BorderMarginProperty =
        DependencyProperty.Register(nameof(BorderMargin), typeof(double), typeof(DragableList),
            new PropertyMetadata(5.0, OnAnyDependencyPropertyChanged));

    /// <summary>
    /// アイテム間のマージン
    /// </summary>
    public double BorderMargin
    {
        get => (double)GetValue(BorderMarginProperty);
        set => SetValue(BorderMarginProperty, value);
    }

    /// <summary>
    /// アイテムの角の丸み
    /// </summary>
    public static readonly DependencyProperty BorderCornerRadiusProperty =
        DependencyProperty.Register(nameof(BorderCornerRadius), typeof(CornerRadius), typeof(DragableList),
            new PropertyMetadata(new CornerRadius(8.0), OnAnyDependencyPropertyChanged));

    /// <summary>
    /// 各アイテムの角の丸み
    /// </summary>
    public CornerRadius BorderCornerRadius
    {
        get => (CornerRadius)GetValue(BorderCornerRadiusProperty);
        set => SetValue(BorderCornerRadiusProperty, value);
    }
    #endregion

    #region パブリックプロパティとイベント

    /// <summary>
    /// 現在の並び順でソートされたアイテムソース
    /// </summary>
    public IEnumerable<object> SortedItemsSource { get; private set; } = [];

    /// <summary>
    /// アイテムの順序が変更された時のイベントハンドラの型定義
    /// </summary>
    public delegate void OrderChangedEventHandler(object sender, IEnumerable<object> SortedItemsSource);

    /// <summary>
    /// ドラッグ&ドロップでアイテムの順序が変更された時に発生するイベント
    /// </summary>
    public event OrderChangedEventHandler? OrderChanged;

    #endregion

    #region プライベートフィールド

    /// <summary>
    /// ドラッグ可能な各アイテムのBorderコントロールのリスト
    /// </summary>
    private List<Border> dragableBorders = new();

    /// <summary>
    /// INotifyPropertyChangedを実装するアイテムのイベント購読管理
    /// </summary>
    private Dictionary<object, INotifyPropertyChanged> subscribedItems = new();

    /// <summary>
    /// 実際に適用されるBorderの幅
    /// </summary>
    private double actualBorderWidth = 100.0;

    /// <summary>
    /// ドラッグ中かどうかのフラグ
    /// </summary>
    bool isDragging = false;

    /// <summary>
    /// 現在ドラッグ中のBorderコントロール
    /// </summary>
    Border? draggedBorder = null;

    /// <summary>
    /// ドラッグ中の最後のインデックス位置
    /// </summary>
    private int _lastIndex = -1;

    #endregion

    #region コンストラクタ

    /// <summary>
    /// DragableListの新しいインスタンスを初期化
    /// </summary>
    public DragableList()
    {
        InitializeComponent();
        
        // 初期サイズを設定
        this.Width = BorderWidth;
        this.Height = BorderHeight;
    }

    #endregion

    #region 静的メソッド

    /// <summary>
    /// 依存関係プロパティが変更された時の共通処理
    /// </summary>
    private static void OnAnyDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DragableList dragableList)
        {
            // プロパティ変更時にアイテムを再描画
            dragableList.RefreshItems();
        }
    }

    #endregion

    #region イベント管理

    /// <summary>
    /// ItemsSourceのイベントを購読
    /// </summary>
    /// <param name="collection">購読対象のコレクション</param>
    private void SubscribeToItemsSourceEvents(ObservableCollection<object>? collection)
    {
        if (collection != null)
        {
            // コレクション変更イベントを購読
            collection.CollectionChanged += ItemsSource_CollectionChanged;
            
            // INotifyPropertyChangedを実装するアイテムのPropertyChangedイベントを購読
            foreach (var item in collection)
            {
                SubscribeToItemPropertyChanged(item);
            }
        }
    }

    /// <summary>
    /// ItemsSourceのイベント購読を解除
    /// </summary>
    /// <param name="collection">購読解除対象のコレクション</param>
    private void UnsubscribeFromItemsSourceEvents(ObservableCollection<object>? collection)
    {
        if (collection != null)
        {
            // コレクション変更イベントの購読を解除
            collection.CollectionChanged -= ItemsSource_CollectionChanged;
        }
        
        // 全てのアイテムのPropertyChangedイベントの購読を解除
        foreach (var kvp in subscribedItems)
        {
            kvp.Value.PropertyChanged -= Item_PropertyChanged;
        }
        subscribedItems.Clear();
    }

    /// <summary>
    /// 個別アイテムのPropertyChangedイベントを購読
    /// </summary>
    /// <param name="item">購読対象のアイテム</param>
    private void SubscribeToItemPropertyChanged(object item)
    {
        if (item is INotifyPropertyChanged notifyPropertyChanged && !subscribedItems.ContainsKey(item))
        {
            notifyPropertyChanged.PropertyChanged += Item_PropertyChanged;
            subscribedItems[item] = notifyPropertyChanged;
        }
    }

    /// <summary>
    /// 個別アイテムのPropertyChangedイベント購読を解除
    /// </summary>
    /// <param name="item">購読解除対象のアイテム</param>
    private void UnsubscribeFromItemPropertyChanged(object item)
    {
        if (subscribedItems.TryGetValue(item, out var notifyPropertyChanged))
        {
            notifyPropertyChanged.PropertyChanged -= Item_PropertyChanged;
            subscribedItems.Remove(item);
        }
    }

    #endregion

    #region イベントハンドラ

    /// <summary>
    /// ItemsSourceのコレクションが変更された時の処理
    /// </summary>
    private void ItemsSource_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        // 追加されたアイテムの処理
        if (e.NewItems != null)
        {
            foreach (var item in e.NewItems)
            {
                SubscribeToItemPropertyChanged(item);
            }
        }

        // 削除されたアイテムの処理
        if (e.OldItems != null)
        {
            foreach (var item in e.OldItems)
            {
                UnsubscribeFromItemPropertyChanged(item);
            }
        }

        // UIスレッドでUIを更新
        Dispatcher.BeginInvoke(() => RefreshItems());
    }

    /// <summary>
    /// アイテムのプロパティが変更された時の処理
    /// </summary>
    private void Item_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        // 変更されたプロパティが表示テキストに影響するかをチェック
        if (string.IsNullOrEmpty(DisplayMemberPath) || e.PropertyName == DisplayMemberPath || string.IsNullOrEmpty(e.PropertyName))
        {
            // UIスレッドでUIを更新
            Dispatcher.BeginInvoke(() => UpdateItemDisplay(sender));
        }
    }

    /// <summary>
    /// 順序変更イベントを発生させる
    /// </summary>
    protected virtual void OnOrderChanged()
    {
        OrderChanged?.Invoke(this, SortedItemsSource);
    }

    #endregion

    #region 表示テキストとUI更新メソッド

    /// <summary>
    /// DisplayMemberPathまたはToString()を使用してアイテムの表示テキストを取得
    /// </summary>
    /// <param name="item">表示テキストを取得するアイテム</param>
    /// <returns>表示テキスト</returns>
    private string GetDisplayText(object? item)
    {
        if (item == null)
            return string.Empty;

        if (string.IsNullOrEmpty(DisplayMemberPath))
            return item.ToString() ?? string.Empty;

        try
        {
            // リフレクションを使用してプロパティ値を取得
            var property = item.GetType().GetProperty(DisplayMemberPath);
            if (property != null)
            {
                var value = property.GetValue(item);
                return value?.ToString() ?? string.Empty;
            }
        }
        catch (Exception)
        {
            // プロパティアクセスに失敗した場合はToString()にフォールバック
        }

        return item.ToString() ?? string.Empty;
    }

    /// <summary>
    /// 変更されたアイテムの表示を更新
    /// </summary>
    /// <param name="changedItem">変更されたアイテム</param>
    private void UpdateItemDisplay(object? changedItem)
    {
        if (changedItem == null || ItemsSource == null)
            return;

        var index = ItemsSource.ToList().IndexOf(changedItem);
        if (index >= 0 && index < dragableBorders.Count)
        {
            var border = dragableBorders[index];
            if (border.Child is TextBlock textBlock)
            {
                textBlock.Text = GetDisplayText(changedItem);
                
                // 自動幅調整が有効な場合は幅を再計算
                if (AutoWidthEnabled)
                {
                    var newWidth = CalculateOptimalWidth();
                    if (Math.Abs(actualBorderWidth - newWidth) > 1.0) // 大きな変更がある場合のみ更新
                    {
                        actualBorderWidth = newWidth;
                        
                        // UserControlの幅を更新
                        this.Width = actualBorderWidth;
                        
                        // 全てのBorderの幅を更新
                        foreach (var dragableBorder in dragableBorders)
                        {
                            dragableBorder.Width = actualBorderWidth;
                        }
                    }
                }
            }
        }
    }

    /// <summary>
    /// アイテムを再描画
    /// </summary>
    private void RefreshItems()
    {
        AddItemsBorder();
    }

    #endregion

    #region 幅計算メソッド

    /// <summary>
    /// テキストの表示幅を測定
    /// </summary>
    /// <param name="text">測定対象のテキスト</param>
    /// <returns>テキストの幅（ピクセル）</returns>
    private double MeasureTextWidth(string text)
    {
        var formattedText = new FormattedText(
            text,
            System.Globalization.CultureInfo.CurrentCulture,
            FlowDirection.LeftToRight,
            new Typeface(FontFamily, FontStyle, FontWeight, FontStretches.Normal),
            FontSize,
            Brushes.Black,
            VisualTreeHelper.GetDpi(this).PixelsPerDip);

        return formattedText.Width;
    }

    /// <summary>
    /// アイテムの最適な幅を計算
    /// </summary>
    /// <returns>最適な幅</returns>
    private double CalculateOptimalWidth()
    {
        if (ItemsSource == null || !ItemsSource.Any())
            return BorderWidth;

        double maxTextWidth = 0;
        foreach (var item in ItemsSource)
        {
            var textWidth = MeasureTextWidth(GetDisplayText(item));
            if (textWidth > maxTextWidth)
                maxTextWidth = textWidth;
        }

        // テキスト幅にパディングを追加
        double totalPadding = BorderPadding.Left + BorderPadding.Right + 10; // 追加マージン
        double calculatedWidth = maxTextWidth + totalPadding;

        // 最小・最大幅の制約を適用
        calculatedWidth = Math.Max(calculatedWidth, MinWidth);
        calculatedWidth = Math.Min(calculatedWidth, MaxWidth);

        return calculatedWidth;
    }

    #endregion

    #region パブリックメソッド

    /// <summary>
    /// リストにアイテムを追加
    /// </summary>
    /// <param name="item">追加するアイテム</param>
    public void AddItem(object item)
    {
        if (ItemsSource != null)
        {
            ItemsSource.Add(item);
            OnOrderChanged();
        }
    }

    /// <summary>
    /// 指定したインデックスのアイテムを削除
    /// </summary>
    /// <param name="index">削除するアイテムのインデックス</param>
    public void RemoveAt(int index)
    {
        if (ItemsSource != null && index >= 0 && index < ItemsSource.Count)
        {
            ItemsSource.RemoveAt(index);
            OnOrderChanged();
        }
    }

    /// <summary>
    /// 指定したアイテムを削除
    /// </summary>
    /// <param name="item">削除するアイテム</param>
    public void Remove(object item)
    {
        if (ItemsSource != null)
        {
            ItemsSource.Remove(item);
            OnOrderChanged();
        }
    }

    /// <summary>
    /// 全てのアイテムをクリア
    /// </summary>
    public void Clear()
    {
        if (ItemsSource != null)
        {
            ItemsSource.Clear();
        }
    }

    #endregion

    #region UI要素作成メソッド

    /// <summary>
    /// ItemsSourceの各アイテムに対応するBorderコントロールを作成・配置
    /// </summary>
    private void AddItemsBorder()
    {
        // 既存のUI要素をクリア
        MainCanvas.Children.Clear();
        dragableBorders.Clear();

        if (ItemsSource is null)
        {
            SortedItemsSource = Enumerable.Empty<object>();
            // UserControlの幅を最小値に設定
            this.Width = MinWidth;
            return;
        }

        // 実際のBorder幅を計算
        actualBorderWidth = AutoWidthEnabled ? CalculateOptimalWidth() : BorderWidth;

        // UserControl自体の幅を設定
        this.Width = actualBorderWidth;

        int index = 0;
        var totalHeight = 0.0;
        
        // 各アイテムに対してBorderコントロールを作成
        foreach (var item in ItemsSource)
        {
            Border border = new()
            {
                Width = actualBorderWidth,
                Height = BorderHeight,
                Background = Brushes.LightGray,
                BorderBrush = Brushes.Black,
                Cursor = Cursors.Hand,
                Padding = BorderPadding,
                CornerRadius = BorderCornerRadius,
                Tag = index // インデックスを保存
            };
            
            // マウスイベントを設定
            border.MouseLeftButtonDown += Border_MouseLeftButtonDown;
            // コンテキストメニューを設定
            Border_SetContextMenu(border);

            // テキスト表示用のTextBlockを作成
            TextBlock textBlock = new()
            {
                Foreground = Brushes.Black,
                Text = GetDisplayText(item),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                FontSize = FontSize,
                FontFamily = FontFamily,
                FontWeight = FontWeight,
                FontStyle = FontStyle
            };

            border.Child = textBlock;
            
            // Canvas上での位置を計算・設定
            var topPosition = index * (BorderMargin + BorderHeight);
            Canvas.SetTop(border, topPosition);
            
            // リストとCanvasに追加
            dragableBorders.Add(border);
            MainCanvas.Children.Add(border);

            totalHeight = topPosition + BorderHeight;
            index++;
        }
        
        // UserControlの高さも設定
        this.Height = Math.Max(totalHeight, BorderHeight);
        
        // ソート済みアイテムソースを更新
        SortedItemsSource = ItemsSource.ToList();
    }

    #endregion

    #region ドラッグ&ドロップ処理

    /// <summary>
    /// 指定した要素を指定位置にアニメーション移動
    /// </summary>
    /// <param name="element">移動対象の要素</param>
    /// <param name="toY">移動先のY座標</param>
    private void AnimateTo(UIElement element, double toY)
    {
        var animation = new DoubleAnimation
        {
            To = toY,
            Duration = new Duration(TimeSpan.FromMilliseconds(200)),
            EasingFunction = new QuadraticEase { EasingMode = EasingMode.EaseOut }
        };
        element.BeginAnimation(Canvas.TopProperty, animation);
    }

    /// <summary>
    /// ドラッグ中にアイテムの位置を再配置
    /// </summary>
    /// <param name="oldDraggedIndex">ドラッグ開始時のインデックス</param>
    /// <param name="newDraggedIndex">新しいインデックス</param>
    private void RearrangeItems(int oldDraggedIndex, int newDraggedIndex)
    {
        for (var i = 0; i < dragableBorders.Count; i++)
        {
            var item = dragableBorders[i];
            if (item == draggedBorder) continue; // ドラッグ中のアイテムはスキップ

            double targetY;
            int currentIndex = i;

            if (oldDraggedIndex < newDraggedIndex) // 下方向にドラッグ
            {
                if (currentIndex > oldDraggedIndex && currentIndex <= newDraggedIndex)
                {
                    targetY = (currentIndex - 1) * (BorderHeight + BorderMargin);
                }
                else
                {
                    targetY = currentIndex * (BorderHeight + BorderMargin);
                }
            }
            else // 上方向にドラッグ
            {
                if (currentIndex >= newDraggedIndex && currentIndex < oldDraggedIndex)
                {
                    targetY = (currentIndex + 1) * (BorderHeight + BorderMargin);
                }
                else
                {
                    targetY = currentIndex * (BorderHeight + BorderMargin);
                }
            }
            AnimateTo(item, targetY);
        }
    }

    /// <summary>
    /// Borderのマウス左ボタン押下時の処理（ドラッグ開始）
    /// </summary>
    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border border)
            return;

        // マウスキャプチャとドラッグ状態の設定
        border.CaptureMouse();
        isDragging = true;
        draggedBorder = border;
        _lastIndex = dragableBorders.IndexOf(border);

        // 既存のアニメーションを停止してマニュアル制御を可能にする
        border.BeginAnimation(Canvas.TopProperty, null);

        // ドラッグ中の視覚効果を設定
        Panel.SetZIndex(border, 1); // 最前面に表示
        border.Effect = new DropShadowEffect
        {
            ShadowDepth = 4,
            Direction = 330,
            Color = Colors.Black,
            Opacity = 0.5,
            BlurRadius = 4
        };

        // 拡大効果を追加
        var scaleTransform = new ScaleTransform(1.1, 1.1);
        border.RenderTransformOrigin = new Point(0.5, 0.5);
        border.RenderTransform = scaleTransform;

        // マウスイベントを購読
        this.MouseMove += UserControl_MouseMove;
        this.MouseLeftButtonUp += UserControl_MouseLeftButtonUp;
    }

    /// <summary>
    /// マウス移動時の処理（ドラッグ中）
    /// </summary>
    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        if (isDragging && draggedBorder is Border border)
        {
            // マウス位置を取得してBorderを追従させる
            Point position = e.GetPosition(MainCanvas);
            Canvas.SetLeft(border, position.X - border.ActualWidth / 2);
            Canvas.SetTop(border, position.Y - border.ActualHeight / 2);

            // 新しいインデックス位置を計算
            int oldIndex = dragableBorders.IndexOf(border);
            int newIndex = (int)Math.Clamp(Math.Round((position.Y - border.ActualHeight / 2) / (BorderHeight + BorderMargin)), 0, dragableBorders.Count - 1);

            // インデックスが変更された場合はアイテムを再配置
            if (newIndex != _lastIndex)
            {
                RearrangeItems(oldIndex, newIndex);
                _lastIndex = newIndex;
            }
        }
    }

    /// <summary>
    /// マウス左ボタン離上時の処理（ドラッグ終了）
    /// </summary>
    private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (isDragging && draggedBorder is Border border)
        {
            // ドラッグ状態の終了
            border.ReleaseMouseCapture();
            isDragging = false;

            // 視覚効果をリセット
            border.Effect = null;
            Panel.SetZIndex(border, 0);

            // スケール変換をアニメーションで元に戻す
            if (border.RenderTransform is ScaleTransform scaleTransform)
            {
                var animX = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(100));
                var animY = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(100));
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animX);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animY);
            }

            // 順序変更の確定処理
            int oldIndex = dragableBorders.IndexOf(border);
            int newIndex = _lastIndex;

            Canvas.SetLeft(border, 0); // X座標をリセット

            bool orderChanged = oldIndex != newIndex;

            if (orderChanged)
            {
                // dragableBordersリストの順序を更新
                dragableBorders.RemoveAt(oldIndex);
                dragableBorders.Insert(newIndex, border);
                
                // ItemsSourceの順序を更新
                var reorderedItems = dragableBorders.Select(b => ItemsSource.ElementAt((int)b.Tag)).ToList();
                ItemsSource.Clear();
                foreach (var item in reorderedItems)
                {
                    ItemsSource.Add(item);
                }
            }

            // 全てのBorderの位置とTagを更新
            for (int i = 0; i < dragableBorders.Count; i++)
            {
                dragableBorders[i].Tag = i; // 新しい位置を反映
                AnimateTo(dragableBorders[i], i * (BorderMargin + BorderHeight));
            }

            // 順序変更イベントを発生
            if (orderChanged)
            {
                SortedItemsSource = ItemsSource.ToList();
                OnOrderChanged();
            }

            // ドラッグ状態をリセット
            draggedBorder = null;
            _lastIndex = -1;
        }
        
        // マウスイベントの購読を解除
        this.MouseMove -= UserControl_MouseMove;
        this.MouseLeftButtonUp -= UserControl_MouseLeftButtonUp;
    }

    /// <summary>
    /// Borderのコンテキストメニューを設定
    /// </summary>
    /// <param name="border">コンテキストメニューを設定するBorder</param>
    private void Border_SetContextMenu(Border border)
    {
        ContextMenu contextMenu = new ContextMenu();
        MenuItem removeItem = new MenuItem { Header = "削除" };
        removeItem.Click += (s, args) => 
        {
            var index = (int)border.Tag;
            if (index >= 0 && index < ItemsSource.Count)
            {
                RemoveAt(index);
            }
        };
        contextMenu.Items.Add(removeItem);
        border.ContextMenu = contextMenu;
    }

    #endregion
}

