# DragableList - ItemsSource�ύX�Ή��̉��P

## �����������P�_

### 1. INotifyPropertyChanged�Ή�
- **�ʃA�C�e���̃v���p�e�B�ύX�Ď�**: ItemsSource�̊e�v�f��INotifyPropertyChanged���������Ă���ꍇ�A����PropertyChanged�C�x���g���Ď�
- **�\���̎����X�V**: DisplayMemberPath�Ŏw�肳�ꂽ�v���p�e�B���ύX���ꂽ�ꍇ�A�Y������A�C�e���̕\���������X�V
- **�����I�ȍX�V**: �ύX���ꂽ�A�C�e���݂̂��X�V���A�S�̂̍ĕ`��������

### 2. UI�X���b�h���S���̌���
- **Dispatcher.BeginInvoke�g�p**: CollectionChanged��PropertyChanged�C�x���g���o�b�N�O���E���h�X���b�h���甭�������ꍇ�ł����S��UI�X�V
- **�񓯊��X�V**: UI�X�V������񓯊��Ŏ��s���AUI�̃u���b�L���O��h��

### 3. �C�x���g�Ǘ��̋���
- **�K�؂ȃC�x���g�w��/����**: ItemsSource���ύX���ꂽ�ۂ̓K�؂ȃC�x���g�n���h���̊Ǘ�
- **���������[�N�h�~**: �s�v�ɂȂ����C�x���g�n���h���̊m���ȉ���
- **���I�ȍw�ǊǗ�**: �R���N�V�����ɃA�C�e�����ǉ�/�폜���ꂽ�ۂ̓��I�ȃC�x���g�w�ǊǗ�

### 4. �h���b�O&�h���b�v�̉��P
- **ItemsSource�Ƃ̓���**: �h���b�O&�h���b�v�ɂ����ёւ���ItemsSource�ɐ��������f
- **Tag�̍X�V**: Border�v�f��Tag�v���p�e�B�𐳂����X�V���A�C���f�b�N�X�̐�������ێ�

## �g�p��

### ��{�I�Ȏg�p���@
```xml
<uc:DragableList 
    ItemsSource="{Binding VariableItems}" 
    DisplayMemberPath="DisplayText" />
```

### �v���p�e�B�ύX�ʒm�Ή��̃N���X��
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
                OnPropertyChanged(nameof(DisplayText)); // �ˑ��v���p�e�B���ʒm
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

## �e�X�g�@�\

�A�v���P�[�V�����Ɉȉ��̃e�X�g�{�^�����ǉ�����܂����F

1. **Modify First**: �ŏ��̃A�C�e���̃v���p�e�B��ύX�iBitWidth��Type�j
2. **Add Signal**: �V���������_���ȃV�O�i����ǉ�
3. **Remove Last**: �Ō�̃A�C�e�����폜

�����̃{�^�����g�p���āA�ȉ��̓�����m�F�ł��܂��F
- �ʃA�C�e���̃v���p�e�B�ύX���̎����\���X�V
- �R���N�V�����ւ̃A�C�e���ǉ�/�폜���̎������f
- AutoWidthEnabled���L���ȏꍇ�̎���������

## �Ή�����ύX�̎��

1. **�R���N�V�����̕ύX**
   - Add / Remove / Insert / Clear
   - �����I��UI�v�f�̒ǉ�/�폜

2. **�ʃA�C�e���̃v���p�e�B�ύX**
   - DisplayMemberPath�Ŏw�肳�ꂽ�v���p�e�B�̕ύX
   - �Y������TextBlock�̃e�L�X�g�̂ݍX�V

3. **���̎�������**
   - �e�L�X�g�ύX���̍œK���Čv�Z
   - �K�v�ɉ����đS�Ă̗v�f�̕����X�V