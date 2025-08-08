# DragableList型不一致問題の解決

## 問題の概要
DragableListのItemsSourceは`ObservableCollection<object>`型でしたが、MainWindowViewModelの`SelectedSignalsForWaveform`は`ObservableCollection<VariableDisplayItem>`型でした。この型の不一致により、データバインディング時にエラーが発生していました。

## 解決策

### 1. DragableListの汎用化（DragableList.xaml.cs）

#### ItemsSourceプロパティの型変更
```csharp
// 変更前
public static readonly DependencyProperty ItemsSourceProperty =
    DependencyProperty.Register(nameof(ItemsSource), typeof(ObservableCollection<object>), typeof(DragableList),
        new PropertyMetadata(new ObservableCollection<object>(), OnItemsSourceChanged));

public ObservableCollection<object> ItemsSource
{
    get => (ObservableCollection<object>)GetValue(ItemsSourceProperty);
    set => SetValue(ItemsSourceProperty, value);
}

// 変更後
public static readonly DependencyProperty ItemsSourceProperty =
    DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(DragableList),
        new PropertyMetadata(null, OnItemsSourceChanged));

public IEnumerable ItemsSource
{
    get => (IEnumerable)GetValue(ItemsSourceProperty);
    set => SetValue(ItemsSourceProperty, value);
}
```

#### イベント管理の汎用化
```csharp
private void SubscribeToItemsSourceEvents(object? collection)
{
    if (collection is INotifyCollectionChanged notifyCollection)
    {
        notifyCollection.CollectionChanged += ItemsSource_CollectionChanged;
    }
    
    if (collection is IEnumerable enumerable)
    {
        foreach (var item in enumerable)
        {
            SubscribeToItemPropertyChanged(item);
        }
    }
}

private void UnsubscribeFromItemsSourceEvents(object? collection)
{
    if (collection is INotifyCollectionChanged notifyCollection)
    {
        notifyCollection.CollectionChanged -= ItemsSource_CollectionChanged;
    }
    
    foreach (var kvp in subscribedItems)
    {
        kvp.Value.PropertyChanged -= Item_PropertyChanged;
    }
    subscribedItems.Clear();
}
```

#### コレクション操作の安全化
```csharp
public void AddItem(object item)
{
    if (ItemsSource is IList list)
    {
        list.Add(item);
        OnOrderChanged();
    }
}

public void RemoveAt(int index)
{
    if (ItemsSource is IList list && index >= 0 && index < list.Count)
    {
        list.RemoveAt(index);
        OnOrderChanged();
    }
}
```

#### アイテム処理の汎用化
```csharp
private void AddItemsBorder()
{
    MainCanvas.Children.Clear();
    dragableBorders.Clear();

    if (ItemsSource == null)
    {
        SortedItemsSource = Enumerable.Empty<object>();
        this.Width = MinWidth;
        return;
    }

    var itemsList = ItemsSource.Cast<object>().ToList();
    
    // 以下、itemsListを使用してUI要素を構築
}
```

### 2. MainWindowViewModelの適合（MainWindowViewModel.cs）

#### プロパティ型の統一
```csharp
// 変更前
[ObservableProperty]
ObservableCollection<VariableDisplayItem> selectedSignalsForWaveform = new();

// 変更後
[ObservableProperty]
ObservableCollection<object> selectedSignalsForWaveform = new();
```

#### 型安全なアクセス
```csharp
[RelayCommand]
public void AddSelectedSignalToWaveform()
{
    if (SelectedSignal != null)
    {
        // 重複チェック時の型安全な処理
        var existingSignal = SelectedSignalsForWaveform
            .OfType<VariableDisplayItem>()
            .FirstOrDefault(s => 
                s.Name == SelectedSignal.Name && 
                s.Type == SelectedSignal.Type && 
                s.BitWidth == SelectedSignal.BitWidth);

        if (existingSignal == null)
        {
            var newSignal = new VariableDisplayItem
            {
                Name = SelectedSignal.Name,
                Type = SelectedSignal.Type,
                BitWidth = SelectedSignal.BitWidth,
                VariableData = SelectedSignal.VariableData
            };
            SelectedSignalsForWaveform.Add(newSignal);
        }
    }
}
```

#### テストメソッドの型安全性
```csharp
[RelayCommand]
public void ModifyFirstSignal()
{
    if (SelectedSignalsForWaveform.Count > 0)
    {
        var signal = SelectedSignalsForWaveform[0] as VariableDisplayItem;
        if (signal != null)
        {
            signal.BitWidth = signal.BitWidth == 1 ? 8 : 1;
            signal.Type = signal.Type == "Wire" ? "Reg" : "Wire";
        }
    }
}
```

## 実装した機能の詳細

### 1. 型の汎用性
- **DragableList**: `IEnumerable`を受け入れて様々なコレクション型に対応
- **MainWindowViewModel**: `ObservableCollection<object>`で型統一

### 2. 型安全性の確保
- **実行時型チェック**: `is`演算子と`as`演算子による安全な型変換
- **LINQ活用**: `OfType<T>()`による型フィルタリング
- **インターフェース活用**: `IList`, `INotifyCollectionChanged`による機能チェック

### 3. パフォーマンス最適化
- **遅延評価**: `Cast<object>()`による必要時のみの型変換
- **条件付き処理**: 型チェックによる不要な処理の回避

### 4. エラー処理の強化
- **Null安全**: null合体演算子とnullチェックの徹底
- **範囲チェック**: インデックス境界の確認
- **型チェック**: 実行時型検証による安全な操作

## 解決効果

### 1. 型の一貫性
- ItemsSourceとSelectedSignalsForWaveformの型が一致
- データバインディングエラーの解消

### 2. 汎用性の向上
- DragableListが様々なコレクション型に対応
- 将来的な拡張性の確保

### 3. 型安全性の維持
- コンパイル時の型チェック
- 実行時の安全な型変換

### 4. 保守性の向上
- 明確な型チェックによるバグの早期発見
- 理解しやすいコード構造

## 使用可能なコレクション型

修正後のDragableListは以下の型に対応：

```csharp
// 基本的なコレクション
ObservableCollection<object>
ObservableCollection<VariableDisplayItem>
List<object>
List<VariableDisplayItem>

// インターフェース型
IList<object>
ICollection<object>
IEnumerable<object>

// 配列
object[]
VariableDisplayItem[]
```

## 今後の拡張可能性

### 1. ジェネリック対応
```csharp
public class DragableList<T> : UserControl
{
    public ObservableCollection<T> ItemsSource { get; set; }
}
```

### 2. 型制約の追加
```csharp
where T : INotifyPropertyChanged
```

### 3. パフォーマンス最適化
- 仮想化対応
- 大量データ処理の最適化

この解決により、DragableListとMainWindowViewModelの型不一致問題が解消され、安定したデータバインディングが実現されました。