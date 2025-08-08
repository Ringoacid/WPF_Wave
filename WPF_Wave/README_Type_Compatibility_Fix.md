# DragableList�^�s��v���̉���

## ���̊T�v
DragableList��ItemsSource��`ObservableCollection<object>`�^�ł������AMainWindowViewModel��`SelectedSignalsForWaveform`��`ObservableCollection<VariableDisplayItem>`�^�ł����B���̌^�̕s��v�ɂ��A�f�[�^�o�C���f�B���O���ɃG���[���������Ă��܂����B

## ������

### 1. DragableList�̔ėp���iDragableList.xaml.cs�j

#### ItemsSource�v���p�e�B�̌^�ύX
```csharp
// �ύX�O
public static readonly DependencyProperty ItemsSourceProperty =
    DependencyProperty.Register(nameof(ItemsSource), typeof(ObservableCollection<object>), typeof(DragableList),
        new PropertyMetadata(new ObservableCollection<object>(), OnItemsSourceChanged));

public ObservableCollection<object> ItemsSource
{
    get => (ObservableCollection<object>)GetValue(ItemsSourceProperty);
    set => SetValue(ItemsSourceProperty, value);
}

// �ύX��
public static readonly DependencyProperty ItemsSourceProperty =
    DependencyProperty.Register(nameof(ItemsSource), typeof(IEnumerable), typeof(DragableList),
        new PropertyMetadata(null, OnItemsSourceChanged));

public IEnumerable ItemsSource
{
    get => (IEnumerable)GetValue(ItemsSourceProperty);
    set => SetValue(ItemsSourceProperty, value);
}
```

#### �C�x���g�Ǘ��̔ėp��
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

#### �R���N�V��������̈��S��
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

#### �A�C�e�������̔ėp��
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
    
    // �ȉ��AitemsList���g�p����UI�v�f���\�z
}
```

### 2. MainWindowViewModel�̓K���iMainWindowViewModel.cs�j

#### �v���p�e�B�^�̓���
```csharp
// �ύX�O
[ObservableProperty]
ObservableCollection<VariableDisplayItem> selectedSignalsForWaveform = new();

// �ύX��
[ObservableProperty]
ObservableCollection<object> selectedSignalsForWaveform = new();
```

#### �^���S�ȃA�N�Z�X
```csharp
[RelayCommand]
public void AddSelectedSignalToWaveform()
{
    if (SelectedSignal != null)
    {
        // �d���`�F�b�N���̌^���S�ȏ���
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

#### �e�X�g���\�b�h�̌^���S��
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

## ���������@�\�̏ڍ�

### 1. �^�̔ėp��
- **DragableList**: `IEnumerable`���󂯓���ėl�X�ȃR���N�V�����^�ɑΉ�
- **MainWindowViewModel**: `ObservableCollection<object>`�Ō^����

### 2. �^���S���̊m��
- **���s���^�`�F�b�N**: `is`���Z�q��`as`���Z�q�ɂ����S�Ȍ^�ϊ�
- **LINQ���p**: `OfType<T>()`�ɂ��^�t�B���^�����O
- **�C���^�[�t�F�[�X���p**: `IList`, `INotifyCollectionChanged`�ɂ��@�\�`�F�b�N

### 3. �p�t�H�[�}���X�œK��
- **�x���]��**: `Cast<object>()`�ɂ��K�v���݂̂̌^�ϊ�
- **�����t������**: �^�`�F�b�N�ɂ��s�v�ȏ����̉��

### 4. �G���[�����̋���
- **Null���S**: null���̉��Z�q��null�`�F�b�N�̓O��
- **�͈̓`�F�b�N**: �C���f�b�N�X���E�̊m�F
- **�^�`�F�b�N**: ���s���^���؂ɂ����S�ȑ���

## ��������

### 1. �^�̈�ѐ�
- ItemsSource��SelectedSignalsForWaveform�̌^����v
- �f�[�^�o�C���f�B���O�G���[�̉���

### 2. �ėp���̌���
- DragableList���l�X�ȃR���N�V�����^�ɑΉ�
- �����I�Ȋg�����̊m��

### 3. �^���S���̈ێ�
- �R���p�C�����̌^�`�F�b�N
- ���s���̈��S�Ȍ^�ϊ�

### 4. �ێ琫�̌���
- ���m�Ȍ^�`�F�b�N�ɂ��o�O�̑�������
- �������₷���R�[�h�\��

## �g�p�\�ȃR���N�V�����^

�C�����DragableList�͈ȉ��̌^�ɑΉ��F

```csharp
// ��{�I�ȃR���N�V����
ObservableCollection<object>
ObservableCollection<VariableDisplayItem>
List<object>
List<VariableDisplayItem>

// �C���^�[�t�F�[�X�^
IList<object>
ICollection<object>
IEnumerable<object>

// �z��
object[]
VariableDisplayItem[]
```

## ����̊g���\��

### 1. �W�F�l���b�N�Ή�
```csharp
public class DragableList<T> : UserControl
{
    public ObservableCollection<T> ItemsSource { get; set; }
}
```

### 2. �^����̒ǉ�
```csharp
where T : INotifyPropertyChanged
```

### 3. �p�t�H�[�}���X�œK��
- ���z���Ή�
- ��ʃf�[�^�����̍œK��

���̉����ɂ��ADragableList��MainWindowViewModel�̌^�s��v��肪��������A���肵���f�[�^�o�C���f�B���O����������܂����B