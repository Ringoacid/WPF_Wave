# MainWindowViewModel �R�����g�ǉ�������

## �T�v
MainWindowViewModel.cs�t�@�C���ɕ�I�ȓ��{��R�����g��ǉ����A�R�[�h�̉ǐ��ƕێ琫��啝�Ɍ��コ���܂����B

## �ǉ������R�����g�̏ڍ�

### 1. �N���X���x����XML�h�L�������g�R�����g

#### ModuleTreeNode �N���X
```csharp
/// <summary>
/// ���W���[���̃c���[�\����\�����邽�߂̃m�[�h�N���X
/// VCD�t�@�C������ǂݍ��񂾃��W���[���K�w��TreeView�ŕ\�����邽�߂Ɏg�p
/// </summary>
```
- �ړI�Ɨp�r�𖾊m��
- VCD�t�@�C���Ƃ̊֘A�������
- TreeView�Ƃ̘A�g�ɂ��ċL�q

#### VariableDisplayItem �N���X
```csharp
/// <summary>
/// �ϐ��i�M���j�̕\���p�A�C�e���N���X
/// VCD�t�@�C���̕ϐ�����UI�ɕ\�����邽�߂̃v���p�e�B�ύX�ʒm�@�\�t�����b�p�[
/// </summary>
```
- MVVM�p�^�[���ł̖����𖾊m��
- �v���p�e�B�ύX�ʒm�@�\�̏d�v��������

#### MainWindowViewModel �N���X
```csharp
/// <summary>
/// ���C���E�B���h�E�̃r���[���f��
/// VCD�g�`�r���[�A�A�v���P�[�V�����̎�v�ȃr�W�l�X���W�b�N��UI��Ԃ��Ǘ�
/// MVVM�p�^�[���Ɋ�Â���View��Model�𒇉�����������
/// </summary>
```
- �A�[�L�e�N�`����̖����𖾋L
- MVVM�p�^�[���ł̈ʒu�Â������

### 2. ���[�W�����ɂ��_���I�\����

```csharp
#region �e�[�}�֘A
#region VCD�t�@�C���ƃ��W���[���E�M���Ǘ�  
#region �e�X�g�E�f���p�f�[�^
#region �e�X�g�p�R�}���h
```
- �@�\�ʂɃR�[�h�𐮗�
- �i�r�Q�[�V�����̌���
- �ӔC�͈̖͂��m��

### 3. �v���p�e�B�̏ڍא���

#### �e�[�}�֘A�v���p�e�B
```csharp
/// <summary>
/// �_�[�N���[�h���L�����ǂ����̃t���O
/// </summary>
[ObservableProperty]
bool isDarkMode = true;

/// <summary>
/// ���C�g���[�h���L�����ǂ����i�_�[�N���[�h�̋t�j
/// </summary>
public bool IsLightMode => !IsDarkMode;
```

#### VCD�t�@�C���֘A�v���p�e�B
```csharp
/// <summary>
/// ���W���[���̃c���[�\��
/// VCD�t�@�C������ǂݍ��񂾃��W���[���K�w��TreeView�ŕ\�����邽�߂Ɏg�p
/// </summary>
[ObservableProperty]
ObservableCollection<ModuleTreeNode> moduleTree = new();

/// <summary>
/// ���ݓǂݍ��܂�Ă���VCD�t�@�C���̃f�[�^
/// null�̏ꍇ��VCD�t�@�C�����ǂݍ��܂�Ă��Ȃ���Ԃ�����
/// </summary>
Vcd? activeVcd;
```

### 4. ���\�b�h�̏ڍׂȐ���

#### �p�����[�^�Ɩ߂�l�̐���
```csharp
/// <summary>
/// Module�I�u�W�F�N�g����ModuleTreeNode���ċA�I�ɍ쐬
/// VCD�t�@�C������ǂݍ��񂾃��W���[���K�w��TreeView�\���p�ɕϊ�����
/// </summary>
/// <param name="module">�ϊ��Ώۂ�Module�I�u�W�F�N�g</param>
/// <returns>�쐬���ꂽModuleTreeNode</returns>
private ModuleTreeNode CreateModuleTreeNode(Module module)
```

#### �����̖ړI�ƕ���p
```csharp
/// <summary>
/// �I�����ꂽ���W���[���̐M�����X�g���X�V
/// ���ݑI������Ă��郂�W���[���Ɋ܂܂��S�Ă̕ϐ���SignalList�ɔ��f����
/// </summary>
private void UpdateSignalList()
```

### 5. VariableDisplayItem�N���X�̏ڍ׃R�����g

#### �v���p�e�B�ύX�ʒm�p�^�[���̐���
```csharp
public string Name 
{ 
    get => name;
    set
    {
        if (name != value)
        {
            name = value;
            OnPropertyChanged();
            OnPropertyChanged(nameof(DisplayText)); // �\���e�L�X�g���X�V�ʒm
        }
    }
}
```

#### �v�Z�v���p�e�B�̐���
```csharp
/// <summary>
/// �\���p�t�H�[�}�b�g�ς݃e�L�X�g
/// "Type: Name (BitWidth bits)" �̌`���ŕ\��
/// ��: "Wire: clk (1 bits)", "Reg: counter (8 bits)"
/// </summary>
public string DisplayText => $"{Type}: {Name} ({BitWidth} bits)";
```

### 6. RelayCommand���\�b�h�̏ڍא���

#### �e�X�g�p�R�}���h�̖ړI
```csharp
/// <summary>
/// �ŏ��̐M���̃v���p�e�B��ύX����e�X�g�R�}���h
/// �v���p�e�B�ύX�ʒm�̓���m�F�p
/// BitWidth��Type�����݂ɕύX����DragableList�̎����X�V���e�X�g
/// </summary>
[RelayCommand]
public void ModifyFirstSignal()
```

#### �����_���f�[�^�����̐���
```csharp
/// <summary>
/// �����_���ȃe�X�g�M����ǉ�����R�}���h
/// �R���N�V�����ύX�̓���m�F�p
/// �����_���Ȗ��O�A�^�C�v�A�r�b�g���Ń_�~�[�M���𐶐��E�ǉ�
/// </summary>
[RelayCommand]
public void AddTestSignal()
```

### 7. �C�����C���R�����g�ɂ�鏈���̏ڍא���

#### ��������̐���
```csharp
if(App.Current.ThemeMode == ThemeMode.Light)
{
    // ���C�g���[�h����_�[�N���[�h�ɐ؂�ւ�
    App.Current.ThemeMode = ThemeMode.Dark;
    IsDarkMode = true;
}
else
{
    // �_�[�N���[�h���烉�C�g���[�h�ɐ؂�ւ�
    App.Current.ThemeMode = ThemeMode.Light;
    IsDarkMode = false;
}
```

#### �d�v�Ȑ����d�l�̐���
```csharp
// �T�u���W���[���݂̂��ċA�I�ɒǉ��i�ϐ��͊܂߂Ȃ��j
// �ϐ��͑I�����ꂽ���W���[���� SignalList �ŕʓr�\��
foreach (var subModule in module.SubModules)
```

### 8. �e�X�g�f�[�^�̐���

#### �T���v���f�[�^�̖ړI
```csharp
/// <summary>
/// �e�X�g�p�̒P���ȕ�����R���N�V����
/// DragableList�̊�{����m�F�p
/// </summary>
[ObservableProperty]
ObservableCollection<object> hoges = ["a", "b", "c", "d", "longlonglonglong_e"];

/// <summary>
/// �e�X�g�p�̃T���v���M���R���N�V����
/// DragableList��DisplayMemberPath�@�\�ƃv���p�e�B�ύX�ʒm�̃e�X�g�p
/// </summary>
```

## ���P����

### 1. �J�������̌���
- �V�K�J���҂̃I���{�[�f�B���O���ԒZ�k
- �R�[�h���r���[�̌�����
- IntelliSense�ł̏ڍ׏��\��

### 2. �ێ琫�̌���
- �e�N���X�E���\�b�h�̐ӔC�͈͂����m
- VCD�t�@�C����UI�̊֘A�����������₷��
- MVVM�p�^�[���̎����Ӑ}�����m

### 3. �R�[�h�i���̌���
- �r�W�l�X���W�b�N�̈Ӑ}�����m
- �e�X�g�R�[�h�̖ړI���������₷��
- �G���[�n���h�����O�̈Ӑ}�����m

### 4. �A�[�L�e�N�`���̗��𑣐i
- MVVM�p�^�[���̖������S�����m
- �f�[�^�t���[�̗������e��
- UI�X�V���J�j�Y���̗��𑣐i

## �R�����g�L�q���j

### 1. �ړI�u��
- �u�������邩�v�����łȂ��u�Ȃ��������邩�v���L�q
- �r�W�l�X�v���Ƃ̊֘A���𖾋L

### 2. ��̗�̒�
- DisplayText�v���p�e�B�̏o�͗���L��
- �e�X�g�f�[�^�̋�̓I�ȗp�r�����

### 3. �֘A���̖��L
- ���̃N���X��v���p�e�B�Ƃ̊֌W�����
- MVVM�p�^�[���ł̖����𖾊m��

### 4. ����Ǝd�l�̖��L
- null�`�F�b�N�̏d�v��
- �v���p�e�B�ύX�ʒm�̘A�������

���̕�I�ȃR�����g�ǉ��ɂ��AMainWindowViewModel�N���X�͓��{����ł̊J���ɂ����āA��藝�����₷���A�ێ炵�₷���R�[�h�ɂȂ�܂����B