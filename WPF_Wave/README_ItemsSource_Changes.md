# DragableList - ItemsSource変更対応の改善

## 実装した改善点

### 1. INotifyPropertyChanged対応
- **個別アイテムのプロパティ変更監視**: ItemsSourceの各要素がINotifyPropertyChangedを実装している場合、そのPropertyChangedイベントを監視
- **表示の自動更新**: DisplayMemberPathで指定されたプロパティが変更された場合、該当するアイテムの表示を自動更新
- **効率的な更新**: 変更されたアイテムのみを更新し、全体の再描画を避ける

### 2. UIスレッド安全性の向上
- **Dispatcher.BeginInvoke使用**: CollectionChangedやPropertyChangedイベントがバックグラウンドスレッドから発生した場合でも安全にUI更新
- **非同期更新**: UI更新処理を非同期で実行し、UIのブロッキングを防ぐ

### 3. イベント管理の強化
- **適切なイベント購読/解除**: ItemsSourceが変更された際の適切なイベントハンドラの管理
- **メモリリーク防止**: 不要になったイベントハンドラの確実な解除
- **動的な購読管理**: コレクションにアイテムが追加/削除された際の動的なイベント購読管理

### 4. ドラッグ&ドロップの改善
- **ItemsSourceとの同期**: ドラッグ&ドロップによる並び替えをItemsSourceに正しく反映
- **Tagの更新**: Border要素のTagプロパティを正しく更新し、インデックスの整合性を保持

## 使用例

### 基本的な使用方法
```xml
<uc:DragableList 
    ItemsSource="{Binding VariableItems}" 
    DisplayMemberPath="DisplayText" />
```

### プロパティ変更通知対応のクラス例
```csharp
public class VariableDisplayItem : INotifyPropertyChanged
{
    private string name = string.Empty;
    private string type = string.Empty;
    private int bitWidth;

    public string Name 
    { 
        get => name;
        set
        {
            if (name != value)
            {
                name = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(DisplayText)); // 依存プロパティも通知
            }
        }
    }

    public string DisplayText => $"{Type}: {Name} ({BitWidth} bits)";
    
    public event PropertyChangedEventHandler? PropertyChanged;
    
    protected virtual void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
```

## テスト機能

アプリケーションに以下のテストボタンが追加されました：

1. **Modify First**: 最初のアイテムのプロパティを変更（BitWidthとType）
2. **Add Signal**: 新しいランダムなシグナルを追加
3. **Remove Last**: 最後のアイテムを削除

これらのボタンを使用して、以下の動作を確認できます：
- 個別アイテムのプロパティ変更時の自動表示更新
- コレクションへのアイテム追加/削除時の自動反映
- AutoWidthEnabledが有効な場合の自動幅調整

## 対応する変更の種類

1. **コレクションの変更**
   - Add / Remove / Insert / Clear
   - 自動的にUI要素の追加/削除

2. **個別アイテムのプロパティ変更**
   - DisplayMemberPathで指定されたプロパティの変更
   - 該当するTextBlockのテキストのみ更新

3. **幅の自動調整**
   - テキスト変更時の最適幅再計算
   - 必要に応じて全ての要素の幅を更新