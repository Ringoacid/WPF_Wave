# DragableList Width���̉���

## ���̌���

DragableList��Width��0�ɂȂ��Ă��܂����́A�ȉ��̕����̗v���ɂ����̂ł����F

### 1. UserControl���̂̃T�C�Y�ݒ�s��
- `DragableList.xaml`��UserControl���̂�Width/Height���ݒ肳��Ă��Ȃ�
- ������Canvas�ɂ��T�C�Y�w�肪�Ȃ�
- WPF�̃��C�A�E�g�V�X�e���ł́A�����I�ȃT�C�Y���Ȃ��ƃR���g���[�����K�؂ɕ\������Ȃ�

### 2. �v�Z���ꂽ����UserControl�ɔ��f����Ă��Ȃ�
- `CalculateOptimalWidth()`�Ōv�Z����`actualBorderWidth`�͓�����Border�v�f�ɂ̂ݓK�p
- UserControl���̂�Width�v���p�e�B�͍X�V����Ă��Ȃ�����

### 3. ���C�A�E�g�ݒ�̖��
- MainWindow.xaml��`HorizontalAlignment="Right"`���ݒ肳��Ă������A�K�؂�Width���Ȃ����ߕ\������Ȃ�
- Grid��ColumnDefinition��`Width="*"`�ɂȂ��Ă���AAuto���ɑΉ����Ă��Ȃ�

## ��������������

### 1. UserControl�̃T�C�Y�𓮓I�ɐݒ�
```csharp
// AddItemsBorder()���\�b�h��
this.Width = actualBorderWidth;
this.Height = Math.Max(totalHeight, BorderHeight);

// UpdateItemDisplay()���\�b�h��
if (AutoWidthEnabled)
{
    var newWidth = CalculateOptimalWidth();
    if (Math.Abs(actualBorderWidth - newWidth) > 1.0)
    {
        actualBorderWidth = newWidth;
        this.Width = actualBorderWidth; // UserControl�����X�V
        // �S�Ă�Border�����X�V
    }
}
```

### 2. �����T�C�Y�̐ݒ�
```csharp
public DragableList()
{
    InitializeComponent();
    
    // �����T�C�Y��ݒ�
    this.Width = BorderWidth;
    this.Height = BorderHeight;
}
```

### 3. ���C�A�E�g�̉��P
MainWindow.xaml�ŁF
- Grid��ColumnDefinition��`Width="Auto"`�ɕύX�i�R���e���c�ɍ��킹�Ď����T�C�Y�����j
- `HorizontalAlignment="Right"`���폜
- `VerticalAlignment="Top"`��ݒ�

### 4. ��̃R���N�V�������̑Ή�
```csharp
if (ItemsSource is null)
{
    SortedItemsSource = Enumerable.Empty<object>();
    this.Width = MinWidth; // �ŏ�����ݒ�
    return;
}
```

## ����̉��P�_

### Before�i��莞�j
- UserControl��Width��0
- �R���e���c���\������Ȃ�
- AutoWidthEnabled���@�\���Ȃ�

### After�i�C����j
- UserControl���K�؂ȃT�C�Y�ŕ\��
- �R���e���c�̕��ɉ�����AutoWidth�@�\�����퓮��
- �A�C�e���ǉ�/�폜���̓��I���T�C�Y
- �v���p�e�B�ύX���̎���������

## �ǉ��̍l������

### �p�t�H�[�}���X�œK��
- ���̕ύX��1.0�s�N�Z���ȏ�̍�������ꍇ�̂ݎ��s
- �s�v�ȍČv�Z�������

### ���C�A�E�g�̈��萫
- �����T�C�Y��ݒ肵�ă��C�A�E�g�̈��萫���m��
- Height �����I�Ɍv�Z���ăR���e���c�S�̂��\�������悤�ɂ���

### �����̊g����
- MinWidth/MaxWidth�̐��񂪓K�؂ɓK�p�����
- AutoWidthEnabled�̐؂�ւ�������ɋ@�\����

���̏C���ɂ��ADragableList�͓K�؂ȃT�C�Y�ŕ\������A�R���e���c�ɉ��������I�ȃT�C�Y�������\�ɂȂ�܂����B