# DragableList - DisplayMemberPath �@�\

## �T�v
`DragableList` �R���g���[���� `DisplayMemberPath` �v���p�e�B��ǉ����܂����B����ɂ��AListView �Ɠ��l�ɁA�I�u�W�F�N�g�̓���̃v���p�e�B��\���p�̃e�L�X�g�Ƃ��Ďw�肷�邱�Ƃ��ł��܂��B

## �g�p���@

### 1. �P���ȕ�����̏ꍇ�i�]���ʂ�j
```xml
<uc:DragableList ItemsSource="{Binding StringCollection}" />
```
���̏ꍇ�A�e�I�u�W�F�N�g�� `ToString()` ���\�b�h���g�p����܂��B

### 2. DisplayMemberPath���g�p����ꍇ
```xml
<uc:DragableList 
    ItemsSource="{Binding VariableCollection}" 
    DisplayMemberPath="DisplayText" />
```
���̏ꍇ�A�e�I�u�W�F�N�g�� `DisplayText` �v���p�e�B�̒l���\������܂��B

## �����ڍ�

### �V�����v���p�e�B
- `DisplayMemberPath` (string): �\���Ɏg�p����v���p�e�B�̃p�X���w��

### ����
1. `DisplayMemberPath` ���ݒ肳��Ă��Ȃ��ꍇ�F�]���ʂ� `ToString()` ���g�p
2. `DisplayMemberPath` ���ݒ肳��Ă���ꍇ�F���t���N�V�������g�p���Ďw�肳�ꂽ�v���p�e�B�̒l���擾
3. �v���p�e�B��������Ȃ��ꍇ�F�t�H�[���o�b�N�Ƃ��� `ToString()` ���g�p

### ��
```csharp
public class VariableDisplayItem
{
    public string Name { get; set; }
    public string Type { get; set; }
    public int BitWidth { get; set; }
    
    public string DisplayText => $"{Type}: {Name} ({BitWidth} bits)";
}
```

��L�̃N���X���g�p����ꍇ�F
```xml
<uc:DragableList 
    ItemsSource="{Binding VariableItems}" 
    DisplayMemberPath="DisplayText" />
```

����ɂ��A�e�A�C�e���� "Wire: clk (1 bits)" �̂悤�Ȍ`���ŕ\������܂��B