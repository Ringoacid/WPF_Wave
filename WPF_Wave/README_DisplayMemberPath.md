# DragableList - DisplayMemberPath 機能

## 概要
`DragableList` コントロールに `DisplayMemberPath` プロパティを追加しました。これにより、ListView と同様に、オブジェクトの特定のプロパティを表示用のテキストとして指定することができます。

## 使用方法

### 1. 単純な文字列の場合（従来通り）
```xml
<uc:DragableList ItemsSource="{Binding StringCollection}" />
```
この場合、各オブジェクトの `ToString()` メソッドが使用されます。

### 2. DisplayMemberPathを使用する場合
```xml
<uc:DragableList 
    ItemsSource="{Binding VariableCollection}" 
    DisplayMemberPath="DisplayText" />
```
この場合、各オブジェクトの `DisplayText` プロパティの値が表示されます。

## 実装詳細

### 新しいプロパティ
- `DisplayMemberPath` (string): 表示に使用するプロパティのパスを指定

### 動作
1. `DisplayMemberPath` が設定されていない場合：従来通り `ToString()` を使用
2. `DisplayMemberPath` が設定されている場合：リフレクションを使用して指定されたプロパティの値を取得
3. プロパティが見つからない場合：フォールバックとして `ToString()` を使用

### 例
```csharp
public class VariableDisplayItem
{
    public string Name { get; set; }
    public string Type { get; set; }
    public int BitWidth { get; set; }
    
    public string DisplayText => $"{Type}: {Name} ({BitWidth} bits)";
}
```

上記のクラスを使用する場合：
```xml
<uc:DragableList 
    ItemsSource="{Binding VariableItems}" 
    DisplayMemberPath="DisplayText" />
```

これにより、各アイテムは "Wire: clk (1 bits)" のような形式で表示されます。