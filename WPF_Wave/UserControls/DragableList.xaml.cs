    using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Effects;

namespace WPF_Wave.UserControls;

/// <summary>
/// DragableList.xaml の相互作用ロジック
/// </summary>
public partial class DragableList : UserControl
{
    #region Dependency Properties
    public static readonly DependencyProperty ItemsSourceProperty =
        DependencyProperty.Register(nameof(ItemsSource), typeof(ObservableCollection<object>), typeof(DragableList),
            new PropertyMetadata(new ObservableCollection<object>(), OnItemsSourceChanged));

    public ObservableCollection<object> ItemsSource
    {
        get => (ObservableCollection<object>)GetValue(ItemsSourceProperty);
        set => SetValue(ItemsSourceProperty, value);
    }

    private static void OnItemsSourceChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DragableList dragableList)
        {
            if (e.OldValue is ObservableCollection<object> oldCollection)
            {
                oldCollection.CollectionChanged -= dragableList.ItemsSource_CollectionChanged;
            }
            if (e.NewValue is ObservableCollection<object> newCollection)
            {
                newCollection.CollectionChanged += dragableList.ItemsSource_CollectionChanged;
            }
            OnAnyDependencyPropertyChanged(d, e);
        }
    }

    private void ItemsSource_CollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        OnAnyDependencyPropertyChanged(this, new DependencyPropertyChangedEventArgs(ItemsSourceProperty, ItemsSource, ItemsSource));
    }

    public static readonly DependencyProperty BorderWidthProperty =
        DependencyProperty.Register(nameof(BorderWidth), typeof(double), typeof(DragableList),
            new PropertyMetadata(300.0, OnAnyDependencyPropertyChanged));

    public double BorderWidth
    {
        get => (double)GetValue(BorderWidthProperty);
        set => SetValue(BorderWidthProperty, value);
    }

    public static readonly DependencyProperty AutoWidthEnabledProperty =
        DependencyProperty.Register(nameof(AutoWidthEnabled), typeof(bool), typeof(DragableList),
            new PropertyMetadata(true, OnAnyDependencyPropertyChanged));

    public bool AutoWidthEnabled
    {
        get => (bool)GetValue(AutoWidthEnabledProperty);
        set => SetValue(AutoWidthEnabledProperty, value);
    }

    public static readonly DependencyProperty MinWidthProperty =
        DependencyProperty.Register(nameof(MinWidth), typeof(double), typeof(DragableList),
            new PropertyMetadata(50.0, OnAnyDependencyPropertyChanged));

    public double MinWidth
    {
        get => (double)GetValue(MinWidthProperty);
        set => SetValue(MinWidthProperty, value);
    }

    public static readonly DependencyProperty MaxWidthProperty =
        DependencyProperty.Register(nameof(MaxWidth), typeof(double), typeof(DragableList),
            new PropertyMetadata(500.0, OnAnyDependencyPropertyChanged));

    public double MaxWidth
    {
        get => (double)GetValue(MaxWidthProperty);
        set => SetValue(MaxWidthProperty, value);
    }

    public static readonly DependencyProperty FontSizeProperty =
        DependencyProperty.Register(nameof(FontSize), typeof(double), typeof(DragableList),
            new PropertyMetadata(14.0, OnAnyDependencyPropertyChanged));

    public double FontSize
    {
        get => (double)GetValue(FontSizeProperty);
        set => SetValue(FontSizeProperty, value);
    }

    public static readonly DependencyProperty FontFamilyProperty =
        DependencyProperty.Register(nameof(FontFamily), typeof(FontFamily), typeof(DragableList),
            new PropertyMetadata(new FontFamily("Segoe UI"), OnAnyDependencyPropertyChanged));

    public FontFamily FontFamily
    {
        get => (FontFamily)GetValue(FontFamilyProperty);
        set => SetValue(FontFamilyProperty, value);
    }

    public static readonly DependencyProperty FontWeightProperty =
        DependencyProperty.Register(nameof(FontWeight), typeof(FontWeight), typeof(DragableList),
            new PropertyMetadata(FontWeights.Normal, OnAnyDependencyPropertyChanged));

    public FontWeight FontWeight
    {
        get => (FontWeight)GetValue(FontWeightProperty);
        set => SetValue(FontWeightProperty, value);
    }

    public static readonly DependencyProperty FontStyleProperty =
        DependencyProperty.Register(nameof(FontStyle), typeof(FontStyle), typeof(DragableList),
            new PropertyMetadata(FontStyles.Normal, OnAnyDependencyPropertyChanged));

    public FontStyle FontStyle
    {
        get => (FontStyle)GetValue(FontStyleProperty);
        set => SetValue(FontStyleProperty, value);
    }

    public static readonly DependencyProperty BorderHeightProperty =
        DependencyProperty.Register(nameof(BorderHeight), typeof(double), typeof(DragableList),
            new PropertyMetadata(50.0, OnAnyDependencyPropertyChanged));

    public double BorderHeight
    {
        get => (double)GetValue(BorderHeightProperty);
        set => SetValue(BorderHeightProperty, value);
    }


    public static readonly DependencyProperty BorderPaddingProperty =
        DependencyProperty.Register(nameof(BorderPadding), typeof(Thickness), typeof(DragableList),
            new PropertyMetadata(new Thickness(10d), OnAnyDependencyPropertyChanged));

    public Thickness BorderPadding
    {
        get => (Thickness)GetValue(BorderPaddingProperty);
        set => SetValue(BorderPaddingProperty, value);
    }


    public static readonly DependencyProperty BorderMarginProperty =
        DependencyProperty.Register(nameof(BorderMargin), typeof(double), typeof(DragableList),
            new PropertyMetadata(5.0, OnAnyDependencyPropertyChanged));

    public double BorderMargin
    {
        get => (double)GetValue(BorderMarginProperty);
        set => SetValue(BorderMarginProperty, value);
    }

    public static readonly DependencyProperty BorderCornerRadiusProperty =
        DependencyProperty.Register(nameof(BorderCornerRadius), typeof(CornerRadius), typeof(DragableList),
            new PropertyMetadata(new CornerRadius(8.0), OnAnyDependencyPropertyChanged));

    public CornerRadius BorderCornerRadius
    {
        get => (CornerRadius)GetValue(BorderCornerRadiusProperty);
        set => SetValue(BorderCornerRadiusProperty, value);
    }
    #endregion

    public IEnumerable<object> SortedItemsSource { get; private set; } = [];

    public delegate void OrderChangedEventHandler(object sender, IEnumerable<object> SortedItemsSource);

    public event OrderChangedEventHandler? OrderChanged;

    private static void OnAnyDependencyPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is DragableList dragableList)
        {
            dragableList.AddItemsBorder();
        }
    }

    protected virtual void OnOrderChanged()
    {
        OrderChanged?.Invoke(this, SortedItemsSource);
    }

    private List<Border> dragableBorders = new();
    private double actualBorderWidth = 100.0;

    public DragableList()
    {
        InitializeComponent();
    }

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

    private double CalculateOptimalWidth()
    {
        if (ItemsSource == null || !ItemsSource.Any())
            return BorderWidth;

        double maxTextWidth = 0;
        foreach (var item in ItemsSource)
        {
            var textWidth = MeasureTextWidth(item.ToString() ?? "");
            if (textWidth > maxTextWidth)
                maxTextWidth = textWidth;
        }

        // Add padding to the text width
        double totalPadding = BorderPadding.Left + BorderPadding.Right + 10; // Extra margin
        double calculatedWidth = maxTextWidth + totalPadding;

        // Apply min/max constraints
        calculatedWidth = Math.Max(calculatedWidth, MinWidth);
        calculatedWidth = Math.Min(calculatedWidth, MaxWidth);

        return calculatedWidth;
    }

    public void AddItem(object item)
    {
        ObservableCollection<object> newValue = new(SortedItemsSource);
        newValue.Add(item);
        ItemsSource = newValue;
        OnOrderChanged();
    }

    public void RemoveAt(int index)
    {
        ObservableCollection<object> newValue = new(SortedItemsSource);
        newValue.RemoveAt(index);
        ItemsSource = newValue;
        OnOrderChanged();
    }

    public void Remove(object item)
    {
        ObservableCollection<object> newValue = new(SortedItemsSource);
        newValue.Remove(item);
        ItemsSource = newValue;
        OnOrderChanged();
    }

    public void Clear()
    {
        ItemsSource = new ObservableCollection<object>();
    }


    private void AddItemsBorder()
    {
        MainCanvas.Children.Clear();
        dragableBorders.Clear();

        if (ItemsSource is null)
        {
            SortedItemsSource = Enumerable.Empty<object>();
            return;
        }

        // Calculate the actual border width
        actualBorderWidth = AutoWidthEnabled ? CalculateOptimalWidth() : BorderWidth;

        int index = 0;
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
                Tag = index
            };
            border.MouseLeftButtonDown += Border_MouseLeftButtonDown;
            Border_SetContextMenu(border);

            TextBlock textBlock = new()
            {
                Foreground = Brushes.Black,
                Text = item.ToString(),
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
                TextWrapping = TextWrapping.Wrap,
                FontSize = FontSize,
                FontFamily = FontFamily,
                FontWeight = FontWeight,
                FontStyle = FontStyle
            };

            border.Child = textBlock;
            Canvas.SetTop(border, index * (BorderMargin + BorderHeight));
            dragableBorders.Add(border);
            MainCanvas.Children.Add(border);

            index++;
        }
        SortedItemsSource = [ItemsSource];
    }

    bool isDragging = false;
    Border? draggedBorder = null;
    private int _lastIndex = -1;

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

    private void RearrangeItems(int oldDraggedIndex, int newDraggedIndex)
    {
        for (var i = 0; i < dragableBorders.Count; i++)
        {
            var item = dragableBorders[i];
            if (item == draggedBorder) continue;

            double targetY;
            int currentIndex = i;

            if (oldDraggedIndex < newDraggedIndex) // Dragging down
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
            else // Dragging up
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

    private void Border_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
    {
        if (sender is not Border border)
            return;

        border.CaptureMouse();
        isDragging = true;
        draggedBorder = border;
        _lastIndex = dragableBorders.IndexOf(border);

        // Remove any existing animations from the Top property to allow setting it manually
        border.BeginAnimation(Canvas.TopProperty, null);

        Panel.SetZIndex(border, 1);
        border.Effect = new DropShadowEffect
        {
            ShadowDepth = 4,
            Direction = 330,
            Color = Colors.Black,
            Opacity = 0.5,
            BlurRadius = 4
        };

        var scaleTransform = new ScaleTransform(1.1, 1.1);
        border.RenderTransformOrigin = new Point(0.5, 0.5);
        border.RenderTransform = scaleTransform;

        this.MouseMove += UserControl_MouseMove;
        this.MouseLeftButtonUp += UserControl_MouseLeftButtonUp;
    }

    private void UserControl_MouseMove(object sender, MouseEventArgs e)
    {
        if (isDragging && draggedBorder is Border border)
        {
            Point position = e.GetPosition(MainCanvas);
            Canvas.SetLeft(border, position.X - border.ActualWidth / 2);
            Canvas.SetTop(border, position.Y - border.ActualHeight / 2);

            int oldIndex = dragableBorders.IndexOf(border);
            int newIndex = (int)Math.Clamp(Math.Round((position.Y - border.ActualHeight / 2) / (BorderHeight + BorderMargin)), 0, dragableBorders.Count - 1);

            if (newIndex != _lastIndex)
            {
                RearrangeItems(oldIndex, newIndex);
                _lastIndex = newIndex;
            }
        }
    }

    private void UserControl_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
    {
        if (isDragging && draggedBorder is Border border)
        {
            border.ReleaseMouseCapture();
            isDragging = false;

            border.Effect = null;
            Panel.SetZIndex(border, 0);

            if (border.RenderTransform is ScaleTransform scaleTransform)
            {
                var animX = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(100));
                var animY = new DoubleAnimation(1.0, TimeSpan.FromMilliseconds(100));
                scaleTransform.BeginAnimation(ScaleTransform.ScaleXProperty, animX);
                scaleTransform.BeginAnimation(ScaleTransform.ScaleYProperty, animY);
            }

            int oldIndex = dragableBorders.IndexOf(border);
            int newIndex = _lastIndex;

            Canvas.SetLeft(border, 0);

            bool orderChanged = oldIndex != newIndex;

            if (orderChanged)
            {
                dragableBorders.RemoveAt(oldIndex);
                dragableBorders.Insert(newIndex, border);
            }

            for (int i = 0; i < dragableBorders.Count; i++)
            {
                AnimateTo(dragableBorders[i], i * (BorderMargin + BorderHeight));
            }

            if (orderChanged)
            {
                var sourceList = ItemsSource.ToList();
                SortedItemsSource = dragableBorders.Select(b => sourceList[(int)b.Tag]).ToList();

                OnOrderChanged();
            }

            draggedBorder = null;
            _lastIndex = -1;
        }
        this.MouseMove -= UserControl_MouseMove;
        this.MouseLeftButtonUp -= UserControl_MouseLeftButtonUp;
    }


    private void Border_SetContextMenu(Border border)
    {
        ContextMenu contextMenu = new ContextMenu();
        MenuItem removeItem = new MenuItem { Header = "削除" };
        removeItem.Click += (s, args) => Remove(ItemsSource[(int)border.Tag]);
        contextMenu.Items.Add(removeItem);
        border.ContextMenu = contextMenu;
    }
}

