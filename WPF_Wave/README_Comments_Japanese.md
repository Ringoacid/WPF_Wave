# DragableList �R�����g���{�ꉻ�ƒǉ�

## �T�v
DragableList�N���X�̑S�Ẳp��R�����g����{��ɖ|�󂵁A�R�����g���s�����Ă����ӏ��ɐV�������{��R�����g��ǉ����܂����B

## ���{�����ύX

### 1. �p��R�����g�̓��{��|��

#### Before (�p��)
```csharp
// Set initial size
// Subscribe to PropertyChanged events for items that implement INotifyPropertyChanged
// Handle items being added
// Use reflection to get the property value
// Only update if significant change
// Update UserControl width
// Calculate the actual border width
// Dragging down
// Remove any existing animations from the Top property to allow setting it manually
```

#### After (���{��)
```csharp
// �����T�C�Y��ݒ�
// INotifyPropertyChanged����������A�C�e����PropertyChanged�C�x���g���w��
// �ǉ����ꂽ�A�C�e���̏���
// ���t���N�V�������g�p���ăv���p�e�B�l���擾
// �傫�ȕύX������ꍇ�̂ݍX�V
// UserControl�̕����X�V
// ���ۂ�Border�����v�Z
// �������Ƀh���b�O
// �����̃A�j���[�V�������~���ă}�j���A��������\�ɂ���
```

### 2. �V�K�ǉ������R�����g

#### XML�h�L�������g�R�����g
- �S�Ẵp�u���b�N�v���p�e�B�ƃ��\�b�h��`<summary>`�^�O��ǉ�
- �p�����[�^�Ɩ߂�l�̐�����`<param>`��`<returns>`�^�O�Œǉ�

```csharp
/// <summary>
/// �h���b�O�\�ȃ��X�g�R���g���[��
/// �A�C�e���̕��ёւ��A�ǉ��A�폜���\�Ȑ������X�g
/// </summary>

/// <summary>
/// ���X�g�ɕ\������A�C�e���̃R���N�V����
/// </summary>

/// <summary>
/// �\���Ɏg�p����v���p�e�B�̃p�X�iListView��DisplayMemberPath�Ɠ��l�j
/// </summary>
```

#### ���[�W�����̐���
- `#region`�ŃR�[�h��_���I�ȃO���[�v�ɕ���
- �e���[�W�����ɓK�؂ȓ��{�ꖼ��ݒ�

```csharp
#region �ˑ��֌W�v���p�e�B
#region �p�u���b�N�v���p�e�B�ƃC�x���g  
#region �v���C�x�[�g�t�B�[���h
#region �R���X�g���N�^
#region �ÓI���\�b�h
#region �C�x���g�Ǘ�
#region �C�x���g�n���h��
#region �\���e�L�X�g��UI�X�V���\�b�h
#region ���v�Z���\�b�h
#region �p�u���b�N���\�b�h
#region UI�v�f�쐬���\�b�h
#region �h���b�O&�h���b�v����
```

#### �ڍׂȃC�����C���R�����g
- ���G�ȃ��W�b�N�ɏڍׂȐ�����ǉ�
- �e�����̖ړI�Ɠ���𖾊m��

```csharp
// �}�E�X�L���v�`���ƃh���b�O��Ԃ̐ݒ�
// �h���b�O���̎��o���ʂ�ݒ�
// �g����ʂ�ǉ�
// �V�����C���f�b�N�X�ʒu���v�Z
// �C���f�b�N�X���ύX���ꂽ�ꍇ�̓A�C�e�����Ĕz�u
// �����ύX�̊m�菈��
// dragableBorders���X�g�̏������X�V
// ItemsSource�̏������X�V
```

### 3. �t�B�[���h�ƃv���p�e�B�̏ڍא���

�e�t�B�[���h�ɖړI�Ɨp�r���������R�����g��ǉ��F

```csharp
/// <summary>
/// �h���b�O�\�Ȋe�A�C�e����Border�R���g���[���̃��X�g
/// </summary>
private List<Border> dragableBorders = new();

/// <summary>
/// INotifyPropertyChanged����������A�C�e���̃C�x���g�w�ǊǗ�
/// </summary>
private Dictionary<object, INotifyPropertyChanged> subscribedItems = new();

/// <summary>
/// ���ۂɓK�p�����Border�̕�
/// </summary>
private double actualBorderWidth = 100.0;
```

### 4. ���\�b�h�̏ڍׂȐ���

�e���\�b�h�̖ړI�A�p�����[�^�A�߂�l�A����p���ڍׂɋL�q�F

```csharp
/// <summary>
/// DisplayMemberPath�܂���ToString()���g�p���ăA�C�e���̕\���e�L�X�g���擾
/// </summary>
/// <param name="item">�\���e�L�X�g���擾����A�C�e��</param>
/// <returns>�\���e�L�X�g</returns>

/// <summary>
/// �h���b�O���ɃA�C�e���̈ʒu���Ĕz�u
/// </summary>
/// <param name="oldDraggedIndex">�h���b�O�J�n���̃C���f�b�N�X</param>
/// <param name="newDraggedIndex">�V�����C���f�b�N�X</param>
```

## ���P����

### 1. �R�[�h�̉ǐ�����
- ���{��R�����g�ɂ��A���{�l�J���҂̗������e��
- �e�����̖ړI�����m

### 2. �����e�i���X������
- ���G�ȃh���b�O&�h���b�v�����̗��ꂪ�������₷��
- �e���\�b�h�̖����ƐӔC�����m

### 3. �J����������
- IntelliSense�ł̓��{������\��
- �V�K�J���҂̃I���{�[�f�B���O���ԒZ�k

### 4. �h�L�������e�[�V�����i������
- XML�h�L�������g�R�����g�ɂ�鎩���h�L�������g�����Ή�
- API���t�@�����X�̓��{�ꉻ

## �R�����g�L�q���j

### 1. �ړI�d��
- �u�������邩�v�����łȂ��u�Ȃ��������邩�v���L�q

### 2. ��̐�
- ���ۓI�Ȑ����ł͂Ȃ��A��̓I�ȓ�����L�q

### 3. �����̒�
- �֘A���鑼�̃��\�b�h��v���p�e�B�Ƃ̊֌W�����

### 4. ��O�����̐���
- �G���[�����ƃt�H�[���o�b�N�����̐���

���̉��P�ɂ��ADragableList�N���X�͓��{����ł̊J���ɂ����āA��藝�����₷���A�����e�i���X���₷���R�[�h�ɂȂ�܂����B